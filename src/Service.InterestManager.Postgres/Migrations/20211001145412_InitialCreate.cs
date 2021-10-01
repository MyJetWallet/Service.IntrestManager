using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "interest_manager");

            migrationBuilder.CreateTable(
                name: "interestratecalculation",
                schema: "interest_manager",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Symbol = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NewBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    Apy = table.Column<decimal>(type: "numeric", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interestratecalculation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "interestratesettings",
                schema: "interest_manager",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Asset = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RangeFrom = table.Column<decimal>(type: "numeric", nullable: false),
                    RangeTo = table.Column<decimal>(type: "numeric", nullable: false),
                    Apy = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interestratesettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_interestratecalculation_Symbol",
                schema: "interest_manager",
                table: "interestratecalculation",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_interestratecalculation_WalletId",
                schema: "interest_manager",
                table: "interestratecalculation",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_interestratecalculation_WalletId_Symbol",
                schema: "interest_manager",
                table: "interestratecalculation",
                columns: new[] { "WalletId", "Symbol" });

            migrationBuilder.CreateIndex(
                name: "IX_interestratecalculation_WalletId_Symbol_Date",
                schema: "interest_manager",
                table: "interestratecalculation",
                columns: new[] { "WalletId", "Symbol", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_interestratesettings_Asset",
                schema: "interest_manager",
                table: "interestratesettings",
                column: "Asset");

            migrationBuilder.CreateIndex(
                name: "IX_interestratesettings_WalletId",
                schema: "interest_manager",
                table: "interestratesettings",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_interestratesettings_WalletId_Asset",
                schema: "interest_manager",
                table: "interestratesettings",
                columns: new[] { "WalletId", "Asset" });

            migrationBuilder.CreateIndex(
                name: "IX_interestratesettings_WalletId_Asset_RangeFrom_RangeTo",
                schema: "interest_manager",
                table: "interestratesettings",
                columns: new[] { "WalletId", "Asset", "RangeFrom", "RangeTo" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "interestratecalculation",
                schema: "interest_manager");

            migrationBuilder.DropTable(
                name: "interestratesettings",
                schema: "interest_manager");
        }
    }
}
