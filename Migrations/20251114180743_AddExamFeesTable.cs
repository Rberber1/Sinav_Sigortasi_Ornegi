using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webprojem.Migrations
{
    /// <inheritdoc />
    public partial class AddExamFeesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExamCode = table.Column<string>(type: "TEXT", nullable: false),
                    ExamFullName = table.Column<string>(type: "TEXT", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamFees", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamFees");
        }
    }
}
