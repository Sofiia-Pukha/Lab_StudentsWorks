using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebMVC.Domain.Entities;
using WebMVC.Infrastructure;

namespace WebMVC.Controllers;

public class TeachersController : Controller
{
    private readonly AppDbContext _context;

    public TeachersController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var teachers = await _context.Teachers
            .Include(t => t.Department)
            .Include(t => t.Reviews)
            .ToListAsync();
            
        return View(teachers);
    }

    public async Task<IActionResult> Create()
    {
        var departments = await _context.Departments.ToListAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Teacher teacher)
    {
        _context.Add(teacher);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}