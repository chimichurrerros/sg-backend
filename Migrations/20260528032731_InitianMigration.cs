using System;
using BackEnd.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class InitianMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'account_type_enum') THEN
        CREATE TYPE account_type_enum AS ENUM ('cash','checking','savings');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'bank_movement_type_enum') THEN
        CREATE TYPE bank_movement_type_enum AS ENUM ('debit','credit');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'bill_state_enum') THEN
        CREATE TYPE bill_state_enum AS ENUM ('pending','paid','voided');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'bill_type_enum') THEN
        CREATE TYPE bill_type_enum AS ENUM ('contado','credito');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'check_status_enum') THEN
        CREATE TYPE check_status_enum AS ENUM ('pending','cashed','voided');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'check_type_enum') THEN
        CREATE TYPE check_type_enum AS ENUM ('day','deferred');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'purchase_request_state_enum') THEN
        CREATE TYPE purchase_request_state_enum AS ENUM ('pending','approved','rejected','completed');
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'sales_order_state_enum') THEN
        CREATE TYPE sales_order_state_enum AS ENUM ('pending','confirmed','cancelled');
    END IF;
END
$$;");

            migrationBuilder.CreateTable(
                name: "AccountPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsAcceptor = table.Column<bool>(type: "boolean", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("AccountPlans_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AffectsPayroll = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("AttendanceTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ruc = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Banks_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Ruc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Customers_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EntityTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntryModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EntryModels_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormulaTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("FormulaTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Genders_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Modules_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PayrollStatus_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PayrollTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Positions_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ProcessTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductBrands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ProductBrands_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ProductCategories_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PurchaseReturnReasons_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Roles_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ScheduleTypes_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("States_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ruc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    BusinessName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FantasyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Suppliers_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountType = table.Column<AccountTypeEnum>(type: "account_type_enum", nullable: false),
                    BankId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "text", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Accounts_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Accounts_BankId_fkey",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Entities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityTypeId = table.Column<int>(type: "integer", nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Entities_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Entities_EntityTypeId_fkey",
                        column: x => x.EntityTypeId,
                        principalTable: "EntityTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EntryModelDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntryModelId = table.Column<int>(type: "integer", nullable: false),
                    AccountPlanId = table.Column<int>(type: "integer", nullable: false),
                    IsDebit = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EntryModelDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "EntryModelDetails_AccountPlanId_fkey",
                        column: x => x.AccountPlanId,
                        principalTable: "AccountPlans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "EntryModelDetails_EntryModelId_fkey",
                        column: x => x.EntryModelId,
                        principalTable: "EntryModels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PayrollUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PayrollTypeId = table.Column<int>(type: "integer", nullable: false),
                    FormulaTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Formula = table.Column<string>(type: "text", nullable: true),
                    IpsDeductible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PayrollUpdates_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PayrollUpdates_FormulaTypeId_fkey",
                        column: x => x.FormulaTypeId,
                        principalTable: "FormulaTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PayrollUpdates_PayrollTypeId_fkey",
                        column: x => x.PayrollTypeId,
                        principalTable: "PayrollTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PayrollProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PayrollStatusId = table.Column<int>(type: "integer", nullable: false),
                    ProcessTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PayDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PayrollProcesses_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PayrollProcesses_PayrollStatusId_fkey",
                        column: x => x.PayrollStatusId,
                        principalTable: "PayrollStatus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PayrollProcesses_ProcessTypeId_fkey",
                        column: x => x.ProcessTypeId,
                        principalTable: "ProcessTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductCategoryId = table.Column<int>(type: "integer", nullable: true),
                    ProductBrandId = table.Column<int>(type: "integer", nullable: true),
                    MinimumStock = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    IsService = table.Column<bool>(type: "boolean", nullable: true),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 10m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Products_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Products_ProductBrandId_fkey",
                        column: x => x.ProductBrandId,
                        principalTable: "ProductBrands",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Products_ProductCategoryId_fkey",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Users_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Users_RoleId_fkey",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleTypeId = table.Column<int>(type: "integer", nullable: false),
                    ArrivalTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DepartureTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    NumberOfHours = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Schedules_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Schedules_ScheduleTypeId_fkey",
                        column: x => x.ScheduleTypeId,
                        principalTable: "ScheduleTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccountantProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StateId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("AccountantProcesses_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "AccountantProcesses_StateId_fkey",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Total = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    StateId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PaymentOrders_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentOrders_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PaymentOrders_SupplierId_fkey",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    ProductCategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("SupplierCategories_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "SupplierCategories_ProductCategoryId_fkey",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "SupplierCategories_SupplierId_fkey",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    MovementType = table.Column<BankMovementTypeEnum>(type: "bank_movement_type_enum", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("BankMovements_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "BankMovements_AccountId_fkey",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LegalPersons",
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    BussinessName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FantasyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("LegalPersons_pkey", x => x.EntityId);
                    table.ForeignKey(
                        name: "LegalPersons_EntityId_fkey",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PhysicalPersons",
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    GenderId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Lastname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PhysicalPersons_pkey", x => x.EntityId);
                    table.ForeignKey(
                        name: "PhysicalPersons_EntityId_fkey",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PhysicalPersons_GenderId_fkey",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Stocks_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Stocks_BranchId_fkey",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Stocks_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Total = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("CustomerQuotes_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "CustomerQuotes_CustomerId_fkey",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "CustomerQuotes_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PurchaseRequestState = table.Column<PurchaseRequestStateEnum>(type: "purchase_request_state_enum", nullable: false),
                    Observation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PurchaseRequests_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PurchaseRequests_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountantProcessId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ModuleId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Entries_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Entries_AccountantProcessId_fkey",
                        column: x => x.AccountantProcessId,
                        principalTable: "AccountantProcesses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Entries_ModuleId_fkey",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Checks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    BankMovementId = table.Column<int>(type: "integer", nullable: true),
                    Number = table.Column<string>(type: "text", nullable: false),
                    EmisionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AvailabilityDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    MaturityDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IssuingBank = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<CheckTypeEnum>(type: "check_type_enum", nullable: false),
                    Receiver = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    ConciliationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<CheckStatusEnum>(type: "check_status_enum", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Checks_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checks_BankMovements_BankMovementId",
                        column: x => x.BankMovementId,
                        principalTable: "BankMovements",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentOrderMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentOrderId = table.Column<int>(type: "integer", nullable: false),
                    BankMovementId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PaymentOrderMovements_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PaymentOrderMovements_BankMovementId_fkey",
                        column: x => x.BankMovementId,
                        principalTable: "BankMovements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PaymentOrderMovements_PaymentOrderId_fkey",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerQuoteDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerQuoteId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("CustomerQuoteDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "CustomerQuoteDetails_CustomerQuoteId_fkey",
                        column: x => x.CustomerQuoteId,
                        principalTable: "CustomerQuotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "CustomerQuoteDetails_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    CustomerQuoteId = table.Column<int>(type: "integer", nullable: true),
                    Number = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ImportValue = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    SalesOrderState = table.Column<SalesOrderStateEnum>(type: "sales_order_state_enum", nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: true),
                    SaleCondition = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("SalesOrders_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "SalesOrders_BranchId_fkey",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "SalesOrders_CustomerId_fkey",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "SalesOrders_CustomerQuoteId_fkey",
                        column: x => x.CustomerQuoteId,
                        principalTable: "CustomerQuotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "SalesOrders_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequestDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseRequestId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PurchaseRequestDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PurchaseRequestDetails_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PurchaseRequestDetails_PurchaseRequestId_fkey",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    PurchaseRequestId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Total = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("SupplierQuotes_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "SupplierQuotes_PurchaseRequestId_fkey",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "SupplierQuotes_SupplierId_fkey",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EntryDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntryId = table.Column<int>(type: "integer", nullable: false),
                    AccountPlanId = table.Column<int>(type: "integer", nullable: false),
                    Debit = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EntryDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "EntryDetails_AccountPlanId_fkey",
                        column: x => x.AccountPlanId,
                        principalTable: "AccountPlans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "EntryDetails_EntryId_fkey",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesOrderId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    QuantityOrdered = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    QuantityInvoiced = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("SalesOrderDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "SalesOrderDetails_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "SalesOrderDetails_SalesOrderId_fkey",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    PurchaseRequestId = table.Column<int>(type: "integer", nullable: false),
                    SupplierQuoteId = table.Column<int>(type: "integer", nullable: true),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Total = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PurchaseOrders_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PurchaseOrders_PurchaseRequestId_fkey",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PurchaseOrders_SupplierId_fkey",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PurchaseOrders_SupplierQuoteId_fkey",
                        column: x => x.SupplierQuoteId,
                        principalTable: "SupplierQuotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierQuoteDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierQuoteId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    QuantityAvailable = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("SupplierQuoteDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "SupplierQuoteDetails_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "SupplierQuoteDetails_SupplierQuoteId_fkey",
                        column: x => x.SupplierQuoteId,
                        principalTable: "SupplierQuotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillType = table.Column<BillTypeEnum>(type: "bill_type_enum", nullable: false),
                    BillState = table.Column<BillStateEnum>(type: "bill_state_enum", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    SalesOrderId = table.Column<int>(type: "integer", nullable: true),
                    PurchaseOrderId = table.Column<int>(type: "integer", nullable: true),
                    Stamp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PaymentTerms = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Total = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    IsCredit = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Bills_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Bills_CustomerId_fkey",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Bills_PurchaseOrderId_fkey",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Bills_SalesOrderId_fkey",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOrderId = table.Column<int>(type: "integer", nullable: false),
                    SupplierQuoteDetailId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    QuantityOrdered = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    QuantityReceived = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    QuantityReturned = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PurchaseOrderDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PurchaseOrderDetails_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PurchaseOrderDetails_PurchaseOrderId_fkey",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PurchaseOrderDetails_SupplierQuoteDetailId_fkey",
                        column: x => x.SupplierQuoteDetailId,
                        principalTable: "SupplierQuoteDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BillDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("BillDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "BillDetails_BillId_fkey",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "BillDetails_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CreditNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Total = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("CreditNotes_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "CreditNotes_BillId_fkey",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentOrderBills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentOrderId = table.Column<int>(type: "integer", nullable: false),
                    BillId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PaymentOrderBills_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PaymentOrderBills_BillId_fkey",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PaymentOrderBills_PaymentOrderId_fkey",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOrderId = table.Column<int>(type: "integer", nullable: false),
                    BillId = table.Column<int>(type: "integer", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    ReasonId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Observation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Total = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PurchaseReturns_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PurchaseReturns_BillId_fkey",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PurchaseReturns_BranchId_fkey",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PurchaseReturns_PurchaseOrderId_fkey",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PurchaseReturns_ReasonId_fkey",
                        column: x => x.ReasonId,
                        principalTable: "PurchaseReturnReasons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CreditNoteDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreditNoteId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("CreditNoteDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "CreditNoteDetails_CreditNoteId_fkey",
                        column: x => x.CreditNoteId,
                        principalTable: "CreditNotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "CreditNoteDetails_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentOrderCreditNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentOrderId = table.Column<int>(type: "integer", nullable: false),
                    CreditNoteId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PaymentOrderCreditNotes_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PaymentOrderCreditNotes_CreditNoteId_fkey",
                        column: x => x.CreditNoteId,
                        principalTable: "CreditNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "PaymentOrderCreditNotes_PaymentOrderId_fkey",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseReturnId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PurchaseReturnDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PurchaseReturnDetails_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PurchaseReturnDetails_PurchaseReturnId_fkey",
                        column: x => x.PurchaseReturnId,
                        principalTable: "PurchaseReturns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    AttendanceTypeId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckIn = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    CheckOut = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    MinutesLate = table.Column<int>(type: "integer", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Attendances_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Attendances_AttendanceTypeId_fkey",
                        column: x => x.AttendanceTypeId,
                        principalTable: "AttendanceTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BossId = table.Column<int>(type: "integer", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Departments_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Departments_BranchId_fkey",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    FileNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AreaId = table.Column<int>(type: "integer", nullable: false),
                    InmediatlyBossId = table.Column<int>(type: "integer", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    HireDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MaritalStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Employees_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "Employees_AreaId_fkey",
                        column: x => x.AreaId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "Employees_BranchId_fkey",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "Employees_EntityId_fkey",
                        column: x => x.EntityId,
                        principalTable: "PhysicalPersons",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "Employees_InmediatlyBossId_fkey",
                        column: x => x.InmediatlyBossId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    RelationType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Lastname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EmployeeRelations_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "EmployeeRelations_EmployeeId_fkey",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PayrollProcessDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PayrollProcessId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    PayrollUpdateId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PayrollProcessDetails_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PayrollProcessDetails_EmployeeId_fkey",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PayrollProcessDetails_PayrollProcessId_fkey",
                        column: x => x.PayrollProcessId,
                        principalTable: "PayrollProcesses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PayrollProcessDetails_PayrollUpdateId_fkey",
                        column: x => x.PayrollUpdateId,
                        principalTable: "PayrollUpdates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PositionByScheduleByEmployee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PositionId = table.Column<int>(type: "integer", nullable: false),
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PositionByScheduleByEmployee_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PositionByScheduleByEmployee_EmployeeId_fkey",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PositionByScheduleByEmployee_PositionId_fkey",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "PositionByScheduleByEmployee_ScheduleId_fkey",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountantProcesses_StateId",
                table: "AccountantProcesses",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "AccountPlans_Code_key",
                table: "AccountPlans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_BankId",
                table: "Accounts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_AttendanceTypeId",
                table: "Attendances",
                column: "AttendanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId",
                table: "Attendances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BankMovements_AccountId",
                table: "BankMovements",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_BillId",
                table: "BillDetails",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillDetails_ProductId",
                table: "BillDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_CustomerId",
                table: "Bills",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_PurchaseOrderId",
                table: "Bills",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_SalesOrderId",
                table: "Bills",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Checks_BankMovementId",
                table: "Checks",
                column: "BankMovementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteDetails_CreditNoteId",
                table: "CreditNoteDetails",
                column: "CreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteDetails_ProductId",
                table: "CreditNoteDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_BillId",
                table: "CreditNotes",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerQuoteDetails_CustomerQuoteId",
                table: "CustomerQuoteDetails",
                column: "CustomerQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerQuoteDetails_ProductId",
                table: "CustomerQuoteDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerQuotes_CustomerId",
                table: "CustomerQuotes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerQuotes_UserId",
                table: "CustomerQuotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "Customers_Ruc_key",
                table: "Customers",
                column: "Ruc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_BossId",
                table: "Departments",
                column: "BossId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_BranchId",
                table: "Departments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRelations_DocumentNumber",
                table: "EmployeeRelations",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRelations_EmployeeId_RelationType",
                table: "EmployeeRelations",
                columns: new[] { "EmployeeId", "RelationType" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRelations_OneActiveSpouse",
                table: "EmployeeRelations",
                column: "EmployeeId",
                unique: true,
                filter: "\"RelationType\" = 1 AND \"EndDate\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_AreaId",
                table: "Employees",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BranchId",
                table: "Employees",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EntityId",
                table: "Employees",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_InmediatlyBossId",
                table: "Employees",
                column: "InmediatlyBossId");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_EntityTypeId",
                table: "Entities",
                column: "EntityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_AccountantProcessId",
                table: "Entries",
                column: "AccountantProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_ModuleId",
                table: "Entries",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryDetails_AccountPlanId",
                table: "EntryDetails",
                column: "AccountPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryDetails_EntryId",
                table: "EntryDetails",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryModelDetails_AccountPlanId",
                table: "EntryModelDetails",
                column: "AccountPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryModelDetails_EntryModelId",
                table: "EntryModelDetails",
                column: "EntryModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderBills_BillId",
                table: "PaymentOrderBills",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderBills_PaymentOrderId",
                table: "PaymentOrderBills",
                column: "PaymentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderCreditNotes_CreditNoteId",
                table: "PaymentOrderCreditNotes",
                column: "CreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderCreditNotes_PaymentOrderId",
                table: "PaymentOrderCreditNotes",
                column: "PaymentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderMovements_BankMovementId",
                table: "PaymentOrderMovements",
                column: "BankMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderMovements_PaymentOrderId",
                table: "PaymentOrderMovements",
                column: "PaymentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_StateId",
                table: "PaymentOrders",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_SupplierId",
                table: "PaymentOrders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollProcessDetails_EmployeeId",
                table: "PayrollProcessDetails",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollProcessDetails_PayrollProcessId",
                table: "PayrollProcessDetails",
                column: "PayrollProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollProcessDetails_PayrollUpdateId",
                table: "PayrollProcessDetails",
                column: "PayrollUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollProcesses_PayrollStatusId",
                table: "PayrollProcesses",
                column: "PayrollStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollProcesses_ProcessTypeId",
                table: "PayrollProcesses",
                column: "ProcessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollUpdates_FormulaTypeId",
                table: "PayrollUpdates",
                column: "FormulaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollUpdates_PayrollTypeId",
                table: "PayrollUpdates",
                column: "PayrollTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_RoleId",
                table: "Permissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalPersons_GenderId",
                table: "PhysicalPersons",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionByScheduleByEmployee_EmployeeId",
                table: "PositionByScheduleByEmployee",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionByScheduleByEmployee_PositionId",
                table: "PositionByScheduleByEmployee",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionByScheduleByEmployee_ScheduleId",
                table: "PositionByScheduleByEmployee",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductBrandId",
                table: "Products",
                column: "ProductBrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCategoryId",
                table: "Products",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_ProductId",
                table: "PurchaseOrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_PurchaseOrderId",
                table: "PurchaseOrderDetails",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_SupplierQuoteDetailId",
                table: "PurchaseOrderDetails",
                column: "SupplierQuoteDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PurchaseRequestId",
                table: "PurchaseOrders",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierQuoteId",
                table: "PurchaseOrders",
                column: "SupplierQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestDetails_ProductId",
                table: "PurchaseRequestDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestDetails_PurchaseRequestId",
                table: "PurchaseRequestDetails",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_UserId",
                table: "PurchaseRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_ProductId",
                table: "PurchaseReturnDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_PurchaseReturnId",
                table: "PurchaseReturnDetails",
                column: "PurchaseReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnReasons_Name",
                table: "PurchaseReturnReasons",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_BillId",
                table: "PurchaseReturns",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_BranchId",
                table: "PurchaseReturns",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_PurchaseOrderId",
                table: "PurchaseReturns",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_ReasonId",
                table: "PurchaseReturns",
                column: "ReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_ProductId",
                table: "SalesOrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_SalesOrderId",
                table: "SalesOrderDetails",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_BranchId",
                table: "SalesOrders",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CustomerId",
                table: "SalesOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CustomerQuoteId",
                table: "SalesOrders",
                column: "CustomerQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_UserId",
                table: "SalesOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ScheduleTypeId",
                table: "Schedules",
                column: "ScheduleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_BranchId",
                table: "Stocks",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ProductId",
                table: "Stocks",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategories_ProductCategoryId",
                table: "SupplierCategories",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategories_SupplierId",
                table: "SupplierCategories",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierQuoteDetails_ProductId",
                table: "SupplierQuoteDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierQuoteDetails_SupplierQuoteId",
                table: "SupplierQuoteDetails",
                column: "SupplierQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierQuotes_PurchaseRequestId",
                table: "SupplierQuotes",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierQuotes_SupplierId",
                table: "SupplierQuotes",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "Suppliers_Ruc_key",
                table: "Suppliers",
                column: "Ruc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "Users_Email_key",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "Attendances_EmployeeId_fkey",
                table: "Attendances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FkDepartmentsBoss",
                table: "Departments",
                column: "BossId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FkDepartmentsBoss",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "BillDetails");

            migrationBuilder.DropTable(
                name: "Checks");

            migrationBuilder.DropTable(
                name: "CreditNoteDetails");

            migrationBuilder.DropTable(
                name: "CustomerQuoteDetails");

            migrationBuilder.DropTable(
                name: "EmployeeRelations");

            migrationBuilder.DropTable(
                name: "EntryDetails");

            migrationBuilder.DropTable(
                name: "EntryModelDetails");

            migrationBuilder.DropTable(
                name: "LegalPersons");

            migrationBuilder.DropTable(
                name: "PaymentOrderBills");

            migrationBuilder.DropTable(
                name: "PaymentOrderCreditNotes");

            migrationBuilder.DropTable(
                name: "PaymentOrderMovements");

            migrationBuilder.DropTable(
                name: "PayrollProcessDetails");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "PositionByScheduleByEmployee");

            migrationBuilder.DropTable(
                name: "PurchaseOrderDetails");

            migrationBuilder.DropTable(
                name: "PurchaseRequestDetails");

            migrationBuilder.DropTable(
                name: "PurchaseReturnDetails");

            migrationBuilder.DropTable(
                name: "SalesOrderDetails");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "SupplierCategories");

            migrationBuilder.DropTable(
                name: "AttendanceTypes");

            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropTable(
                name: "AccountPlans");

            migrationBuilder.DropTable(
                name: "EntryModels");

            migrationBuilder.DropTable(
                name: "CreditNotes");

            migrationBuilder.DropTable(
                name: "BankMovements");

            migrationBuilder.DropTable(
                name: "PaymentOrders");

            migrationBuilder.DropTable(
                name: "PayrollProcesses");

            migrationBuilder.DropTable(
                name: "PayrollUpdates");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "SupplierQuoteDetails");

            migrationBuilder.DropTable(
                name: "PurchaseReturns");

            migrationBuilder.DropTable(
                name: "AccountantProcesses");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "PayrollStatus");

            migrationBuilder.DropTable(
                name: "ProcessTypes");

            migrationBuilder.DropTable(
                name: "FormulaTypes");

            migrationBuilder.DropTable(
                name: "PayrollTypes");

            migrationBuilder.DropTable(
                name: "ScheduleTypes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "PurchaseReturnReasons");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "ProductBrands");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "SalesOrders");

            migrationBuilder.DropTable(
                name: "SupplierQuotes");

            migrationBuilder.DropTable(
                name: "CustomerQuotes");

            migrationBuilder.DropTable(
                name: "PurchaseRequests");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "PhysicalPersons");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Entities");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "EntityTypes");
        }
    }
}
