using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class IndexPriceIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_interestratecalculation_IndexPrice",
                schema: "interest_manager",
                table: "interestratecalculation",
                column: "IndexPrice");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_interestratecalculation_IndexPrice",
                schema: "interest_manager",
                table: "interestratecalculation");
        }
    }
}
