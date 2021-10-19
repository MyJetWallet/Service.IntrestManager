using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class InitialCreate5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "HistoryId",
                schema: "interest_manager",
                table: "interestratepaid",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "HistoryId",
                schema: "interest_manager",
                table: "interestratecalculation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HistoryId",
                schema: "interest_manager",
                table: "interestratepaid");

            migrationBuilder.DropColumn(
                name: "HistoryId",
                schema: "interest_manager",
                table: "interestratecalculation");
        }
    }
}
