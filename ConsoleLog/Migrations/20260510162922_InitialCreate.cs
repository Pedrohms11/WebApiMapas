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
                name: "Auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tabela = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RegistroId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Acao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DadosAntigos = table.Column<string>(type: "TEXT", nullable: false),
                    DadosNovos = table.Column<string>(type: "TEXT", nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EmailUsuario = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PerfilUsuario = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Maquina = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Detalhes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Origem = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "LogsRequisicao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Operacao = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Endpoint = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Parametros = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RequestBody = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ResponseBody = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    DuracaoMs = table.Column<long>(type: "INTEGER", nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EmailUsuario = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PerfilUsuario = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Maquina = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Sucesso = table.Column<bool>(type: "INTEGER", nullable: false),
                    MensagemErro = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Origem = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsRequisicao", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Acao",
                table: "Auditoria",
                column: "Acao");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_DataHora",
                table: "Auditoria",
                column: "DataHora");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_RegistroId",
                table: "Auditoria",
                column: "RegistroId");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Tabela",
                table: "Auditoria",
                column: "Tabela");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Usuario",
                table: "Auditoria",
                column: "Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Localizacoes_Cep",
                table: "Localizacoes",
                column: "Cep");

            migrationBuilder.CreateIndex(
                name: "IX_Localizacoes_Timestamp",
                table: "Localizacoes",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_LogsRequisicao_DataHora",
                table: "LogsRequisicao",
                column: "DataHora");

            migrationBuilder.CreateIndex(
                name: "IX_LogsRequisicao_Endpoint",
                table: "LogsRequisicao",
                column: "Endpoint");

            migrationBuilder.CreateIndex(
                name: "IX_LogsRequisicao_Operacao",
                table: "LogsRequisicao",
                column: "Operacao");

            migrationBuilder.CreateIndex(
                name: "IX_LogsRequisicao_Sucesso",
                table: "LogsRequisicao",
                column: "Sucesso");

            migrationBuilder.CreateIndex(
                name: "IX_LogsRequisicao_Usuario",
                table: "LogsRequisicao",
                column: "Usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditoria");

            migrationBuilder.DropTable(
                name: "Localizacoes");

            migrationBuilder.DropTable(
                name: "LogsRequisicao");
        }
    }
}
