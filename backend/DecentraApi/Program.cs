using System.Security.Claims;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using DecentraApi.Data;
using DecentraApi.Services;
using DecentraApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Configure specific ports - Kestrel configuration
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // HTTP port
    serverOptions.ListenAnyIP(5002);
    
    // HTTPS port
    // serverOptions.ListenAnyIP(7002, listenOptions =>
    // {
    //     listenOptions.UseHttps();
    // });
});

// Add services to the container
builder.Services.AddDbContext<DecentraDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication - Fixed configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

// Add logging for debugging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // First try to get token from Authorization header
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
                context.Token = authorizationHeader.Substring("Bearer ".Length).Trim();
                Console.WriteLine($"Token found in Authorization header: {context.Token.Substring(0, Math.Min(20, context.Token.Length))}...");
            }
            // If no token in header, check cookie
            else if (context.Request.Cookies.TryGetValue("jwt-token", out string cookieToken))
            {
                context.Token = cookieToken;
                Console.WriteLine($"Token found in cookie: {cookieToken.Substring(0, Math.Min(20, cookieToken.Length))}...");
            }
            else
            {
                Console.WriteLine("No token found in header or cookie");
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("JWT Token validated successfully");
            
            // Debug: Print all claims in the validated token
            if (context.Principal?.Claims?.Any() == true)
            {
                Console.WriteLine("All claims in validated token:");
                foreach (var claim in context.Principal.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }
            }
            
            // Try different claim names for userId
            var userId1 = context.Principal?.FindFirst("userId")?.Value;
            var userId2 = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId3 = context.Principal?.FindFirst("sub")?.Value;
            var userId4 = context.Principal?.FindFirst("id")?.Value;
            
            Console.WriteLine($"User ID from 'userId' claim: {userId1}");
            Console.WriteLine($"User ID from 'NameIdentifier' claim: {userId2}");
            Console.WriteLine($"User ID from 'sub' claim: {userId3}");
            Console.WriteLine($"User ID from 'id' claim: {userId4}");
            
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true,
        ValidateActor = false,
        ValidateTokenReplay = false
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddHttpClient();

// Add custom services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PhotoService>();
builder.Services.AddScoped<AppealService>();

// Add Swagger/OpenAPI
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "DecentraApi", 
        Version = "v1",
        Description = "Taxi Service Car Quality Checking API"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new string[] { }
        }
    });

    // Enable file upload support in Swagger
    c.OperationFilter<FileUploadOperationFilter>();
});

// Add CORS with credentials support
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173","https://localhost:5173") // your FE
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()  // REQUIRED for cookies
));

var app = builder.Build();

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DecentraDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DecentraApi v1.0");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "DecentraApi - Taxi Quality Control";
        c.DefaultModelsExpandDepth(-1);
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        
        // Custom CSS for better appearance
        c.InjectStylesheet("/swagger-ui/custom.css");
    });
}

// app.UseHttpsRedirection();
app.UseCors();

// Authentication and authorization middleware MUST be in this order
app.UseAuthentication();
app.UseAuthorization();

// Add middleware for debugging authentication
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    Console.WriteLine($"User authenticated: {context.User?.Identity?.IsAuthenticated}");
    
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        var userId = context.User.FindFirst("userId")?.Value;
        Console.WriteLine($"Authenticated user ID: {userId}");
    }
    
    await next();
});

// Map endpoints
app.MapAuthEndpoints();
app.MapPhotoEndpoints();
app.MapAppealEndpoints();

// Health check endpoint
app.MapGet("/", () => "DecentraApi is running!")
   .WithName("HealthCheck")
   .WithSummary("Health check endpoint");

Console.WriteLine("DecentraApi starting...");
Console.WriteLine("HTTP: http://localhost:5002");
Console.WriteLine("HTTPS: https://localhost:7002");
Console.WriteLine("Swagger: http://localhost:5002/swagger or https://localhost:7002/swagger");

app.Run();

// File upload operation filter for Swagger
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || 
                       p.ParameterType == typeof(IFormFileCollection) ||
                       p.ParameterType == typeof(List<IFormFile>))
            .ToArray();

        if (fileParams.Any())
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["files"] = new OpenApiSchema
                                {
                                    Type = "array",
                                    Items = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}