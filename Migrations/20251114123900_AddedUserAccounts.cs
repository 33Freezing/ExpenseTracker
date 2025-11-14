using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Accounts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_IdentityUserId",
                table: "Accounts",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_AspNetUsers_IdentityUserId",
                table: "Accounts",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_AspNetUsers_IdentityUserId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_IdentityUserId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Accounts");
        }
    }
}
