using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Service.InterestManager.Postrges.Migrations
{
    public partial class InitialCreate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "indexprice",
                schema: "interest_manager",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Asset = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PriceInUsd = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indexprice", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_indexprice_Asset",
                schema: "interest_manager",
                table: "indexprice",
                column: "Asset");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "indexprice",
                schema: "interest_manager");
        }
    }
}
