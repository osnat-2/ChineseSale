using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class updateDonorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Present_Donor_DonorId",
                table: "Present");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Donor",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Donor",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Donor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Donor",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donor_Email",
                table: "Donor",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Present_Donor_DonorId",
                table: "Present",
                column: "DonorId",
                principalTable: "Donor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Present_Donor_DonorId",
                table: "Present");

            migrationBuilder.DropIndex(
                name: "IX_Donor_Email",
                table: "Donor");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Donor");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Donor");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Donor");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Donor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Present_Donor_DonorId",
                table: "Present",
                column: "DonorId",
                principalTable: "Donor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
