using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class InitialCreate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "interestratepaid",
                schema: "interest_manager",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Symbol = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interestratepaid", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_interestratepaid_Symbol",
                schema: "interest_manager",
                table: "interestratepaid",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_interestratepaid_WalletId",
                schema: "interest_manager",
                table: "interestratepaid",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_interestratepaid_WalletId_Symbol",
                schema: "interest_manager",
                table: "interestratepaid",
                columns: new[] { "WalletId", "Symbol" });

            migrationBuilder.CreateIndex(
                name: "IX_interestratepaid_WalletId_Symbol_Date",
                schema: "interest_manager",
                table: "interestratepaid",
                columns: new[] { "WalletId", "Symbol", "Date" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "interestratepaid",
                schema: "interest_manager");
        }
    }
}
