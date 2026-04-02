using System.ComponentModel.DataAnnotations;

namespace ToDoListAPI.models;

public class LoginModel {
    [Required]
    public string Email {get; set;}
    [Required]
    public string Password {get; set;}
}