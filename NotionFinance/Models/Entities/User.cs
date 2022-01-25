using System.ComponentModel.DataAnnotations;

namespace NotionFinance.Models;

public class User
{
    public long Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    
    public List<Account> Accounts { get; set; }
    public Membership Membership { get; set; }
    public bool IsAdmin { get; set; }
    public NotionUserSettings NotionUserSettings { get; set; }
}