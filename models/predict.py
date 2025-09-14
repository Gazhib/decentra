import os
import io
import base64
from typing import List, Optional, Tuple, Dict, Any

import numpy as np
import torch
import torch.nn as nn
import torch.nn.functional as F
import torchvision.transforms as transforms
from torchvision import models
import torchvision
from torchvision.models.detection.faster_rcnn import FastRCNNPredictor
from torchvision.models.detection.mask_rcnn import MaskRCNNPredictor
from PIL import Image
from fastapi import FastAPI, UploadFile, File, HTTPException, Query
from fastapi.middleware.cors import CORSMiddleware

# Optional dependencies
try:
    import cv2
except Exception:
    cv2 = None

try:
    import pycocotools.mask as mask_utils
    _HAS_COCO = True
except Exception:
    mask_utils = None
    _HAS_COCO = False


# =========================
# Config & Initialization
# =========================
ALLOWED_MIME_TYPES = {
    'image/jpeg', 'image/png', 'image/webp', 'image/bmp', 'image/tiff'
}

MAX_FILES = 4

# Model paths via env vars with sensible defaults
CLASSIFIER_WEIGHTS = os.getenv('CLASSIFIER_WEIGHTS', 'resnet_car_classifier_best.pth')
DETECTOR_WEIGHTS = os.getenv('DETECTOR_WEIGHTS', 'maskrcnn_last.pth')
DETECTOR_CLASSES = os.getenv('DETECTOR_CLASSES', 'classes.json')

DEVICE = torch.device('cuda:0' if torch.cuda.is_available() else 'cpu')

# =========================
# Load classifier (clean/dirty)
# =========================
classifier = models.resnet50(weights=models.ResNet50_Weights.DEFAULT)
num_ftrs = classifier.fc.in_features
classifier.fc = nn.Sequential(
    nn.Dropout(p=0.2),
    nn.Linear(num_ftrs, 2)
)
if not os.path.exists(CLASSIFIER_WEIGHTS):
    raise RuntimeError(f"Classifier weights not found at {CLASSIFIER_WEIGHTS}")
classifier.load_state_dict(torch.load(CLASSIFIER_WEIGHTS, map_location='cpu'))
classifier = classifier.to(DEVICE)
classifier.eval()
class_names = ['clean', 'dirty']

classifier_preprocess = transforms.Compose([
    transforms.Resize(256),
    transforms.CenterCrop(224),
    transforms.ToTensor(),
    transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])
])

# =========================
# Load detector (Mask R-CNN damages)
# =========================
def _load_damage_classes(path: str) -> Dict[str, int]:
    if os.path.exists(path):
        import json
        with open(path, 'r') as f:
            mapping = json.load(f)
        # ensure ints
        return {k: int(v) for k, v in mapping.items()}
    # fallback: try to read from checkpoint payload if present
    ckpt = torch.load(DETECTOR_WEIGHTS, map_location='cpu')
    if isinstance(ckpt, dict) and 'class_mapping' in ckpt:
        return {k: int(v) for k, v in ckpt['class_mapping'].items()}
    raise RuntimeError("Damage classes mapping not found. Provide classes.json or include 'class_mapping' in checkpoint.")


def _build_maskrcnn(num_classes: int):
    model = torchvision.models.detection.maskrcnn_resnet50_fpn(weights=None)
    in_features = model.roi_heads.box_predictor.cls_score.in_features
    model.roi_heads.box_predictor = FastRCNNPredictor(in_features, num_classes)
    in_features_mask = model.roi_heads.mask_predictor.conv5_mask.in_channels
    model.roi_heads.mask_predictor = MaskRCNNPredictor(in_features_mask, 256, num_classes)
    return model


damage_class_mapping = _load_damage_classes(DETECTOR_CLASSES)
damage_num_classes = len(damage_class_mapping) + 1  # + background
damage_id_to_name = {0: 'background', **{int(v): k for k, v in damage_class_mapping.items()}}

detector = _build_maskrcnn(damage_num_classes)
if not os.path.exists(DETECTOR_WEIGHTS):
    raise RuntimeError(f"Detector weights not found at {DETECTOR_WEIGHTS}")
detector_ckpt = torch.load(DETECTOR_WEIGHTS, map_location='cpu')
detector.load_state_dict(detector_ckpt['model_state_dict'] if isinstance(detector_ckpt, dict) and 'model_state_dict' in detector_ckpt else detector_ckpt)
detector = detector.to(DEVICE)
detector.eval()

detector_preprocess = transforms.ToTensor()


# =========================
# Utilities
# =========================
def _validate_file(file: UploadFile):
    if file.content_type not in ALLOWED_MIME_TYPES:
        raise HTTPException(status_code=415, detail=f"Unsupported content type: {file.content_type}")


def _classify(image: Image.Image) -> Tuple[str, float]:
    t = classifier_preprocess(image).unsqueeze(0).to(DEVICE)
    with torch.no_grad():
        logits = classifier(t)
        probs = F.softmax(logits, dim=1)
        conf, pred = torch.max(probs, 1)
    return class_names[pred.item()], float(conf.item())


def _detect(image: Image.Image,
            score_thr: float,
            mask_thr: float,
            max_detections: int) -> Dict[str, Any]:
    t = detector_preprocess(image).to(DEVICE)
    with torch.no_grad():
        out = detector([t])[0]

    boxes = out.get('boxes', torch.empty(0, 4))
    labels = out.get('labels', torch.empty(0, dtype=torch.int64))
    scores = out.get('scores', torch.empty(0))
    masks = out.get('masks', None)

    boxes = boxes.detach().cpu().numpy() if boxes is not None else np.zeros((0, 4))
    labels = labels.detach().cpu().numpy().astype(int) if labels is not None else np.zeros((0,), dtype=int)
    scores = scores.detach().cpu().numpy() if scores is not None else np.zeros((0,))
    if masks is not None:
        masks = masks.detach().cpu().numpy()[:, 0, :, :]  # NxHxW

    keep = scores >= score_thr
    boxes = boxes[keep]
    labels = labels[keep]
    scores = scores[keep]
    if masks is not None:
        masks = masks[keep]

    # Limit detections
    if len(boxes) > max_detections:
        order = np.argsort(-scores)[:max_detections]
        boxes = boxes[order]
        labels = labels[order]
        scores = scores[order]
        if masks is not None:
            masks = masks[order]

    detections = []
    for i in range(len(boxes)):
        x1, y1, x2, y2 = [float(v) for v in boxes[i]]
        cls_id = int(labels[i])
        det = {
            'label': damage_id_to_name.get(cls_id, f'id_{cls_id}'),
            'label_id': cls_id,
            'score': float(scores[i]),
            'bbox_xyxy': [x1, y1, x2, y2],
        }
        if masks is not None:
            det['mask'] = (masks[i] >= mask_thr)
        detections.append(det)

    return {
        'count': len(detections),
        'instances': detections,
    }


def _mask_to_rle_simple(mask: np.ndarray) -> Dict[str, Any]:
    # Simple RLE over C-order flattened array; compact but not COCO-compatible
    arr = mask.astype(np.uint8).flatten(order='C')
    counts = []
    last = arr[0]
    run = 1
    for v in arr[1:]:
        if v == last:
            run += 1
        else:
            counts.append(run)
            run = 1
            last = v
    counts.append(run)
    return {'format': 'rle', 'rle_type': 'simple', 'size': [int(mask.shape[0]), int(mask.shape[1])], 'counts': counts}


def _mask_to_rle_coco(mask: np.ndarray) -> Dict[str, Any]:
    if not _HAS_COCO:
        return _mask_to_rle_simple(mask)
    rle = mask_utils.encode(np.asfortranarray(mask.astype(np.uint8)))
    rle['counts'] = rle['counts'].decode('ascii')
    return {'format': 'rle', 'rle_type': 'coco', 'size': [int(mask.shape[0]), int(mask.shape[1])], 'counts': rle['counts']}


def _mask_to_polygons(mask: np.ndarray) -> Dict[str, Any]:
    if cv2 is None:
        return _mask_to_rle_simple(mask)
    contours, _ = cv2.findContours(mask.astype(np.uint8), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    polys: List[List[List[int]]] = []
    for cnt in contours:
        if len(cnt) < 3:
            continue
        poly = [[int(p[0][0]), int(p[0][1])] for p in cnt]
        polys.append(poly)
    return {'format': 'polygons', 'polygons': polys}


def _build_visual_overlay(image: np.ndarray, detections: Dict[str, Any], alpha: float = 0.5) -> np.ndarray:
    overlay = image.astype(np.float32).copy()
    H, W = image.shape[:2]
    rng = np.random.RandomState(123)
    colors = (rng.rand(max(1, detections['count']), 3) * 255).astype(np.float32)
    for i, inst in enumerate(detections['instances']):
        if 'mask' in inst and isinstance(inst['mask'], np.ndarray):
            m = inst['mask']
            if m.shape[0] == H and m.shape[1] == W and m.any():
                overlay[m] = (1.0 - alpha) * overlay[m] + alpha * colors[i % len(colors)]
    overlay = np.clip(overlay, 0, 255).astype(np.uint8)
    # Draw bboxes and labels using matplotlib-like colors is heavier; skip to keep fast
    return overlay


def _encode_png_base64(img: np.ndarray) -> str:
    from PIL import Image
    import io as _io
    pil_img = Image.fromarray(img)
    buf = _io.BytesIO()
    pil_img.save(buf, format='PNG')
    return base64.b64encode(buf.getvalue()).decode('ascii')


# =========================
# FastAPI App
# =========================
app = FastAPI(title='Car Analysis API', version='1.0.0')

# CORS (allow local dev and .NET apps to call)
app.add_middleware(
    CORSMiddleware,
    allow_origins=['*'],  # tighten in production
    allow_credentials=True,
    allow_methods=['*'],
    allow_headers=['*'],
)


@app.get('/health')
def health():
    return {'status': 'ok'}


@app.post('/api/analyze')
async def analyze(
    files: List[UploadFile] = File(..., description='Up to 4 images'),
    score_threshold: float = Query(0.5, ge=0.0, le=1.0),
    mask_threshold: float = Query(0.5, ge=0.0, le=1.0),
    max_detections: int = Query(100, ge=1, le=1000),
    mask_format: str = Query('coco_rle', pattern='^(none|simple_rle|coco_rle|polygons)$'),
    include_visual: bool = Query(False),
):
    if not files:
        raise HTTPException(status_code=400, detail='No files uploaded')
    if len(files) > MAX_FILES:
        raise HTTPException(status_code=400, detail=f'Maximum {MAX_FILES} files are allowed')

    results: List[Dict[str, Any]] = []
    for f in files:
        _validate_file(f)
        b = await f.read()
        try:
            img = Image.open(io.BytesIO(b)).convert('RGB')
        except Exception as e:
            raise HTTPException(status_code=400, detail=f'Invalid image {f.filename}: {e}')

        # Classification
        cls_label, cls_conf = _classify(img)

        # Detection
        det = _detect(img, score_threshold, mask_threshold, max_detections)

        # Prepare masks in required format
        H, W = img.size[1], img.size[0]
        for inst in det['instances']:
            if 'mask' not in inst or mask_format == 'none':
                inst.pop('mask', None)
                continue
            mask = inst.pop('mask')
            if mask_format == 'simple_rle':
                inst['segmentation'] = _mask_to_rle_simple(mask)
            elif mask_format == 'coco_rle':
                inst['segmentation'] = _mask_to_rle_coco(mask)
            elif mask_format == 'polygons':
                inst['segmentation'] = _mask_to_polygons(mask)
            else:
                inst.pop('mask', None)

        # Optional visualization
        visual_b64 = None
        if include_visual:
            img_np = np.array(img)
            overlay = _build_visual_overlay(img_np, det, alpha=0.5)
            visual_b64 = _encode_png_base64(overlay)

        # Convert label ids to names in instances are already included
        results.append({
            'filename': f.filename,
            'content_type': f.content_type,
            'size_bytes': len(b),
            'classification': {
                'label': cls_label,
                'confidence': round(cls_conf, 6),
            },
            'detection': det,
            'visualization_png_base64': visual_b64,
        })

    return {'results': results}


# How to run:
#   uvicorn models.predict:app --host 0.0.0.0 --port 8000 --workers 1
# Example C# (.NET) client pseudo-code:
#
# using var client = new HttpClient();
# using var form = new MultipartFormDataContent();
# foreach (var path in imagePaths) {
#     var bytes = await File.ReadAllBytesAsync(path);
#     var content = new ByteArrayContent(bytes);
#     content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // or png/webp
#     form.Add(content, "files", Path.GetFileName(path));
# }
# var url = "http://localhost:8000/api/analyze?score_threshold=0.4&mask_threshold=0.5&mask_format=coco_rle&include_visual=true";
# var resp = await client.PostAsync(url, form);
# var json = await resp.Content.ReadAsStringAsync();