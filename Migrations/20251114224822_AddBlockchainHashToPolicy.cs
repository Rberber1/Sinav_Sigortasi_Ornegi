using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webprojem.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockchainHashToPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlockchainHash",
                table: "Policies",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockchainHash",
                table: "Policies");
        }
    }
}
