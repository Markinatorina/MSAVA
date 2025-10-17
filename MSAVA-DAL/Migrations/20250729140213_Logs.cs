using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSAVA_DAL.Migrations
{
    /// <inheritdoc />
    public partial class Logs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "StackTrace",
                table: "ErrorLogs");

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "ErrorLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessLogs_FileRefs_FileRefId",
                        column: x => x.FileRefId,
                        principalTable: "FileRefs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLogs_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_FileRefId",
                table: "AccessLogs",
                column: "FileRefId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLogs_UserId",
                table: "AccessLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogs_AdminId",
                table: "UserLogs",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogs_UserId",
                table: "UserLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessLogs");

            migrationBuilder.DropTable(
                name: "UserLogs");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "ErrorLogs");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ErrorLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StackTrace",
                table: "ErrorLogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
