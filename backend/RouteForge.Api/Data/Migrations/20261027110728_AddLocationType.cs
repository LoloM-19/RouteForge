
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RouteForge.Api.Data.Migrations
{
    public partial class AddLocationType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "DeliveryLocations",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(
                "UPDATE \"DeliveryLocations\" " +
                "SET \"Type\" = 0 " +
                "WHERE \"Name\" = 'Johannesburg Warehouse';");
                    }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "DeliveryLocations");
        }
    }
}
