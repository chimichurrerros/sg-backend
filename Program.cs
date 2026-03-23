using BackEnd.Context;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using BackEnd.Models.Responses.Application;
using BackEnd.Constants.Errors;
using BackEnd.Services;

/*************************************************************************************************************/

// Nota para decirle al front: Si el codigo http es 401 o 403 redirigir al login, si es 500 decile 
// front que se arregle el solo xd

/*************************************************************************************************************/

var builder = WebApplication.CreateBuilder(args);

// JWT configuration
var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["current_user"];
                return Task.CompletedTask;
            }
        };
    });

// CORS configuracion
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Custom validation error response
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // 1. We extract the errors cleanly
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(er => er.ErrorMessage).ToArray()
                );

            // 2. We create your consistent DTO
            var response = new ApiResponseDto<object>
            {
                Success = false,
                Message = ApplicationError.ValidationFailed,
                Data = null,
                Errors = errors // Here go messages like "Name is required", etc.
            };

            // 3. We return the BadRequest with your object
            return new BadRequestObjectResult(response);
        };
    });

// This is usually outside the scope of the services, use it sparingly.
builder.Services.AddHttpContextAccessor(); // I need this to set cookies in AuthService.

// The application services should be here <-------------------------------------------------------------
builder.Services.AddScoped<AuthService, AuthService>();
builder.Services.AddScoped<UsuarioService>();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// AutoMapper configuration
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Controller configuration
builder.Services.AddControllers();

// Swagger configuration
builder.Services.AddSwaggerGen();

/*************************************************************************************************************/

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevPolicy");
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/*************************************************************************************************************/