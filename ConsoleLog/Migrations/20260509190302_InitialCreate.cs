using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleLog.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Localizacoes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Logradouro = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Cep = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    LastSyncAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localizacoes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Localizacoes_Cep",
                table: "Localizacoes",
                column: "Cep");

            migrationBuilder.CreateIndex(
                name: "IX_Localizacoes_LastSyncAt",
                table: "Localizacoes",
                column: "LastSyncAt");

            migrationBuilder.CreateIndex(
                name: "IX_Localizacoes_Timestamp",
                table: "Localizacoes",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Localizacoes");
        }
    }
}
