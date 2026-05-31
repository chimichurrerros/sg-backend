using System;
using System.Threading.Tasks;
using BackEnd.DTOs.Responses.AccountingReport;
using BackEnd.Extensions;
using BackEnd.Services;
using BackEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers.Accounting;

[Route("api/accounting/reports")]
[ApiController]
[Authorize]
public class AccountingReportsController(AccountingReportService reportService) : ControllerBase
{
    private readonly AccountingReportService _reportService = reportService;

    [HttpGet("libro-diario")]
    public async Task<ActionResult<JournalBookDto>> GetJournalBook(
        [FromQuery] int accountantProcessId, 
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate)
    {
        var result = await _reportService.GetJournalBookAsync(accountantProcessId, startDate, endDate);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return StatusCode(500);
    }

    [HttpGet("libro-mayor")]
    public async Task<ActionResult<LedgerBookDto>> GetLedgerBook(
        [FromQuery] int accountantProcessId, 
        [FromQuery] int? accountPlanId, 
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate)
    {
        var result = await _reportService.GetLedgerBookAsync(accountantProcessId, accountPlanId, startDate, endDate);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return StatusCode(500);
    }

    [HttpGet("balance-general")]
    public async Task<ActionResult<BalanceSheetDto>> GetBalanceSheet(
        [FromQuery] int accountantProcessId, 
        [FromQuery] DateTime? endDate)
    {
        var result = await _reportService.GetBalanceSheetAsync(accountantProcessId, endDate);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return StatusCode(500);
    }

    [HttpGet("balance-sumas-saldos")]
    public async Task<ActionResult<TrialBalanceDto>> GetTrialBalance(
        [FromQuery] int accountantProcessId, 
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate)
    {
        var result = await _reportService.GetTrialBalanceAsync(accountantProcessId, startDate, endDate);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return StatusCode(500);
    }

    [HttpGet("balance-resultados")]
    public async Task<ActionResult<IncomeStatementDto>> GetIncomeStatement(
        [FromQuery] int accountantProcessId, 
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate)
    {
        var result = await _reportService.GetIncomeStatementAsync(accountantProcessId, startDate, endDate);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.ErrorType == ErrorType.NotFound)
            return this.HandleNotFoundProblem(result);

        if (result.ErrorType == ErrorType.Validation)
            return this.HandleBadRequestProblem(result);

        return StatusCode(500);
    }
}
