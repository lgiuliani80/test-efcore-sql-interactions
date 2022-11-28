using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestEFCore.Migrations
{
    public partial class AddAuthor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Posts",
                type: "NVARCHAR(80)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"CREATE FUNCTION CountAdminPosts (@id INT) 
                RETURNS INT 
                AS 
                BEGIN 
                    RETURN (SELECT COUNT(*) FROM Posts WHERE BlogId = @id AND Author = 'Admin') 
                END");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Posts");
        }
    }
}
