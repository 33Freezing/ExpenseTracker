using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTrackerWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIconColorToAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Accounts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Accounts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Accounts");
        }
    }
}
