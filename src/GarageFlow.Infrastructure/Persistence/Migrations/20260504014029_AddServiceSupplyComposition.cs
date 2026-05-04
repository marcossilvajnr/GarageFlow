using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceSupplyComposition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_supplies",
                columns: table => new
                {
                    supply_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supply_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_supplies", x => new { x.service_id, x.supply_id });
                    table.ForeignKey(
                        name: "FK_service_supplies_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_service_supplies_service_id_supply_id",
                table: "service_supplies",
                columns: new[] { "service_id", "supply_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_supplies");
        }
    }
}
