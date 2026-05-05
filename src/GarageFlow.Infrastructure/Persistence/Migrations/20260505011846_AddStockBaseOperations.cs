using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockBaseOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_type = table.Column<int>(type: "integer", nullable: false),
                    total_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    available_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    reserved_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    minimum_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stocks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stock_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    reason = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    stock_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_operations", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_operations_stocks_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stock_operations_stock_id_created_at",
                table: "stock_operations",
                columns: new[] { "stock_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_stocks_item_type_item_id",
                table: "stocks",
                columns: new[] { "item_type", "item_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_operations");

            migrationBuilder.DropTable(
                name: "stocks");
        }
    }
}
