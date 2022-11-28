using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestEFCore.Migrations
{
    public partial class AddNumberOfReadsSP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE PROCEDURE IncrementNumberOfReads AS
                BEGIN
                    UPDATE Posts SET NumberOfReads = NumberOfReads + 1
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IncrementNumberOfReads");
        }
    }
}
