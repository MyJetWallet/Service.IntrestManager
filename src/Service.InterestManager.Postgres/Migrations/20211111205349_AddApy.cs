using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class AddApy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Apy",
                schema: "interest_manager",
                table: "interestratesettings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apy",
                schema: "interest_manager",
                table: "interestratesettings");
        }
    }
}
