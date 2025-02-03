using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServer.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class Add_PublicKeyCredential_Properties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CredentialCreatedDate",
                schema: "Fido",
                table: "PublicKeyCredentials",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CredentialLastUsedDate",
                schema: "Fido",
                table: "PublicKeyCredentials",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CredentialName",
                schema: "Fido",
                table: "PublicKeyCredentials",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CredentialCreatedDate",
                schema: "Fido",
                table: "PublicKeyCredentials");

            migrationBuilder.DropColumn(
                name: "CredentialLastUsedDate",
                schema: "Fido",
                table: "PublicKeyCredentials");

            migrationBuilder.DropColumn(
                name: "CredentialName",
                schema: "Fido",
                table: "PublicKeyCredentials");
        }
    }
}
