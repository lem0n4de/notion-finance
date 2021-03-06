using Microsoft.EntityFrameworkCore;
using NotionFinance.Models;

namespace NotionFinance.Data;

public class UserDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
}