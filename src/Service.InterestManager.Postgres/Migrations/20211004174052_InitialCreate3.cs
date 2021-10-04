using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class InitialCreate3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                schema: "interest_manager",
                table: "interestratepaid",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                schema: "interest_manager",
                table: "interestratepaid",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                schema: "interest_manager",
                table: "interestratepaid");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "interest_manager",
                table: "interestratepaid");
        }
    }
}
