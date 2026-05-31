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

public class AccountingReportService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    private bool IsDebitNature(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return true;
        // Activos (1) y Egresos (4) son de naturaleza deudora (Debit nature)
        string activoPrefix = ((int)AccountantPlanMap.Activos).ToString();
        string egresoPrefix = ((int)AccountantPlanMap.Egresos).ToString();
        return code.StartsWith(activoPrefix) || code.StartsWith(egresoPrefix);
    }

    private List<AccountPlan> GetLeafDescendants(int accountId, List<AccountPlan> allAccounts)
    {
        var leaves = new List<AccountPlan>();
        var childMap = allAccounts
            .Where(a => a.ParentId.HasValue)
            .GroupBy(a => a.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        void Traverse(int id)
        {
            var account = allAccounts.FirstOrDefault(a => a.Id == id);
            if (account == null) return;

            if (account.IsAcceptor)
            {
                leaves.Add(account);
            }

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
    public async Task<Result<JournalBookDto>> GetJournalBookAsync(int accountantProcessId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Entries
            .Include(e => e.EntryDetails)
            .ThenInclude(d => d.AccountPlan)
            .AsNoTracking()
            .Where(e => e.AccountantProcessId == accountantProcessId);

        if (startDate.HasValue)
        {
            query = query.Where(e => e.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.Date <= endDate.Value);
        }

        var entries = await query.OrderBy(e => e.Date).ThenBy(e => e.Id).ToListAsync();

        var journalBook = new JournalBookDto();

        foreach (var entry in entries)
        {
            var journalEntry = new JournalEntryDto
            {
                EntryId = entry.Id,
                Date = entry.Date,
                Description = entry.Description,
                ModuleName = GetModuleName(entry.Module),
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
    public async Task<Result<LedgerBookDto>> GetLedgerBookAsync(int accountantProcessId, int? accountPlanId, DateTime? startDate, DateTime? endDate)
    {
        var allAccounts = await _context.AccountPlans
            .AsNoTracking()
            .Where(ap => ap.AccountantProcessId == accountantProcessId)
            .ToListAsync();

        var accountsToLedger = allAccounts.AsEnumerable();
        if (accountPlanId.HasValue)
        {
            accountsToLedger = accountsToLedger.Where(ap => ap.Id == accountPlanId.Value);
        }

        var ledgerBook = new LedgerBookDto();

        foreach (var account in accountsToLedger.OrderBy(a => a.Code))
        {
            var leaves = GetLeafDescendants(account.Id, allAccounts);
            var leafIds = leaves.Select(l => l.Id).ToList();

            if (!leafIds.Any()) continue;

            var detailsQuery = _context.EntryDetails
                .Include(d => d.Entry)
                .AsNoTracking()
                .Where(d => leafIds.Contains(d.AccountPlanId) && d.Entry.AccountantProcessId == accountantProcessId);

            var allDetails = await detailsQuery.ToListAsync();

            var initialDetails = allDetails;
            var movementDetails = allDetails;

            if (startDate.HasValue)
            {
                initialDetails = allDetails.Where(d => d.Entry.Date < startDate.Value).ToList();
                movementDetails = allDetails.Where(d => d.Entry.Date >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                movementDetails = movementDetails.Where(d => d.Entry.Date <= endDate.Value).ToList();
            }

            decimal initialDebit = initialDetails.Sum(d => d.Debit);
            decimal initialCredit = initialDetails.Sum(d => d.Credit);
            bool isDebit = IsDebitNature(account.Code);
            decimal initialBalance = isDebit ? (initialDebit - initialCredit) : (initialCredit - initialDebit);

            var movements = new List<LedgerMovementDto>();
            decimal runningBalance = initialBalance;

            var orderedMovements = movementDetails
                .OrderBy(d => d.Entry.Date)
                .ThenBy(d => d.Entry.Id)
                .ToList();

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

            decimal totalDebit = movementDetails.Sum(d => d.Debit);
            decimal totalCredit = movementDetails.Sum(d => d.Credit);
            decimal finalBalance = runningBalance;

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
    // 3. BALANCE DE SUMAS Y SALDOS
    // ==========================================
    public async Task<Result<TrialBalanceDto>> GetTrialBalanceAsync(int accountantProcessId, DateTime? startDate, DateTime? endDate)
    {
        var allAccounts = await _context.AccountPlans
            .AsNoTracking()
            .Where(ap => ap.AccountantProcessId == accountantProcessId)
            .ToListAsync();

        var detailsQuery = _context.EntryDetails
            .Include(d => d.Entry)
            .AsNoTracking()
            .Where(d => d.Entry.AccountantProcessId == accountantProcessId);

        if (startDate.HasValue)
        {
            detailsQuery = detailsQuery.Where(d => d.Entry.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            detailsQuery = detailsQuery.Where(d => d.Entry.Date <= endDate.Value);
        }

        var details = await detailsQuery.ToListAsync();
        var detailsByAccount = details.GroupBy(d => d.AccountPlanId)
            .ToDictionary(g => g.Key, g => new { Debit = g.Sum(d => d.Debit), Credit = g.Sum(d => d.Credit) });

        var trialBalance = new TrialBalanceDto();
        var items = new List<TrialBalanceItemDto>();

        foreach (var account in allAccounts.OrderBy(a => a.Code))
        {
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

            decimal debitBalance = 0;
            decimal creditBalance = 0;

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

        var leafItems = items.Where(i => i.IsAcceptor).ToList();
        trialBalance.TotalDebitSum = leafItems.Sum(i => i.DebitSum);
        trialBalance.TotalCreditSum = leafItems.Sum(i => i.CreditSum);
        trialBalance.TotalDebitBalance = leafItems.Sum(i => i.DebitBalance);
        trialBalance.TotalCreditBalance = leafItems.Sum(i => i.CreditBalance);

        return Result<TrialBalanceDto>.Success(trialBalance);
    }

    // ==========================================
    // 4. BALANCE GENERAL
    // ==========================================
    public async Task<Result<BalanceSheetDto>> GetBalanceSheetAsync(int accountantProcessId, DateTime? endDate)
    {
        var allAccounts = await _context.AccountPlans
            .AsNoTracking()
            .Where(ap => ap.AccountantProcessId == accountantProcessId)
            .ToListAsync();

        var detailsQuery = _context.EntryDetails
            .Include(d => d.Entry)
            .AsNoTracking()
            .Where(d => d.Entry.AccountantProcessId == accountantProcessId);

        if (endDate.HasValue)
        {
            detailsQuery = detailsQuery.Where(d => d.Entry.Date <= endDate.Value);
        }

        var details = await detailsQuery.ToListAsync();
        var detailsByAccount = details.GroupBy(d => d.AccountPlanId)
            .ToDictionary(g => g.Key, g => new { Debit = g.Sum(d => d.Debit), Credit = g.Sum(d => d.Credit) });

        var balanceSheet = new BalanceSheetDto();

        foreach (var account in allAccounts.OrderBy(a => a.Code))
        {
            string code = account.Code;
            string assetPrefix = ((int)AccountantPlanMap.Activos).ToString();
            string liabilityPrefix = ((int)AccountantPlanMap.Pasivos).ToString();

            bool isAsset = code.StartsWith(assetPrefix);
            bool isLiabilityOrEquity = code.StartsWith(liabilityPrefix);

            if (!isAsset && !isLiabilityOrEquity) continue;

            bool isEquity = false;
            bool isLiability = false;

            if (isLiabilityOrEquity)
            {
                string lowerName = (account.Name ?? "").ToLower();
                if (lowerName.Contains("patrimonio") || 
                    lowerName.Contains("capital") || 
                    lowerName.Contains("reserva") || 
                    lowerName.Contains("resultado") || 
                    code.StartsWith("2.3") || 
                    code.StartsWith("2.5"))
                {
                    isEquity = true;
                }
                else
                {
                    isLiability = true;
                }
            }

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

        balanceSheet.TotalAssets = balanceSheet.Assets.Where(a => a.IsAcceptor).Sum(a => a.Balance);
        balanceSheet.TotalLiabilities = balanceSheet.Liabilities.Where(l => l.IsAcceptor).Sum(l => l.Balance);
        balanceSheet.TotalEquity = balanceSheet.Equity.Where(e => e.IsAcceptor).Sum(e => e.Balance);
        balanceSheet.TotalLiabilitiesAndEquity = balanceSheet.TotalLiabilities + balanceSheet.TotalEquity;

        return Result<BalanceSheetDto>.Success(balanceSheet);
    }

    // ==========================================
    // 5. BALANCE DE RESULTADOS
    // ==========================================
    public async Task<Result<IncomeStatementDto>> GetIncomeStatementAsync(int accountantProcessId, DateTime? startDate, DateTime? endDate)
    {
        var allAccounts = await _context.AccountPlans
            .AsNoTracking()
            .Where(ap => ap.AccountantProcessId == accountantProcessId)
            .ToListAsync();

        var detailsQuery = _context.EntryDetails
            .Include(d => d.Entry)
            .AsNoTracking()
            .Where(d => d.Entry.AccountantProcessId == accountantProcessId);

        if (startDate.HasValue)
        {
            detailsQuery = detailsQuery.Where(d => d.Entry.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            detailsQuery = detailsQuery.Where(d => d.Entry.Date <= endDate.Value);
        }

        var details = await detailsQuery.ToListAsync();
        var detailsByAccount = details.GroupBy(d => d.AccountPlanId)
            .ToDictionary(g => g.Key, g => new { Debit = g.Sum(d => d.Debit), Credit = g.Sum(d => d.Credit) });

        var incomeStatement = new IncomeStatementDto();

        foreach (var account in allAccounts.OrderBy(a => a.Code))
        {
            string code = account.Code;
            string revenuePrefix = ((int)AccountantPlanMap.Ingresos).ToString();
            string expensePrefix = ((int)AccountantPlanMap.Egresos).ToString();

            bool isRevenue = code.StartsWith(revenuePrefix);
            bool isExpense = code.StartsWith(expensePrefix);

            if (!isRevenue && !isExpense) continue;

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

            if (isRevenue)
            {
                incomeStatement.Revenues.Add(item);
            }
            else if (isExpense)
            {
                incomeStatement.Expenses.Add(item);
            }
        }

        incomeStatement.TotalRevenues = incomeStatement.Revenues.Where(r => r.IsAcceptor).Sum(r => r.Balance);
        incomeStatement.TotalExpenses = incomeStatement.Expenses.Where(e => e.IsAcceptor).Sum(e => e.Balance);
        incomeStatement.NetIncome = incomeStatement.TotalRevenues - incomeStatement.TotalExpenses;

        return Result<IncomeStatementDto>.Success(incomeStatement);
    }
}
