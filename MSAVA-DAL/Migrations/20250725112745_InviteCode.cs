using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M_SAVA_DAL.Migrations
{
    /// <inheritdoc />
    public partial class InviteCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InviteCodes_Users_OwnerId",
                table: "InviteCodes");

            migrationBuilder.RenameColumn(
                name: "InviteCode",
                table: "Users",
                newName: "InviteCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_InviteCodeId",
                table: "Users",
                column: "InviteCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InviteCodes_Users_OwnerId",
                table: "InviteCodes",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_InviteCodes_InviteCodeId",
                table: "Users",
                column: "InviteCodeId",
                principalTable: "InviteCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InviteCodes_Users_OwnerId",
                table: "InviteCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_InviteCodes_InviteCodeId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_InviteCodeId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "InviteCodeId",
                table: "Users",
                newName: "InviteCode");

            migrationBuilder.AddForeignKey(
                name: "FK_InviteCodes_Users_OwnerId",
                table: "InviteCodes",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
