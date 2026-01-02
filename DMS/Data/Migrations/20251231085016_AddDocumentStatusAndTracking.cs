using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMS.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentStatusAndTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Folders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Folders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParentFolderId",
                table: "Folders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Documents",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DownloadCount",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FolderPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FolderId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionType = table.Column<int>(type: "int", nullable: false),
                    GrantedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderPermissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FolderPermissions_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Folders_CreatedBy",
                table: "Folders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentFolderId",
                table: "Folders",
                column: "ParentFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ApprovedBy",
                table: "Documents",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FolderPermissions_FolderId",
                table: "FolderPermissions",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderPermissions_UserId",
                table: "FolderPermissions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_ApprovedBy",
                table: "Documents",
                column: "ApprovedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_AspNetUsers_CreatedBy",
                table: "Folders",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Folders_ParentFolderId",
                table: "Folders",
                column: "ParentFolderId",
                principalTable: "Folders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_ApprovedBy",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Folders_AspNetUsers_CreatedBy",
                table: "Folders");

            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Folders_ParentFolderId",
                table: "Folders");

            migrationBuilder.DropTable(
                name: "FolderPermissions");

            migrationBuilder.DropIndex(
                name: "IX_Folders_CreatedBy",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Folders_ParentFolderId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ApprovedBy",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "ParentFolderId",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DownloadCount",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Documents");
        }
    }
}
