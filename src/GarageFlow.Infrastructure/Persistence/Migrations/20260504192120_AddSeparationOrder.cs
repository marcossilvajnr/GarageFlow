using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSeparationOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "separation_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    execution_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    stockist_id = table.Column<Guid>(type: "uuid", nullable: true),
                    confirmed_by_stockist_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    confirmed_by_mechanic_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_separation_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "separation_order_parts",
                columns: table => new
                {
                    part_id = table.Column<Guid>(type: "uuid", nullable: false),
                    separation_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    part_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    is_reserved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_separation_order_parts", x => new { x.separation_order_id, x.part_id });
                    table.ForeignKey(
                        name: "FK_separation_order_parts_separation_orders_separation_order_id",
                        column: x => x.separation_order_id,
                        principalTable: "separation_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "separation_order_supplies",
                columns: table => new
                {
                    supply_id = table.Column<Guid>(type: "uuid", nullable: false),
                    separation_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supply_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit = table.Column<int>(type: "integer", nullable: false),
                    is_reserved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_separation_order_supplies", x => new { x.separation_order_id, x.supply_id });
                    table.ForeignKey(
                        name: "FK_separation_order_supplies_separation_orders_separation_orde~",
                        column: x => x.separation_order_id,
                        principalTable: "separation_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_separation_orders_execution_order_id",
                table: "separation_orders",
                column: "execution_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_separation_orders_status",
                table: "separation_orders",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "separation_order_parts");

            migrationBuilder.DropTable(
                name: "separation_order_supplies");

            migrationBuilder.DropTable(
                name: "separation_orders");
        }
    }
}
