using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; 
using WebMVC.Domain.Entities;
using WebMVC.Infrastructure;

namespace WebMVC.Controllers;

public class WorksController : Controller
{
    private readonly AppDbContext _context;

    public WorksController(AppDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous] 
    public async Task<IActionResult> Index(string? searchString, int? categoryId, int? departmentId, int? teacherId)
    {
        var worksQuery = _context.Works
            .Include(w => w.Category)
            .Include(w => w.Student)
            .Include(w => w.Teacher)
                .ThenInclude(t => t.Department) 
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            worksQuery = worksQuery.Where(w => w.Title.Contains(searchString));
        }

        if (categoryId.HasValue)
        {
            worksQuery = worksQuery.Where(w => w.CategoryId == categoryId.Value);
        }

        if (teacherId.HasValue)
        {
            worksQuery = worksQuery.Where(w => w.TeacherId == teacherId.Value);
        }

        if (departmentId.HasValue)
        {
            worksQuery = worksQuery.Where(w => w.Teacher.DepartmentId == departmentId.Value);
        }

        ViewBag.ReviewedWorkIds = await _context.Reviews.Select(r => r.WorkId).Distinct().ToListAsync();

        ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId);
        ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", departmentId);
        ViewBag.Teachers = new SelectList(await _context.Teachers.ToListAsync(), "Id", "FullName", teacherId);
        ViewBag.CurrentSearch = searchString;

        return View(await worksQuery.ToListAsync());
    }
    
    [Authorize(Roles = "StudentAuthor,Admin")]
    public IActionResult Create()
    {
        ViewBag.Teachers = new SelectList(_context.Teachers, "Id", "FullName");
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
        ViewBag.Students = new SelectList(_context.Users, "Id", "Username");
        
        return View();
    }

    [Authorize(Roles = "StudentAuthor,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Work work)
    {
        work.CreatedAt = DateTime.UtcNow;

        ModelState.Remove("Category");
        ModelState.Remove("Student");
        ModelState.Remove("Teacher");

        if (ModelState.IsValid)
        {
            _context.Add(work);
            await _context.SaveChangesAsync();
            return RedirectToAction("Create", "Reviews", new { workId = work.Id });
        }

        ViewBag.Teachers = new SelectList(_context.Teachers, "Id", "FullName", work.TeacherId);
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", work.CategoryId);
        ViewBag.Students = new SelectList(_context.Users, "Id", "Username", work.StudentId);
        return View(work);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var work = await _context.Works.FindAsync(id);
        if (work == null) return NotFound();

        ViewBag.Teachers = new SelectList(_context.Teachers, "Id", "FullName", work.TeacherId);
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", work.CategoryId);
        ViewBag.Students = new SelectList(_context.Users, "Id", "Username", work.StudentId);
        
        return View(work);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Work work)
    {
        if (id != work.Id) return NotFound();

        var workInDb = await _context.Works.FindAsync(id);
        if (workInDb == null) return NotFound();

        ModelState.Remove("Category");
        ModelState.Remove("Student");
        ModelState.Remove("Teacher");
        ModelState.Remove("Reviews");

        if (ModelState.IsValid)
        {
            try
            {
                workInDb.Title = work.Title;
                workInDb.CategoryId = work.CategoryId;
                workInDb.TeacherId = work.TeacherId;
                workInDb.ExecutionYear = work.ExecutionYear;
                workInDb.Grade = work.Grade;
                workInDb.FileUrl = work.FileUrl;

                _context.Update(workInDb);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkExists(work.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        
        ViewBag.Teachers = new SelectList(_context.Teachers, "Id", "FullName", work.TeacherId);
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", work.CategoryId);
        ViewBag.Students = new SelectList(_context.Users, "Id", "Username", work.StudentId);
        return View(work);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var work = await _context.Works.FirstOrDefaultAsync(m => m.Id == id);
        if (work == null) return NotFound();

        _context.Works.Remove(work);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool WorkExists(int id)
    {
        return _context.Works.Any(e => e.Id == id);
    }
}