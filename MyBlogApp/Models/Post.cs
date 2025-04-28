using System;
using System.Collections.Generic;

namespace MyBlogApp.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // 新增這行：存登入使用者的名字
        public int? UserId { get; set; }
        public string? User { get; set; } 

        // 分類 (Category)
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        // 留言 (Comments)
        public List<Comment>? Comments { get; set; }
    }
}
