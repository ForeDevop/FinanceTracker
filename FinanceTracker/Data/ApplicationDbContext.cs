using FinanceTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Data;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions options) : base(options)
	{

	}

	public DbSet<Category> Category { get; set; }
	public DbSet<Transaction> Transactions { get; set; }
}
