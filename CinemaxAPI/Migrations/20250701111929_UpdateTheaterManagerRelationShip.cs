using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaxAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTheaterManagerRelationShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Theaters_TheaterId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Theaters_AspNetUsers_ManagerId",
                table: "Theaters");

            migrationBuilder.DropIndex(
                name: "IX_Theaters_ManagerId",
                table: "Theaters");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Theaters");

            migrationBuilder.AddColumn<int>(
                name: "ManagedTheaterId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ManagedTheaterId",
                table: "AspNetUsers",
                column: "ManagedTheaterId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Theaters_ManagedTheaterId",
                table: "AspNetUsers",
                column: "ManagedTheaterId",
                principalTable: "Theaters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Theaters_TheaterId",
                table: "AspNetUsers",
                column: "TheaterId",
                principalTable: "Theaters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Theaters_ManagedTheaterId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Theaters_TheaterId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ManagedTheaterId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ManagedTheaterId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                table: "Theaters",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Theaters_ManagerId",
                table: "Theaters",
                column: "ManagerId",
                unique: true,
                filter: "[ManagerId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Theaters_TheaterId",
                table: "AspNetUsers",
                column: "TheaterId",
                principalTable: "Theaters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Theaters_AspNetUsers_ManagerId",
                table: "Theaters",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
