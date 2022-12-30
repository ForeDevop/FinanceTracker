using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Data;
using FinanceTracker.Models;

namespace FinanceTracker.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context) => _context = context;

    // GET: Category
    public async Task<IActionResult> Index() => View(await _context.Category.ToListAsync());

    // GET: Category/Create
    public IActionResult Upsert(int id = 0)
    {
        return id == 0 
            ? View(new Category()) 
            : View(_context.Category.Find(id));
    }

    // POST: Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert([Bind("CategoryId,Title,Icon,Type")] Category category)
    {
        if (ModelState.IsValid)
        {
            if (category.CategoryId == 0)
            {
                _context.Add(category);
            }
            else
            {
                _context.Update(category);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(category);
    }

    // POST: Category/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Category == null)
        {
            return Problem("Entity set 'ApplicationDbContext.Category'  is null.");
        }
        var category = await _context.Category.FindAsync(id);
        if (category != null)
        {
            _context.Category.Remove(category);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}