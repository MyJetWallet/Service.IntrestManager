using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class InterestRateIterations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatePaid",
                schema: "interest_manager",
                table: "interestratepaid",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<long>(
                name: "Iteration",
                schema: "interest_manager",
                table: "interestratepaid",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatePaid",
                schema: "interest_manager",
                table: "interestratepaid");

            migrationBuilder.DropColumn(
                name: "Iteration",
                schema: "interest_manager",
                table: "interestratepaid");
        }
    }
}
