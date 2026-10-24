using System;
using EcommerceApp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EcommerceApp.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261023180000_AddAlbumes")]
    public partial class AddAlbumes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DuracionSegundos",
                table: "Musicas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Albumes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ImagenUrl = table.Column<string>(type: "text", nullable: true),
                    ImagenPublicId = table.Column<string>(type: "text", nullable: true),
                    ArtistaId = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albumes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albumes_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlbumMusicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlbumId = table.Column<int>(type: "integer", nullable: false),
                    MusicaId = table.Column<int>(type: "integer", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumMusicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumMusicas_Albumes_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumMusicas_Musicas_MusicaId",
                        column: x => x.MusicaId,
                        principalTable: "Musicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albumes_ArtistaId",
                table: "Albumes",
                column: "ArtistaId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumMusicas_AlbumId_MusicaId",
                table: "AlbumMusicas",
                columns: new[] { "AlbumId", "MusicaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumMusicas_MusicaId",
                table: "AlbumMusicas",
                column: "MusicaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AlbumMusicas");
            migrationBuilder.DropTable(name: "Albumes");
            migrationBuilder.DropColumn(name: "DuracionSegundos", table: "Musicas");
        }
    }
}
