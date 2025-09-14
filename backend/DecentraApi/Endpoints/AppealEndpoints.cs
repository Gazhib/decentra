using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DecentraApi.Services;
using DecentraApi.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace DecentraApi.Endpoints;

public static class AppealEndpoints
{
    public static void MapAppealEndpoints(this WebApplication app)
    {
        var appeals = app.MapGroup("/api/appeals").WithTags("Appeals");

        // POST /api/appeals - User creates an appeal
        appeals.MapPost("/", [Authorize] async (
                [FromBody] MakeAppealRequest request,
                AppealService appealService,
                HttpContext httpContext) =>
            {
                Console.WriteLine($"Description: {request.Description}");
                

                if (string.IsNullOrWhiteSpace(request.Description))
                {
                    return Results.BadRequest(new MakeAppealResponse
                    {
                        Success = false,
                        Message = "Description is required"
                    });
                }

                var result = await appealService.MakeAppeal(request, httpContext);

                return result.Success
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .WithName("MakeAppeal")
            .WithSummary("Create a new appeal for photo analysis results")
            .WithDescription("Allows users to appeal photo analysis results they disagree with")
            .Accepts<MakeAppealRequest>("application/json")
            .Produces<MakeAppealResponse>(200)
            .Produces<MakeAppealResponse>(400)
            .Produces(401)
            .Produces(500);

        // GET /api/appeals - Admin gets all appeals
        appeals.MapGet("/", [Authorize] async (
                AppealService appealService,
                HttpContext httpContext) =>
            {
                try
                {
                    var result = await appealService.GetAppeals(httpContext);

                    return result.Success
                        ? Results.Ok(result)
                        : Results.Forbid(); // Return 403 for non-admin users
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        detail: $"An error occurred while retrieving appeals: {ex.Message}",
                        statusCode: 500
                    );
                }
            })
            .WithName("GetAllAppeals")
            .WithSummary("Get all appeals (Admin only)")
            .WithDescription("Retrieve all appeals submitted by users. Requires admin privileges.")
            .Produces<GetAppealsResponse>(200)
            .Produces(401)
            .Produces(403)
            .Produces(500);

        // GET /api/appeals/{id} - Admin gets specific appeal
        appeals.MapGet("/{id:int}", [Authorize] async (
                int id,
                AppealService appealService,
                HttpContext httpContext) =>
            {
                try
                {
                    if (id <= 0)
                    {
                        return Results.BadRequest(new GetAppealResponse
                        {
                            Success = false,
                            Message = "Invalid appeal ID"
                        });
                    }

                    var result = await appealService.GetAppeal(id, httpContext);

                    if (!result.Success)
                    {
                        if (result.Message.Contains("Access denied"))
                        {
                            return Results.Forbid(); // 403 for non-admin
                        }
                        if (result.Message.Contains("not found"))
                        {
                            return Results.NotFound(result); // 404 for not found
                        }
                        return Results.BadRequest(result); // 400 for other errors
                    }

                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        detail: $"An error occurred while retrieving appeal {id}: {ex.Message}",
                        statusCode: 500
                    );
                }
            })
            .WithName("GetAppealById")
            .WithSummary("Get specific appeal details (Admin only)")
            .WithDescription("Retrieve detailed information about a specific appeal including photo analysis data. Requires admin privileges.")
            .Produces<GetAppealResponse>(200)
            .Produces<GetAppealResponse>(400)
            .Produces(401)
            .Produces(403)
            .Produces<GetAppealResponse>(404)
            .Produces(500);

        // PATCH /api/appeals/{id}/status - Admin updates appeal status
        appeals.MapPatch("/{id:int}/status", [Authorize] async (
                int id,
                [FromBody] UpdateAppealStatusRequest request,
                AppealService appealService,
                HttpContext httpContext) =>
            {
                try
                {
                    if (id <= 0)
                    {
                        return Results.BadRequest(new UpdateAppealStatusResponse
                        {
                            Success = false,
                            Message = "Invalid appeal ID"
                        });
                    }

                    Console.WriteLine($"Updating appeal {id} status to: {request.Appealed}");

                    var result = await appealService.UpdateAppealStatus(id, request.Appealed, httpContext);

                    if (!result.Success)
                    {
                        if (result.Message.Contains("Access denied"))
                        {
                            return Results.Forbid(); // 403 for non-admin
                        }
                        if (result.Message.Contains("not found"))
                        {
                            return Results.NotFound(result); // 404 for not found
                        }
                        return Results.BadRequest(result); // 400 for other errors
                    }

                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        detail: $"An error occurred while updating appeal {id}: {ex.Message}",
                        statusCode: 500
                    );
                }
            })
            .WithName("UpdateAppealStatus")
            .WithSummary("Update appeal status (Admin only)")
            .WithDescription("Update the appealed status of an appeal. Set to true if appeal is accepted, false if rejected. Requires admin privileges.")
            .Accepts<UpdateAppealStatusRequest>("application/json")
            .Produces<UpdateAppealStatusResponse>(200)
            .Produces<UpdateAppealStatusResponse>(400)
            .Produces(401)
            .Produces(403)
            .Produces<UpdateAppealStatusResponse>(404)
            .Produces(500);
    }
}