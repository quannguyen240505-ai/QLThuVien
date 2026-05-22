using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIQLTV.Migrations
{
    /// <inheritdoc />
    public partial class AddResetPasswordPin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResetPasswordPinUsed",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordPin",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordPinExpires",
                table: "Users",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResetPasswordPinUsed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResetPasswordPin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResetPasswordPinExpires",
                table: "Users");
        }
    }
}
