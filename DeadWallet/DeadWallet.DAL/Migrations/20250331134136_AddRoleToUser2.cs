using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadWallet.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToUser2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DeadWalletUsers",
                columns: new[] { "Id", "FirstName", "LastName", "Password", "Role", "Username" },
                values: new object[] { 1, "Admin", "Admin", "AQAAAAIAAYagAAAAEGFNYh / EgkDsjALf1Ct6Yv2XG + UrxClo3CNe6IGwRgGZHsgSzxuaPreGUJ7BNZ07yQ ==", "Admin", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DeadWalletUsers",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
