using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using DecentraApi.Data;
using DecentraApi.Models;
using DecentraApi.DTOs;

namespace DecentraApi.Services
{
    public class PhotoService
    {
        private readonly DecentraDbContext _context;
        private readonly ILogger<PhotoService> _logger;

        public PhotoService(DecentraDbContext context, ILogger<PhotoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PhotoUploadResponse> UploadPhotosAsync(HttpRequest request, HttpContext httpContext)
        {
            try
            {
                var userId = GetUserIdFromContext(httpContext);
                if (userId == null)
                {
                    return new PhotoUploadResponse
                    {
                        Success = false,
                        Message = "User not authenticated"
                    };
                }

                var form = await request.ReadFormAsync();
                var photos = new[]
                {
                    form.Files.GetFile("leftSide"),
                    form.Files.GetFile("rightSide"),
                    form.Files.GetFile("front"),
                    form.Files.GetFile("back")
                };

                // Validate that all 4 photos are provided
                if (photos.Any(p => p == null))
                {
                    return new PhotoUploadResponse
                    {
                        Success = false,
                        Message = "All 4 photos are required: leftSide, rightSide, front, back"
                    };
                }

                // Validate file types and sizes
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
                const int maxFileSize = 10 * 1024 * 1024; // 10MB

                foreach (var photo in photos)
                {
                    if (photo.Length > maxFileSize)
                    {
                        return new PhotoUploadResponse
                        {
                            Success = false,
                            Message = $"File {photo.FileName} exceeds maximum size of 10MB"
                        };
                    }

                    var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        return new PhotoUploadResponse
                        {
                            Success = false,
                            Message = $"File {photo.FileName} has unsupported format. Allowed: jpg, jpeg, png, bmp"
                        };
                    }
                }

                var photoIds = new List<int>();
                var photoTypes = new[] { "leftSide", "rightSide", "front", "back" };
                var analysisResponse = await SendToPythonServerAsync(photos);

                // Process each photo
                for (int i = 0; i < photos.Length; i++)
                {
                    var photoFile = photos[i];
                    var photoType = photoTypes[i];

                    // Convert to base64 for storage
                    string base64Data;
                    using (var memoryStream = new MemoryStream())
                    {
                        await photoFile.CopyToAsync(memoryStream);
                        var fileBytes = memoryStream.ToArray();
                        base64Data = Convert.ToBase64String(fileBytes);
                    }

                    // TODO: Send to Python server for analysis
                    // var analysisResult = await SendToPythonServerAsync(photos);

                    // Python server analysis logic here
                    // For now, return placeholder analysis results
                    var analysisResult = analysisResponse?.results?.ElementAtOrDefault(i);

                    // Default empty markers
                    var rust = string.Empty;
                    var dent = string.Empty;
                    var scratch = string.Empty;
                    var dust = string.Empty;

                    // List of detected damage class names (from your Python model)
                    var detectedClasses = new List<string>();

                    // Collect mask / segmentation info to store as JSON
                    object masksObj = new { instances = new List<object>() };

                    if (analysisResult != null)
                    {
                        // Classification -> dust/dirty mapping
                        if (analysisResult.classification != null &&
                            string.Equals(analysisResult.classification.label, "dirty", StringComparison.OrdinalIgnoreCase))
                        {
                            dust = "dust";
                        }

                        // Detection instances: map labels to fields and collect masks
                        if (analysisResult.detection != null && analysisResult.detection.instances != null)
                        {
                            var instancesList = new List<object>();
                            foreach (var inst in analysisResult.detection.instances)
                            {
                                // inst is a JsonElement or object; normalize via serialization
                                var instJson = JsonSerializer.Serialize(inst);
                                var instObj = JsonSerializer.Deserialize<Dictionary<string, object?>>(instJson);
                                if (instObj != null && instObj.TryGetValue("label", out var labelObj) && labelObj != null)
                                {
                                    var labelStr = labelObj.ToString() ?? string.Empty;
                                    detectedClasses.Add(labelStr);

                                    // Map to our simple fields
                                    if (labelStr.Equals("Rust", StringComparison.OrdinalIgnoreCase) || labelStr.Equals("Corrosion", StringComparison.OrdinalIgnoreCase))
                                        rust = "rust";
                                    if (labelStr.Equals("Dent", StringComparison.OrdinalIgnoreCase))
                                        dent = "dent";
                                    if (labelStr.Equals("Scratch", StringComparison.OrdinalIgnoreCase) || labelStr.Equals("Paint chip", StringComparison.OrdinalIgnoreCase))
                                        scratch = "scratch";

                                    instancesList.Add(instObj);
                                }
                            }

                            masksObj = new { instances = instancesList };
                        }
                    }
                    // Create photo entity - each photo analyzes all 4 aspects
                    var photoEntity = new Photo
                    {
                        LastUpdated = DateTime.UtcNow,
                        Rust = rust,
                        Dent = dent,
                        Scratch = scratch,
                        Dust = dust,
                        Image = base64Data,
                        DamageClasses = JsonSerializer.Serialize(detectedClasses),
                        Masks = JsonSerializer.Serialize(masksObj),
                    };

                    _context.Photos.Add(photoEntity);
                    await _context.SaveChangesAsync();

                    photoIds.Add(photoEntity.Id);
                }

                // Update user with photo IDs
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    user.PhotoIds = photoIds.ToArray();
                    await _context.SaveChangesAsync();
                }

                return new PhotoUploadResponse
                {
                    Success = true,
                    Message = "Фото успешно добавлены и проанализированы",
                    PhotoIds = photoIds.ToArray()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Что то пошло не так...");
                return new PhotoUploadResponse
                {
                    Success = false,
                    Message = $"Error uploading photos: {ex.Message}"
                };
            }
        }

        public async Task<GetPhotosResponse> GetUserPhotosAsync(HttpContext httpContext)
        {
            try
            {
                var userId = GetUserIdFromContext(httpContext);
                if (userId == null)
                {
                    return new GetPhotosResponse
                    {
                        Success = false,
                        Message = "User not authenticated"
                    };
                }

                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null || user.PhotoIds == null || user.PhotoIds.Length == 0)
                {
                    return new GetPhotosResponse
                    {
                        Success = true,
                        Message = "No photos found",
                        Photos = new List<PhotoResponse>()
                    };
                }

                var userPhotos = await _context.Photos
                    .Where(p => user.PhotoIds.Contains(p.Id))
                    .OrderBy(p => p.Id)
                    .Select(p => new PhotoResponse
                    {
                        Id = p.Id,
                        LastUpdated = p.LastUpdated,
                        Rust = p.Rust,
                        Dent = p.Dent,
                        Scratch = p.Scratch,
                        Dust = p.Dust,
                        Image = p.Image,
                        DamageClasses = p.DamageClasses,
                        Masks = p.Masks,
                    })
                    .ToListAsync();

                return new GetPhotosResponse
                {
                    Success = true,
                    Message = "Photos retrieved successfully",
                    Photos = userPhotos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving photos for user");
                return new GetPhotosResponse
                {
                    Success = false,
                    Message = $"Error retrieving photos: {ex.Message}"
                };
            }
        }

        private int? GetUserIdFromContext(HttpContext httpContext)
        {
            Console.WriteLine("=== GetUserIdFromContext Debug ===");
            Console.WriteLine($"User authenticated: {httpContext.User?.Identity?.IsAuthenticated}");

            // Debug: Print all available claims first
            if (httpContext.User?.Claims?.Any() == true)
            {
                Console.WriteLine("Available claims in PhotoService:");
                foreach (var claim in httpContext.User.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }
            }
            else
            {
                Console.WriteLine("No claims found in user context");
            }

            // Method 1: Try different claim names for user ID (prioritize the ones that work)
            var claimNames = new[] {
                ClaimTypes.NameIdentifier, // This one is working! (has value "2")
                "userId",
                "sub",
                "id",
                "nameid",
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" // Full URL format
            };

            foreach (var claimName in claimNames)
            {
                var userIdClaim = httpContext.User?.FindFirst(claimName);
                if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
                {
                    Console.WriteLine($"Found claim '{claimName}': {userIdClaim.Value}");
                    if (int.TryParse(userIdClaim.Value, out int claimUserId))
                    {
                        Console.WriteLine($"Successfully parsed user ID from '{claimName}' claim: {claimUserId}");
                        return claimUserId;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse '{userIdClaim.Value}' as integer from '{claimName}' claim");
                    }
                }
            }

            // Method 2: From cookies (fallback - direct cookie access)
            if (httpContext.Request.Cookies.TryGetValue("UserId", out var cookieUserIdObj) &&
                !string.IsNullOrEmpty(cookieUserIdObj) &&
                int.TryParse(cookieUserIdObj, out int parsedCookieUserId))
            {
                Console.WriteLine($"Found user ID from UserId cookie: {parsedCookieUserId}");
                return parsedCookieUserId;
            }

            // Method 3: From session (if using sessions)
            if (httpContext.Session != null && httpContext.Session.TryGetValue("UserId", out var sessionBytes) && sessionBytes != null)
            {
                var sessionUserId = System.Text.Encoding.UTF8.GetString(sessionBytes);
                if (int.TryParse(sessionUserId, out int parsedSessionUserId))
                {
                    Console.WriteLine($"Found user ID from session: {parsedSessionUserId}");
                    return parsedSessionUserId;
                }
            }

            Console.WriteLine("No user ID found in any context");
            Console.WriteLine("=== End GetUserIdFromContext Debug ===");
            return null; // User ID not found
        }

        private class PythonAnalysisInstance
        {
            public string? label { get; set; }
            public double? confidence { get; set; }
        }

        private class PythonAnalysisDetection
        {
            public int? count { get; set; }
            public List<object>? instances { get; set; }
        }

        private class PythonAnalysisResult
        {
            public string? filename { get; set; }
            public string? content_type { get; set; }
            public int? size_bytes { get; set; }
            public PythonAnalysisClassification? classification { get; set; }
            public PythonAnalysisDetection? detection { get; set; }
            public string? visualization_png_base64 { get; set; }
        }

        private class PythonAnalysisClassification
        {
            public string? label { get; set; }
            public double? confidence { get; set; }
        }

        private class PythonAnalysisResponse
        {
            public List<PythonAnalysisResult>? results { get; set; }
        }

        private async Task<PythonAnalysisResponse> SendToPythonServerAsync(IFormFile[] photos)
{
    try
    {
        using var httpClient = new HttpClient();
        using var content = new MultipartFormDataContent();

        foreach (var photo in photos)
        {
            var stream = photo.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(photo.ContentType);
            // Field name must be "files" for each image
            content.Add(fileContent, "files", photo.FileName);
        }

        var pythonServiceUrl = "http://localhost:8000/api/analyze";
        httpClient.Timeout = TimeSpan.FromMinutes(2);

        var response = await httpClient.PostAsync(pythonServiceUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            
            // Print Python's response to console
            Console.WriteLine("=== Python ML Service Response ===");
            Console.WriteLine(jsonResponse);
            Console.WriteLine("=== End Python Response ===");
            
            var analysisResult = JsonSerializer.Deserialize<PythonAnalysisResponse>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            return analysisResult ?? new PythonAnalysisResponse();
        }
        else
        {
            // Print error response from Python service
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("=== Python ML Service ERROR ===");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Reason: {response.ReasonPhrase}");
            Console.WriteLine($"Error Content: {errorContent}");
            Console.WriteLine("=== End Python Error ===");
            
            _logger.LogError("Python ML service returned error: {StatusCode} - {ReasonPhrase}",
                response.StatusCode, response.ReasonPhrase);
            return new PythonAnalysisResponse();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("=== Python ML Service Exception ===");
        Console.WriteLine($"Exception: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        Console.WriteLine("=== End Python Exception ===");
        
        _logger.LogError(ex, "Error communicating with Python ML service");
        return new PythonAnalysisResponse();
    }
}
    }

}