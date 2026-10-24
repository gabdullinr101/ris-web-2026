using backend.Auth;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Имена полей в ошибках проверки - как в JSON: title, hallId, description
builder.Services.AddControllers(options =>
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider()));
// Ошибки отдаются в одном виде - ProblemDetails, без stack trace
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// База PostgreSQL через EF Core. Строка подключения - по ключу ConnectionStrings:Default.
// Пароль в Git не лежит: он задаётся через dotnet user-secrets (см. README).
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// DI: когда контроллеру нужен IBookingService, он получает BookingService.
// Scoped - свой экземпляр на каждый HTTP-запрос, как и у DbContext.
builder.Services.AddScoped<IBookingService, BookingService>();

// Вход через cookie и hash паролей библиотекой ASP.NET Identity (ЛР11)
builder.Services.AddCookieLogin();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// CORS: ответы API может читать только наш клиент Vue. Не "*", потому что cookie входа.
const string ClientCors = "client";
var clientOrigin = builder.Configuration["ClientOrigin"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
    options.AddPolicy(ClientCors, policy => policy
        .WithOrigins(clientOrigin)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

var app = builder.Build();

// Любое неожиданное исключение - безопасный ответ 500 в виде ProblemDetails
app.UseExceptionHandler();
// Пустые ответы 401 и 403 тоже получают тело ProblemDetails
app.UseStatusCodePages();

// Swagger доступен только в режиме Development: http://localhost:5000/swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Перенаправление на HTTPS из шаблона не включаю: проверка курса идёт по http://localhost:5000.

app.UseCors(ClientCors);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Служебные адреса. Записи на тренировку они не принимают и не создают.

// GET /health - только «процесс жив». Не доказывает, что создание записей (POST /api/bookings) работает.
app.MapGet("/health", () => Results.Text("ok"))
    .WithTags("Служебные");

// GET /info - имя приложения, среда и учебная пометка.
app.MapGet("/info", (IWebHostEnvironment env) => Results.Ok(new
{
    name = "Бронирование тренировок · API",
    environment = env.EnvironmentName,
    version = "ЛР13: предзащита",
}))
    .WithTags("Служебные");

app.Run();
