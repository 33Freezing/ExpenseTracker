using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTrackerWebApp.Migrations
{
    /// <inheritdoc />
    public partial class RenamedToIsReoccuring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Reoccuring",
                table: "Transactions",
                newName: "IsReoccuring");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsReoccuring",
                table: "Transactions",
                newName: "Reoccuring");
        }
    }
}
