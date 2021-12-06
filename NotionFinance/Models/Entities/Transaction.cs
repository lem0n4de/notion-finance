using System.ComponentModel.DataAnnotations;

namespace NotionFinance.Models;

public class Transaction
{
    public int TransactionId { get; set; }
    [Required] public string Asset { get; set; }
    public string? Ticker { get; set; }
    [Required] public string Position { get; set; }
    [Required] public float Price { get; set; }

    [Required] public User User { get; set; }
    public int UserId { get; set; }
    [Required] public Account Account { get; set; }
    public int AccountId { get; set; }
}