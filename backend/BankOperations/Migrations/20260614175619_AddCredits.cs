using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankOperations.Migrations
{
    /// <inheritdoc />
    public partial class AddCredits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CreditServices",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CreditServices_Name",
                table: "CreditServices",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CreditServices_Name",
                table: "CreditServices");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "CreditServices");
        }
    }
}
