using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Имена полей в ошибках проверки - как в JSON: title, hallId, description
builder.Services.AddControllers(options =>
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider()));
// Ошибки отдаются в одном виде - ProblemDetails, без stack trace
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI: когда контроллеру нужен IBookingService, он получает BookingService.
// Singleton - один общий экземпляр на всё приложение, поэтому все запросы видят один список в памяти.
builder.Services.AddSingleton<IBookingService, BookingService>();

var app = builder.Build();

// Любое неожиданное исключение - безопасный ответ 500 в виде ProblemDetails
app.UseExceptionHandler();

// Swagger доступен только в режиме Development: http://localhost:5000/swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Перенаправление на HTTPS из шаблона не включаю: проверка курса идёт по http://localhost:5000.

app.UseAuthorization();

app.MapControllers();

// Служебные адреса. Записи на тренировку они не принимают и не создают.

// GET /health - только «процесс жив». Не доказывает, что создание записей работает (оно будет в ЛР8).
app.MapGet("/health", () => Results.Text("ok"))
    .WithTags("Служебные");

// GET /info - имя приложения, среда и учебная пометка.
app.MapGet("/info", (IWebHostEnvironment env) => Results.Ok(new
{
    name = "Бронирование тренировок · API",
    environment = env.EnvironmentName,
    version = "ЛР8: REST записей, данные в памяти",
}))
    .WithTags("Служебные");

app.Run();
