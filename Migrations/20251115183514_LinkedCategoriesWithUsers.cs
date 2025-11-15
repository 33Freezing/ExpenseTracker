using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Migrations
{
    /// <inheritdoc />
    public partial class LinkedCategoriesWithUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Categories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "IdentityUserId",
                value: "4e08d54b-16f0-47a0-afaf-afc12dbdedc8");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "IdentityUserId",
                value: "4e08d54b-16f0-47a0-afaf-afc12dbdedc8");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "IdentityUserId",
                value: "4e08d54b-16f0-47a0-afaf-afc12dbdedc8");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "IdentityUserId",
                value: "4e08d54b-16f0-47a0-afaf-afc12dbdedc8");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "IdentityUserId",
                value: "4e08d54b-16f0-47a0-afaf-afc12dbdedc8");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "IdentityUserId",
                value: "4e08d54b-16f0-47a0-afaf-afc12dbdedc8");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "IdentityUserId",
                value: "4e08d54b-16f0-47a0-afaf-afc12dbdedc8");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8,
                column: "IdentityUserId",
                value: "4e08d54b-16f0-47a0-afaf-afc12dbdedc8");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9,
                column: "IdentityUserId",
                value: "4e08d54b-16f0-47a0-afaf-afc12dbdedc8");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10,
                column: "IdentityUserId",
                value: "4e08d54b-16f0-47a0-afaf-afc12dbdedc8");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 11,
                column: "IdentityUserId",
                value: "4e08d54b-16f0-47a0-afaf-afc12dbdedc8");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IdentityUserId",
                table: "Categories",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_AspNetUsers_IdentityUserId",
                table: "Categories",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_AspNetUsers_IdentityUserId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Categories_IdentityUserId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Categories");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
