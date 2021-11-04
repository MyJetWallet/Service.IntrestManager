using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class AddLastTs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastTs",
                schema: "interest_manager",
                table: "paidhistory",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTs",
                schema: "interest_manager",
                table: "interestratesettings",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTs",
                schema: "interest_manager",
                table: "interestratepaid",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTs",
                schema: "interest_manager",
                table: "interestratecalculation",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTs",
                schema: "interest_manager",
                table: "calculationhistory",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastTs",
                schema: "interest_manager",
                table: "paidhistory");

            migrationBuilder.DropColumn(
                name: "LastTs",
                schema: "interest_manager",
                table: "interestratesettings");

            migrationBuilder.DropColumn(
                name: "LastTs",
                schema: "interest_manager",
                table: "interestratepaid");

            migrationBuilder.DropColumn(
                name: "LastTs",
                schema: "interest_manager",
                table: "interestratecalculation");

            migrationBuilder.DropColumn(
                name: "LastTs",
                schema: "interest_manager",
                table: "calculationhistory");
        }
    }
}
