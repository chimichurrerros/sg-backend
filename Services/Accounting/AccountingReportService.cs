using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.DTOs.Responses.AccountingReport;
using BackEnd.Utils;
using BackEnd.Services.Accounting;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

// Servicio encargado de la generación de reportes contables del sistema.
// Proporciona datos para el Libro Diario, Libro Mayor, Balance de Sumas y Saldos, Balance General y Estado de Resultados.
public class AccountingReportService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    // Determina si una cuenta es de naturaleza deudora (aumenta por el Debe, disminuye por el Haber).
    // En el plan de cuentas, los Activos (código que inicia con 1) y los Egresos/Gastos (código que inicia con 4) son deudores.
    private bool IsDebitNature(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return true;
        // Obtenemos los prefijos correspondientes a Activos y Egresos desde el mapeo del plan contable
        string activoPrefix = ((int)AccountantPlanMap.Activos).ToString();
        string egresoPrefix = ((int)AccountantPlanMap.Egresos).ToString();
        // Si el código de la cuenta inicia con el prefijo de Activos o Egresos, es de naturaleza deudora
        return code.StartsWith(activoPrefix) || code.StartsWith(egresoPrefix);
    }

    // Retorna de forma recursiva todas las cuentas descendientes de una cuenta padre que sean imputables (aceptoras de movimientos).
    // En contabilidad, solo se pueden registrar transacciones en cuentas de último nivel (hojas).
    private List<AccountPlan> GetLeafDescendants(int accountId, List<AccountPlan> allAccounts)
    {
        var leaves = new List<AccountPlan>();
        // Agrupamos todas las cuentas por su ID padre para acelerar la búsqueda de hijos en memoria
        var childMap = allAccounts
            .Where(a => a.ParentId.HasValue)
            .GroupBy(a => a.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Función recursiva local para recorrer el árbol del plan de cuentas
        void Traverse(int id)
        {
            var account = allAccounts.FirstOrDefault(a => a.Id == id);
            if (account == null) return;

            // Si la cuenta es imputable (es aceptora de transacciones), la añadimos a la lista de hojas
            if (account.IsAcceptor)
            {
                leaves.Add(account);
            }

            // Si la cuenta actual tiene hijas, llamamos recursivamente a Traverse para cada una de ellas
            if (childMap.TryGetValue(id, out var children))
            {
                foreach (var child in children)
                {
                    Traverse(child.Id);
                }
            }
        }

        Traverse(accountId);
        return leaves;
    }

    // ==========================================
    // 1. LIBRO DIARIO
    // ==========================================
    // Genera el reporte del Libro Diario para un período contable y rango de fechas específicos.
    // Muestra todos los asientos contables registrados de forma cronológica con sus respectivos detalles de Debe y Haber.
    public async Task<Result<JournalBookDto>> GetJournalBookAsync(int accountantProcessId, DateTime? startDate, DateTime? endDate)
    {
        // Iniciamos la consulta sobre la tabla de Asientos (Entries), incluyendo sus detalles y las cuentas contables asociadas
        var query = _context.Entries
            .Include(e => e.EntryDetails)
            .ThenInclude(d => d.AccountPlan)
            .AsNoTracking()
            .Where(e => e.AccountantProcessId == accountantProcessId);

        // Si se define una fecha de inicio, filtramos los asientos con fecha mayor o igual
        if (startDate.HasValue)
        {
            query = query.Where(e => e.Date >= startDate.Value);
        }

        // Si se define una fecha de fin, filtramos los asientos con fecha menor o igual
        if (endDate.HasValue)
        {
            query = query.Where(e => e.Date <= endDate.Value);
        }

        // Ejecutamos la consulta ordenando por el identificador del asiento y luego por fecha
        var entries = await query.OrderBy(e => e.Id).ThenBy(e => e.Date).ToListAsync();

        var journalBook = new JournalBookDto();

        // Mapeamos los datos obtenidos a la estructura de transferencia (DTO) del Libro Diario
        foreach (var entry in entries)
        {
            var journalEntry = new JournalEntryDto
            {
                EntryId = entry.Id,
                Date = entry.Date,
                Description = entry.Description,
                ModuleName = GetModuleName(entry.Module), // Obtenemos el nombre amigable del módulo de origen
                Details = entry.EntryDetails.Select(d => new JournalEntryDetailDto
                {
                    AccountId = d.AccountPlanId,
                    AccountCode = d.AccountPlan.Code,
                    AccountName = d.AccountPlan.Name,
                    Debit = d.Debit,
                    Credit = d.Credit
                }).ToList()
            };

            journalBook.Entries.Add(journalEntry);
        }

        return Result<JournalBookDto>.Success(journalBook);
    }

    // Retorna la representación en texto legible del módulo que generó el asiento contable automático.
    private string GetModuleName(ModuleEnum module)
    {
        return module switch
        {
            ModuleEnum.Sales => "Ventas",
            ModuleEnum.Purchases => "Compras",
            ModuleEnum.Inventory => "Inventario",
            ModuleEnum.Salary => "Sueldos",
            _ => module.ToString()
        };
    }

    // ==========================================
    // 2. LIBRO MAYOR
    // ==========================================
    // Genera el reporte del Libro Mayor para un período, una cuenta opcional y un rango de fechas.
    // Detalla el saldo inicial, los movimientos cronológicos (con saldo acumulado) y el saldo final de cada cuenta.
    public async Task<Result<LedgerBookDto>> GetLedgerBookAsync(int accountantProcessId, int? accountPlanId, DateTime? startDate, DateTime? endDate)
    {
        // Obtenemos todas las cuentas contables asociadas al período de contabilidad actual
        var allAccounts = await _context.AccountPlans
            .AsNoTracking()
            .Where(ap => ap.AccountantProcessId == accountantProcessId)
            .ToListAsync();

        // Si se solicitó una cuenta específica, filtramos la lista; de lo contrario, procesamos todas las cuentas
        var accountsToLedger = allAccounts.AsEnumerable();
        if (accountPlanId.HasValue)
        {
            accountsToLedger = accountsToLedger.Where(ap => ap.Id == accountPlanId.Value);
        }

        var ledgerBook = new LedgerBookDto();

        // Iteramos sobre las cuentas seleccionadas ordenadas por su código contable (estructura jerárquica)
        foreach (var account in accountsToLedger.OrderBy(a => a.Code))
        {
            // Obtenemos las cuentas descendientes de último nivel que pueden registrar movimientos
            var leaves = GetLeafDescendants(account.Id, allAccounts);
            var leafIds = leaves.Select(l => l.Id).ToList();

            // Si la cuenta o sus ramas no tienen cuentas imputables, no hay movimientos para mostrar
            if (!leafIds.Any()) continue;

            // Consultamos todos los detalles de asientos contables asociados a las cuentas hojas identificadas
            var detailsQuery = _context.EntryDetails
                .Include(d => d.Entry)
                .AsNoTracking()
                .Where(d => leafIds.Contains(d.AccountPlanId) && d.Entry.AccountantProcessId == accountantProcessId);

            var allDetails = await detailsQuery.ToListAsync();

            // Inicializamos las listas para clasificar movimientos históricos y del periodo
            var initialDetails = new List<EntryDetail>();
            var movementDetails = allDetails;

            // Si hay filtro de fecha de inicio, dividimos los movimientos:
            // - Menores a la fecha: para calcular el saldo acumulado histórico (Saldo Inicial)
            // - Mayores o iguales: representan las transacciones del periodo consultado
            if (startDate.HasValue)
            {
                initialDetails = allDetails.Where(d => d.Entry.Date < startDate.Value).ToList();
                movementDetails = allDetails.Where(d => d.Entry.Date >= startDate.Value).ToList();
            }

            // Si hay filtro de fecha final, acotamos los movimientos del periodo
            if (endDate.HasValue)
            {
                // Agregamos 1 día a la fecha final para incluir transacciones con hora dentro de ese mismo día
                var endLimit = endDate.Value.Date.AddDays(1);
                movementDetails = movementDetails.Where(d => d.Entry.Date < endLimit).ToList();
            }

            // Sumamos los montos del Debe y Haber del periodo anterior para calcular el saldo de apertura
            decimal initialDebit = initialDetails.Sum(d => d.Debit);
            decimal initialCredit = initialDetails.Sum(d => d.Credit);
            bool isDebit = IsDebitNature(account.Code);
            // Calculamos el saldo inicial según la naturaleza de la cuenta (Deudora: Debe - Haber, Acreedora: Haber - Debe)
            decimal initialBalance = isDebit ? (initialDebit - initialCredit) : (initialCredit - initialDebit);

            var movements = new List<LedgerMovementDto>();
            decimal runningBalance = initialBalance;

            // Ordenamos los movimientos del periodo por ID del asiento y luego por fecha
            var orderedMovements = movementDetails
                .OrderBy(d => d.Entry.Id)
                .ThenBy(d => d.Entry.Date)
                .ToList();

            // Procesamos cada movimiento actualizando el saldo acumulado (runningBalance) y mapeándolo al DTO
            foreach (var detail in orderedMovements)
            {
                if (isDebit)
                {
                    runningBalance += detail.Debit - detail.Credit;
                }
                else
                {
                    runningBalance += detail.Credit - detail.Debit;
                }

                movements.Add(new LedgerMovementDto
                {
                    EntryId = detail.EntryId,
                    Date = detail.Entry.Date,
                    Description = detail.Entry.Description,
                    Debit = detail.Debit,
                    Credit = detail.Credit,
                    RunningBalance = runningBalance
                });
            }

            // Obtenemos los totales acumulados de Debe y Haber de los movimientos mostrados en el reporte
            decimal totalDebit = movementDetails.Sum(d => d.Debit);
            decimal totalCredit = movementDetails.Sum(d => d.Credit);
            decimal finalBalance = runningBalance;

            // Añadimos la cuenta procesada con toda su información al reporte del Libro Mayor
            ledgerBook.Accounts.Add(new LedgerAccountDto
            {
                AccountId = account.Id,
                AccountCode = account.Code,
                AccountName = account.Name,
                InitialBalance = initialBalance,
                Movements = movements,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                FinalBalance = finalBalance
            });
        }

        return Result<LedgerBookDto>.Success(ledgerBook);
    }

    // ==========================================
    // 3. BALANCE DE SUMAS Y SALDOS (TRIAL BALANCE)
    // ==========================================
    // Genera el Balance de Sumas y Saldos (Pre-balance) para comprobar la partida doble.
    // Lista todas las cuentas con la suma total de Debe y Haber del periodo y calcula sus saldos deudores o acreedores.
    public async Task<Result<TrialBalanceDto>> GetTrialBalanceAsync(int accountantProcessId, DateTime? startDate, DateTime? endDate)
    {
        // Obtenemos el catálogo completo de cuentas contables del período
        var allAccounts = await _context.AccountPlans
            .AsNoTracking()
            .Where(ap => ap.AccountantProcessId == accountantProcessId)
            .ToListAsync();

        // Inicializamos la consulta de detalles de asientos contables del período
        var detailsQuery = _context.EntryDetails
            .Include(d => d.Entry)
            .AsNoTracking()
            .Where(d => d.Entry.AccountantProcessId == accountantProcessId);

        // Filtramos por fecha de inicio si se especifica
        if (startDate.HasValue)
        {
            detailsQuery = detailsQuery.Where(d => d.Entry.Date >= startDate.Value);
        }

        // Filtramos por fecha de fin (día completo) si se especifica
        if (endDate.HasValue)
        {
            var endLimit = endDate.Value.Date.AddDays(1);
            detailsQuery = detailsQuery.Where(d => d.Entry.Date < endLimit);
        }

        var details = await detailsQuery.ToListAsync();
        // Agrupamos en memoria los montos de Debe y Haber por el identificador de la cuenta contable
        var detailsByAccount = details.GroupBy(d => d.AccountPlanId)
            .ToDictionary(g => g.Key, g => new { Debit = g.Sum(d => d.Debit), Credit = g.Sum(d => d.Credit) });

        var trialBalance = new TrialBalanceDto();
        var items = new List<TrialBalanceItemDto>();

        // Calculamos las sumas acumuladas para cada cuenta (incluyendo subcuentas) ordenadas jerárquicamente
        foreach (var account in allAccounts.OrderBy(a => a.Code))
        {
            var leaves = GetLeafDescendants(account.Id, allAccounts);
            decimal debitSum = 0;
            decimal creditSum = 0;

            // Consolidamos las sumas de Debe y Haber de todas las cuentas hojas dependientes de la cuenta actual
            foreach (var leaf in leaves)
            {
                if (detailsByAccount.TryGetValue(leaf.Id, out var sum))
                {
                    debitSum += sum.Debit;
                    creditSum += sum.Credit;
                }
            }

            decimal debitBalance = 0;
            decimal creditBalance = 0;

            // Determinamos el saldo resultante (si el Debe supera al Haber es Saldo Deudor, caso contrario Saldo Acreedor)
            if (debitSum > creditSum)
            {
                debitBalance = debitSum - creditSum;
            }
            else if (creditSum > debitSum)
            {
                creditBalance = creditSum - debitSum;
            }

            items.Add(new TrialBalanceItemDto
            {
                AccountId = account.Id,
                AccountCode = account.Code,
                AccountName = account.Name,
                IsAcceptor = account.IsAcceptor,
                DebitSum = debitSum,
                CreditSum = creditSum,
                DebitBalance = debitBalance,
                CreditBalance = creditBalance
            });
        }

        trialBalance.Items = items;

        // Calculamos los totales finales sumando únicamente las cuentas de último nivel (hojas/imputables) para no duplicar montos
        var leafItems = items.Where(i => i.IsAcceptor).ToList();
        trialBalance.TotalDebitSum = leafItems.Sum(i => i.DebitSum);
        trialBalance.TotalCreditSum = leafItems.Sum(i => i.CreditSum);
        trialBalance.TotalDebitBalance = leafItems.Sum(i => i.DebitBalance);
        trialBalance.TotalCreditBalance = leafItems.Sum(i => i.CreditBalance);

        return Result<TrialBalanceDto>.Success(trialBalance);
    }

    // ==========================================
    // 4. BALANCE GENERAL (BALANCE SHEET)
    // ==========================================
    // Genera el Balance General a una fecha determinada, mostrando el estado de la ecuación contable: Activos = Pasivos + Patrimonio.
    public async Task<Result<BalanceSheetDto>> GetBalanceSheetAsync(int accountantProcessId, DateTime? endDate)
    {
        // Obtenemos el catálogo de cuentas para el período
        var allAccounts = await _context.AccountPlans
            .AsNoTracking()
            .Where(ap => ap.AccountantProcessId == accountantProcessId)
            .ToListAsync();

        // Consultamos los detalles de asientos contables del período
        var detailsQuery = _context.EntryDetails
            .Include(d => d.Entry)
            .AsNoTracking()
            .Where(d => d.Entry.AccountantProcessId == accountantProcessId);

        // Filtramos por fecha final si se especifica (incluyendo el día completo)
        if (endDate.HasValue)
        {
            var endLimit = endDate.Value.Date.AddDays(1);
            detailsQuery = detailsQuery.Where(d => d.Entry.Date < endLimit);
        }

        var details = await detailsQuery.ToListAsync();
        // Agrupamos en memoria los montos de Debe y Haber por el identificador de la cuenta contable
        var detailsByAccount = details.GroupBy(d => d.AccountPlanId)
            .ToDictionary(g => g.Key, g => new { Debit = g.Sum(d => d.Debit), Credit = g.Sum(d => d.Credit) });

        var balanceSheet = new BalanceSheetDto();

        string assetPrefix = ((int)AccountantPlanMap.Activos).ToString();
        string liabilityPrefix = ((int)AccountantPlanMap.Pasivos).ToString();
        string equityPrefix = ((int)AccountantPlanMap.PatrimonioNeto).ToString();

        // Procesamos las cuentas correspondientes al Balance General (Activos, Pasivos y Patrimonio)
        foreach (var account in allAccounts.OrderBy(a => a.Code))
        {
            string code = account.Code;

            // Identificamos las cuentas pertenecientes a Activos (1), Pasivos (2) y Patrimonio Neto (3)
            bool isAsset = code.StartsWith(assetPrefix);
            bool isLiability = code.StartsWith(liabilityPrefix);
            bool isEquity = code.StartsWith(equityPrefix);

            // Si la cuenta no pertenece a ninguna de estas clasificaciones, no forma parte del Balance General
            if (!isAsset && !isLiability && !isEquity) continue;

            // Obtenemos las cuentas hojas asociadas y consolidamos sumas
            var leaves = GetLeafDescendants(account.Id, allAccounts);
            decimal debitSum = 0;
            decimal creditSum = 0;

            foreach (var leaf in leaves)
            {
                if (detailsByAccount.TryGetValue(leaf.Id, out var sum))
                {
                    debitSum += sum.Debit;
                    creditSum += sum.Credit;
                }
            }

            // Calculamos el saldo acumulado según la clasificación (Activos: Debe - Haber, Pasivos/Patrimonio: Haber - Debe)
            decimal balance = 0;
            if (isAsset)
            {
                balance = debitSum - creditSum;
            }
            else
            {
                balance = creditSum - debitSum;
            }

            var item = new BalanceSheetItemDto
            {
                AccountId = account.Id,
                AccountCode = account.Code,
                AccountName = account.Name,
                Balance = balance,
                IsAcceptor = account.IsAcceptor
            };

            // Clasificamos y asignamos el registro al grupo correspondiente en el DTO
            if (isAsset)
            {
                balanceSheet.Assets.Add(item);
            }
            else if (isLiability)
            {
                balanceSheet.Liabilities.Add(item);
            }
            else if (isEquity)
            {
                balanceSheet.Equity.Add(item);
            }
        }

        // Calculamos el Resultado del Ejercicio (Ingresos - Egresos) de forma dinámica
        decimal totalRevenues = 0;
        decimal totalExpenses = 0;
        string revenuePrefix = ((int)AccountantPlanMap.Ingresos).ToString();
        string expensePrefix = ((int)AccountantPlanMap.Egresos).ToString();

        foreach (var account in allAccounts.Where(a => a.IsAcceptor))
        {
            string code = account.Code;
            bool isRevenue = code.StartsWith(revenuePrefix);
            bool isExpense = code.StartsWith(expensePrefix);

            if (!isRevenue && !isExpense) continue;

            if (detailsByAccount.TryGetValue(account.Id, out var sum))
            {
                if (isRevenue)
                {
                    totalRevenues += sum.Credit - sum.Debit;
                }
                else if (isExpense)
                {
                    totalExpenses += sum.Debit - sum.Credit;
                }
            }
        }

        decimal netIncome = totalRevenues - totalExpenses;

        // Inyectamos el Resultado del Ejercicio como cuenta virtual en el Patrimonio Neto (código 5.99)
        string netIncomeAccountName = netIncome >= 0 
            ? "Resultado del Ejercicio (Utilidad)" 
            : "Resultado del Ejercicio (Pérdida)";

        var netIncomeItem = new BalanceSheetItemDto
        {
            AccountId = 999999, // ID virtual
            AccountCode = $"{equityPrefix}.99",
            AccountName = netIncomeAccountName,
            Balance = netIncome,
            IsAcceptor = true
        };
        balanceSheet.Equity.Add(netIncomeItem);

        // Calculamos los totales finales consolidando los saldos de las cuentas imputables (hojas) de cada grupo
        balanceSheet.TotalAssets = balanceSheet.Assets.Where(a => a.IsAcceptor).Sum(a => a.Balance);
        balanceSheet.TotalLiabilities = balanceSheet.Liabilities.Where(l => l.IsAcceptor).Sum(l => l.Balance);
        balanceSheet.TotalEquity = balanceSheet.Equity.Where(e => e.IsAcceptor).Sum(e => e.Balance);
        balanceSheet.TotalLiabilitiesAndEquity = balanceSheet.TotalLiabilities + balanceSheet.TotalEquity;

        return Result<BalanceSheetDto>.Success(balanceSheet);
    }

    // ==========================================
    // 5. ESTADO DE RESULTADOS (INCOME STATEMENT)
    // ==========================================
    // Genera el reporte de Estado de Resultados (Ingresos y Egresos) para medir la rentabilidad del periodo: Utilidad/Pérdida = Ingresos - Egresos.
    public async Task<Result<IncomeStatementDto>> GetIncomeStatementAsync(int accountantProcessId, DateTime? startDate, DateTime? endDate)
    {
        // Obtenemos el catálogo de cuentas del período
        var allAccounts = await _context.AccountPlans
            .AsNoTracking()
            .Where(ap => ap.AccountantProcessId == accountantProcessId)
            .ToListAsync();

        // Iniciamos la consulta sobre detalles de asientos contables del período
        var detailsQuery = _context.EntryDetails
            .Include(d => d.Entry)
            .AsNoTracking()
            .Where(d => d.Entry.AccountantProcessId == accountantProcessId);

        // Filtramos por fecha inicial si se define
        if (startDate.HasValue)
        {
            detailsQuery = detailsQuery.Where(d => d.Entry.Date >= startDate.Value);
        }

        // Filtramos por fecha final (día completo) si se define
        if (endDate.HasValue)
        {
            var endLimit = endDate.Value.Date.AddDays(1);
            detailsQuery = detailsQuery.Where(d => d.Entry.Date < endLimit);
        }

        var details = await detailsQuery.ToListAsync();
        // Agrupamos en memoria los montos de Debe y Haber por el identificador de la cuenta contable
        var detailsByAccount = details.GroupBy(d => d.AccountPlanId)
            .ToDictionary(g => g.Key, g => new { Debit = g.Sum(d => d.Debit), Credit = g.Sum(d => d.Credit) });

        var incomeStatement = new IncomeStatementDto();

        // Procesamos las cuentas del Estado de Resultados (Ingresos y Egresos)
        foreach (var account in allAccounts.OrderBy(a => a.Code))
        {
            string code = account.Code;
            string revenuePrefix = ((int)AccountantPlanMap.Ingresos).ToString();
            string expensePrefix = ((int)AccountantPlanMap.Egresos).ToString();

            // Identificamos las cuentas pertenecientes a Ingresos (inician con 3) y Egresos (inician con 4)
            bool isRevenue = code.StartsWith(revenuePrefix);
            bool isExpense = code.StartsWith(expensePrefix);

            // Si la cuenta no forma parte de ingresos ni egresos, pasamos a la siguiente
            if (!isRevenue && !isExpense) continue;

            // Obtenemos las cuentas hojas asociadas y consolidamos sumas
            var leaves = GetLeafDescendants(account.Id, allAccounts);
            decimal debitSum = 0;
            decimal creditSum = 0;

            foreach (var leaf in leaves)
            {
                if (detailsByAccount.TryGetValue(leaf.Id, out var sum))
                {
                    debitSum += sum.Debit;
                    creditSum += sum.Credit;
                }
            }

            // Calculamos el saldo según la clasificación (Ingresos: Haber - Debe, Egresos: Debe - Haber)
            decimal balance = 0;
            if (isRevenue)
            {
                balance = creditSum - debitSum;
            }
            else
            {
                balance = debitSum - creditSum;
            }

            var item = new IncomeStatementItemDto
            {
                AccountId = account.Id,
                AccountCode = account.Code,
                AccountName = account.Name,
                Balance = balance,
                IsAcceptor = account.IsAcceptor
            };

            // Clasificamos y asignamos el registro al grupo correspondiente en el DTO
            if (isRevenue)
            {
                incomeStatement.Revenues.Add(item);
            }
            else if (isExpense)
            {
                incomeStatement.Expenses.Add(item);
            }
        }

        // Calculamos los totales finales consolidando saldos de cuentas imputables y restamos para obtener el Resultado Neto
        incomeStatement.TotalRevenues = incomeStatement.Revenues.Where(r => r.IsAcceptor).Sum(r => r.Balance);
        incomeStatement.TotalExpenses = incomeStatement.Expenses.Where(e => e.IsAcceptor).Sum(e => e.Balance);
        incomeStatement.NetIncome = incomeStatement.TotalRevenues - incomeStatement.TotalExpenses;

        return Result<IncomeStatementDto>.Success(incomeStatement);
    }
}
