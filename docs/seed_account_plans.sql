-- SQL script to seed AccountPlans in PostgreSQL
-- Save this file as docs/seed_account_plans.sql

BEGIN;

-- 1. Ensure the AccountantProcess with Id = 1 exists to satisfy the foreign key constraint
INSERT INTO "AccountantProcesses" ("Id", "Name", "StartDate", "EndDate", "IsClosed")
SELECT 1, 'Periodo Inicial', '2026-01-01', '2026-12-31', false
WHERE NOT EXISTS (SELECT 1 FROM "AccountantProcesses" WHERE "Id" = 1);

-- 2. Insert Root AccountPlans (ParentId IS NULL)
INSERT INTO "AccountPlans" ("Id", "Code", "Name", "IsAcceptor", "Order", "AccountantProcessId", "ParentId")
VALUES
(1, '1', 'ACTIVOS', false, 1, 1, NULL),
(2, '2', 'PASIVOS', false, 1, 1, NULL),
(3, '3', 'INGRESOS', false, 1, 1, NULL),
(4, '4', 'EGRESOS', false, 1, 1, NULL),
(5, '5', 'PATRIMONIO_NETO', false, 1, 1, NULL)
ON CONFLICT ("Id") DO UPDATE SET
    "Code" = EXCLUDED."Code",
    "Name" = EXCLUDED."Name",
    "IsAcceptor" = EXCLUDED."IsAcceptor",
    "Order" = EXCLUDED."Order",
    "AccountantProcessId" = EXCLUDED."AccountantProcessId",
    "ParentId" = EXCLUDED."ParentId";

-- 3. Insert Child AccountPlans (ParentId IS NOT NULL)
-- Note: Inserted after root accounts to satisfy the self-referencing foreign key constraint
INSERT INTO "AccountPlans" ("Id", "Code", "Name", "IsAcceptor", "Order", "AccountantProcessId", "ParentId")
VALUES
(6, '1.1', 'Cajas', true, 1, 1, 1),
(7, '1.2', 'Cuentas', true, 2, 1, 1),
(8, '1.3', 'Bancos', true, 3, 1, 1),
(9, '2.1', 'ProveedoresAPagar', true, 1, 1, 2),
(10, '2.2', 'SalariosAPagar', true, 2, 1, 2),
(11, '3.1', 'Ventas', true, 1, 1, 3),
(12, '4.1', 'ComprasAProveedores', true, 1, 1, 4),
(13, '4.2', 'PagosDeSalarios', true, 2, 1, 4),
(14, '2.3', 'IVA_DEBITO', true, 3, 1, 2),
(15, '1.4', 'IVA_CREDITO', true, 4, 1, 1),
(16, '2.4', 'IPS_A_Pagar', true, 4, 1, 2)
ON CONFLICT ("Id") DO UPDATE SET
    "Code" = EXCLUDED."Code",
    "Name" = EXCLUDED."Name",
    "IsAcceptor" = EXCLUDED."IsAcceptor",
    "Order" = EXCLUDED."Order",
    "AccountantProcessId" = EXCLUDED."AccountantProcessId",
    "ParentId" = EXCLUDED."ParentId";

-- 4. Update the Identity Sequence to prevent future primary key collisions
SELECT setval(pg_get_serial_sequence('"AccountPlans"', 'Id'), COALESCE(MAX("Id"), 1)) FROM "AccountPlans";

COMMIT;
