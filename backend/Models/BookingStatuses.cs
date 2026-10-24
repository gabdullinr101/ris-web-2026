namespace backend.Models;

// Четыре кода статуса записи. Пишутся везде одинаково: в JSON, в коде и в базе.
public static class BookingStatuses
{
    public const string New = "New";
    public const string InProgress = "InProgress";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = { New, InProgress, Closed, Cancelled };

    // Допустимые переходы из docs/data-model.md
    public static bool CanMove(string from, string to)
    {
        return (from, to) switch
        {
            (New, InProgress) => true,
            (New, Cancelled) => true,
            (InProgress, Closed) => true,
            (InProgress, Cancelled) => true,
            _ => false,
        };
    }
}
