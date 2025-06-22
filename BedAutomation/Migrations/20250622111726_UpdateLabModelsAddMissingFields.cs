using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BedAutomation.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLabModelsAddMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstimatedTAT",
                table: "LabTests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SampleType",
                table: "LabTests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataType",
                table: "LabParameters",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedTAT",
                table: "LabTests");

            migrationBuilder.DropColumn(
                name: "SampleType",
                table: "LabTests");

            migrationBuilder.DropColumn(
                name: "DataType",
                table: "LabParameters");
        }
    }
}
