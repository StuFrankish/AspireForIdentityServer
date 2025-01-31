using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServer.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class MapFido2Entities_ToFidoSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fido2AuthenticatorTransports_Fido2PublicKeyCredentials_PublicKeyCredentialId",
                table: "Fido2AuthenticatorTransports");

            migrationBuilder.DropForeignKey(
                name: "FK_Fido2DevicePublicKeys_Fido2PublicKeyCredentials_PublicKeyCredentialId",
                table: "Fido2DevicePublicKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_Fido2PublicKeyCredentials_AspNetUsers_UserId",
                table: "Fido2PublicKeyCredentials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fido2PublicKeyCredentials",
                table: "Fido2PublicKeyCredentials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fido2DevicePublicKeys",
                table: "Fido2DevicePublicKeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fido2AuthenticatorTransports",
                table: "Fido2AuthenticatorTransports");

            migrationBuilder.EnsureSchema(
                name: "Fido");

            migrationBuilder.RenameTable(
                name: "Fido2PublicKeyCredentials",
                newName: "PublicKeyCredentials",
                newSchema: "Fido");

            migrationBuilder.RenameTable(
                name: "Fido2DevicePublicKeys",
                newName: "DevicePublicKeys",
                newSchema: "Fido");

            migrationBuilder.RenameTable(
                name: "Fido2AuthenticatorTransports",
                newName: "AuthenticatorTransports",
                newSchema: "Fido");

            migrationBuilder.RenameIndex(
                name: "IX_Fido2PublicKeyCredentials_UserId",
                schema: "Fido",
                table: "PublicKeyCredentials",
                newName: "IX_PublicKeyCredentials_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PublicKeyCredentials",
                schema: "Fido",
                table: "PublicKeyCredentials",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DevicePublicKeys",
                schema: "Fido",
                table: "DevicePublicKeys",
                columns: new[] { "PublicKeyCredentialId", "Value" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthenticatorTransports",
                schema: "Fido",
                table: "AuthenticatorTransports",
                columns: new[] { "PublicKeyCredentialId", "Value" });

            migrationBuilder.AddForeignKey(
                name: "FK_AuthenticatorTransports_PublicKeyCredentials_PublicKeyCredentialId",
                schema: "Fido",
                table: "AuthenticatorTransports",
                column: "PublicKeyCredentialId",
                principalSchema: "Fido",
                principalTable: "PublicKeyCredentials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DevicePublicKeys_PublicKeyCredentials_PublicKeyCredentialId",
                schema: "Fido",
                table: "DevicePublicKeys",
                column: "PublicKeyCredentialId",
                principalSchema: "Fido",
                principalTable: "PublicKeyCredentials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicKeyCredentials_AspNetUsers_UserId",
                schema: "Fido",
                table: "PublicKeyCredentials",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthenticatorTransports_PublicKeyCredentials_PublicKeyCredentialId",
                schema: "Fido",
                table: "AuthenticatorTransports");

            migrationBuilder.DropForeignKey(
                name: "FK_DevicePublicKeys_PublicKeyCredentials_PublicKeyCredentialId",
                schema: "Fido",
                table: "DevicePublicKeys");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicKeyCredentials_AspNetUsers_UserId",
                schema: "Fido",
                table: "PublicKeyCredentials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PublicKeyCredentials",
                schema: "Fido",
                table: "PublicKeyCredentials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DevicePublicKeys",
                schema: "Fido",
                table: "DevicePublicKeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthenticatorTransports",
                schema: "Fido",
                table: "AuthenticatorTransports");

            migrationBuilder.RenameTable(
                name: "PublicKeyCredentials",
                schema: "Fido",
                newName: "Fido2PublicKeyCredentials");

            migrationBuilder.RenameTable(
                name: "DevicePublicKeys",
                schema: "Fido",
                newName: "Fido2DevicePublicKeys");

            migrationBuilder.RenameTable(
                name: "AuthenticatorTransports",
                schema: "Fido",
                newName: "Fido2AuthenticatorTransports");

            migrationBuilder.RenameIndex(
                name: "IX_PublicKeyCredentials_UserId",
                table: "Fido2PublicKeyCredentials",
                newName: "IX_Fido2PublicKeyCredentials_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fido2PublicKeyCredentials",
                table: "Fido2PublicKeyCredentials",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fido2DevicePublicKeys",
                table: "Fido2DevicePublicKeys",
                columns: new[] { "PublicKeyCredentialId", "Value" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fido2AuthenticatorTransports",
                table: "Fido2AuthenticatorTransports",
                columns: new[] { "PublicKeyCredentialId", "Value" });

            migrationBuilder.AddForeignKey(
                name: "FK_Fido2AuthenticatorTransports_Fido2PublicKeyCredentials_PublicKeyCredentialId",
                table: "Fido2AuthenticatorTransports",
                column: "PublicKeyCredentialId",
                principalTable: "Fido2PublicKeyCredentials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fido2DevicePublicKeys_Fido2PublicKeyCredentials_PublicKeyCredentialId",
                table: "Fido2DevicePublicKeys",
                column: "PublicKeyCredentialId",
                principalTable: "Fido2PublicKeyCredentials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fido2PublicKeyCredentials_AspNetUsers_UserId",
                table: "Fido2PublicKeyCredentials",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
