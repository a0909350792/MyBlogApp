using System;

namespace MyBlogApp.Models
{
    public class User
    {
        public int Id { get; set; }  // 自己管理的整數主鍵
        public string UserName { get; set; }
        public string PasswordHash { get; set; }  // 直接存密碼（建議還是哈希過的）
        public string FullName { get; set; }

        public string? Role { get; set; }
        public DateTime JoinedDate { get; set; }
    }
}
