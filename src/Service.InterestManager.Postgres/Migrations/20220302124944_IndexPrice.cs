using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class IndexPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "IndexPrice",
                schema: "interest_manager",
                table: "interestratepaid",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IndexPrice",
                schema: "interest_manager",
                table: "interestratecalculation",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndexPrice",
                schema: "interest_manager",
                table: "interestratepaid");

            migrationBuilder.DropColumn(
                name: "IndexPrice",
                schema: "interest_manager",
                table: "interestratecalculation");
        }
    }
}
