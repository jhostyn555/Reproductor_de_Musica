using EcommerceApp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceApp.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261026100000_CancionesDeUsuarios")]
    public partial class CancionesDeUsuarios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreadoPorId",
                table: "Musicas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Musicas",
                type: "text",
                nullable: false,
                defaultValue: "Aprobada");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CreadoPorId", table: "Musicas");
            migrationBuilder.DropColumn(name: "Estado", table: "Musicas");
        }
    }
}
