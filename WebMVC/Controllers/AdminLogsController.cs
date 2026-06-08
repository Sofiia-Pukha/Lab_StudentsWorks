using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMVC.Infrastructure;

namespace WebMVC.Controllers;

[Authorize(Roles = "Admin")]
public class AdminLogsController : Controller
{
    private readonly AppDbContext _context;

    public AdminLogsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var logs = await _context.AdminLogs
            .Include(l => l.Admin)
            .OrderByDescending(l => l.ActionTime)
            .ToListAsync();

        return View(logs);
    }
}