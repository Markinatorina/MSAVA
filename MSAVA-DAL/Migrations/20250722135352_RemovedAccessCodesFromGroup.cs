using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M_SAVA_DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemovedAccessCodesFromGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessGroups_AccessCodes_AccessCodeId",
                table: "AccessGroups");

            migrationBuilder.DropIndex(
                name: "IX_AccessGroups_AccessCodeId",
                table: "AccessGroups");

            migrationBuilder.DropColumn(
                name: "AccessCodeId",
                table: "AccessGroups");

            migrationBuilder.DropColumn(
                name: "SubGroupsIds",
                table: "AccessGroups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccessCodeId",
                table: "AccessGroups",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid[]>(
                name: "SubGroupsIds",
                table: "AccessGroups",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroups_AccessCodeId",
                table: "AccessGroups",
                column: "AccessCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessGroups_AccessCodes_AccessCodeId",
                table: "AccessGroups",
                column: "AccessCodeId",
                principalTable: "AccessCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
