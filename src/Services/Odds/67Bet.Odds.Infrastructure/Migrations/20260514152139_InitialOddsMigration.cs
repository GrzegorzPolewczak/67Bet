using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _67Bet.Odds.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialOddsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SportKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalMarkets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalMarkets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalMarkets_ExternalEvents_ExternalEventId",
                        column: x => x.ExternalEventId,
                        principalTable: "ExternalEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalOutcomes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalMarketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalOutcomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalOutcomes_ExternalMarkets_ExternalMarketId",
                        column: x => x.ExternalMarketId,
                        principalTable: "ExternalMarkets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalEvents_ExternalId",
                table: "ExternalEvents",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalMarkets_ExternalEventId",
                table: "ExternalMarkets",
                column: "ExternalEventId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalOutcomes_ExternalMarketId",
                table: "ExternalOutcomes",
                column: "ExternalMarketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalOutcomes");

            migrationBuilder.DropTable(
                name: "ExternalMarkets");

            migrationBuilder.DropTable(
                name: "ExternalEvents");
        }
    }
}
