using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace prriva_10.Migrations
{
    /// <inheritdoc />
    public partial class Valeeva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Izmers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Typ = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Izmers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Kategors",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Typ = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategors", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Privozs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kolvo = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privozs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Prodajas",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kolvo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prodajas", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tovar",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IzmerID = table.Column<int>(type: "int", nullable: false),
                    KategorID = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    Kol_vo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tovar", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tovar_Izmers_IzmerID",
                        column: x => x.IzmerID,
                        principalTable: "Izmers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tovar_Kategors_KategorID",
                        column: x => x.KategorID,
                        principalTable: "Kategors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrivozsTovars",
                columns: table => new
                {
                    PrivozsID = table.Column<int>(type: "int", nullable: false),
                    TovarID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivozsTovars", x => new { x.PrivozsID, x.TovarID });
                    table.ForeignKey(
                        name: "FK_PrivozsTovars_Privozs_PrivozsID",
                        column: x => x.PrivozsID,
                        principalTable: "Privozs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrivozsTovars_Tovar_TovarID",
                        column: x => x.TovarID,
                        principalTable: "Tovar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProdajasTovars",
                columns: table => new
                {
                    ProdajasID = table.Column<int>(type: "int", nullable: false),
                    TovarsID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdajasTovars", x => new { x.ProdajasID, x.TovarsID });
                    table.ForeignKey(
                        name: "FK_ProdajasTovars_Prodajas_ProdajasID",
                        column: x => x.ProdajasID,
                        principalTable: "Prodajas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProdajasTovars_Tovar_TovarsID",
                        column: x => x.TovarsID,
                        principalTable: "Tovar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrivozsTovars_TovarID",
                table: "PrivozsTovars",
                column: "TovarID");

            migrationBuilder.CreateIndex(
                name: "IX_ProdajasTovars_TovarsID",
                table: "ProdajasTovars",
                column: "TovarsID");

            migrationBuilder.CreateIndex(
                name: "IX_Tovar_IzmerID",
                table: "Tovar",
                column: "IzmerID");

            migrationBuilder.CreateIndex(
                name: "IX_Tovar_KategorID",
                table: "Tovar",
                column: "KategorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrivozsTovars");

            migrationBuilder.DropTable(
                name: "ProdajasTovars");

            migrationBuilder.DropTable(
                name: "Privozs");

            migrationBuilder.DropTable(
                name: "Prodajas");

            migrationBuilder.DropTable(
                name: "Tovar");

            migrationBuilder.DropTable(
                name: "Izmers");

            migrationBuilder.DropTable(
                name: "Kategors");
        }
    }
}
