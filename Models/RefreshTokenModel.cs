namespace ToDoListAPI.models;

public class RefreshTokenModel {
    public int Id {get; set;}
    public string Token {get; set;}
    public string UserId {get; set;}

    public DateTime ExpiresAt {get; set;}
    public DateTime CreatedAt {get; set;}
    public DateTime? RevokedAt {get; set;}

    public bool IfExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IfActive => RevokedAt == null && !IfExpired;
}