using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projekt_zespolowy.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoSavingsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AutoSaveAmount",
                table: "SavingsGoals",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AutoSaveDay",
                table: "SavingsGoals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoSaveEnabled",
                table: "SavingsGoals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAutoSaveDate",
                table: "SavingsGoals",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoSaveAmount",
                table: "SavingsGoals");

            migrationBuilder.DropColumn(
                name: "AutoSaveDay",
                table: "SavingsGoals");

            migrationBuilder.DropColumn(
                name: "IsAutoSaveEnabled",
                table: "SavingsGoals");

            migrationBuilder.DropColumn(
                name: "NextAutoSaveDate",
                table: "SavingsGoals");
        }
    }
}
