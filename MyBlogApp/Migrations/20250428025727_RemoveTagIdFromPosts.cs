using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBlogApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTagIdFromPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Posts",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "User",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Posts",
                newName: "UserName");
        }
    }
}
