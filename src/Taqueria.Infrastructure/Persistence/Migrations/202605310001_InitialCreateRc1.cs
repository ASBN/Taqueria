using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taqueria.Infrastructure.Persistence.Migrations;

[DbContext(typeof(TaqueriaDbContext))]
[Migration("202605310001_InitialCreateRc1")]
public partial class InitialCreateRc1 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CategoriasProducto",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(maxLength: 80, nullable: false),
                OrdenVisual = table.Column<int>(nullable: false),
                Activa = table.Column<bool>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CategoriasProducto", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Mesas",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Numero = table.Column<int>(nullable: false),
                Capacidad = table.Column<int>(nullable: false),
                Activa = table.Column<bool>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Mesas", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Usuarios",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(maxLength: 120, nullable: false),
                NombreUsuario = table.Column<string>(maxLength: 80, nullable: false),
                PasswordHash = table.Column<string>(maxLength: 500, nullable: false),
                Rol = table.Column<int>(nullable: false),
                Activo = table.Column<bool>(nullable: false),
                DebeCambiarPassword = table.Column<bool>(nullable: false),
                FechaCreacionUtc = table.Column<DateTime>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Usuarios", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Productos",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CategoriaProductoId = table.Column<int>(nullable: false),
                Nombre = table.Column<string>(maxLength: 120, nullable: false),
                OrdenVisual = table.Column<int>(nullable: false),
                Activo = table.Column<bool>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Productos", x => x.Id);
                table.ForeignKey(
                    name: "FK_Productos_CategoriasProducto_CategoriaProductoId",
                    column: x => x.CategoriaProductoId,
                    principalTable: "CategoriasProducto",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Comandas",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Folio = table.Column<string>(maxLength: 30, nullable: false),
                MesaId = table.Column<int>(nullable: false),
                UsuarioMeseroId = table.Column<int>(nullable: false),
                UsuarioCancelacionId = table.Column<int>(nullable: true),
                Estado = table.Column<int>(nullable: false),
                FechaHoraAperturaUtc = table.Column<DateTime>(nullable: false),
                FechaHoraEnvioCocinaUtc = table.Column<DateTime>(nullable: true),
                FechaHoraEntregaUtc = table.Column<DateTime>(nullable: true),
                FechaHoraCobroUtc = table.Column<DateTime>(nullable: true),
                FechaHoraCancelacionUtc = table.Column<DateTime>(nullable: true),
                MotivoCancelacion = table.Column<string>(maxLength: 300, nullable: true),
                TotalCobrado = table.Column<decimal>(precision: 10, scale: 2, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Comandas", x => x.Id);
                table.ForeignKey(
                    name: "FK_Comandas_Mesas_MesaId",
                    column: x => x.MesaId,
                    principalTable: "Mesas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Comandas_Usuarios_UsuarioMeseroId",
                    column: x => x.UsuarioMeseroId,
                    principalTable: "Usuarios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Comandas_Usuarios_UsuarioCancelacionId",
                    column: x => x.UsuarioCancelacionId,
                    principalTable: "Usuarios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PreciosProducto",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductoId = table.Column<int>(nullable: false),
                Precio = table.Column<decimal>(precision: 10, scale: 2, nullable: false),
                VigenteDesdeUtc = table.Column<DateTime>(nullable: false),
                FechaCreacionUtc = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PreciosProducto", x => x.Id);
                table.ForeignKey(
                    name: "FK_PreciosProducto_Productos_ProductoId",
                    column: x => x.ProductoId,
                    principalTable: "Productos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Comensales",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ComandaId = table.Column<int>(nullable: false),
                Numero = table.Column<int>(nullable: false),
                Activo = table.Column<bool>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Comensales", x => x.Id);
                table.ForeignKey(
                    name: "FK_Comensales_Comandas_ComandaId",
                    column: x => x.ComandaId,
                    principalTable: "Comandas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PagosComanda",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ComandaId = table.Column<int>(nullable: false),
                MetodoPago = table.Column<int>(nullable: false),
                Importe = table.Column<decimal>(precision: 10, scale: 2, nullable: false),
                FechaHoraPagoUtc = table.Column<DateTime>(nullable: false),
                UsuarioCobroId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PagosComanda", x => x.Id);
                table.ForeignKey(
                    name: "FK_PagosComanda_Comandas_ComandaId",
                    column: x => x.ComandaId,
                    principalTable: "Comandas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PagosComanda_Usuarios_UsuarioCobroId",
                    column: x => x.UsuarioCobroId,
                    principalTable: "Usuarios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ComandaDetalles",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ComensalId = table.Column<int>(nullable: false),
                ProductoId = table.Column<int>(nullable: false),
                CategoriaProductoIdHistorico = table.Column<int>(nullable: false),
                NombreCategoriaHistorico = table.Column<string>(maxLength: 80, nullable: false),
                NombreProductoHistorico = table.Column<string>(maxLength: 120, nullable: false),
                PrecioUnitarioHistorico = table.Column<decimal>(precision: 10, scale: 2, nullable: false),
                Cantidad = table.Column<int>(nullable: false),
                CantidadPreparada = table.Column<int>(nullable: false),
                CantidadEntregada = table.Column<int>(nullable: false),
                Subtotal = table.Column<decimal>(precision: 10, scale: 2, nullable: false),
                FechaHoraCapturaUtc = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComandaDetalles", x => x.Id);
                table.ForeignKey(
                    name: "FK_ComandaDetalles_Comensales_ComensalId",
                    column: x => x.ComensalId,
                    principalTable: "Comensales",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ComandaDetalles_Productos_ProductoId",
                    column: x => x.ProductoId,
                    principalTable: "Productos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "EntregasParciales",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ComandaDetalleId = table.Column<int>(nullable: false),
                Cantidad = table.Column<int>(nullable: false),
                FechaHoraListaUtc = table.Column<DateTime>(nullable: false),
                UsuarioCocinaId = table.Column<int>(nullable: false),
                Estado = table.Column<int>(nullable: false),
                FechaHoraEntregaUtc = table.Column<DateTime>(nullable: true),
                UsuarioMeseroEntregaId = table.Column<int>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EntregasParciales", x => x.Id);
                table.ForeignKey(
                    name: "FK_EntregasParciales_ComandaDetalles_ComandaDetalleId",
                    column: x => x.ComandaDetalleId,
                    principalTable: "ComandaDetalles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EntregasParciales_Usuarios_UsuarioCocinaId",
                    column: x => x.UsuarioCocinaId,
                    principalTable: "Usuarios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_EntregasParciales_Usuarios_UsuarioMeseroEntregaId",
                    column: x => x.UsuarioMeseroEntregaId,
                    principalTable: "Usuarios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_CategoriasProducto_Nombre", table: "CategoriasProducto", column: "Nombre", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Mesas_Numero", table: "Mesas", column: "Numero", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Usuarios_NombreUsuario", table: "Usuarios", column: "NombreUsuario", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Productos_CategoriaProductoId", table: "Productos", column: "CategoriaProductoId");
        migrationBuilder.CreateIndex(name: "IX_Productos_Nombre", table: "Productos", column: "Nombre", unique: true);
        migrationBuilder.CreateIndex(name: "IX_PreciosProducto_ProductoId_VigenteDesdeUtc", table: "PreciosProducto", columns: new[] { "ProductoId", "VigenteDesdeUtc" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_Comandas_Folio", table: "Comandas", column: "Folio", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Comandas_MesaId", table: "Comandas", column: "MesaId");
        migrationBuilder.CreateIndex(name: "IX_Comandas_UsuarioMeseroId", table: "Comandas", column: "UsuarioMeseroId");
        migrationBuilder.CreateIndex(name: "IX_Comandas_UsuarioCancelacionId", table: "Comandas", column: "UsuarioCancelacionId");
        migrationBuilder.CreateIndex(name: "IX_Comandas_Estado_FechaHoraCobroUtc", table: "Comandas", columns: new[] { "Estado", "FechaHoraCobroUtc" });
        migrationBuilder.CreateIndex(name: "IX_Comensales_ComandaId_Numero", table: "Comensales", columns: new[] { "ComandaId", "Numero" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_PagosComanda_ComandaId", table: "PagosComanda", column: "ComandaId");
        migrationBuilder.CreateIndex(name: "IX_PagosComanda_UsuarioCobroId", table: "PagosComanda", column: "UsuarioCobroId");
        migrationBuilder.CreateIndex(name: "IX_ComandaDetalles_ComensalId", table: "ComandaDetalles", column: "ComensalId");
        migrationBuilder.CreateIndex(name: "IX_ComandaDetalles_ProductoId", table: "ComandaDetalles", column: "ProductoId");
        migrationBuilder.CreateIndex(name: "IX_EntregasParciales_ComandaDetalleId", table: "EntregasParciales", column: "ComandaDetalleId");
        migrationBuilder.CreateIndex(name: "IX_EntregasParciales_UsuarioCocinaId", table: "EntregasParciales", column: "UsuarioCocinaId");
        migrationBuilder.CreateIndex(name: "IX_EntregasParciales_UsuarioMeseroEntregaId", table: "EntregasParciales", column: "UsuarioMeseroEntregaId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "EntregasParciales");
        migrationBuilder.DropTable(name: "PagosComanda");
        migrationBuilder.DropTable(name: "ComandaDetalles");
        migrationBuilder.DropTable(name: "Comensales");
        migrationBuilder.DropTable(name: "PreciosProducto");
        migrationBuilder.DropTable(name: "Comandas");
        migrationBuilder.DropTable(name: "Productos");
        migrationBuilder.DropTable(name: "Mesas");
        migrationBuilder.DropTable(name: "Usuarios");
        migrationBuilder.DropTable(name: "CategoriasProducto");
    }
}
