using System.ComponentModel.DataAnnotations;

namespace NotionFinance.Models;

public class Account
{
    public long AccountId { get; set; }
    [Required]
    public string Name { get; set; }
    public List<Transaction> Transactions { get; set; }
}