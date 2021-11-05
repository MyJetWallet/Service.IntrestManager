using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class AddLastTsIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_paidhistory_LastTs",
                schema: "interest_manager",
                table: "paidhistory",
                column: "LastTs");

            migrationBuilder.CreateIndex(
                name: "IX_interestratesettings_LastTs",
                schema: "interest_manager",
                table: "interestratesettings",
                column: "LastTs");

            migrationBuilder.CreateIndex(
                name: "IX_interestratepaid_LastTs",
                schema: "interest_manager",
                table: "interestratepaid",
                column: "LastTs");

            migrationBuilder.CreateIndex(
                name: "IX_interestratecalculation_LastTs",
                schema: "interest_manager",
                table: "interestratecalculation",
                column: "LastTs");

            migrationBuilder.CreateIndex(
                name: "IX_calculationhistory_LastTs",
                schema: "interest_manager",
                table: "calculationhistory",
                column: "LastTs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_paidhistory_LastTs",
                schema: "interest_manager",
                table: "paidhistory");

            migrationBuilder.DropIndex(
                name: "IX_interestratesettings_LastTs",
                schema: "interest_manager",
                table: "interestratesettings");

            migrationBuilder.DropIndex(
                name: "IX_interestratepaid_LastTs",
                schema: "interest_manager",
                table: "interestratepaid");

            migrationBuilder.DropIndex(
                name: "IX_interestratecalculation_LastTs",
                schema: "interest_manager",
                table: "interestratecalculation");

            migrationBuilder.DropIndex(
                name: "IX_calculationhistory_LastTs",
                schema: "interest_manager",
                table: "calculationhistory");
        }
    }
}
