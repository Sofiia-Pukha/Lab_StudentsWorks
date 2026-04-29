using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebMVC.Domain.Entities;
using WebMVC.Infrastructure;

namespace WebMVC.Controllers;

[Authorize(Roles = "Admin")]
public class TeachersController : Controller
{
    private readonly AppDbContext _context;

    public TeachersController(AppDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var teachers = await _context.Teachers
            .Include(t => t.Department)
            .Include(t => t.Reviews)
            .OrderBy(t => t.FullName)
            .ToListAsync();
            
        return View(teachers);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Teacher teacher)
    {
        ModelState.Remove("Department");
        
        if (ModelState.IsValid)
        {
            _context.Add(teacher);
            await _context.SaveChangesAsync();

            int adminId = GetCurrentAdminId();
            if (adminId != 0)
            {
                _context.AdminLogs.Add(new AdminLog {
                    AdminId = adminId,
                    TargetTable = "Teachers",
                    TargetColumn = "Created",
                    OldValue = "-",
                    NewValue = $"Додано викладача: {teacher.FullName}",
                    ActionTime = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", teacher.DepartmentId);
        return View(teacher);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null) return NotFound();

        ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", teacher.DepartmentId);
        return View(teacher);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Teacher teacher)
    {
        if (id != teacher.Id) return NotFound();

        var teacherInDb = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id);
        if (teacherInDb == null) return NotFound();

        ModelState.Remove("Department");

        if (ModelState.IsValid)
        {
            try
            {
                int adminId = GetCurrentAdminId();
                if (adminId != 0)
                {
                    if (teacherInDb.FullName != teacher.FullName)
                    {
                        _context.AdminLogs.Add(new AdminLog {
                            AdminId = adminId,
                            TargetTable = "Teachers",
                            TargetColumn = "FullName",
                            OldValue = teacherInDb.FullName,
                            NewValue = teacher.FullName,
                            ActionTime = DateTime.UtcNow
                        });
                    }
                    if (teacherInDb.DepartmentId != teacher.DepartmentId)
                    {
                        _context.AdminLogs.Add(new AdminLog {
                            AdminId = adminId,
                            TargetTable = "Teachers",
                            TargetColumn = "DepartmentId",
                            OldValue = teacherInDb.DepartmentId.ToString(),
                            NewValue = teacher.DepartmentId.ToString(),
                            ActionTime = DateTime.UtcNow
                        });
                    }
                }

                teacherInDb.FullName = teacher.FullName;
                teacherInDb.DepartmentId = teacher.DepartmentId;

                _context.Update(teacherInDb);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(teacher.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", teacher.DepartmentId);
        return View(teacher);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.Id == id);
        if (teacher == null) return NotFound();

        int adminId = GetCurrentAdminId();
        if (adminId != 0)
        {
            _context.AdminLogs.Add(new AdminLog {
                AdminId = adminId,
                TargetTable = "Teachers",
                TargetColumn = "Deleted",
                OldValue = $"Видалено викладача: {teacher.FullName}",
                NewValue = "-",
                ActionTime = DateTime.UtcNow
            });
        }

        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TeacherExists(int id)
    {
        return _context.Teachers.Any(e => e.Id == id);
    }

    private int GetCurrentAdminId()
    {
        var claimIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(claimIdStr) && int.TryParse(claimIdStr, out int parsedId)) return parsedId;
        var fallbackAdmin = _context.Users.FirstOrDefault(u => u.Role == UserRole.Admin);
        return fallbackAdmin?.Id ?? 0;
    }
}