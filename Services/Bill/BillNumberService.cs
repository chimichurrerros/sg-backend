using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class BillNumberService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<string> GetNextBillNumber(int branchId)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext(CONCAT('bill_number_seq_', {0})))", branchId);

        var sequence = await _context.BillNumberSequences
            .FirstOrDefaultAsync(s => s.BranchId == branchId);

        if (sequence == null)
        {
            sequence = new BillNumberSequence
            {
                BranchId = branchId,
                LastNumber = 1
            };
            _context.BillNumberSequences.Add(sequence);
        }
        else
        {
            sequence.LastNumber++;
        }

        await _context.SaveChangesAsync();

        return $"{branchId:D3}-001-{sequence.LastNumber:D7}";
    }

    public async Task<string> GetNextCreditNoteNumber(int branchId)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext(CONCAT('credit_note_seq_', {0})))", branchId);

        var sequence = await _context.CreditNoteNumberSequences
            .FirstOrDefaultAsync(s => s.BranchId == branchId);

        if (sequence == null)
        {
            sequence = new CreditNoteNumberSequence
            {
                BranchId = branchId,
                LastNumber = 1
            };
            _context.CreditNoteNumberSequences.Add(sequence);
        }
        else
        {
            sequence.LastNumber++;
        }

        await _context.SaveChangesAsync();

        return $"{branchId:D3}-001-{sequence.LastNumber:D7}";
    }
}
