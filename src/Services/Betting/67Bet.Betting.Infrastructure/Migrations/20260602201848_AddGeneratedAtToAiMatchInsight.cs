using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _67Bet.Betting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedAtToAiMatchInsight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFreebet",
                table: "Tickets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeneratedAt",
                table: "AiMatchInsights",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFreebet",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "GeneratedAt",
                table: "AiMatchInsights");
        }
    }
}
