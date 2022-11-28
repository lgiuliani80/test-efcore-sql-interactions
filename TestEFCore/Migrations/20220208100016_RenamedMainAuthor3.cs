using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestEFCore.Migrations
{
    public partial class RenamedMainAuthor3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MainAuthor2",
                table: "Blogs",
                newName: "MainAuthor3");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MainAuthor3",
                table: "Blogs",
                newName: "MainAuthor2");
        }
    }
}
