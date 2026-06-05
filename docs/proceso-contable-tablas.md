# Ubicacion de tablas del proceso contable

Este documento resume donde estan definidas las tablas del proceso contable en el backend.

## 1) Plan de cuentas

- Tabla: AccountPlans
- Modelo: [Models/Account/AccountPlan.cs](../Models/Account/AccountPlan.cs#L6)
- DbSet: [Infrastructure/Context/AppDbContext.cs](../Infrastructure/Context/AppDbContext.cs#L22)
- Snapshot: [Migrations/AppDbContextModelSnapshot.cs](../Migrations/AppDbContextModelSnapshot.cs#L106)
- Creacion inicial: [Migrations/20260427122550_InitialMigration.cs](../Migrations/20260427122550_InitialMigration.cs#L22)

## 2) Proceso contable

- Tabla: AccountantProcesses
- Modelo: [Models/Account/AccountantProcess.cs](../Models/Account/AccountantProcess.cs#L6)
- DbSet: [Infrastructure/Context/AppDbContext.cs](../Infrastructure/Context/AppDbContext.cs#L24)
- Snapshot: [Migrations/AppDbContextModelSnapshot.cs](../Migrations/AppDbContextModelSnapshot.cs#L136)
- Creacion inicial: [Migrations/20260427122550_InitialMigration.cs](../Migrations/20260427122550_InitialMigration.cs#L524)

## 3) Asientos

- Tabla: Entries
- Modelo: [Models/Entry/Entry.cs](../Models/Entry/Entry.cs#L6)
- FK a proceso contable: [Models/Entry/Entry.cs](../Models/Entry/Entry.cs#L10)
- DbSet: [Infrastructure/Context/AppDbContext.cs](../Infrastructure/Context/AppDbContext.cs#L62)
- Snapshot: [Migrations/AppDbContextModelSnapshot.cs](../Migrations/AppDbContextModelSnapshot.cs#L845)
- Creacion inicial: [Migrations/20260427122550_InitialMigration.cs](../Migrations/20260427122550_InitialMigration.cs#L659)

## 4) Asiento detalle

- Tabla: EntryDetails
- Modelo: [Models/Entry/EntryDetail.cs](../Models/Entry/EntryDetail.cs#L6)
- FK a plan de cuentas: [Models/Entry/EntryDetail.cs](../Models/Entry/EntryDetail.cs#L12)
- DbSet: [Infrastructure/Context/AppDbContext.cs](../Infrastructure/Context/AppDbContext.cs#L64)
- Snapshot: [Migrations/AppDbContextModelSnapshot.cs](../Migrations/AppDbContextModelSnapshot.cs#L877)
- Creacion inicial: [Migrations/20260427122550_InitialMigration.cs](../Migrations/20260427122550_InitialMigration.cs#L765)

## Extra relacionado

- Plantilla de asientos (cabecera): EntryModels -> [Migrations/20260427122550_InitialMigration.cs](../Migrations/20260427122550_InitialMigration.cs#L142)
- Plantilla de asientos (detalle): EntryModelDetails -> [Migrations/20260427122550_InitialMigration.cs](../Migrations/20260427122550_InitialMigration.cs#L374)

## Relacion funcional rapida

- AccountantProcesses -> Entries -> EntryDetails -> AccountPlans
