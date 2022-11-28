using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestEFCore.Migrations
{
    public partial class AddSPPostsWithReadsGreatedThen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE PostsWithReadsGreatedThen 
                    @num INT
                    AS
                    BEGIN
                        SELECT * FROM Posts WHERE NumberOfReads > @num ORDER BY NumberOfReads DESC
                    END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE PostsWithReadsGreatedThen");
        }
    }
}
