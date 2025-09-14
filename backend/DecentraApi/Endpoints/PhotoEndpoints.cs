using Microsoft.AspNetCore.Authorization;
using DecentraApi.Services;
using DecentraApi.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace DecentraApi.Endpoints
{
    public static class PhotoEndpoints
    {
        public static void MapPhotoEndpoints(this WebApplication app)
        {
            var photos = app.MapGroup("/api/photos").WithTags("Photos");

            // POST /api/photos/upload
            photos.MapPost("/upload", [Authorize] async (
                HttpRequest request,
                PhotoService photoService,
                HttpContext httpContext) =>
            {
                var result = await photoService.UploadPhotosAsync(request, httpContext);
                
                if (result.Success)
                {
                    return Results.Ok(result);
                }
                
                return Results.BadRequest(result);
            })
            .Accepts<PhotoUploadRequest>("multipart/form-data")
            .WithName("UploadPhotos")
            .WithSummary("Upload 4 car photos for analysis")
            .WithDescription("Upload left side, right side, front, and back photos of car for damage analysis")
            .Produces<PhotoUploadResponse>(200)
            .Produces<PhotoUploadResponse>(400)
            .Produces(401);

            // GET /api/photos
            photos.MapGet("/", [Authorize] async (
                PhotoService photoService,
                HttpContext httpContext) =>
            {
                var result = await photoService.GetUserPhotosAsync(httpContext);
                
                if (result.Success)
                {
                    return Results.Ok(result);
                }
                
                return Results.BadRequest(result);
            })
            .WithName("GetPhotos")
            .WithSummary("Get user's photos")
            .WithDescription("Retrieve all photos and analysis results for the authenticated user")
            .Produces<GetPhotosResponse>(200)
            .Produces<GetPhotosResponse>(400)
            .Produces(401);
        }
    }
}