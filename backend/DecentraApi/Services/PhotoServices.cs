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
                    var analysisResult = "analysis_placeholder";

                    // Create photo entity - each photo analyzes all 4 aspects
                    var photoEntity = new Photo
                    {
                        LastUpdated = DateTime.UtcNow,
                        Rust = analysisResult,
                        Dent = analysisResult, 
                        Scratch = analysisResult,
                        Dust = analysisResult,
                        Image = base64Data,
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
                    Message = "Photos uploaded and analyzed successfully",
                    PhotoIds = photoIds.ToArray()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photos for user");
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
            if (httpContext.Request.Cookies.TryGetValue("UserId", out string cookieUserId) &&
                int.TryParse(cookieUserId, out int parsedCookieUserId))
            {
                Console.WriteLine($"Found user ID from UserId cookie: {parsedCookieUserId}");
                return parsedCookieUserId;
            }

            // Method 3: From session (if using sessions)
            if (httpContext.Session != null && httpContext.Session.TryGetValue("UserId", out byte[] sessionBytes))
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

            // private async Task<PythonAnalysisResponse> SendToPythonServerAsync(IFormFile[] photos)
            // {
            //     try
            //     {
            //         using var httpClient = new HttpClient();
            //         using var content = new MultipartFormDataContent();
            //         
            //         // Add photos to multipart content
            //         var photoNames = new[] { "leftSide", "rightSide", "front", "back" };
            //         for (int i = 0; i < photos.Length; i++)
            //         {
            //             var photo = photos[i];
            //             var photoName = photoNames[i];
            //             
            //             using var photoStream = photo.OpenReadStream();
            //             var photoContent = new StreamContent(photoStream);
            //             photoContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(photo.ContentType);
            //             
            //             content.Add(photoContent, photoName, photo.FileName);
            //         }
            //
            //         // Configure Python ML service endpoint
            //         var pythonServiceUrl = "http://localhost:5000/analyze"; // Change port as needed
            //         
            //         // Set timeout for ML processing
            //         httpClient.Timeout = TimeSpan.FromMinutes(5);
            //         
            //         _logger.LogInformation("Sending photos to Python ML service at {Url}", pythonServiceUrl);
            //         
            //         var response = await httpClient.PostAsync(pythonServiceUrl, content);
            //         
            //         if (response.IsSuccessStatusCode)
            //         {
            //             var jsonResponse = await response.Content.ReadAsStringAsync();
            //             var analysisResult = System.Text.Json.JsonSerializer.Deserialize<PythonAnalysisResponse>(
            //                 jsonResponse, 
            //                 new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            //             
            //             _logger.LogInformation("Successfully received analysis from Python ML service");
            //             return analysisResult ?? new PythonAnalysisResponse();
            //         }
            //         else
            //         {
            //             _logger.LogError("Python ML service returned error: {StatusCode} - {ReasonPhrase}", 
            //                 response.StatusCode, response.ReasonPhrase);
            //             
            //             // Return default values on error
            //             return new PythonAnalysisResponse
            //             {
            //                 Rust = "analysis_failed",
            //                 Dent = "analysis_failed",
            //                 Scratch = "analysis_failed",
            //                 Dust = "analysis_failed"
            //             };
            //         }
            //     }
            //     catch (HttpRequestException ex)
            //     {
            //         _logger.LogError(ex, "Network error communicating with Python ML service");
            //         return new PythonAnalysisResponse
            //         {
            //             Rust = "network_error",
            //             Dent = "network_error", 
            //             Scratch = "network_error",
            //             Dust = "network_error"
            //         };
            //     }
            //     catch (TaskCanceledException ex)
            //     {
            //         _logger.LogError(ex, "Timeout communicating with Python ML service");
            //         return new PythonAnalysisResponse
            //         {
            //             Rust = "timeout_error",
            //             Dent = "timeout_error",
            //             Scratch = "timeout_error", 
            //             Dust = "timeout_error"
            //         };
            //     }
            //     catch (Exception ex)
            //     {
            //         _logger.LogError(ex, "Unexpected error communicating with Python ML service");
            //         return new PythonAnalysisResponse
            //         {
            //             Rust = "unexpected_error",
            //             Dent = "unexpected_error",
            //             Scratch = "unexpected_error",
            //             Dust = "unexpected_error"
            //         };
            //     }
            // }
    }
}