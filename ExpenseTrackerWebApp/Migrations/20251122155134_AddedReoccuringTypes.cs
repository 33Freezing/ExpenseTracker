using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTrackerWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddedReoccuringTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReoccuranceFrequency",
                table: "Transactions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Reoccuring",
                table: "Transactions",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReoccuranceFrequency",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Reoccuring",
                table: "Transactions");
        }
    }
}
