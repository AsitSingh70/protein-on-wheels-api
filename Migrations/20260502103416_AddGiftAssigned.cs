using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProteinOnWheelsAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftAssigned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GiftAssigned",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiftAssigned",
                table: "Orders");
        }
    }
}
