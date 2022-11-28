using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestEFCore.Migrations
{
    public partial class RenamedTableComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("Comments", newName: "CommentsToPosts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("CommentsToPosts", newName: "Comments");
        }
    }
}
