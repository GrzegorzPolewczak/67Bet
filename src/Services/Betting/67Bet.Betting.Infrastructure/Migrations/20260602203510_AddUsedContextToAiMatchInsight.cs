using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _67Bet.Betting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsedContextToAiMatchInsight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsedContext",
                table: "AiMatchInsights",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedContext",
                table: "AiMatchInsights");
        }
    }
}
