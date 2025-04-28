using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MyBlogApp.Models;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyBlogApp.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var postsQuery = _context.Posts.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
            }

            if (categoryId.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.CategoryId == categoryId);
            }

            var posts = await postsQuery
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new Post
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    Category = p.Category,
                    CreatedAt = p.CreatedAt,
                    Comments = p.Comments
                })
                .ToListAsync();

            return View(posts);  // 傳遞資料給視圖
        }



        // 顯示單一文章
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts.Include(p => p.Category).Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // 顯示創建文章的表單
        [HttpGet]

        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }
        // 處理新增文章
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,CategoryId")] Post post)
        {
            if (ModelState.IsValid)
            {
                // 設置創建時間
                post.CreatedAt = DateTime.Now;

                // 把登入者名字存到 User 欄位
                post.User = HttpContext.Session.GetString("UserName");

                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // 如果驗證失敗，列出所有錯誤
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"欄位: {key}, 錯誤: {error.ErrorMessage}");
                }
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);
            return View(post);
        }







        //編輯文章
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) 
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["Categories"] = _context.Categories.ToList();
            return View(post);
        }
        //接收編輯文章的表單提交
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,CategoryId")] Post post)
        {
            if (id != post.Id) 
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingPost = await _context.Posts.FindAsync(
                        post.Id);
                    if (existingPost == null)
                    {
                        return NotFound();
                    }

                    existingPost.Title = post.Title;
                    existingPost.Content = post.Content;
                    existingPost.CategoryId = post.CategoryId;
                    existingPost.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = _context.Categories.ToList();
            return View(post);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content) 
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["CommentError"] = "留言不能為空";
                return RedirectToAction("Details", new { id = postId });
            }

            var comment = new Comment
            {
                PostId = postId,
                Content = content,
                CreatedAt = DateTime.Now
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            TempData["CommentSuccess"] = "留言成功！";  // 新增成功訊息

            return RedirectToAction("Details", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            int postId = comment.PostId; // 留言刪除後要回到原本的文章
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["CommentSuccess"] = "留言已刪除！";
            return RedirectToAction("Details", new { id = postId });
        }

        // 顯示刪除文章的確認頁面
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // 處理刪除文章的請求
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                Console.WriteLine($"Post with ID {id} not found.");
                return NotFound();
            }

            // 顯示將要刪除的文章
            Console.WriteLine($"Deleting Post: {post.Title}");

            var comments = _context.Comments.Where(c => c.PostId == id);
            _context.Comments.RemoveRange(comments);

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}

