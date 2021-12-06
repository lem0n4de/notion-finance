namespace NotionFinance.Models;

public class UserDTO
{
    public long Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public List<Account> Accounts { get; set; }
    public Membership Membership { get; set; }
    public bool IsAdmin { get; set; }

    public static UserDTO FromUser(User user)
    {
        return new UserDTO()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Accounts = user.Accounts,
            Membership = user.Membership,
            IsAdmin = user.IsAdmin
        };
    }
}