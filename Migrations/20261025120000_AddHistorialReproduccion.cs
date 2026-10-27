using System;
using EcommerceApp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EcommerceApp.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261025120000_AddHistorialReproduccion")]
    public partial class AddHistorialReproduccion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistorialReproducciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    MusicaId = table.Column<int>(type: "integer", nullable: false),
                    FechaReproduccion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialReproducciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialReproducciones_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialReproducciones_Musicas_MusicaId",
                        column: x => x.MusicaId,
                        principalTable: "Musicas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialReproducciones_MusicaId",
                table: "HistorialReproducciones",
                column: "MusicaId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialReproducciones_UserId_FechaReproduccion",
                table: "HistorialReproducciones",
                columns: new[] { "UserId", "FechaReproduccion" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HistorialReproducciones");
        }
    }
}
