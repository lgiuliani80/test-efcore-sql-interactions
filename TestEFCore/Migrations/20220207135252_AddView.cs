using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestEFCore.Migrations
{
    public partial class AddView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE VIEW BlogWithPostCount AS (SELECT *, (SELECT COUNT(*) FROM Posts WHERE Posts.BlogId = Blogs.BlogId) AS PostCount FROM Blogs)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW BlogWithPostCount");
        }
    }
}
