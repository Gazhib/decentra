using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DecentraApi.Services;
using DecentraApi.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace DecentraApi.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var auth = app.MapGroup("/api/auth").WithTags("Authentication");

            // POST /api/auth/CreateUser
            auth.MapPost("/CreateUser", async (CreateUserRequest request, AuthService authService, HttpContext httpContext) =>
            {
                var result = await authService.CreateUserAsync(request, httpContext);
                
                if (result.Success)
                {
                    return Results.Ok(result);
                }
                
                return Results.BadRequest(result);
            })
            .WithName("CreateUser")
            .WithSummary("Create a new user")
            .WithDescription("Creates a new user account and sets authentication cookie")
            .Produces<AuthResponse>(200)
            .Produces<AuthResponse>(400);

            // POST /api/auth/Login
            auth.MapPost("/Login", async (LoginRequest request, AuthService authService, HttpContext httpContext) =>
            {
                var result = await authService.LoginAsync(request, httpContext);
                
                if (result.Success)
                {
                    return Results.Ok(result);
                }
                
                return Results.Unauthorized();
            })
            .WithName("Login")
            .WithSummary("Login user")
            .WithDescription("Authenticates user and sets authentication cookie")
            .Produces<AuthResponse>(200)
            .Produces(401);

            // GET /api/auth/UserProfile
            auth.MapGet("/UserProfile", [Authorize] async (ClaimsPrincipal user, AuthService authService) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Results.BadRequest("Invalid user ID in token");
                }

                var profile = await authService.GetUserProfileAsync(userId);
                
                if (profile == null)
                {
                    return Results.NotFound("User not found");
                }
                
                return Results.Ok(profile);
            })
            .WithName("GetUserProfile")
            .WithSummary("Get current user profile")
            .WithDescription("Retrieves the profile of the currently authenticated user")
            .Produces<UserProfileResponse>(200)
            .Produces(401)
            .Produces(404);

            // POST /api/auth/Logout
            auth.MapPost("/Logout", [Authorize] (AuthService authService, HttpContext httpContext) =>
            {
                // Clear the JWT token cookie
                authService.Logout(httpContext);
                return Results.Ok(new { message = "Logged out successfully" });
            })
            .WithName("Logout")
            .WithSummary("Logout user")
            .WithDescription("Logs out the user by clearing the authentication cookie")
            .Produces(200)
            .Produces(401);

            // GET /api/auth/IsActive/{id}
            auth.MapGet("/IsActive/{id}", [Authorize] async (int id, AuthService authService) =>
            {
                var result = await authService.CheckIsActiveAsync(id);
                return Results.Ok(result);
            })
            .WithName("IsActive")
            .WithSummary("Check if user is active")
            .WithDescription("Check the active status of a user by their ID. Returns whether the user account is active and accessible.")
            .WithMetadata(new SwaggerOperationAttribute("Check User Active Status", "Verify if a user account is active"))
            .Produces<IsActiveResponse>(200, "application/json")
            .Produces(401);

            // GET /api/auth/CheckAuth
            auth.MapGet("/CheckAuth", [Authorize] (ClaimsPrincipal user) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
                
                return Results.Ok(new 
                { 
                    isAuthenticated = true,
                    userId = userIdClaim,
                    role = roleClaim,
                    message = "User is authenticated" 
                });
            })
            .WithName("CheckAuth")
            .WithSummary("Check authentication status")
            .WithDescription("Verifies if the current request is authenticated via cookie")
            .Produces(200)
            .Produces(401);
            
            auth.MapGet("/me", [Authorize] async (ClaimsPrincipal user, AuthService authService) =>
                {
                    var result = await authService.GetCurrentUserAsync(user);
    
                    if (result.IsAuthenticated)
                    {
                        return Results.Ok(result);
                    }
    
                    return Results.Unauthorized();
                })
                .WithName("GetMe")
                .WithSummary("Get current authenticated user info")
                .WithDescription("Returns authentication status, token validity, user role and basic profile information")
                .Produces<AuthMeResponse>(200)
                .Produces(401);
        }
    }
}