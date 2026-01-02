using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMS.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_ApprovedBy",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "FolderPermissions");

            migrationBuilder.DropTable(
                name: "FolderShares");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ApprovedBy",
                table: "Documents");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Folders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Folders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Folders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedBy",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_DeletedBy",
                table: "Folders",
                column: "DeletedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_AspNetUsers_DeletedBy",
                table: "Folders",
                column: "DeletedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_AspNetUsers_DeletedBy",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Folders_DeletedBy",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Folders");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedBy",
                table: "Documents",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FolderPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FolderId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GrantedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrantedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PermissionType = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "FolderShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    FolderId = table.Column<int>(type: "int", nullable: false),
                    SharedBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PermissionType = table.Column<int>(type: "int", nullable: false),
                    SharedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderShares_AspNetUsers_SharedBy",
                        column: x => x.SharedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FolderShares_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FolderShares_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ApprovedBy",
                table: "Documents",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_CourseId",
                table: "Enrollments",
                columns: new[] { "StudentId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FolderPermissions_FolderId",
                table: "FolderPermissions",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderPermissions_UserId",
                table: "FolderPermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderShares_CourseId",
                table: "FolderShares",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderShares_FolderId_CourseId",
                table: "FolderShares",
                columns: new[] { "FolderId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FolderShares_SharedBy",
                table: "FolderShares",
                column: "SharedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_ApprovedBy",
                table: "Documents",
                column: "ApprovedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
