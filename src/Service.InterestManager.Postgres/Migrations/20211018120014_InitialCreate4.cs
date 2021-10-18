using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class InitialCreate4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SettingsJson",
                schema: "interest_manager",
                table: "calculationhistory",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SettingsJson",
                schema: "interest_manager",
                table: "calculationhistory");
        }
    }
}
