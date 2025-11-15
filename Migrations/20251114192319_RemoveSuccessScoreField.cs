using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webprojem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSuccessScoreField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuccessScore",
                table: "Policies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "SuccessScore",
                table: "Policies",
                type: "REAL",
                nullable: true);
        }
    }
}
