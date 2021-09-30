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
                    Apy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interestratesettings", x => x.Id);
                });

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
                name: "interestratesettings",
                schema: "interest_manager");
        }
    }
}
