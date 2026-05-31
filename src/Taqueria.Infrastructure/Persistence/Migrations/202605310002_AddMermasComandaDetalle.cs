using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taqueria.Infrastructure.Persistence.Migrations;

[DbContext(typeof(TaqueriaDbContext))]
[Migration("202605310002_AddMermasComandaDetalle")]
public partial class AddMermasComandaDetalle : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MermasComandaDetalle",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ComandaDetalleId = table.Column<int>(nullable: false),
                Cantidad = table.Column<int>(nullable: false),
                ImporteHistorico = table.Column<decimal>(precision: 10, scale: 2, nullable: false),
                Motivo = table.Column<string>(maxLength: 300, nullable: false),
                FechaHoraRegistroUtc = table.Column<DateTime>(nullable: false),
                UsuarioRegistroId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MermasComandaDetalle", x => x.Id);
                table.ForeignKey(
                    name: "FK_MermasComandaDetalle_ComandaDetalles_ComandaDetalleId",
                    column: x => x.ComandaDetalleId,
                    principalTable: "ComandaDetalles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_MermasComandaDetalle_Usuarios_UsuarioRegistroId",
                    column: x => x.UsuarioRegistroId,
                    principalTable: "Usuarios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_MermasComandaDetalle_ComandaDetalleId",
            table: "MermasComandaDetalle",
            column: "ComandaDetalleId");

        migrationBuilder.CreateIndex(
            name: "IX_MermasComandaDetalle_UsuarioRegistroId",
            table: "MermasComandaDetalle",
            column: "UsuarioRegistroId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "MermasComandaDetalle");
    }
}
