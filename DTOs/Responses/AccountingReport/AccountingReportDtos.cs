using System;
using System.Collections.Generic;

namespace BackEnd.DTOs.Responses.AccountingReport;

// ==========================================
// LIBRO DIARIO
// ==========================================
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

// ==========================================
// LIBRO MAYOR
// ==========================================
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

// ==========================================
// BALANCE DE SUMAS Y SALDOS
// ==========================================
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
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
}

// ==========================================
// BALANCE GENERAL
// ==========================================
public class BalanceSheetDto
{
    public List<BalanceSheetItemDto> Assets { get; set; } = [];
    public List<BalanceSheetItemDto> Liabilities { get; set; } = [];
    public List<BalanceSheetItemDto> Equity { get; set; } = [];
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

// ==========================================
// BALANCE DE RESULTADOS
// ==========================================
public class IncomeStatementDto
{
    public List<IncomeStatementItemDto> Revenues { get; set; } = [];
    public List<IncomeStatementItemDto> Expenses { get; set; } = [];
    public decimal TotalRevenues { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome { get; set; }
}

public class IncomeStatementItemDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public decimal Balance { get; set; }
    public bool IsAcceptor { get; set; }
}
