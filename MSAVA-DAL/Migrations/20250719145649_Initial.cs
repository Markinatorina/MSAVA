using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSAVA_DAL.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxUses = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AccessCodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessGroupDBId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessGroups_AccessCodes_AccessCodeId",
                        column: x => x.AccessCodeId,
                        principalTable: "AccessCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessGroups_AccessGroups_AccessGroupDBId",
                        column: x => x.AccessGroupDBId,
                        principalTable: "AccessGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FileRefs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileHash = table.Column<byte[]>(type: "bytea", maxLength: 32, nullable: false),
                    FileExtension = table.Column<short>(type: "smallint", nullable: false),
                    PublicDownload = table.Column<bool>(type: "boolean", nullable: false),
                    PublicViewing = table.Column<bool>(type: "boolean", nullable: false),
                    AccessGroupId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileRefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileRefs_AccessGroups_AccessGroupId",
                        column: x => x.AccessGroupId,
                        principalTable: "AccessGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "bytea", nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false),
                    IsWhitelisted = table.Column<bool>(type: "boolean", nullable: false),
                    InviteCode = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessGroupDBId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_AccessGroups_AccessGroupDBId",
                        column: x => x.AccessGroupDBId,
                        principalTable: "AccessGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    StackTrace = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErrorLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FileData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SizeInBytes = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Checksum = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    FileExtension = table.Column<string>(type: "text", nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false),
                    Categories = table.Column<string[]>(type: "text[]", nullable: false),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    PublicViewing = table.Column<bool>(type: "boolean", nullable: false),
                    DownloadCount = table.Column<long>(type: "bigint", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileData_FileRefs_FileReferenceId",
                        column: x => x.FileReferenceId,
                        principalTable: "FileRefs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileData_Users_LastModifiedById",
                        column: x => x.LastModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileData_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InviteCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxUses = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InviteCodes_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jwts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false),
                    IsWhitelisted = table.Column<bool>(type: "boolean", nullable: false),
                    InviteCode = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenString = table.Column<string>(type: "text", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jwts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jwts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessCodes_OwnerId",
                table: "AccessCodes",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroups_AccessCodeId",
                table: "AccessGroups",
                column: "AccessCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroups_AccessGroupDBId",
                table: "AccessGroups",
                column: "AccessGroupDBId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessGroups_OwnerId",
                table: "AccessGroups",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_UserId",
                table: "ErrorLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileData_FileReferenceId",
                table: "FileData",
                column: "FileReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FileData_LastModifiedById",
                table: "FileData",
                column: "LastModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_FileData_OwnerId",
                table: "FileData",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_FileRefs_AccessGroupId",
                table: "FileRefs",
                column: "AccessGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_InviteCodes_OwnerId",
                table: "InviteCodes",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Jwts_UserId",
                table: "Jwts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AccessGroupDBId",
                table: "Users",
                column: "AccessGroupDBId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessCodes_Users_OwnerId",
                table: "AccessCodes",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessGroups_Users_OwnerId",
                table: "AccessGroups",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessCodes_Users_OwnerId",
                table: "AccessCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessGroups_Users_OwnerId",
                table: "AccessGroups");

            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.DropTable(
                name: "FileData");

            migrationBuilder.DropTable(
                name: "InviteCodes");

            migrationBuilder.DropTable(
                name: "Jwts");

            migrationBuilder.DropTable(
                name: "FileRefs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AccessGroups");

            migrationBuilder.DropTable(
                name: "AccessCodes");
        }
    }
}
