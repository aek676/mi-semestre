using MongoDB.Bson;

namespace MiCuatri.API.Models; // <--- Namespace corregido

public class User
{
    public ObjectId Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string BlackboardCookies { get; set; } = string.Empty;
    public DateTime? LastSync { get; set; }
}