using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BedAutomation.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMedicalRecordFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "MedicalRecords");

            migrationBuilder.RenameColumn(
                name: "VisitDate",
                table: "MedicalRecords",
                newName: "RecordDate");

            migrationBuilder.RenameColumn(
                name: "PresentIllness",
                table: "MedicalRecords",
                newName: "HistoryOfPresentIllness");

            migrationBuilder.AddColumn<DateTime>(
                name: "AdmissionDate",
                table: "MedicalRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DischargeDate",
                table: "MedicalRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DischargeSummary",
                table: "MedicalRecords",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "MedicalRecords",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MedicalRecords",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdmissionDate",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "DischargeDate",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "DischargeSummary",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MedicalRecords");

            migrationBuilder.RenameColumn(
                name: "RecordDate",
                table: "MedicalRecords",
                newName: "VisitDate");

            migrationBuilder.RenameColumn(
                name: "HistoryOfPresentIllness",
                table: "MedicalRecords",
                newName: "PresentIllness");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "MedicalRecords",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
