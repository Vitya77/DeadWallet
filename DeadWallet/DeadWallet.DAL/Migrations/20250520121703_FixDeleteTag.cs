using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeadWallet.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Tags_TagId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Tags_TagId",
                table: "Transactions",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Tags_TagId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Tags_TagId",
                table: "Transactions",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id");
        }
    }
}
