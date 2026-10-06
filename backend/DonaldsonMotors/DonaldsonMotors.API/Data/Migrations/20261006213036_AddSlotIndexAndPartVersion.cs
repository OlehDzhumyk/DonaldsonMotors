using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonaldsonMotors.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSlotIndexAndPartVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Parts",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SlotStart",
                table: "Bookings",
                column: "SlotStart",
                unique: true,
                filter: "\"Status\" <> 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_SlotStart",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Parts");
        }
    }
}
