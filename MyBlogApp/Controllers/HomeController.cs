using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlogApp.Models;

namespace MyBlogApp.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string search, int? categoryId)
    {
        var posts = _context.Posts.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            posts = posts.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
        }

        if (categoryId.HasValue)
        {
            posts = posts.Where(p => p.CategoryId == categoryId.Value);
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;

        return View(await posts.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }
    // 加在你的 HomeController 裡
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .Include(p => p.Category)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        return View(post);  // 會去找 Views/Home/Details.cshtml
    }

}

