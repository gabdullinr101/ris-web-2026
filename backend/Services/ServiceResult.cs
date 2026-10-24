namespace backend.Services;

// Итог операции сервиса. Контроллер переводит его в HTTP-код:
// Ok → 200/201, Invalid → 400, NotFound → 404, Conflict → 409.
public enum ResultKind
{
    Ok,
    Invalid,
    NotFound,
    Conflict,
}

public class ServiceResult<T>
{
    public ResultKind Kind { get; private init; }
    public T? Value { get; private init; }

    // Для Invalid: ошибки по полям (как в docs/api-contract.md: { "errors": { "title": "..." } })
    public Dictionary<string, string[]> Errors { get; private init; } = new();

    // Для NotFound и Conflict: понятный человеку текст
    public string Message { get; private init; } = "";

    public static ServiceResult<T> Ok(T value) => new() { Kind = ResultKind.Ok, Value = value };

    public static ServiceResult<T> Invalid(string field, string message) =>
        new() { Kind = ResultKind.Invalid, Errors = new() { [field] = new[] { message } } };

    public static ServiceResult<T> NotFound(string message) =>
        new() { Kind = ResultKind.NotFound, Message = message };

    public static ServiceResult<T> Conflict(string message) =>
        new() { Kind = ResultKind.Conflict, Message = message };
}
