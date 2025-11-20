using DrawebData.Models;
using DrawebData.Repos;
using DrawebData.TransferObjects;
using DrawebData.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string not found");

builder.Services
    .AddDbContext<DrawebDbContext>(options => options.UseMySQL(connectionString));
builder.Services
    .AddScoped<IUserRepo, UserRepo>();
builder.Services
    .AddScoped<IDrawingRepo, DrawingRepo>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:Issuer"];
        options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:Audience"];
        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!));
    });

builder.Services
    .AddAuthorization();

builder.Services
    .AddCors(options =>
    {
        options.AddPolicy(name: "DevelopmentPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
            });
    });


var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {   /*TODO
        var exceptionHandlerPathFeature =
            context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature.Error;

        // Log the exception (usando ILogger)
        var logger = errorApp.ApplicationServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Unhandled exception occurred.");

        // Map specific exceptions to HTTP status codes
        context.Response.ContentType = "application/json";
        if (exception is UserNotFoundException) // Tu excepción personalizada del DAO
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.WriteAsJsonAsync(new { message = exception.Message });
        }
        else if (exception is DataAccessException) // Otra excepción personalizada
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "A database error occurred." });
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
        }*/
    });
});


app.MapPost("/draweb-api/users", async Task<Results<Created<UserDTO>, Conflict<object>, InternalServerError, BadRequest<object>>>(
    IUserRepo userRepo, User user, 
    ILogger<RouteHandler> logger) =>
{
    if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
    {
        return TypedResults.BadRequest<object>(new {Message = "Username, email and password parameters are required"});
    }

    //TODO> implement password and email checking
    var result = await userRepo.CreateUser(user.Username, user.Email, user.Password);
    
    if (result.IsSuccess)
    {
        logger.LogInformation(result.Message);
        return TypedResults.Created($"/draweb-api/users/{result.Data!.Id}", result.Data);
    }
    else if (result.ErrorType.Equals(ErrorType.UserAlreadyExists))
    {
        logger.LogInformation(result.Message);
        return TypedResults.Conflict<object>(new {Message="A user with the same information already exists"});
    }
    else
    {
        logger.LogWarning(result.Message);
        return TypedResults.InternalServerError();
    }
});


app.MapPost("/draweb-api/authenticate", async Task<Results<BadRequest<object>, Ok<object>, UnauthorizedHttpResult>> (
    IUserRepo userRepo,
    User user,
    ILogger<RouteHandler> logger,
    IConfiguration configuration) =>
{
    if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
    {
        logger.LogWarning("A login attempt with wrong format was made");
        return TypedResults.BadRequest<object>(new { Message = "Username and password parameters are required"});
    }

    var result = await userRepo.Login(user.Username, user.Password);

    if (result.IsSuccess)
    {
        logger.LogInformation(result.Message);

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        List<Claim> claims = [
            new(JwtRegisteredClaimNames.Sid, result.Data!.Id.ToString()),
            new(JwtRegisteredClaimNames.Nickname, result.Data.Username),
            new(JwtRegisteredClaimNames.Email, result.Data.Email),
            new(ClaimTypes.Role, "Member")
        ];

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(configuration["Jwt:ExpirationInMinutes"] ?? "60"))
        };

        var tokenHandler = new JsonWebTokenHandler();
        string token = tokenHandler.CreateToken(tokenDescriptor);
        return TypedResults.Ok<object>(new { Token = token });
    }
    else
    {
        logger.LogWarning(result.Message);
        return TypedResults.Unauthorized();
    }
});


app.MapPost("/draweb-api/users/{id}/drawings", async Task<Results<Ok<DrawingDTO>, BadRequest<object>, NotFound, InternalServerError>> (
    int id, 
    Drawing drawing, 
    IDrawingRepo drawRepo, 
    ILogger<RouteHandler> logger) =>
{
    if (string.IsNullOrWhiteSpace(drawing.Title) || string.IsNullOrWhiteSpace(drawing.Svg))
    {
        logger.LogWarning("A drawing saving attempt with wrong format was made");
        return TypedResults.BadRequest<object>(new { Message = "Title and svg format of the drawing are required"});
    }

    var result = await drawRepo.SaveDraw(id, drawing.Title, drawing.Svg);

    if (result.IsSuccess)
    {
        logger.LogInformation(result.Message);
        return TypedResults.Ok(result.Data);
    }
    else if (result.ErrorType == ErrorType.ResourceDoesNotExist)
    {
        logger.LogWarning(result.Message);
        return TypedResults.NotFound();
    } 
    else
    {
        logger.LogWarning(result.Message);
        return TypedResults.InternalServerError();
    }
}).RequireAuthorization();


app.MapGet("/draweb-api/users/drawings/{id}", async Task<Results<Ok<object>, NotFound, Conflict<object>, InternalServerError>>  (
    int id, 
    IDrawingRepo drawRepo, 
    ILogger<RouteHandler> logger) =>
{
    if (id < 1)
    {
        logger.LogWarning("Attempted to get a drawing with non-valid ID: {id}", id);
        return TypedResults.NotFound();
    }

    var result = await drawRepo.GetSvgDrawing(id);
    if (result.IsSuccess)
    {
        logger.LogInformation(result.Message);
        return TypedResults.Ok<object>(new { Svg = result.Data});
    }
    else if (result.ErrorType == ErrorType.ResourceDoesNotExist)
    {
        logger.LogWarning(result.Message);
        return TypedResults.NotFound();
    }
    else if (result.ErrorType == ErrorType.FailedOperationExecution)
    {
        logger.LogWarning(result.Message);
        return TypedResults.Conflict<object>(new{ Message = "The drawing is corrupted." });
    }
    else
    {
        logger.LogWarning(result.Message);
        return TypedResults.InternalServerError();
    }
}).RequireAuthorization();


app.MapGet("/draweb-api/users/{id}/drawings", async Task<Results<Ok<List<DrawingDTO>>, BadRequest<object>, NotFound, InternalServerError>> (
    int id,
    int? size,
    DateTime? cursor,
    IDrawingRepo drawRepo, 
    ILogger<RouteHandler> logger) =>
{
    if (size == null || size < 0)
    {
        logger.LogWarning("A Drawing retrieving attempt with wrong format was made");
        return TypedResults.BadRequest<object>(new { Message = "A page size and last drawing update date are required"});
    }

    var result = await drawRepo.GetDrawsByUserIdWithPagination(id, cursor ?? DateTime.MinValue, (int)size);

    if (result.IsSuccess)
    {
        logger.LogInformation(result.Message);
        return TypedResults.Ok(result.Data);
    }
    else if(result.ErrorType == ErrorType.ResourceDoesNotExist)
    {
        logger.LogWarning(result.Message);
        return TypedResults.NotFound();
    }
    else
    {
        logger.LogWarning(result.Message);
        return TypedResults.InternalServerError();
    }
}).RequireAuthorization();


app.MapDelete("/draweb-api/users/drawings/{id}", async Task<Results<Ok, NotFound, InternalServerError>> (
    int id, 
    IDrawingRepo drawingRepo, 
    ILogger<RouteHandler> logger) =>
{
    if (id < 1)
    {
        logger.LogWarning("Attempted to delete a drawing with non-valid ID: {id}", id);
        return TypedResults.NotFound();
    }

    var result = await drawingRepo.DeleteDrawing(id);
    if (result.IsSuccess)
    {
        logger.LogInformation(result.Message);
        return TypedResults.Ok();
    } 
    else if (result.ErrorType == ErrorType.ResourceDoesNotExist) 
    {
        logger.LogWarning(result.Message);
        return TypedResults.NotFound();
    }
    else
    {
        logger.LogWarning(result.Message);
        return TypedResults.InternalServerError();
    }
}).RequireAuthorization();


app.MapPatch("/draweb-api/users/drawings/{id}", async(int id, IDrawingRepo drawRepo, ILogger<RouteHandler> logger) =>
{
    //TODO
}).RequireAuthorization();


app.UseCors("DevelopmentPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.Run();

public record User(string? Username, string? Email, string? Password);
public record Drawing(string? Title, string? Svg);