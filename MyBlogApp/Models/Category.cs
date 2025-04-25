using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlogApp.Models
{
    public class Category
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "分類名稱不能為空")]

        public string? Name { get; set; }

    }
}
