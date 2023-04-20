using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoList.MVC.Migrations
{
    /// <inheritdoc />
    public partial class RenamedApplicationUserToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToDoLists_AspNetUsers_ApplicationUserId",
                table: "ToDoLists");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "ToDoLists",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ToDoLists_ApplicationUserId",
                table: "ToDoLists",
                newName: "IX_ToDoLists_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToDoLists_AspNetUsers_UserId",
                table: "ToDoLists",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToDoLists_AspNetUsers_UserId",
                table: "ToDoLists");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ToDoLists",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ToDoLists_UserId",
                table: "ToDoLists",
                newName: "IX_ToDoLists_ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToDoLists_AspNetUsers_ApplicationUserId",
                table: "ToDoLists",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
