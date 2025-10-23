using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSAVA_DAL.Migrations
{
    /// <inheritdoc />
    public partial class DataUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileData_Users_OwnerId",
                table: "FileData");

            migrationBuilder.DropIndex(
                name: "IX_FileData_OwnerId",
                table: "FileData");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "FileData",
                newName: "OriginalCreator");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "FileData",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileData_CreatorId",
                table: "FileData",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileData_Users_CreatorId",
                table: "FileData",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileData_Users_CreatorId",
                table: "FileData");

            migrationBuilder.DropIndex(
                name: "IX_FileData_CreatorId",
                table: "FileData");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "FileData");

            migrationBuilder.RenameColumn(
                name: "OriginalCreator",
                table: "FileData",
                newName: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_FileData_OwnerId",
                table: "FileData",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileData_Users_OwnerId",
                table: "FileData",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
