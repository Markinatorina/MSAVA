using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M_SAVA_DAL.Migrations
{
    /// <inheritdoc />
    public partial class AccessGroupUserMtM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccessGroups_AccessGroups_AccessGroupsId",
                table: "UserAccessGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAccessGroups_Users_UsersId",
                table: "UserAccessGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAccessGroups",
                table: "UserAccessGroups");

            migrationBuilder.RenameTable(
                name: "UserAccessGroups",
                newName: "AccessGroupDBUserDB");

            migrationBuilder.RenameIndex(
                name: "IX_UserAccessGroups_UsersId",
                table: "AccessGroupDBUserDB",
                newName: "IX_AccessGroupDBUserDB_UsersId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccessGroupDBUserDB",
                table: "AccessGroupDBUserDB",
                columns: new[] { "AccessGroupsId", "UsersId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AccessGroupDBUserDB_AccessGroups_AccessGroupsId",
                table: "AccessGroupDBUserDB",
                column: "AccessGroupsId",
                principalTable: "AccessGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessGroupDBUserDB_Users_UsersId",
                table: "AccessGroupDBUserDB",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessGroupDBUserDB_AccessGroups_AccessGroupsId",
                table: "AccessGroupDBUserDB");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessGroupDBUserDB_Users_UsersId",
                table: "AccessGroupDBUserDB");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccessGroupDBUserDB",
                table: "AccessGroupDBUserDB");

            migrationBuilder.RenameTable(
                name: "AccessGroupDBUserDB",
                newName: "UserAccessGroups");

            migrationBuilder.RenameIndex(
                name: "IX_AccessGroupDBUserDB_UsersId",
                table: "UserAccessGroups",
                newName: "IX_UserAccessGroups_UsersId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAccessGroups",
                table: "UserAccessGroups",
                columns: new[] { "AccessGroupsId", "UsersId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccessGroups_AccessGroups_AccessGroupsId",
                table: "UserAccessGroups",
                column: "AccessGroupsId",
                principalTable: "AccessGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccessGroups_Users_UsersId",
                table: "UserAccessGroups",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
