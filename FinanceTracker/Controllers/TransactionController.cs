using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Data;
using FinanceTracker.Models;

namespace FinanceTracker.Controllers;

public class TransactionController : Controller
{
    private readonly ApplicationDbContext _context;

    public TransactionController(ApplicationDbContext context) => _context = context;

    // GET: Transaction
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.Transactions.Include(t => t.Category);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: Transaction/Upsert
    public IActionResult Upsert(int id = 0)
    {
        PopulateCategories();

        return id == 0
            ? View(new Transaction())
            : View(_context.Transactions.Find(id));
    }

    // POST: Transaction/Upsert
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert([Bind("TransactionId,Amount,Note,Date,CategoryId")] Transaction transaction)
    {
        if (!ModelState.IsValid)
        {
            PopulateCategories();

            return View(transaction);
        }

        if (transaction.TransactionId == 0)
        {
            _context.Add(transaction);
        }
        else
        {
            _context.Update(transaction);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: Transaction/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Transactions == null)
        {
            return Problem("Entity set 'ApplicationDbContext.Transactions'  is null.");
        }
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [NonAction]
    public void PopulateCategories()
    {
        var categories = _context.Category.ToList();
        Category defaultCategory = new() { CategoryId = 0, Title = "Choose a Category" };
        categories.Insert(0, defaultCategory);
        ViewBag.Categories = categories;
    }
}