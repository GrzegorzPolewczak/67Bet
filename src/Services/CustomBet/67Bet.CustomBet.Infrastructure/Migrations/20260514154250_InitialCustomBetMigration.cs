using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _67Bet.CustomBet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCustomBetMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomBetRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AiSuggestedOdds = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AdminFinalOdds = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomBetRequests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomBetRequests");
        }
    }
}
