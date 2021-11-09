using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class RenameApyToApr2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Apy",
                schema: "interest_manager",
                table: "interestratecalculation",
                newName: "Apr");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Apr",
                schema: "interest_manager",
                table: "interestratecalculation",
                newName: "Apy");
        }
    }
}
