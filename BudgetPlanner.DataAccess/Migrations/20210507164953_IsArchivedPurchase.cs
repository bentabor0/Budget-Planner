using Microsoft.EntityFrameworkCore.Migrations;

namespace BudgetPlanner.DataAccess.Migrations
{
    public partial class IsArchivedPurchase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Purchases",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Purchases");
        }
    }
}
