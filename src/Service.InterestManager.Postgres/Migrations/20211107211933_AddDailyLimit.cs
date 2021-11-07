using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class AddDailyLimit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailyLimitInUsd",
                schema: "interest_manager",
                table: "interestratesettings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyLimitInUsd",
                schema: "interest_manager",
                table: "interestratesettings");
        }
    }
}
