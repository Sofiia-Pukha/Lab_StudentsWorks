using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebMVC.Domain.Entities;
using WebMVC.Infrastructure;

namespace WebMVC.Controllers;

[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
         return View(await _context.Categories.OrderBy(c => c.Name).ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();

            int currentAdminId = GetCurrentAdminId();
            if (currentAdminId != 0)
            {
                var log = new AdminLog
                {
                    AdminId = currentAdminId,
                    TargetTable = "Categories",
                    TargetColumn = "Created",
                    OldValue = "-",
                    NewValue = $"Створено категорію: '{category.Name}'",
                    ActionTime = DateTime.UtcNow
                };
                _context.AdminLogs.Add(log);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (id != category.Id) return NotFound();

        var categoryInDb = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (categoryInDb == null) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (categoryInDb.Name != category.Name)
                {
                    int currentAdminId = GetCurrentAdminId();
                    if (currentAdminId != 0)
                    {
                        var log = new AdminLog
                        {
                            AdminId = currentAdminId,
                            TargetTable = "Categories",
                            TargetColumn = "Name",
                            OldValue = categoryInDb.Name, 
                            NewValue = category.Name,     
                            ActionTime = DateTime.UtcNow
                        };
                        _context.AdminLogs.Add(log);
                    }

                    categoryInDb.Name = category.Name;
                    _context.Update(categoryInDb);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
        if (category == null) return NotFound();

        int currentAdminId = GetCurrentAdminId();
        if (currentAdminId != 0)
        {
            var log = new AdminLog
            {
                AdminId = currentAdminId,
                TargetTable = "Categories",
                TargetColumn = "Deleted",
                OldValue = $"Видалено категорію: '{category.Name}'",
                NewValue = "-",
                ActionTime = DateTime.UtcNow
            };
            _context.AdminLogs.Add(log);
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }

    private int GetCurrentAdminId()
    {
        var claimIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(claimIdStr) && int.TryParse(claimIdStr, out int parsedId))
        {
            return parsedId;
        }
        
        var fallbackAdmin = _context.Users.FirstOrDefault(u => u.Role == UserRole.Admin);
        return fallbackAdmin?.Id ?? 0;
    }
}