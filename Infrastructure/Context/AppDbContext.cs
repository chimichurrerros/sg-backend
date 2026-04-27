using System;
using System.Collections.Generic;
using BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Permission> Permissions { get; set; }
    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountPlan> AccountPlans { get; set; }

    public virtual DbSet<AccountType> AccountTypes { get; set; }

    public virtual DbSet<AccountantProcess> AccountantProcesses { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<AttendanceType> AttendanceTypes { get; set; }

    public virtual DbSet<Bank> Banks { get; set; }

    public virtual DbSet<BankMovement> BankMovements { get; set; }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillDetail> BillDetails { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Check> Checks { get; set; }

    public virtual DbSet<CreditNote> CreditNotes { get; set; }

    public virtual DbSet<CreditNoteDetail> CreditNoteDetails { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerQuote> CustomerQuotes { get; set; }

    public virtual DbSet<CustomerQuoteDetail> CustomerQuoteDetails { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeKid> EmployeeKids { get; set; }

    public virtual DbSet<Entity> Entities { get; set; }

    public virtual DbSet<EntityType> EntityTypes { get; set; }

    public virtual DbSet<Entry> Entries { get; set; }

    public virtual DbSet<EntryDetail> EntryDetails { get; set; }

    public virtual DbSet<EntryModel> EntryModels { get; set; }

    public virtual DbSet<EntryModelDetail> EntryModelDetails { get; set; }

    public virtual DbSet<FormulaType> FormulaTypes { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<LegalPerson> LegalPersons { get; set; }

    // public virtual DbSet<Lote> Lotes { get; set; }

    public virtual DbSet<MaritalStatus> MaritalStatuses { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<PaymentOrder> PaymentOrders { get; set; }

    public virtual DbSet<PaymentOrderBill> PaymentOrderBills { get; set; }

    public virtual DbSet<PaymentOrderMovement> PaymentOrderMovements { get; set; }

    public virtual DbSet<PayrollProcess> PayrollProcesses { get; set; }

    public virtual DbSet<PayrollProcessDetail> PayrollProcessDetails { get; set; }

    public virtual DbSet<PayrollStatus> PayrollStatuses { get; set; }

    public virtual DbSet<PayrollType> PayrollTypes { get; set; }

    public virtual DbSet<PayrollUpdate> PayrollUpdates { get; set; }

    public virtual DbSet<PhysicalPerson> PhysicalPersons { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<PositionByScheduleByEmployee> PositionByScheduleByEmployees { get; set; }

    public virtual DbSet<ProcessType> ProcessTypes { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductBrand> ProductBrands { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

    public virtual DbSet<PurchaseRequest> PurchaseRequests { get; set; }

    public virtual DbSet<PurchaseRequestDetail> PurchaseRequestDetails { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SalesOrder> SalesOrders { get; set; }

    public virtual DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<ScheduleType> ScheduleTypes { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<SupplierCategory> SupplierCategories { get; set; }

    public virtual DbSet<SupplierQuote> SupplierQuotes { get; set; }

    public virtual DbSet<SupplierQuoteDetail> SupplierQuoteDetails { get; set; }


    // public virtual DbSet<Transaction> Transactions { get; set; }

    // public virtual DbSet<TransactionDetail> TransactionDetails { get; set; }

    // public virtual DbSet<TransactionType> TransactionTypes { get; set; }

    // public virtual DbSet<Transfer> Transfers { get; set; }

    // public virtual DbSet<TransferDetail> TransferDetails { get; set; }

    //  public virtual DbSet<UnitsOfMeasurement> UnitsOfMeasurements { get; set; }

    public virtual DbSet<User> Users { get; set; }

    // public virtual DbSet<Warehouse> Warehouses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Name=ConnectionStrings:DefaultConnection");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Enums section ***************************************************************************************

        modelBuilder.HasPostgresEnum<CheckTypeEnum>();
        modelBuilder.HasPostgresEnum<CheckStatusEnum>();
        modelBuilder.HasPostgresEnum<BillTypeEnum>();
        modelBuilder.HasPostgresEnum<BankMovementTypeEnum>();

        // *****************************************************************************************************

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Accounts_pkey");

            entity.Property(e => e.AvailableBalance).HasPrecision(15, 2);
            entity.Property(e => e.CurrentBalance).HasPrecision(15, 2);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.AccountType).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.AccountTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Accounts_AccountTypeId_fkey");

            entity.HasOne(d => d.Bank).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.BankId)
                .HasConstraintName("Accounts_BankId_fkey");

        });

        modelBuilder.Entity<AccountPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AccountPlans_pkey");

            entity.HasIndex(e => e.Code, "AccountPlans_Code_key").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<AccountType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AccountTypes_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<AccountantProcess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AccountantProcesses_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.State).WithMany(p => p.AccountantProcesses)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AccountantProcesses_StateId_fkey");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Attendances_pkey");

            entity.Property(e => e.MinutesLate).HasDefaultValue(0);

            entity.HasOne(d => d.AttendanceType).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.AttendanceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Attendances_AttendanceTypeId_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Attendances_EmployeeId_fkey");
        });

        modelBuilder.Entity<AttendanceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AttendanceTypes_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Banks_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<BankMovement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BankMovements_pkey");

            entity.Property(e => e.Amount).HasPrecision(15, 2);
            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.MovementType).HasColumnType("bank_movement_type_enum");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);

            entity.HasOne(d => d.Account).WithMany(p => p.BankMovements)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BankMovements_AccountId_fkey");
        });

        modelBuilder.Entity<Check>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Checks_pkey");

            entity.Property(e => e.Status).HasColumnType("check_status_enum");
            entity.Property(e => e.Type).HasColumnType("check_type_enum");
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Bills_pkey");

            entity.Property(e => e.Number).HasMaxLength(50);
            entity.Property(e => e.PaymentTerms).HasMaxLength(100);
            entity.Property(e => e.Stamp).HasMaxLength(50);
            entity.Property(e => e.TaxTotal).HasPrecision(15, 2);
            entity.Property(e => e.Total).HasPrecision(15, 2);

            entity.HasOne(d => d.Customer).WithMany(p => p.Bills)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Bills_CustomerId_fkey");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.Bills)
                .HasForeignKey(d => d.PurchaseOrderId)
                .HasConstraintName("Bills_PurchaseOrderId_fkey");

            entity.HasOne(d => d.SalesOrder).WithMany(p => p.Bills)
                .HasForeignKey(d => d.SalesOrderId)
                .HasConstraintName("Bills_SalesOrderId_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.Bills)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Bills_StateId_fkey");
        });

        modelBuilder.Entity<BillDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BillDetails_pkey");

            entity.Property(e => e.Price).HasPrecision(15, 2);
            entity.Property(e => e.Quantity).HasPrecision(10, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);

            entity.HasOne(d => d.Bill).WithMany(p => p.BillDetails)
                .HasForeignKey(d => d.BillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BillDetails_BillId_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.BillDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BillDetails_ProductId_fkey");
        });


        modelBuilder.Entity<CreditNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CreditNotes_pkey");

            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Total).HasPrecision(15, 2);

            entity.HasOne(d => d.Bill).WithMany(p => p.CreditNotes)
                .HasForeignKey(d => d.BillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CreditNotes_BillId_fkey");
        });

        modelBuilder.Entity<CreditNoteDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CreditNoteDetails_pkey");

            entity.Property(e => e.Price).HasPrecision(15, 2);
            entity.Property(e => e.Quantity).HasPrecision(10, 2);

            entity.HasOne(d => d.CreditNote).WithMany(p => p.CreditNoteDetails)
                .HasForeignKey(d => d.CreditNoteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CreditNoteDetails_CreditNoteId_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.CreditNoteDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CreditNoteDetails_ProductId_fkey");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Customers_pkey");

            entity.HasIndex(e => e.Ruc, "Customers_Ruc_key").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Ruc).HasMaxLength(20);
        });

        modelBuilder.Entity<CustomerQuote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CustomerQuotes_pkey");

            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Total).HasPrecision(15, 2);

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerQuotes)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CustomerQuotes_CustomerId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.CustomerQuotes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CustomerQuotes_UserId_fkey");
        });

        modelBuilder.Entity<CustomerQuoteDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CustomerQuoteDetails_pkey");

            entity.Property(e => e.Price).HasPrecision(15, 2);
            entity.Property(e => e.Quantity).HasPrecision(10, 2);

            entity.HasOne(d => d.CustomerQuote).WithMany(p => p.CustomerQuoteDetails)
                .HasForeignKey(d => d.CustomerQuoteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CustomerQuoteDetails_CustomerQuoteId_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.CustomerQuoteDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CustomerQuoteDetails_ProductId_fkey");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Departments_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Boss).WithMany(p => p.Departments)
                .HasForeignKey(d => d.BossId)
                .HasConstraintName("FkDepartmentsBoss");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Employees_pkey");

            entity.Property(e => e.FileNumber).HasMaxLength(50);

            entity.HasOne(d => d.Area).WithMany(p => p.Employees)
                .HasForeignKey(d => d.AreaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Employees_AreaId_fkey");

            entity.HasOne(d => d.Entity).WithMany(p => p.Employees)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Employees_EntityId_fkey");

            entity.HasOne(d => d.InmediatlyBoss).WithMany(p => p.InverseInmediatlyBoss)
                .HasForeignKey(d => d.InmediatlyBossId)
                .HasConstraintName("Employees_InmediatlyBossId_fkey");
        });

        modelBuilder.Entity<EmployeeKid>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("EmployeeKids_pkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeKids)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EmployeeKids_EmployeeId_fkey");

            entity.HasOne(d => d.Entity).WithMany(p => p.EmployeeKids)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EmployeeKids_EntityId_fkey");
        });

        modelBuilder.Entity<Entity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Entities_pkey");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.DocumentNumber).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(50);

            entity.HasOne(d => d.EntityType).WithMany(p => p.Entities)
                .HasForeignKey(d => d.EntityTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Entities_EntityTypeId_fkey");
        });

        modelBuilder.Entity<EntityType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("EntityTypes_pkey");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Entry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Entries_pkey");

            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.AccountantProcess).WithMany(p => p.Entries)
                .HasForeignKey(d => d.AccountantProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Entries_AccountantProcessId_fkey");

            entity.HasOne(d => d.Module).WithMany(p => p.Entries)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("Entries_ModuleId_fkey");
        });

        modelBuilder.Entity<EntryDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("EntryDetails_pkey");

            entity.Property(e => e.Credit).HasPrecision(15, 2);
            entity.Property(e => e.Debit).HasPrecision(15, 2);

            entity.HasOne(d => d.AccountPlan).WithMany(p => p.EntryDetails)
                .HasForeignKey(d => d.AccountPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EntryDetails_AccountPlanId_fkey");

            entity.HasOne(d => d.Entry).WithMany(p => p.EntryDetails)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EntryDetails_EntryId_fkey");
        });

        modelBuilder.Entity<EntryModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("EntryModels_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<EntryModelDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("EntryModelDetails_pkey");

            entity.HasOne(d => d.AccountPlan).WithMany(p => p.EntryModelDetails)
                .HasForeignKey(d => d.AccountPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EntryModelDetails_AccountPlanId_fkey");

            entity.HasOne(d => d.EntryModel).WithMany(p => p.EntryModelDetails)
                .HasForeignKey(d => d.EntryModelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("EntryModelDetails_EntryModelId_fkey");
        });

        modelBuilder.Entity<FormulaType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FormulaTypes_pkey");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Genders_pkey");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<LegalPerson>(entity =>
        {
            entity.HasKey(e => e.EntityId).HasName("LegalPersons_pkey");

            entity.Property(e => e.EntityId).ValueGeneratedNever();
            entity.Property(e => e.BussinessName).HasMaxLength(150);
            entity.Property(e => e.FantasyName).HasMaxLength(150);

            entity.HasOne(d => d.Entity).WithOne(p => p.LegalPerson)
                .HasForeignKey<LegalPerson>(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("LegalPersons_EntityId_fkey");
        });

        // modelBuilder.Entity<Lote>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("Lotes_pkey");

        //     entity.Property(e => e.LoteNumber).HasMaxLength(100);

        //     entity.HasOne(d => d.Product).WithMany(p => p.Lotes)
        //         .HasForeignKey(d => d.ProductId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("Lotes_ProductId_fkey");
        // });

        modelBuilder.Entity<MaritalStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("MaritalStatus_pkey");

            entity.ToTable("MaritalStatus");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Modules_pkey");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<PaymentOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PaymentOrders_pkey");

            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Total).HasPrecision(15, 2);

            entity.HasOne(d => d.State).WithMany(p => p.PaymentOrders)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PaymentOrders_StateId_fkey");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PaymentOrders)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PaymentOrders_SupplierId_fkey");
        });

        modelBuilder.Entity<PaymentOrderBill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PaymentOrderBills_pkey");

            entity.Property(e => e.Amount).HasPrecision(15, 2);

            entity.HasOne(d => d.Bill).WithMany(p => p.PaymentOrderBills)
                .HasForeignKey(d => d.BillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PaymentOrderBills_BillId_fkey");

            entity.HasOne(d => d.PaymentOrder).WithMany(p => p.PaymentOrderBills)
                .HasForeignKey(d => d.PaymentOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PaymentOrderBills_PaymentOrderId_fkey");
        });

        modelBuilder.Entity<PaymentOrderMovement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PaymentOrderMovements_pkey");

            entity.Property(e => e.Amount).HasPrecision(15, 2);

            entity.HasOne(d => d.BankMovement).WithMany(p => p.PaymentOrderMovements)
                .HasForeignKey(d => d.BankMovementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PaymentOrderMovements_BankMovementId_fkey");

            entity.HasOne(d => d.PaymentOrder).WithMany(p => p.PaymentOrderMovements)
                .HasForeignKey(d => d.PaymentOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PaymentOrderMovements_PaymentOrderId_fkey");
        });

        modelBuilder.Entity<PayrollProcess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PayrollProcesses_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.PayrollStatus).WithMany(p => p.PayrollProcesses)
                .HasForeignKey(d => d.PayrollStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PayrollProcesses_PayrollStatusId_fkey");

            entity.HasOne(d => d.ProcessType).WithMany(p => p.PayrollProcesses)
                .HasForeignKey(d => d.ProcessTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PayrollProcesses_ProcessTypeId_fkey");
        });

        modelBuilder.Entity<PayrollProcessDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PayrollProcessDetails_pkey");

            entity.Property(e => e.Amount).HasPrecision(15, 2);

            entity.HasOne(d => d.Employee).WithMany(p => p.PayrollProcessDetails)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PayrollProcessDetails_EmployeeId_fkey");

            entity.HasOne(d => d.PayrollProcess).WithMany(p => p.PayrollProcessDetails)
                .HasForeignKey(d => d.PayrollProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PayrollProcessDetails_PayrollProcessId_fkey");

            entity.HasOne(d => d.PayrollUpdate).WithMany(p => p.PayrollProcessDetails)
                .HasForeignKey(d => d.PayrollUpdateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PayrollProcessDetails_PayrollUpdateId_fkey");
        });

        modelBuilder.Entity<PayrollStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PayrollStatus_pkey");

            entity.ToTable("PayrollStatus");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<PayrollType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PayrollTypes_pkey");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<PayrollUpdate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PayrollUpdates_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.FormulaType).WithMany(p => p.PayrollUpdates)
                .HasForeignKey(d => d.FormulaTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PayrollUpdates_FormulaTypeId_fkey");

            entity.HasOne(d => d.PayrollType).WithMany(p => p.PayrollUpdates)
                .HasForeignKey(d => d.PayrollTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PayrollUpdates_PayrollTypeId_fkey");
        });

        modelBuilder.Entity<PhysicalPerson>(entity =>
        {
            entity.HasKey(e => e.EntityId).HasName("PhysicalPersons_pkey");

            entity.Property(e => e.EntityId).ValueGeneratedNever();
            entity.Property(e => e.Lastname).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Entity).WithOne(p => p.PhysicalPerson)
                .HasForeignKey<PhysicalPerson>(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PhysicalPersons_EntityId_fkey");

            entity.HasOne(d => d.Gender).WithMany(p => p.PhysicalPeople)
                .HasForeignKey(d => d.GenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PhysicalPersons_GenderId_fkey");

            entity.HasOne(d => d.MaritalStatus).WithMany(p => p.PhysicalPeople)
                .HasForeignKey(d => d.MaritalStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PhysicalPersons_MaritalStatusId_fkey");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Positions_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<PositionByScheduleByEmployee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PositionByScheduleByEmployee_pkey");

            entity.ToTable("PositionByScheduleByEmployee");

            entity.Property(e => e.BasicSalary).HasPrecision(15, 2);

            entity.HasOne(d => d.Employee).WithMany(p => p.PositionByScheduleByEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PositionByScheduleByEmployee_EmployeeId_fkey");

            entity.HasOne(d => d.Position).WithMany(p => p.PositionByScheduleByEmployees)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PositionByScheduleByEmployee_PositionId_fkey");

            entity.HasOne(d => d.Schedule).WithMany(p => p.PositionByScheduleByEmployees)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PositionByScheduleByEmployee_ScheduleId_fkey");
        });

        modelBuilder.Entity<ProcessType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProcessTypes_pkey");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Products_pkey");

            entity.Property(e => e.Cost).HasPrecision(15, 2);
            entity.Property(e => e.MinimumStock).HasPrecision(10, 2);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Price).HasPrecision(15, 2);

            entity.HasOne(d => d.ProductBrand).WithMany(p => p.Products)
                .HasForeignKey(d => d.ProductBrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Products_ProductBrandId_fkey");

            entity.HasOne(d => d.ProductCategory).WithMany(p => p.Products)
                .HasForeignKey(d => d.ProductCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Products_ProductCategoryId_fkey");
        });

        modelBuilder.Entity<ProductBrand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProductBrands_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProductCategories_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PurchaseOrders_pkey");

            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Number).HasMaxLength(50);
            entity.Property(e => e.Total).HasPrecision(15, 2);

            entity.HasOne(d => d.State).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PurchaseOrders_StateId_fkey");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PurchaseOrders_SupplierId_fkey");

            entity.HasOne(d => d.SupplierQuote).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.SupplierQuoteId)
                .HasConstraintName("PurchaseOrders_SupplierQuoteId_fkey");
        });

        modelBuilder.Entity<PurchaseOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PurchaseOrderDetails_pkey");

            entity.Property(e => e.Price).HasPrecision(15, 2);
            entity.Property(e => e.QuantityOrdered).HasPrecision(10, 2);
            entity.Property(e => e.QuantityReceived).HasPrecision(10, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);

            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PurchaseOrderDetails_ProductId_fkey");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.PurchaseOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PurchaseOrderDetails_PurchaseOrderId_fkey");
        });

        modelBuilder.Entity<PurchaseRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PurchaseRequests_pkey");

            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.State).WithMany(p => p.PurchaseRequests)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PurchaseRequests_StateId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.PurchaseRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PurchaseRequests_UserId_fkey");
        });

        modelBuilder.Entity<PurchaseRequestDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PurchaseRequestDetails_pkey");

            entity.Property(e => e.QuantityRequested).HasPrecision(10, 2);

            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseRequestDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PurchaseRequestDetails_ProductId_fkey");

            entity.HasOne(d => d.PurchaseRequest).WithMany(p => p.PurchaseRequestDetails)
                .HasForeignKey(d => d.PurchaseRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PurchaseRequestDetails_PurchaseRequestId_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Roles_pkey");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SalesOrders_pkey");

            // entity.Property(e => e.Date)
            //     .HasDefaultValueSql("CURRENT_TIMESTAMP")
            //     .HasColumnType("timestamp without time zone");
            // entity.Property(e => e.Number).HasMaxLength(50);
            // entity.Property(e => e.Total).HasPrecision(15, 2);

            entity.HasOne(d => d.Customer).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesOrders_CustomerId_fkey");

            entity.HasOne(d => d.CustomerQuote).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.CustomerQuoteId)
                .HasConstraintName("SalesOrders_CustomerQuoteId_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesOrders_StateId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesOrders_UserId_fkey");
        });

        modelBuilder.Entity<SalesOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SalesOrderDetails_pkey");

            entity.Property(e => e.Price).HasPrecision(15, 2);
            entity.Property(e => e.QuantityInvoiced).HasPrecision(10, 2);
            entity.Property(e => e.QuantityOrdered).HasPrecision(10, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);

            entity.HasOne(d => d.Product).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesOrderDetails_ProductId_fkey");

            entity.HasOne(d => d.SalesOrder).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.SalesOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesOrderDetails_SalesOrderId_fkey");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Schedules_pkey");

            entity.Property(e => e.NumberOfHours).HasPrecision(5, 2);

            entity.HasOne(d => d.ScheduleType).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.ScheduleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Schedules_ScheduleTypeId_fkey");
        });

        modelBuilder.Entity<ScheduleType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ScheduleTypes_pkey");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("States_pkey");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Stocks_pkey");

            entity.Property(e => e.Quantity).HasPrecision(10, 2);

            // entity.HasOne(d => d.Lote).WithMany(p => p.Stocks)
            //     .HasForeignKey(d => d.LoteId)
            //     .OnDelete(DeleteBehavior.ClientSetNull)
            //     .HasConstraintName("Stocks_LoteId_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.Stocks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Stocks_ProductId_fkey");

            entity.HasOne(d => d.Branch).WithMany(p => p.Stocks)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Stocks_BranchId_fkey");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Suppliers_pkey");

            entity.HasOne(d => d.Entity).WithMany(p => p.Suppliers)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Suppliers_EntityId_fkey");
        });

        modelBuilder.Entity<SupplierCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SupplierCategories_pkey");

            entity.HasOne(d => d.ProductCategory).WithMany(p => p.SupplierCategories)
                .HasForeignKey(d => d.ProductCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SupplierCategories_ProductCategoryId_fkey");

            entity.HasOne(d => d.Supplier).WithMany(p => p.SupplierCategories)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SupplierCategories_SupplierId_fkey");
        });

        modelBuilder.Entity<SupplierQuote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SupplierQuotes_pkey");

            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Total).HasPrecision(15, 2);

            entity.HasOne(d => d.PurchaseRequest).WithMany(p => p.SupplierQuotes)
                .HasForeignKey(d => d.PurchaseRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SupplierQuotes_PurchaseRequestId_fkey");

            entity.HasOne(d => d.State).WithMany(p => p.SupplierQuotes)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SupplierQuotes_StateId_fkey");

            entity.HasOne(d => d.Supplier).WithMany(p => p.SupplierQuotes)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SupplierQuotes_SupplierId_fkey");
        });

        modelBuilder.Entity<SupplierQuoteDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SupplierQuoteDetails_pkey");

            entity.Property(e => e.Price).HasPrecision(15, 2);
            entity.Property(e => e.QuantityAvailable).HasPrecision(10, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);

            entity.HasOne(d => d.Product).WithMany(p => p.SupplierQuoteDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SupplierQuoteDetails_ProductId_fkey");

            entity.HasOne(d => d.SupplierQuote).WithMany(p => p.SupplierQuoteDetails)
                .HasForeignKey(d => d.SupplierQuoteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SupplierQuoteDetails_SupplierQuoteId_fkey");
        });

        // modelBuilder.Entity<Transaction>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("Transactions_pkey");

        //     entity.Property(e => e.Date)
        //         .HasDefaultValueSql("CURRENT_TIMESTAMP")
        //         .HasColumnType("timestamp without time zone");

        //     entity.HasOne(d => d.State).WithMany(p => p.Transactions)
        //         .HasForeignKey(d => d.StateId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("Transactions_StateId_fkey");

        //     entity.HasOne(d => d.TransactionType).WithMany(p => p.Transactions)
        //         .HasForeignKey(d => d.TransactionTypeId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("Transactions_TransactionTypeId_fkey");

        //     // entity.HasOne(d => d.Transfer).WithMany(p => p.Transactions)
        //     //     .HasForeignKey(d => d.TransferId)
        //     //     .HasConstraintName("Transactions_TransferId_fkey");

        //     entity.HasOne(d => d.User).WithMany(p => p.Transactions)
        //         .HasForeignKey(d => d.UserId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("Transactions_UserId_fkey");
        // });

        // modelBuilder.Entity<TransactionDetail>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("TransactionDetails_pkey");

        //     entity.Property(e => e.Price).HasPrecision(15, 2);
        //     entity.Property(e => e.Quantity).HasPrecision(10, 2);

        //     entity.HasOne(d => d.Lote).WithMany(p => p.TransactionDetails)
        //         .HasForeignKey(d => d.LoteId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("TransactionDetails_LoteId_fkey");

        //     entity.HasOne(d => d.Product).WithMany(p => p.TransactionDetails)
        //         .HasForeignKey(d => d.ProductId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("TransactionDetails_ProductId_fkey");

        //     entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionDetails)
        //         .HasForeignKey(d => d.TransactionId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("TransactionDetails_TransactionId_fkey");
        // });

        // modelBuilder.Entity<TransactionType>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("TransactionTypes_pkey");

        //     entity.Property(e => e.Name).HasMaxLength(100);
        // });

        // modelBuilder.Entity<Transfer>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("Transfers_pkey");

        //     entity.Property(e => e.ReceiptDate).HasColumnType("timestamp without time zone");
        //     entity.Property(e => e.ShipmentDate).HasColumnType("timestamp without time zone");

        //     entity.HasOne(d => d.DestinationWarehouse).WithMany(p => p.TransferDestinationWarehouses)
        //         .HasForeignKey(d => d.DestinationWarehouseId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("Transfers_DestinationWarehouseId_fkey");

        //     entity.HasOne(d => d.SourceWarehouse).WithMany(p => p.TransferSourceWarehouses)
        //         .HasForeignKey(d => d.SourceWarehouseId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("Transfers_SourceWarehouseId_fkey");

        //     entity.HasOne(d => d.State).WithMany(p => p.Transfers)
        //         .HasForeignKey(d => d.StateId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("Transfers_StateId_fkey");

        //     entity.HasOne(d => d.User).WithMany(p => p.Transfers)
        //         .HasForeignKey(d => d.UserId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("Transfers_UserId_fkey");
        // });

        // modelBuilder.Entity<TransferDetail>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("TransferDetails_pkey");

        //     entity.Property(e => e.Quantity).HasPrecision(10, 2);

        //     entity.HasOne(d => d.Lote).WithMany(p => p.TransferDetails)
        //         .HasForeignKey(d => d.LoteId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("TransferDetails_LoteId_fkey");

        //     entity.HasOne(d => d.Product).WithMany(p => p.TransferDetails)
        //         .HasForeignKey(d => d.ProductId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("TransferDetails_ProductId_fkey");

        //     entity.HasOne(d => d.Transfer).WithMany(p => p.TransferDetails)
        //         .HasForeignKey(d => d.TransferId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("TransferDetails_TransferId_fkey");
        // });

        // modelBuilder.Entity<UnitsOfMeasurement>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("UnitsOfMeasurements_pkey");

        //     entity.Property(e => e.Name).HasMaxLength(50);
        // });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);

            entity.HasOne(d => d.Entity).WithMany(p => p.Users)
                .HasForeignKey(d => d.EntityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Users_EntityId_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Users_RoleId_fkey");
        });

        // modelBuilder.Entity<Warehouse>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("Warehouses_pkey");

        //     entity.Property(e => e.Name).HasMaxLength(100);

        //     entity.HasOne(d => d.Branch).WithMany(p => p.Warehouses)
        //         .HasForeignKey(d => d.BranchId)
        //         .OnDelete(DeleteBehavior.ClientSetNull)
        //         .HasConstraintName("Warehouses_BranchId_fkey");
        // });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
