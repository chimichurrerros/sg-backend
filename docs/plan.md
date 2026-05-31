# Plan de Implementación - Informes Contables

Este plan describe el diseño y la estructura para crear los servicios y controladores requeridos para los reportes básicos de contabilidad:
1. **Libro Diario**
2. **Libro Mayor**
3. **Balance General**
4. **Balance de Sumas y Saldos**
5. **Balance de Resultados**

---

## Estructura Propuesta

Se creará un único controlador de reportes contables (`AccountingReportsController.cs`) y un servicio dedicado (`AccountingReportService.cs`) para encapsular las consultas y lógica financiera. Esto mantiene la base de código ordenada y evita la dispersión de controladores individuales de solo lectura.

### 1. DTOs de Respuesta (Response DTOs)

Definiremos los modelos de datos devueltos por cada reporte en `DTOs/Responses/AccountingReport`:

#### **Libro Diario (General Journal)**
```csharp
public class JournalBookDto
{
    public List<JournalEntryDto> Entries { get; set; } = [];
}

public class JournalEntryDto
{
    public int EntryId { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string ModuleName { get; set; } = null!;
    public List<JournalEntryDetailDto> Details { get; set; } = [];
}

public class JournalEntryDetailDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
```

#### **Libro Mayor (General Ledger)**
```csharp
public class LedgerBookDto
{
    public List<LedgerAccountDto> Accounts { get; set; } = [];
}

public class LedgerAccountDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public decimal InitialBalance { get; set; }
    public List<LedgerMovementDto> Movements { get; set; } = [];
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal FinalBalance { get; set; }
}

public class LedgerMovementDto
{
    public int EntryId { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}
```

#### **Balance de Sumas y Saldos (Trial Balance)**
```csharp
public class TrialBalanceDto
{
    public List<TrialBalanceItemDto> Items { get; set; } = [];
    public decimal TotalDebitSum { get; set; }
    public decimal TotalCreditSum { get; set; }
    public decimal TotalDebitBalance { get; set; }
    public decimal TotalCreditBalance { get; set; }
}

public class TrialBalanceItemDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public bool IsAcceptor { get; set; }
    public decimal DebitSum { get; set; }
    public decimal CreditSum { get; set; }
    public decimal DebitBalance { get; set; } // Si Suma Deber > Suma Haber
    public decimal CreditBalance { get; set; } // Si Suma Haber > Suma Deber
}
```

#### **Balance General (Balance Sheet)**
```csharp
public class BalanceSheetDto
{
    public List<BalanceSheetItemDto> Assets { get; set; } = [];      // Cuentas de Activo (Código empieza con 1)
    public List<BalanceSheetItemDto> Liabilities { get; set; } = []; // Cuentas de Pasivo (Código empieza con 2)
    public List<BalanceSheetItemDto> Equity { get; set; } = [];      // Cuentas de Patrimonio Neto (Código empieza con 3)
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal TotalLiabilitiesAndEquity { get; set; }
}

public class BalanceSheetItemDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public decimal Balance { get; set; }
    public bool IsAcceptor { get; set; }
}
```

#### **Balance de Resultados (Income Statement / P&L)**
```csharp
public class IncomeStatementDto
{
    public List<IncomeStatementItemDto> Revenues { get; set; } = []; // Ingresos (Código empieza con 4)
    public List<IncomeStatementItemDto> Expenses { get; set; } = []; // Egresos (Código empieza con 5)
    public decimal TotalRevenues { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome { get; set; } // Ingresos - Egresos
}

public class IncomeStatementItemDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public decimal Balance { get; set; }
    public bool IsAcceptor { get; set; }
}
```

---

## Cambios Propuestos

### Componente de Contabilidad Backend

#### [NEW] [AccountingReportsController.cs](file:///home/cheemstos/Documents/Proyectos/UNIVERSIDAD/SistemaDeGestion/BackEnd/Controllers/Accounting/AccountingReportsController.cs)
Crear un nuevo controlador `AccountingReportsController` expuesto bajo la ruta `api/accounting/reports`:
- `GET api/accounting/reports/libro-diario?accountantProcessId=X&startDate=Y&endDate=Z`
- `GET api/accounting/reports/libro-mayor?accountantProcessId=X&accountPlanId=A&startDate=Y&endDate=Z`
- `GET api/accounting/reports/balance-general?accountantProcessId=X&endDate=Z`
- `GET api/accounting/reports/balance-sumas-saldos?accountantProcessId=X&startDate=Y&endDate=Z`
- `GET api/accounting/reports/balance-resultados?accountantProcessId=X&startDate=Y&endDate=Z`

#### [NEW] [AccountingReportService.cs](file:///home/cheemstos/Documents/Proyectos/UNIVERSIDAD/SistemaDeGestion/BackEnd/Services/Accounting/AccountingReportService.cs)
Crear un servicio para realizar cálculos financieros y consultas a base de datos.
- **Libro Diario**: Recupera asientos (`Entries` y `EntryDetails`) filtrando por proceso contable y fechas.
- **Libro Mayor**: Recupera asientos agrupándolos por cuenta contable. Calcula saldo inicial (asientos anteriores a `startDate` dentro del periodo), lista los movimientos con saldo acumulado, y calcula el saldo final.
- **Balance de Sumas y Saldos**: Calcula sumas de deber y haber agrupadas por cuenta imputable, luego consolida los montos hacia las cuentas padres según la jerarquía de códigos (ej. la cuenta `1.1` suma todos los saldos de `1.1.*`).
- **Balance General**: Filtra cuentas que empiezan con `1` (Activo), `2` (Pasivo), y `3` (Patrimonio Neto), calcula sus saldos acumulados hasta la fecha especificada de forma jerárquica.
- **Balance de Resultados**: Filtra cuentas de Ingresos (habitualmente código `4`) y Egresos (código `5`), consolida saldos y calcula la utilidad/pérdida neta.

#### [MODIFY] [Program.cs](file:///home/cheemstos/Documents/Proyectos/UNIVERSIDAD/SistemaDeGestion/BackEnd/Program.cs)
Registrar `AccountingReportService` en el contenedor de inyección de dependencias:
```csharp
builder.Services.AddScoped<AccountingReportService>();
```

---

## Plan de Verificación

### Pruebas Manuales
Dado que no hay conexión directa a Nuget externa, verificaremos la compilación localmente utilizando `dotnet build --no-restore` tras agregar los archivos, asegurando la correctitud sintáctica del código C# y la inyección de dependencias.
También podemos validar la estructura JSON esperada simulando o inspeccionando los endpoints a través de pruebas de integración si existen.
