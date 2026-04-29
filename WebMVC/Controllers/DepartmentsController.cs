using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebMVC.Domain.Entities;
using WebMVC.Infrastructure;

namespace WebMVC.Controllers;

[Authorize(Roles = "Admin")]
public class DepartmentsController : Controller
{
    private readonly AppDbContext _context;

    public DepartmentsController(AppDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Departments.OrderBy(d => d.Name).ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Department department)
    {
        if (ModelState.IsValid)
        {
            _context.Add(department);
            await _context.SaveChangesAsync();

            int adminId = GetCurrentAdminId();
            if (adminId != 0)
            {
                _context.AdminLogs.Add(new AdminLog {
                    AdminId = adminId,
                    TargetTable = "Departments",
                    TargetColumn = "Created",
                    OldValue = "-",
                    NewValue = $"Створено кафедру: '{department.Name}'",
                    ActionTime = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var department = await _context.Departments.FindAsync(id);
        if (department == null) return NotFound();
        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Department department)
    {
        if (id != department.Id) return NotFound();

        var departmentInDb = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        if (departmentInDb == null) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (departmentInDb.Name != department.Name)
                {
                    int adminId = GetCurrentAdminId();
                    if (adminId != 0)
                    {
                        _context.AdminLogs.Add(new AdminLog {
                            AdminId = adminId,
                            TargetTable = "Departments",
                            TargetColumn = "Name",
                            OldValue = departmentInDb.Name,
                            NewValue = department.Name,
                            ActionTime = DateTime.UtcNow
                        });
                    }
                    
                    departmentInDb.Name = department.Name;
                    _context.Update(departmentInDb);
                }
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(department.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        
        var department = await _context.Departments.FindAsync(id);
        if (department != null)
        {
            int adminId = GetCurrentAdminId();
            if (adminId != 0)
            {
                _context.AdminLogs.Add(new AdminLog {
                    AdminId = adminId,
                    TargetTable = "Departments",
                    TargetColumn = "Deleted",
                    OldValue = $"Видалено кафедру: '{department.Name}'",
                    NewValue = "-",
                    ActionTime = DateTime.UtcNow
                });
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool DepartmentExists(int id)
    {
        return _context.Departments.Any(e => e.Id == id);
    }

    private int GetCurrentAdminId()
    {
        var claimIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(claimIdStr) && int.TryParse(claimIdStr, out int parsedId)) return parsedId;
        var fallbackAdmin = _context.Users.FirstOrDefault(u => u.Role == UserRole.Admin);
        return fallbackAdmin?.Id ?? 0;
    }
}