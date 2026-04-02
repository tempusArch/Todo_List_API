using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ToDoListAPI.models;

public class TodoModel {
    public int Id {get; set;}
    [JsonIgnore]
    public string UserId {get; set;} = string.Empty;
    [Required]
    public string Title {get; set;} = string.Empty;
    [Required]
    public string Description {get; set;} = string.Empty;
    
}