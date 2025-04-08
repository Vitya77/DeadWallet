using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadWallet.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UserEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "DeadWalletUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EmailOtps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOtps", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "DeadWalletUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "Email",
                value: "a@a.com");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtps_Email",
                table: "EmailOtps",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailOtps");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "DeadWalletUsers");
        }
    }
}
