using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSAVA_DAL.Migrations
{
    /// <inheritdoc />
    public partial class InviteLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InviteLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviteCodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InviteLogs_InviteCodes_InviteCodeId",
                        column: x => x.InviteCodeId,
                        principalTable: "InviteCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InviteLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InviteLogs_InviteCodeId",
                table: "InviteLogs",
                column: "InviteCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_InviteLogs_UserId",
                table: "InviteLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InviteLogs");
        }
    }
}
