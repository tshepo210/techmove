using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace glms.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyFieldsToServiceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ConvertedCost",
                table: "ServiceRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConvertedCost",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "ServiceRequests");
        }
    }
}
