using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _67Bet.Odds.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecentScoresToExternalEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecentScores",
                table: "ExternalEvents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecentScores",
                table: "ExternalEvents");
        }
    }
}
