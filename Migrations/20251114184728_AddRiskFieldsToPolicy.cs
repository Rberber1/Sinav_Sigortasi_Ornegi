using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webprojem.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskFieldsToPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Policies",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "SuccessScore",
                table: "Policies",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "SuccessScore",
                table: "Policies");
        }
    }
}
