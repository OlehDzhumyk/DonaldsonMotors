using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonaldsonMotors.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class addPartBarcodeAndCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Parts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "Parts",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "Parts");
        }
    }
}
