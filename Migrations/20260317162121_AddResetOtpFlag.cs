using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProteinOnWheelsAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddResetOtpFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResetOtpVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResetOtpVerified",
                table: "Users");
        }
    }
}
