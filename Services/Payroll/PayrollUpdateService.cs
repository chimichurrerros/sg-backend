using System.Globalization;
using System.Text.RegularExpressions;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.PayrollUpdate;
using BackEnd.DTOs.Responses.PayrollUpdate;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class PayrollUpdateService(AppDbContext context, FormulaEvaluatorService formulaEvaluator)
{
    private readonly AppDbContext _context = context;
    private readonly FormulaEvaluatorService _formulaEvaluator = formulaEvaluator;

    public async Task<Result<List<PayrollUpdateResponseDto>>> GetListAsync()
    {
        var payrollUpdates = await _context.PayrollUpdates
            .AsNoTracking()
            .OrderBy(payrollUpdate => payrollUpdate.Id)
            .Select(payrollUpdate => new PayrollUpdateResponseDto
            {
                Id = payrollUpdate.Id,
                PayrollTypeId = (int)payrollUpdate.PayrollTypeId,
                PayrollTypeName = GetPayrollTypeName(payrollUpdate.PayrollTypeId),
                FormulaTypeId = (int)payrollUpdate.FormulaTypeId,
                FormulaTypeName = GetFormulaTypeName(payrollUpdate.FormulaTypeId),
                Name = payrollUpdate.Name,
                Formula = payrollUpdate.Formula,
                IpsDeductible = payrollUpdate.IpsDeductible
            })
            .ToListAsync();

        return Result<List<PayrollUpdateResponseDto>>.Success(payrollUpdates);
    }

    public async Task<Result<PayrollUpdateResponseDto>> CreateAsync(PayrollUpdateCreateDto request)
    {
        var validation = await ValidateCreateRequestAsync(request);
        if (!validation.IsSuccess)
            return Result<PayrollUpdateResponseDto>.Failure(validation.ErrorMessage!, validation.Errors!, ErrorType.Validation);

        var payrollUpdate = new PayrollUpdate
        {
            Name = request.Name.Trim(),
            PayrollTypeId = (PayrollUpdate.PayrollTypeEnum)request.PayrollTypeId,
            FormulaTypeId = (PayrollUpdate.FormulaTypeEnum)request.FormulaTypeId,
            Formula = request.Formula.Trim(),
            IpsDeductible = request.IpsDeductible
        };

        _context.PayrollUpdates.Add(payrollUpdate);
        await _context.SaveChangesAsync();

        var createdPayrollUpdate = await GetByIdAsync(payrollUpdate.Id);
        return Result<PayrollUpdateResponseDto>.Success(createdPayrollUpdate!);
    }

    private async Task<Result> ValidateCreateRequestAsync(PayrollUpdateCreateDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors["Name"] = [PayrollUpdateError.NameRequired];

        if (string.IsNullOrWhiteSpace(request.Formula))
            errors["Formula"] = [PayrollUpdateError.FormulaRequired];

        if (!Enum.IsDefined(typeof(PayrollUpdate.PayrollTypeEnum), request.PayrollTypeId))
            errors["PayrollTypeId"] = [PayrollUpdateError.InvalidPayrollType];

        if (!Enum.IsDefined(typeof(PayrollUpdate.FormulaTypeEnum), request.FormulaTypeId))
            errors["FormulaTypeId"] = [PayrollUpdateError.InvalidFormulaType];

        if (errors.Count == 0)
        {
            var formulaType = (PayrollUpdate.FormulaTypeEnum)request.FormulaTypeId;

            if (formulaType == PayrollUpdate.FormulaTypeEnum.Fixed)
            {
                if (!decimal.TryParse(request.Formula, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                    errors["Formula"] = [PayrollUpdateError.FixedFormulaMustBeNumeric];
            }
            else if (formulaType == PayrollUpdate.FormulaTypeEnum.Calculated)
            {
                if (!TryValidateCalculatedFormula(request.Formula))
                    errors["Formula"] = [PayrollUpdateError.CalculatedFormulaIsInvalid];
            }
        }

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(value => value));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }

    private bool TryValidateCalculatedFormula(string formula)
    {
        try
        {
            var variables = ExtractVariables(formula)
                .ToDictionary(variable => variable, _ => 1m, StringComparer.OrdinalIgnoreCase);

            _formulaEvaluator.EvaluateFormula(formula, variables);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static IEnumerable<string> ExtractVariables(string formula)
    {
        var matches = Regex.Matches(formula, @"\b[A-Za-z_][A-Za-z0-9_]*\b", RegexOptions.CultureInvariant);
        return matches.Select(match => match.Value).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<PayrollUpdateResponseDto?> GetByIdAsync(int id)
    {
        return await _context.PayrollUpdates
            .AsNoTracking()
            .Where(payrollUpdate => payrollUpdate.Id == id)
            .Select(payrollUpdate => new PayrollUpdateResponseDto
            {
                Id = payrollUpdate.Id,
                PayrollTypeId = (int)payrollUpdate.PayrollTypeId,
                PayrollTypeName = GetPayrollTypeName(payrollUpdate.PayrollTypeId),
                FormulaTypeId = (int)payrollUpdate.FormulaTypeId,
                FormulaTypeName = GetFormulaTypeName(payrollUpdate.FormulaTypeId),
                Name = payrollUpdate.Name,
                Formula = payrollUpdate.Formula,
                IpsDeductible = payrollUpdate.IpsDeductible
            })
            .FirstOrDefaultAsync();
    }

    private static string GetPayrollTypeName(PayrollUpdate.PayrollTypeEnum payrollTypeId)
    {
        return payrollTypeId switch
        {
            PayrollUpdate.PayrollTypeEnum.Earnings => "Earnings",
            PayrollUpdate.PayrollTypeEnum.Deductions => "Deductions",
            _ => "Unknown"
        };
    }

    private static string GetFormulaTypeName(PayrollUpdate.FormulaTypeEnum formulaTypeId)
    {
        return formulaTypeId switch
        {
            PayrollUpdate.FormulaTypeEnum.Fixed => "Fixed",
            PayrollUpdate.FormulaTypeEnum.Calculated => "Calculated",
            _ => "Unknown"
        };
    }
}