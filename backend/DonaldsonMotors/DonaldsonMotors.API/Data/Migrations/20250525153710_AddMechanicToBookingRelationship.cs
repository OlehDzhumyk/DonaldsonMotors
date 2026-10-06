using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonaldsonMotors.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMechanicToBookingRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_MechanicId",
                table: "Bookings",
                column: "MechanicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_MechanicId",
                table: "Bookings",
                column: "MechanicId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_MechanicId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_MechanicId",
                table: "Bookings");
        }
    }
}
