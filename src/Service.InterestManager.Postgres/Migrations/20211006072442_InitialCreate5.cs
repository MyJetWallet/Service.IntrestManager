using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class InitialCreate5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calculationhistory",
                schema: "interest_manager",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompletedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    WalletCount = table.Column<int>(type: "integer", nullable: false),
                    AmountInWalletsInUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    CalculatedAmountInUsd = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculationhistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "paidhistory",
                schema: "interest_manager",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompletedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RangeFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RangeTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    WalletCount = table.Column<int>(type: "integer", nullable: false),
                    TotalPaidInUsd = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paidhistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calculationhistory_CompletedDate",
                schema: "interest_manager",
                table: "calculationhistory",
                column: "CompletedDate");

            migrationBuilder.CreateIndex(
                name: "IX_paidhistory_CompletedDate",
                schema: "interest_manager",
                table: "paidhistory",
                column: "CompletedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calculationhistory",
                schema: "interest_manager");

            migrationBuilder.DropTable(
                name: "paidhistory",
                schema: "interest_manager");
        }
    }
}
