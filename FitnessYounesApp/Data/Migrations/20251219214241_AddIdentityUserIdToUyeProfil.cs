using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessYounesApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityUserIdToUyeProfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "UyeProfiller",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UyeProfiller_IdentityUserId",
                table: "UyeProfiller",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UyeProfiller_AspNetUsers_IdentityUserId",
                table: "UyeProfiller",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UyeProfiller_AspNetUsers_IdentityUserId",
                table: "UyeProfiller");

            migrationBuilder.DropIndex(
                name: "IX_UyeProfiller_IdentityUserId",
                table: "UyeProfiller");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "UyeProfiller");
        }
    }
}
