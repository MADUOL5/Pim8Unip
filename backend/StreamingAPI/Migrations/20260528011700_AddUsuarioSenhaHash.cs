using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioSenhaHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArquivoMidia",
                table: "Conteudos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenhaHash",
                table: "Usuarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenhaHash",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ArquivoMidia",
                table: "Conteudos");
        }
    }
}
