using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _67Bet.CustomBet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RevertAiFieldsFromCustomBet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiAnalysisNote",
                table: "CustomBetRequests");

            migrationBuilder.DropColumn(
                name: "AiCategory",
                table: "CustomBetRequests");

            migrationBuilder.DropColumn(
                name: "AiRiskLevel",
                table: "CustomBetRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiAnalysisNote",
                table: "CustomBetRequests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AiCategory",
                table: "CustomBetRequests",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AiRiskLevel",
                table: "CustomBetRequests",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
