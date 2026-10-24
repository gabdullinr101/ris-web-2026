namespace backend.Models;

// Зал клуба - справочник. В записи хранится только HallId.
public class Hall
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
}
