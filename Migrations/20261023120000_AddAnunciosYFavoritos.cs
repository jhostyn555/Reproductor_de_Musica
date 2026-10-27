using System;
using EcommerceApp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EcommerceApp.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261023120000_AddAnunciosYFavoritos")]
    public partial class AddAnunciosYFavoritos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Anuncios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ImagenUrl = table.Column<string>(type: "text", nullable: true),
                    Enlace = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anuncios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    MusicaId = table.Column<int>(type: "integer", nullable: false),
                    AgregadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favoritos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Musicas_MusicaId",
                        column: x => x.MusicaId,
                        principalTable: "Musicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_MusicaId",
                table: "Favoritos",
                column: "MusicaId");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_UserId_MusicaId",
                table: "Favoritos",
                columns: new[] { "UserId", "MusicaId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Favoritos");
            migrationBuilder.DropTable(name: "Anuncios");
        }
    }
}
