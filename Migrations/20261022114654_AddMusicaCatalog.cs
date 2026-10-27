using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EcommerceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMusicaCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artistas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Generos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Musicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AudioUrl = table.Column<string>(type: "text", nullable: true),
                    AudioPublicId = table.Column<string>(type: "text", nullable: true),
                    ImagenUrl = table.Column<string>(type: "text", nullable: true),
                    ImagenPublicId = table.Column<string>(type: "text", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musicas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicaArtista",
                columns: table => new
                {
                    MusicaId = table.Column<int>(type: "integer", nullable: false),
                    ArtistaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicaArtista", x => new { x.MusicaId, x.ArtistaId });
                    table.ForeignKey(
                        name: "FK_MusicaArtista_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusicaArtista_Musicas_MusicaId",
                        column: x => x.MusicaId,
                        principalTable: "Musicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusicaGenero",
                columns: table => new
                {
                    MusicaId = table.Column<int>(type: "integer", nullable: false),
                    GeneroId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicaGenero", x => new { x.MusicaId, x.GeneroId });
                    table.ForeignKey(
                        name: "FK_MusicaGenero_Generos_GeneroId",
                        column: x => x.GeneroId,
                        principalTable: "Generos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusicaGenero_Musicas_MusicaId",
                        column: x => x.MusicaId,
                        principalTable: "Musicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Artistas",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Bad Bunny" },
                    { 2, "Taylor Swift" },
                    { 3, "Drake" },
                    { 4, "Shakira" },
                    { 5, "The Weeknd" },
                    { 6, "J Balvin" },
                    { 7, "Ariana Grande" },
                    { 8, "Daddy Yankee" },
                    { 9, "Ozuna" },
                    { 10, "Billie Eilish" }
                });

            migrationBuilder.InsertData(
                table: "Generos",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Pop" },
                    { 2, "Rock" },
                    { 3, "Hip-Hop" },
                    { 4, "Electrónica" },
                    { 5, "Reggaetón" },
                    { 6, "Jazz" },
                    { 7, "Clásica" },
                    { 8, "R&B" },
                    { 9, "Metal" },
                    { 10, "Salsa" },
                    { 11, "Cumbia" },
                    { 12, "Bachata" },
                    { 13, "Trap" },
                    { 14, "Indie" },
                    { 15, "Folk" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicaArtista_ArtistaId",
                table: "MusicaArtista",
                column: "ArtistaId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicaGenero_GeneroId",
                table: "MusicaGenero",
                column: "GeneroId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicaArtista");

            migrationBuilder.DropTable(
                name: "MusicaGenero");

            migrationBuilder.DropTable(
                name: "Artistas");

            migrationBuilder.DropTable(
                name: "Generos");

            migrationBuilder.DropTable(
                name: "Musicas");
        }
    }
}
