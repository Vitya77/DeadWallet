using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadWallet.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Budget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgets_DeadWalletUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "DeadWalletUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserBudgets",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BudgetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBudgets", x => new { x.UserId, x.BudgetId });
                    table.ForeignKey(
                        name: "FK_UserBudgets_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBudgets_DeadWalletUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "DeadWalletUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_OwnerId",
                table: "Budgets",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBudgets_BudgetId",
                table: "UserBudgets",
                column: "BudgetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBudgets");

            migrationBuilder.DropTable(
                name: "Budgets");
        }
    }
}
