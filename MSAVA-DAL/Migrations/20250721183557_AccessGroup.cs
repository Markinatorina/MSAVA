using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M_SAVA_DAL.Migrations
{
    /// <inheritdoc />
    public partial class AccessGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessGroups_Users_OwnerId",
                table: "AccessGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_FileRefs_AccessGroups_AccessGroupId",
                table: "FileRefs");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_AccessGroups_AccessGroupDBId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AccessGroupDBId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AccessGroupDBId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PublicViewing",
                table: "FileRefs");

            migrationBuilder.AlterColumn<Guid>(
                name: "AccessGroupId",
                table: "FileRefs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid[]>(
                name: "SubGroupsIds",
                table: "AccessGroups",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);

            migrationBuilder.CreateTable(
                name: "UserAccessGroups",
                columns: table => new
                {
                    AccessGroupsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccessGroups", x => new { x.AccessGroupsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserAccessGroups_AccessGroups_AccessGroupsId",
                        column: x => x.AccessGroupsId,
                        principalTable: "AccessGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAccessGroups_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessGroups_UsersId",
                table: "UserAccessGroups",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessGroups_Users_OwnerId",
                table: "AccessGroups",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FileRefs_AccessGroups_AccessGroupId",
                table: "FileRefs",
                column: "AccessGroupId",
                principalTable: "AccessGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessGroups_Users_OwnerId",
                table: "AccessGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_FileRefs_AccessGroups_AccessGroupId",
                table: "FileRefs");

            migrationBuilder.DropTable(
                name: "UserAccessGroups");

            migrationBuilder.DropColumn(
                name: "SubGroupsIds",
                table: "AccessGroups");

            migrationBuilder.AddColumn<Guid>(
                name: "AccessGroupDBId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AccessGroupId",
                table: "FileRefs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "PublicViewing",
                table: "FileRefs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AccessGroupDBId",
                table: "Users",
                column: "AccessGroupDBId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessGroups_Users_OwnerId",
                table: "AccessGroups",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FileRefs_AccessGroups_AccessGroupId",
                table: "FileRefs",
                column: "AccessGroupId",
                principalTable: "AccessGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_AccessGroups_AccessGroupDBId",
                table: "Users",
                column: "AccessGroupDBId",
                principalTable: "AccessGroups",
                principalColumn: "Id");
        }
    }
}
