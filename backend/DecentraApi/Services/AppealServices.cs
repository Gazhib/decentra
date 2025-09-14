using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using DecentraApi.Data;
using DecentraApi.Models;
using DecentraApi.DTOs;

namespace DecentraApi.Services;

public class AppealService
{
    private readonly DecentraDbContext _context;

    public AppealService(DecentraDbContext context)
    {
        _context = context;
    }

    public async Task<MakeAppealResponse> MakeAppeal(MakeAppealRequest request, HttpContext httpContext)
    {
        try
        {
            var userId = GetUserIdFromContext(httpContext);
            if (userId == null)
            {
                return new MakeAppealResponse
                {
                    Success = false,
                    Message = "User not authenticated"
                };
            }

            // Validate that user owns the photos
            var user = await _context.Users.FindAsync(userId);
            if (user?.PhotoIds == null || !user.PhotoIds.Any())
            {
                return new MakeAppealResponse
                {
                    Success = false,
                    Message = "User has no photos"
                };
            }

            var invalidPhotoIds = request.PhotoIds.Except(user.PhotoIds).ToList();
            if (invalidPhotoIds.Any())
            {
                return new MakeAppealResponse
                {
                    Success = false,
                    Message = $"Invalid photo IDs: {string.Join(", ", invalidPhotoIds)}"
                };
            }

            var existingPhotos = await _context.Photos
                .Where(p => request.PhotoIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var missingPhotos = request.PhotoIds.Except(existingPhotos).ToList();
            if (missingPhotos.Any())
            {
                return new MakeAppealResponse
                {
                    Success = false,
                    Message = $"Photos not found: {string.Join(", ", missingPhotos)}"
                };
            }

            // Create new appeal
            var appeal = new Appeal
            {
                PhotoIds = request.PhotoIds.ToArray(),
                Description = request.Description,
                Appealed = false, // Initially not appealed
            };

            _context.Appeals.Add(appeal);
            await _context.SaveChangesAsync();
            
            user.AppealId = appeal.Id;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();


            return new MakeAppealResponse
            {
                Success = true,
                Message = "Appeal submitted successfully",
                AppealId = appeal.Id
            };
        }
        catch (Exception ex)
        {
            return new MakeAppealResponse
            {
                Success = false,
                Message = $"Error creating appeal: {ex.Message}"
            };
        }
    }

    public async Task<GetAppealsResponse> GetAppeals(HttpContext httpContext)
    {
        try
        {
            // Check if user is admin
            if (!IsAdminUser(httpContext))
            {
                return new GetAppealsResponse
                {
                    Success = false,
                    Message = "Access denied. Admin privileges required.",
                    Appeals = new List<AppealSummary>()
                };
            }

            var appeals = await _context.Appeals.ToListAsync();

            var appealSummaries = appeals.Select(appeal => new AppealSummary
            {
                Id = appeal.Id,
                PhotoIds = appeal.PhotoIds.ToList(),
                Description = appeal.Description,
                Appealed = appeal.Appealed,
            }).ToList();

            return new GetAppealsResponse
            {
                Success = true,
                Message = $"Retrieved {appealSummaries.Count} appeals",
                Appeals = appealSummaries
            };
        }
        catch (Exception ex)
        {
            return new GetAppealsResponse
            {
                Success = false,
                Message = $"Error retrieving appeals: {ex.Message}",
                Appeals = new List<AppealSummary>()
            };
        }
    }

    public async Task<GetAppealResponse> GetAppeal(int appealId, HttpContext httpContext)
    {
        try
        {
            // Check if user is admin
            if (!IsAdminUser(httpContext))
            {
                return new GetAppealResponse
                {
                    Success = false,
                    Message = "Access denied. Admin privileges required."
                };
            }

            var appeal = await _context.Appeals.FindAsync(appealId);
            if (appeal == null)
            {
                return new GetAppealResponse
                {
                    Success = false,
                    Message = $"Appeal with ID {appealId} not found"
                };
            }

            // Get photo details for this appeal
            var photos = await _context.Photos
                .Where(p => appeal.PhotoIds.Contains(p.Id))
                .ToListAsync();

            var photoDetails = photos.Select(photo => new AppealPhotoDetail
            {
                PhotoId = photo.Id,
                Rust = photo.Rust,
                Dent = photo.Dent,
                Scratch = photo.Scratch,
                Dust = photo.Dust,
                LastUpdated = photo.LastUpdated,
                Image = photo.Image,
            }).ToList();

            var appealDetail = new AppealDetail
            {
                Id = appeal.Id,
                Description = appeal.Description,
                Appealed = appeal.Appealed,
                Photos = photoDetails
            };

            return new GetAppealResponse
            {
                Success = true,
                Message = "Appeal retrieved successfully",
                Appeal = appealDetail
            };
        }
        catch (Exception ex)
        {
            return new GetAppealResponse
            {
                Success = false,
                Message = $"Error retrieving appeal: {ex.Message}"
            };
        }
    }

    public async Task<UpdateAppealStatusResponse> UpdateAppealStatus(int appealId, bool appealed, HttpContext httpContext)
    {
        try
        {
            // Check if user is admin
            if (!IsAdminUser(httpContext))
            {
                return new UpdateAppealStatusResponse
                {
                    Success = false,
                    Message = "Access denied. Admin privileges required."
                };
            }

            var appeal = await _context.Appeals.FindAsync(appealId);
            if (appeal == null)
            {
                return new UpdateAppealStatusResponse
                {
                    Success = false,
                    Message = $"Appeal with ID {appealId} not found"
                };
            }

            appeal.Appealed = appealed;

            _context.Appeals.Update(appeal);
            await _context.SaveChangesAsync();

            return new UpdateAppealStatusResponse
            {
                Success = true,
                Message = $"Appeal status updated to {(appealed ? "appealed" : "not appealed")}",
                AppealId = appeal.Id,
                NewStatus = appealed
            };
        }
        catch (Exception ex)
        {
            return new UpdateAppealStatusResponse
            {
                Success = false,
                Message = $"Error updating appeal status: {ex.Message}"
            };
        }
    }

    private int? GetUserIdFromContext(HttpContext httpContext)
    {
        Console.WriteLine("=== GetUserIdFromContext Debug in AppealService ===");
        Console.WriteLine($"User authenticated: {httpContext.User?.Identity?.IsAuthenticated}");
        
        // Method 1: Try different claim names for user ID
        var claimNames = new[] { 
            ClaimTypes.NameIdentifier,
            "userId", 
            "sub", 
            "id", 
            "nameid",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
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
            }
        }

        // Method 2: From cookies (fallback)
        if (httpContext.Request.Cookies.TryGetValue("UserId", out string cookieUserId) &&
            int.TryParse(cookieUserId, out int parsedCookieUserId))
        {
            Console.WriteLine($"Found user ID from UserId cookie: {parsedCookieUserId}");
            return parsedCookieUserId;
        }

        Console.WriteLine("No user ID found in any context");
        Console.WriteLine("=== End GetUserIdFromContext Debug ===");
        return null;
    }

    private bool IsAdminUser(HttpContext httpContext)
    {
        // Check if user has admin role claim
        var roleClaim = httpContext.User?.FindFirst(ClaimTypes.Role);
        if (roleClaim != null && roleClaim.Value.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Alternative: check for admin claim
        var adminClaim = httpContext.User?.FindFirst("isAdmin");
        if (adminClaim != null && bool.TryParse(adminClaim.Value, out bool isAdmin))
        {
            return isAdmin;
        }

        return false;
    }
}