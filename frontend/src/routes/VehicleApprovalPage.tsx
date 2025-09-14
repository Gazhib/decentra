import { useState } from "react";
import UploadPhotoForm from "../features/upload-photo/UploadPhotoForm";
import { useActionData } from "react-router-dom";
import { motion } from "framer-motion";
import { usePictures } from "../entities/picture/model/usePictures";
import AppealList from "../features/appeal/ui/Appeal";
type FileArray = {
  label: string;
  file: File | null;
  name: string;
};

export default function VehicleApprovalPage() {
  const [previews, setPreviews] = useState<string[]>(Array(4).fill(null));
  const [files, setFiles] = useState<FileArray[]>([
    {
      label: "Передняя часть",
      name: "front",
      file: null,
    },
    {
      label: "Задняя часть",
      name: "back",
      file: null,
    },
    {
      label: "Левая часть",
      name: "leftSide",
      file: null,
    },
    {
      label: "Правая часть",
      name: "rightSide",
      file: null,
    },
  ]);

  const addPhoto = (e: React.ChangeEvent<HTMLInputElement>, i: number) => {
    if (e.target.files && e.target.files[0]) {
      setFiles((prev) => {
        const newFiles = [...prev];
        newFiles[i].file = e.target.files![0];
        return newFiles;
      });

      setPreviews((prev) => {
        const newPreviews = [...prev];
        newPreviews[i] = URL.createObjectURL(e.target.files![0]);
        return newPreviews;
      });
    }
  };

  const actionData = useActionData();

  const { photos, isLoading } = usePictures();

  return (
    <main className="max-w-full min-h-full flex flex-col items-center justify-center gap-[20px] bg-white px-[50px] py-[20px] overflow-x-hidden">
      <section className="w-full flex flex-col items-center justify-center gap-[10px]">
        <span className="text-[1.5rem] font-bold">
          Пожалуйста, загрузите фотографии вашего транспортного средства для
          одобрения.
        </span>
        <span className="text-[1rem] text-center text-black/80">
          Убедитесь, что фотографии четкие и показывают все стороны вашего
          автомобиля.
        </span>
      </section>
      <section className="flex flex-col items-center justify-center h-full gap-[20px]">
        <UploadPhotoForm
          previews={previews}
          addPhoto={addPhoto}
          files={files}
        />
        {actionData && actionData.error && (
          <span className="text-red-500">{actionData.error}</span>
        )}
        {actionData && actionData.result && (
          <>
            <span className="text-green-500">{actionData.result.message}</span>
          </>
        )}
        <AppealList isLoading={isLoading} photos={photos} />
      </section>
      <section className="w-full flex items-center justify-center flex-col gap-[10px]">
        <span className="text-[3rem]">Пример фотографий</span>
        <motion.img
          initial={{ opacity: 0, y: 50 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="block w-[50%] rounded-[16px] shadow h-auto object-cover"
          src="https://avtoelektrik-info.ru/wp-content/uploads/2019/08/fe044b5s-1920-1024x769.jpg"
        />
      </section>
    </main>
  );
}

export const action = async ({ request }: { request: Request }) => {
  const formData = await request.formData();
  const front = formData.get("front");
  const back = formData.get("back");
  const leftSide = formData.get("leftSide");
  const rightSide = formData.get("rightSide");
  if (!front || !back || !leftSide || !rightSide) {
    console.log(front, back, leftSide, rightSide);
    return { error: "Пожалуйста, загрузите все 4 фотографии." };
  }

  const response = await fetch(`/api/photos/upload`, {
    method: "POST",
    body: formData,
    credentials: "include",
  });

  if (!response.ok) {
    return {
      message:
        "Ошибка при загрузке фотографий. Пожалуйста, попробуйте еще раз.",
      success: false,
    };
  }

  const result = await response.json();
  return { result, success: true };
};
