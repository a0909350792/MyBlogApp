using Microsoft.AspNetCore.Mvc;
using MyBlogApp.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Identity;
using System.Data;
namespace MyBlogApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        // 顯示所有會員
        public async Task<IActionResult> Index()
        {
            // 獲取所有會員
            var users =  _context.Users.ToList();

            return View(users);  // 傳遞所有會員至視圖
        }
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);  // 返回會員詳細頁面
        }

        // 顯示自己的編輯頁面
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            // 讀取 Session 裡自己的 UserId
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login");  // 如果沒登入，回登入頁
            }

            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login"); // UserId 不是數字，回登入頁
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();  // 找不到自己的帳號
            }

            return View(user);
        }

        // 處理自己的編輯表單
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string fullName, string newPassword, string confirmPassword)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login");  // 如果沒登入，回登入頁
            }

            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // 確認新密碼和確認密碼一致
            if (!string.IsNullOrEmpty(newPassword) && newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "新密碼和確認密碼不一致！";
                return View(user);
            }

            // 更新資料
            user.FullName = fullName;

            if (!string.IsNullOrEmpty(newPassword))
            {
                user.PasswordHash = HashPassword(newPassword);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "資料更新成功！";
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Delete(int id)
        {
            // 從 Session 確認使用者是否已登入
            var currentUserId = HttpContext.Session.GetString("UserId");

            if (currentUserId == null)
            {
                return RedirectToAction("Login");  // 未登入則跳轉至登入頁
            }

            // 確保使用者擁有管理權限（Role = 1）
            var role = HttpContext.Session.GetString("Role");
            if (role == null || role != "1")
            {
                return Forbid();  // 沒有權限則拒絕訪問
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();  // 如果找不到該用戶，顯示 404
            }
            // 確認要刪除的是否為當前登入用戶
            if (user.Id.ToString() == currentUserId)
            {
                TempData["ErrorMessage"] = "您不能刪除自己！";
                return RedirectToAction("Index");  // 返回會員管理頁
            }
            // 刪除用戶資料
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "會員已成功刪除！";
            return RedirectToAction("Index");  // 刪除後重定向回會員管理頁面
        }


        //註冊頁面
        public IActionResult Register()
        {
            return View();
        }
        //處理註冊
        [HttpPost]
        public async Task<IActionResult> Register(string userName, string fullName, string password, string role)
        {
            // 檢查用戶名是否已存在
            if (_context.Users.Any(u => u.UserName == userName))
            {
                TempData["ErrorMessage"] = "用戶名已存在！";
                return View();
            }

            // 創建新用戶並保存
            var user = new User
            {
                UserName = userName,
                FullName = fullName,
                PasswordHash = HashPassword(password),
                Role = role,  // 設定角色，默認為 2（一般用戶）

                JoinedDate = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 註冊成功，重定向到登入頁面
            TempData["SuccessMessage"] = "註冊成功，請登入！";
            return RedirectToAction("Login", "Account");
        }

        //登入頁面
        public IActionResult Login()
        {
            // 檢查使用者是否已經登入
            if (HttpContext.Session.GetString("UserId") != null)
            {
                // 如果已經登入，跳轉到文章頁面
                return RedirectToAction("Index", "Posts");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password, string Role)
        {
            // 先取得對應使用者的資料
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            // 如果使用者不存在或密碼不正確
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                // 顯示錯誤訊息
                ViewData["ErrorMessage"] = "帳號或密碼錯誤";
                return View();
            }

            // 登入成功，這裡可以用 Session 或 Cookie 記錄登入狀態
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("Role", user.Role?.ToString() ?? "2");

            // 根據角色導向不同頁面
            if (user.Role == "1")
            {
                // 管理員跳轉到會員管理
                return RedirectToAction("Index", "Account");
            }
            else
            {
                // 一般使用者跳轉到文章列表
                return RedirectToAction("Index", "Posts");
            }
          
        }


        //登出
        public IActionResult Logout() 
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        private bool VerifyPassword(string password, string storedPasswordHash)
        {
            // 將輸入的密碼進行雜湊，並與存儲的雜湊密碼進行比對
            var passwordHash = HashPassword(password);
            return passwordHash == storedPasswordHash;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        public IActionResult AdminDashboard()
        {
            var userRole = HttpContext.Session.GetInt32("Role");

            if (userRole != 1)  // 只有 Admin 才能進入
            {
                return RedirectToAction("Details", "Account");
            }

            return View();  // Admin 專屬頁面
        }
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int id, string newRole)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Role = newRole;  // 根據傳入的數字來更改角色
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "角色更新成功！";
            return RedirectToAction("Index");  // 重新導向至會員管理頁面
        }

    }
}

    
    

