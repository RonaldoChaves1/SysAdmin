using Serilog;
using Microsoft.AspNetCore.Authentication.Negotiate;

var builder = WebApplication.CreateBuilder(args);

var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled");
var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();

// Adiciona autenticação Windows (AD)
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

// Configurar logs no ELK com fallback para arquivo
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Configurar CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAnyOrigin", builder =>
//    {
//        builder.AllowAnyOrigin()  // Permite qualquer origem
//               .AllowAnyMethod()  // Permite qualquer método HTTP (GET, POST, etc.)
//               .AllowAnyHeader(); // Permite qualquer cabeçalho
//    });
//});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins(allowedOrigins) // Permite apenas esta origem
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


var app = builder.Build();

app.UseStaticFiles();

app.UseCors("AllowSpecificOrigin");


// Configure the HTTP request pipeline.
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
