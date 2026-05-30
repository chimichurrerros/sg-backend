using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.CreditNote;
using BackEnd.DTOs.Responses.CreditNote;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class CreditNoteService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<Result<CreditNoteWrapperDto>> CreateAsync(CreateCreditNoteDto request)
    {
        if (request.BillId <= 0)
            return Result<CreditNoteWrapperDto>.Failure(CreditNoteError.BillIdRequired, ErrorType.Validation);

        var bill = await _context.Bills.Include(b => b.BillDetails).FirstOrDefaultAsync(b => b.Id == request.BillId);
        if (bill == null)
            return Result<CreditNoteWrapperDto>.Failure(CreditNoteError.BillNotFound, ErrorType.NotFound);

        if (request.Details == null || request.Details.Count == 0)
            return Result<CreditNoteWrapperDto>.Failure(CreditNoteError.DetailsRequired, ErrorType.Validation);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var creditNote = new CreditNote
            {
                BillId = request.BillId,
                Date = request.Date == default ? DateTime.UtcNow : request.Date,
                Total = request.Total,
                Reason = request.Reason
            };

            _context.CreditNotes.Add(creditNote);
            await _context.SaveChangesAsync();

            foreach (var d in request.Details)
            {
                _context.CreditNoteDetails.Add(new CreditNoteDetail
                {
                    CreditNoteId = creditNote.Id,
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    Price = d.Price
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Map response
            var response = new CreditNoteResponseDto
            {
                Id = creditNote.Id,
                BillId = creditNote.BillId,
                Date = creditNote.Date,
                Total = creditNote.Total,
                Reason = creditNote.Reason,
                Details = creditNote.CreditNoteDetails.Select(d => new CreditNoteDetailResponseDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? string.Empty,
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList()
            };

            return Result<CreditNoteWrapperDto>.Success(new CreditNoteWrapperDto { CreditNote = response });
        }
        catch (Exception ex)
        {
            await _context.Database.RollbackTransactionAsync();
            return Result<CreditNoteWrapperDto>.Failure($"{CreditNoteError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
        }
    }

    public async Task<Result<CreditNoteWrapperDto>> GetByIdAsync(int id)
    {
        var cn = await _context.CreditNotes
            .Include(c => c.CreditNoteDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cn == null)
            return Result<CreditNoteWrapperDto>.Failure(CreditNoteError.CreditNoteNotFound, ErrorType.NotFound);

        var response = new CreditNoteResponseDto
        {
            Id = cn.Id,
            BillId = cn.BillId,
            Date = cn.Date,
            Total = cn.Total,
            Reason = cn.Reason,
            Details = cn.CreditNoteDetails.Select(d => new CreditNoteDetailResponseDto
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ProductName = d.Product?.Name ?? string.Empty,
                Quantity = d.Quantity,
                Price = d.Price
            }).ToList()
        };

        return Result<CreditNoteWrapperDto>.Success(new CreditNoteWrapperDto { CreditNote = response });
    }
}
