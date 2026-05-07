using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeLinksAtFlowStart : Migration
    {
        private static readonly Guid LegacyEmployeeId = new("11111111-1111-1111-1111-111111111111");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                INSERT INTO employees (
                    id,
                    name,
                    document_type,
                    role,
                    is_active,
                    created_at,
                    email,
                    phone_number,
                    address_street,
                    address_number,
                    address_neighborhood,
                    address_city,
                    address_state,
                    address_zip_code
                )
                SELECT
                    '{LegacyEmployeeId}',
                    'Legacy Migration User',
                    0,
                    4,
                    TRUE,
                    NOW() AT TIME ZONE 'UTC',
                    'legacy.migration@garageflow.local',
                    '11999999999',
                    'Legacy Street',
                    '1',
                    'Centro',
                    'Sao Paulo',
                    'SP',
                    '01001000'
                WHERE NOT EXISTS (
                    SELECT 1 FROM employees WHERE id = '{LegacyEmployeeId}'
                );
            ");

            migrationBuilder.AddColumn<Guid>(
                name: "front_desk_employee_id",
                table: "service_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql($@"
                UPDATE service_orders
                SET front_desk_employee_id = '{LegacyEmployeeId}'
                WHERE front_desk_employee_id IS NULL;
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "front_desk_employee_id",
                table: "service_orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "assigned_supplier_by_employee_id",
                table: "purchase_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql($@"
                UPDATE execution_orders eo
                SET mechanic_id = NULL
                WHERE mechanic_id IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM employees e WHERE e.id = eo.mechanic_id
                  );
            ");

            migrationBuilder.Sql($@"
                UPDATE service_order_diagnostics sod
                SET mechanic_id = '{LegacyEmployeeId}'
                WHERE NOT EXISTS (
                    SELECT 1 FROM employees e WHERE e.id = sod.mechanic_id
                );
            ");

            migrationBuilder.CreateIndex(
                name: "ix_execution_orders_mechanic_id",
                table: "execution_orders",
                column: "mechanic_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_assigned_supplier_by_employee_id",
                table: "purchase_orders",
                column: "assigned_supplier_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_order_diagnostics_mechanic_id",
                table: "service_order_diagnostics",
                column: "mechanic_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_orders_front_desk_employee_id",
                table: "service_orders",
                column: "front_desk_employee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_execution_orders_employees_mechanic_id",
                table: "execution_orders",
                column: "mechanic_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_orders_employees_assigned_supplier_by_employee_id",
                table: "purchase_orders",
                column: "assigned_supplier_by_employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_service_order_diagnostics_employees_mechanic_id",
                table: "service_order_diagnostics",
                column: "mechanic_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_service_orders_employees_front_desk_employee_id",
                table: "service_orders",
                column: "front_desk_employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_execution_orders_employees_mechanic_id",
                table: "execution_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_purchase_orders_employees_assigned_supplier_by_employee_id",
                table: "purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_service_order_diagnostics_employees_mechanic_id",
                table: "service_order_diagnostics");

            migrationBuilder.DropForeignKey(
                name: "FK_service_orders_employees_front_desk_employee_id",
                table: "service_orders");

            migrationBuilder.DropIndex(
                name: "ix_execution_orders_mechanic_id",
                table: "execution_orders");

            migrationBuilder.DropIndex(
                name: "ix_purchase_orders_assigned_supplier_by_employee_id",
                table: "purchase_orders");

            migrationBuilder.DropIndex(
                name: "ix_service_order_diagnostics_mechanic_id",
                table: "service_order_diagnostics");

            migrationBuilder.DropIndex(
                name: "ix_service_orders_front_desk_employee_id",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "front_desk_employee_id",
                table: "service_orders");

            migrationBuilder.DropColumn(
                name: "assigned_supplier_by_employee_id",
                table: "purchase_orders");

            migrationBuilder.Sql($@"
                DELETE FROM employees
                WHERE id = '{LegacyEmployeeId}'
                  AND email = 'legacy.migration@garageflow.local';
            ");
        }
    }
}
