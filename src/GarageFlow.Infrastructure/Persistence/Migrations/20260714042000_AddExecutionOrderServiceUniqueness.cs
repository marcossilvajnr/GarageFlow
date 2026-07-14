using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageFlow.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddExecutionOrderServiceUniqueness : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM execution_orders
            WHERE id IN (
                SELECT id
                FROM (
                    SELECT
                        id,
                        ROW_NUMBER() OVER (
                            PARTITION BY service_order_id, service_id
                            ORDER BY created_at, id
                        ) AS row_number
                    FROM execution_orders
                ) duplicates
                WHERE duplicates.row_number > 1
            );
            """);

        migrationBuilder.CreateIndex(
            name: "ux_execution_orders_service_order_service",
            table: "execution_orders",
            columns: new[] { "service_order_id", "service_id" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ux_execution_orders_service_order_service",
            table: "execution_orders");
    }
}
