using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadWallet.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIdToUserBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserBudgets",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserBudgets");
        }
    }
}
