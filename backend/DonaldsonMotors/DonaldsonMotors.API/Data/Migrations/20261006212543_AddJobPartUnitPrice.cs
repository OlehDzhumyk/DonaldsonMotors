using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonaldsonMotors.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPartUnitPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "JobParts",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Rows from before this column existed: the best price we have is the current one.
            migrationBuilder.Sql(
                """
                UPDATE "JobParts" AS jp SET "UnitPrice" = p."Price"
                FROM "Parts" AS p WHERE p."Id" = jp."PartId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "JobParts");
        }
    }
}
