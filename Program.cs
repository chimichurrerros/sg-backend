using BackEnd.Infrastructure.Context;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using BackEnd.Constants.Errors;
using BackEnd.Services;
using BackEnd.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

//*******************************************************************************************************
//*                 ╭─────────────────────────────────────────────────────────╮
//*                 │ Usually this is the only thing they will have to modify │
//*                 ╰─────────────────────────────────────────────────────────╯
//*******************************************************************************************************

//-------------------------------------------------------------------------------------------------------
// This is usually outside the scope of the services, use it sparingly.
// I need this to set cookies in AuthService.
builder.Services.AddHttpContextAccessor(); 
//-------------------------------------------------------------------------------------------------------
// The application services should be here 
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<AuthService, AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<ProductBrandsService>();
builder.Services.AddScoped<ProductCategoriesService>();
builder.Services.AddScoped<ProductsService>();
builder.Services.AddScoped<BranchService>();
// ------------------------------------------------------------------------------------------------------
// Authorization configuration
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddAuthorization();
// ------------------------------------------------------------------------------------------------------
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
// ------------------------------------------------------------------------------------------------------

//*******************************************END-END-END*************************************************

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// AutoMapper configuration
builder.Services.AddAutoMapper(typeof(Program).Assembly);

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

// Controller configuration
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Title = ApplicationError.ValidationFailed
            };
            return new BadRequestObjectResult(problemDetails);
        };
    });

// Swagger configuration
builder.Services.AddSwaggerGen();

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