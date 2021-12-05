using Microsoft.Build.Framework;

namespace NotionFinance.Models;

public class RegisterUserForm
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [Required]
    public string Username { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}