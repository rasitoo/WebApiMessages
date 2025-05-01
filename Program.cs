using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApiMessages.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Por favor, ingrese el token JWT"
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
                }
            },
            new string[] { }
        }
    });
});

//Parte del DBCONTEXT
var dbHost = builder.Configuration["DbSettings:Host"];
var dbPort = builder.Configuration["DbSettings:Port"];
var dbUsername = builder.Configuration["DbSettings:Username"];
var dbPassword = builder.Configuration["DbSettings:Password"];
var dbName = builder.Configuration["DbSettings:Database"];
var connectionString = $"Host={dbHost};Username={dbUsername};Password={dbPassword};Database={dbName};Port={dbPort}";
builder.Services.AddDbContext<MessageContext>(opt =>
    opt.UseNpgsql(connectionString, b => b.MigrationsAssembly("WebApiMessages")));



var secretkey = builder.Configuration["JwtSettings:SecretKey"];
Console.WriteLine(secretkey); //¿?¿?¿?¿?¿?¿?¿?¿?¿?¿?¿?¿?¿?
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "RegisterSystem",

                ValidateAudience = true,
                ValidAudience = "LoginUser",

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UsuarioAutenticado", policy =>
        policy.RequireAuthenticatedUser());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MessageContext>();
    dbContext.Database.Migrate(); // si da error borrar migrations y poner en consola de comandos dotnet ef migrations add InitialCreate y dotnet ef database update
}

app.UseSwagger();
app.UseSwaggerUI();


app.MapControllers();
app.Run();
