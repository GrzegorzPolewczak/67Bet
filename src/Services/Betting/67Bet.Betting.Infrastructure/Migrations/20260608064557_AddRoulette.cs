using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _67Bet.Betting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoulette : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RouletteRounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SpinResult = table.Column<int>(type: "int", nullable: false),
                    TotalStake = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPayout = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPayoutSettled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouletteRounds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RouletteBets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BetType = table.Column<int>(type: "int", nullable: false),
                    ChosenNumber = table.Column<int>(type: "int", nullable: true),
                    Stake = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsWon = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Payout = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RouletteRoundId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouletteBets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouletteBets_RouletteRounds_RouletteRoundId",
                        column: x => x.RouletteRoundId,
                        principalTable: "RouletteRounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RouletteBets_RouletteRoundId",
                table: "RouletteBets",
                column: "RouletteRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_RouletteRounds_UserId",
                table: "RouletteRounds",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouletteBets");

            migrationBuilder.DropTable(
                name: "RouletteRounds");
        }
    }
}
