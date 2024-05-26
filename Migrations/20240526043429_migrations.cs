using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lab01.Migrations
{
    /// <inheritdoc />
    public partial class migrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    joinin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCard = table.Column<int>(type: "int", nullable: true),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    UserStatus = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: true),
                    LockOut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    verifyAccount = table.Column<bool>(type: "bit", nullable: false),
                    ResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.ID);
                    table.ForeignKey(
                        name: "FK_users_roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "roles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { 1, "User" },
                    { 2, "HRM" },
                    { 3, "Admin" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "ID", "AccessFailedCount", "Address", "Birthday", "Email", "FirstName", "Gender", "IdCard", "Image", "LastName", "LockOut", "Password", "Phone", "ResetToken", "RoleID", "UserName", "UserStatus", "joinin", "verifyAccount" },
                values: new object[,]
                {
                    { 1, 0, "Lai Vung", null, "phamquangthanhmax14@gmail.com", "Pham", "Male", null, null, "Quang Thanh", null, "827ccb0eea8a706c4c34a16891f84e7b", "0939371017", null, 1, "thanhmax14", false, new DateTime(2024, 5, 26, 11, 34, 27, 777, DateTimeKind.Local).AddTicks(1023), true },
                    { 2, 0, "Lai Vung", null, "phamquangthanhmax11@gmail.com", "Le", null, null, null, "Thi Kiwi", null, "827ccb0eea8a706c4c34a16891f84e7b", "1254659899", null, 2, "HRM", false, new DateTime(2024, 5, 26, 11, 34, 27, 777, DateTimeKind.Local).AddTicks(1037), true },
                    { 3, 0, "Lai Vung", null, "phamquangthanhmax124@gmail.com", "Pham", null, null, null, "Quang Thanh 1", null, "827ccb0eea8a706c4c34a16891f84e7b", "454976486525", null, 3, "admin", false, new DateTime(2024, 5, 26, 11, 34, 27, 777, DateTimeKind.Local).AddTicks(1040), true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_RoleID",
                table: "users",
                column: "RoleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
