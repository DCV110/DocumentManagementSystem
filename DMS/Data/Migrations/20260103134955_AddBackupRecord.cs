using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMS.Migrations
{
    /// <inheritdoc />
    public partial class AddBackupRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackupRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BackupName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DatabaseBackupPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilesBackupPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DatabaseSize = table.Column<long>(type: "bigint", nullable: false),
                    FilesSize = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRestored = table.Column<bool>(type: "bit", nullable: false),
                    RestoredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RestoredBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RestoreNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackupRecords_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackupRecords_CreatedBy",
                table: "BackupRecords",
                column: "CreatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackupRecords");
        }
    }
}
