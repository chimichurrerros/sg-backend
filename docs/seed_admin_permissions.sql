-- SQL script to seed all permissions for the ADMIN role in PostgreSQL
-- Save this file as docs/seed_admin_permissions.sql

DO $$
DECLARE
    admin_role_id INT;
    permission_name TEXT;
    permissions_list TEXT[] := ARRAY[
        'accounts.view', 'accounts.create', 'accounts.update', 'accounts.delete',
        'suppliers.view', 'suppliers.create', 'suppliers.update', 'suppliers.delete',
        'banks.view', 'banks.create', 'banks.update', 'banks.delete',
        'bankMovements.view', 'bankMovements.create', 'bankMovements.update', 'bankMovements.delete',
        'services.view', 'services.create', 'services.update', 'services.delete',
        'supplierCategories.view', 'supplierCategories.create', 'supplierCategories.update', 'supplierCategories.delete',
        'purchaseReturns.view', 'purchaseReturns.create', 'purchaseReturns.update', 'purchaseReturns.delete',
        'employees.view', 'employees.create', 'employees.update', 'employees.delete',
        'paymentOrders.view', 'paymentOrders.create', 'paymentOrders.update', 'paymentOrders.delete',
        'bills.view', 'bills.create', 'bills.update', 'bills.delete',
        'entries.view', 'entries.create', 'entries.update', 'entries.delete',
        'accountantProcesses.view', 'accountantProcesses.create', 'accountantProcesses.update', 'accountantProcesses.delete',
        'accountPlans.view', 'accountPlans.create', 'accountPlans.update', 'accountPlans.delete',
        'accountingReports.view', 'accountingReports.create', 'accountingReports.update', 'accountingReports.delete',
        'schedules.view', 'schedules.create', 'schedules.update', 'schedules.delete',
        'purchaseOrderForSuppliers.view', 'purchaseOrderForSuppliers.create', 'purchaseOrderForSuppliers.update', 'purchaseOrderForSuppliers.delete',
        'checks.view', 'checks.create', 'checks.update', 'checks.delete',
        'branches.view', 'branches.create', 'branches.update', 'branches.delete',
        'purchaseRequests.view', 'purchaseRequests.create', 'purchaseRequests.update', 'purchaseRequests.delete',
        'organizations.view', 'organizations.create', 'organizations.update', 'organizations.delete',
        'states.view', 'states.create', 'states.update', 'states.delete',
        'positions.view', 'positions.create', 'positions.update', 'positions.delete',
        'customers.view', 'customers.create', 'customers.update', 'customers.delete',
        'departments.view', 'departments.create', 'departments.update', 'departments.delete',
        'employeeAssignments.view', 'employeeAssignments.create', 'employeeAssignments.update', 'employeeAssignments.delete',
        'creditNotes.view', 'creditNotes.create', 'creditNotes.update', 'creditNotes.delete',
        'customerQuotes.view', 'customerQuotes.create', 'customerQuotes.update', 'customerQuotes.delete',
        'purchaseReceipts.view', 'purchaseReceipts.create', 'purchaseReceipts.update', 'purchaseReceipts.delete',
        'requestForQuotations.view', 'requestForQuotations.create', 'requestForQuotations.update', 'requestForQuotations.delete',
        'salesReturns.view', 'salesReturns.create', 'salesReturns.update', 'salesReturns.delete',
        'payrollVariables.view', 'payrollVariables.create', 'payrollVariables.update', 'payrollVariables.delete',
        'payrollUpdates.view', 'payrollUpdates.create', 'payrollUpdates.update', 'payrollUpdates.delete',
        'manualConcepts.view', 'manualConcepts.create', 'manualConcepts.update', 'manualConcepts.delete',
        'salesOrders.view', 'salesOrders.create', 'salesOrders.update', 'salesOrders.delete',
        'purchaseOrders.view', 'purchaseOrders.create', 'purchaseOrders.update', 'purchaseOrders.delete',
        'stock.view', 'stock.create', 'stock.update', 'stock.delete',
        'supplierQuotes.view', 'supplierQuotes.create', 'supplierQuotes.update', 'supplierQuotes.delete',
        'payrollProcesses.view', 'payrollProcesses.create', 'payrollProcesses.update', 'payrollProcesses.delete',
        'billDetails.view', 'billDetails.create', 'billDetails.update', 'billDetails.delete',
        'attendance.view', 'attendance.create', 'attendance.update', 'attendance.delete',
        'productBrands.view', 'productBrands.create', 'productBrands.update', 'productBrands.delete',
        'productCategories.view', 'productCategories.create', 'productCategories.update', 'productCategories.delete',
        'users.view', 'users.create', 'users.update', 'users.delete',
        'products.view', 'products.create', 'products.update', 'products.delete',
        'permissions.view', 'permissions.create', 'permissions.update', 'permissions.delete',
        'roles.view', 'roles.create', 'roles.update', 'roles.delete'
    ];
BEGIN
    -- 1. Ensure the ADMIN role exists
    INSERT INTO "Roles" ("Name")
    SELECT 'ADMIN'
    WHERE NOT EXISTS (SELECT 1 FROM "Roles" WHERE UPPER("Name") = 'ADMIN');

    -- 2. Get the ADMIN role ID
    SELECT "Id" INTO admin_role_id FROM "Roles" WHERE UPPER("Name") = 'ADMIN';

    -- 3. Delete existing permissions for ADMIN to prevent duplicates
    DELETE FROM "Permissions" WHERE "RoleId" = admin_role_id;

    -- 4. Insert all permissions for ADMIN role
    FOREACH permission_name IN ARRAY permissions_list LOOP
        INSERT INTO "Permissions" ("Id", "Name", "RoleId")
        VALUES (gen_random_uuid(), permission_name, admin_role_id);
    END LOOP;

    RAISE NOTICE 'Permissions seeded successfully for ADMIN role.';
END $$;
