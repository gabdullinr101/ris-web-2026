using backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI: когда контроллеру нужен IBookingService, он получает BookingService.
// Singleton - один общий экземпляр на всё приложение, поэтому все запросы видят один список в памяти.
builder.Services.AddSingleton<IBookingService, BookingService>();

var app = builder.Build();

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
    version = "ЛР7: каркас Web API",
}))
    .WithTags("Служебные");

app.Run();
