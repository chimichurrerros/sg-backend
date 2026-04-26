using AutoMapper;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.CustomerQuote;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Responses.CustomerQuote;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

/// <summary>
/// Service responsible for customer quote business logic.
/// It handles quote lifecycle, validation, and expiration rules.
/// </summary>
public class CustomerQuoteService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private const int QuoteValidityDays = 10;

    /// <summary>
    /// Retrieves all customer quotes without pagination and updates expired quotes before returning data.
    /// </summary>
    /// <returns>List of all customer quotes wrapped in a response DTO.</returns>
    public async Task<Result<ListCustomerQuotesWrapperDto>> GetAllAsync()
    {
        await ExpireQuotesIfNeededAsync();

        var quotes = await _context.CustomerQuotes
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.User)
            .Include(q => q.CustomerQuoteDetails)
                .ThenInclude(d => d.Product)
            .OrderByDescending(q => q.Id)
            .ToListAsync();

        var quoteDtos = _mapper.Map<List<CustomerQuoteResponseDto>>(quotes);

        return Result<ListCustomerQuotesWrapperDto>.Success(new ListCustomerQuotesWrapperDto
        {
            CustomerQuotes = quoteDtos
        });
    }

    /// <summary>
    /// Retrieves a paginated list of customer quotes and updates expired quotes before returning data.
    /// </summary>
    /// <param name="pagination">Pagination parameters (Page and PageSize).</param>
    /// <returns>Paginated list of quotes wrapped with pagination metadata.</returns>
    public async Task<Result<ListCustomerQuotesWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        await ExpireQuotesIfNeededAsync();

        var quotesQuery = _context.CustomerQuotes
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.User)
            .Include(q => q.CustomerQuoteDetails)
                .ThenInclude(d => d.Product);

        var totalElements = await quotesQuery.CountAsync();

        var quotes = await quotesQuery
            .OrderByDescending(q => q.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var quoteDtos = _mapper.Map<List<CustomerQuoteResponseDto>>(quotes);
        var paginationData = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListCustomerQuotesWrapperDto>.Success(new ListCustomerQuotesWrapperDto
        {
            CustomerQuotes = quoteDtos,
            Pagination = paginationData
        });
    }

    /// <summary>
    /// Retrieves one customer quote by its identifier.
    /// </summary>
    /// <param name="id">Quote identifier.</param>
    /// <returns>A wrapped quote or not found error.</returns>
    public async Task<Result<CustomerQuoteWrapperDto>> GetByIdAsync(int id)
    {
        await ExpireQuotesIfNeededAsync();

        var quote = await _context.CustomerQuotes
            .AsNoTracking()
            .Include(q => q.Customer)
            .Include(q => q.User)
            .Include(q => q.CustomerQuoteDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
            return Result<CustomerQuoteWrapperDto>.Failure(CustomerQuoteError.CustomerQuoteNotFound, ErrorType.NotFound);

        return Result<CustomerQuoteWrapperDto>.Success(_mapper.Map<CustomerQuoteWrapperDto>(quote));
    }

    /// <summary>
    /// Creates a new customer quote if the customer does not have another active quote.
    /// Quotes are valid for 10 days.
    /// </summary>
    /// <param name="request">Create request with header and detail lines.</param>
    /// <returns>The created quote or validation/conflict errors.</returns>
    public async Task<Result<CustomerQuoteWrapperDto>> CreateAsync(CreateCustomerQuoteRequestDto request)
    {
        await ExpireQuotesIfNeededAsync(request.CustomerId);

        var validationResult = await ValidateCreateRequestAsync(request);
        if (!validationResult.IsSuccess)
            return Result<CustomerQuoteWrapperDto>.Failure(
                validationResult.ErrorMessage!,
                validationResult.Errors!,
                validationResult.ErrorType);

        var hasOpenQuote = await _context.CustomerQuotes.AnyAsync(q =>
            q.CustomerId == request.CustomerId && q.Status == CustomerQuote.QuoteStatus.Open);

        if (hasOpenQuote)
        {
            var errors = new Dictionary<string, string[]>
            {
                [nameof(request.CustomerId)] = [CustomerQuoteError.ExistingOpenQuote]
            };

            return Result<CustomerQuoteWrapperDto>.Failure(
                CustomerQuoteError.ExistingOpenQuote,
                errors,
                ErrorType.Conflict);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var quote = _mapper.Map<CustomerQuote>(request);
            quote.Date = DateTime.UtcNow;
            quote.Status = CustomerQuote.QuoteStatus.Open;
            quote.Total = CalculateTotal(request.Details);

            _context.CustomerQuotes.Add(quote);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var createdQuote = await _context.CustomerQuotes
                .AsNoTracking()
                .Include(q => q.Customer)
                .Include(q => q.User)
                .Include(q => q.CustomerQuoteDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(q => q.Id == quote.Id);

            return Result<CustomerQuoteWrapperDto>.Success(_mapper.Map<CustomerQuoteWrapperDto>(createdQuote));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Updates an active customer quote.
    /// Expired quotes cannot be updated and a new one must be created.
    /// </summary>
    /// <param name="id">Quote identifier.</param>
    /// <param name="request">Updated quote data.</param>
    /// <returns>Updated quote or validation/not found/conflict errors.</returns>
    public async Task<Result<CustomerQuoteWrapperDto>> UpdateAsync(int id, UpdateCustomerQuoteRequestDto request)
    {
        var quote = await _context.CustomerQuotes
            .Include(q => q.CustomerQuoteDetails)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quote == null)
            return Result<CustomerQuoteWrapperDto>.Failure(CustomerQuoteError.CustomerQuoteNotFound, ErrorType.NotFound);

        await ExpireQuoteIfNeededAsync(quote);

        if (quote.Status == CustomerQuote.QuoteStatus.Expired)
        {
            var errors = new Dictionary<string, string[]>
            {
                [nameof(id)] = [CustomerQuoteError.QuoteExpired]
            };

            return Result<CustomerQuoteWrapperDto>.Failure(
                CustomerQuoteError.QuoteExpired,
                errors,
                ErrorType.Conflict);
        }

        var validationResult = await ValidateUpdateRequestAsync(request, quote.CustomerId, quote.UserId);
        if (!validationResult.IsSuccess)
            return Result<CustomerQuoteWrapperDto>.Failure(
                validationResult.ErrorMessage!,
                validationResult.Errors!,
                validationResult.ErrorType);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            quote.CustomerId = request.CustomerId;
            quote.UserId = request.UserId;
            quote.Total = CalculateTotal(request.Details);

            _context.CustomerQuoteDetails.RemoveRange(quote.CustomerQuoteDetails);

            quote.CustomerQuoteDetails = request.Details
                .Select(d => new CustomerQuoteDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    Price = d.Price
                })
                .ToList();

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var updatedQuote = await _context.CustomerQuotes
                .AsNoTracking()
                .Include(q => q.Customer)
                .Include(q => q.User)
                .Include(q => q.CustomerQuoteDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(q => q.Id == id);

            return Result<CustomerQuoteWrapperDto>.Success(_mapper.Map<CustomerQuoteWrapperDto>(updatedQuote));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<Result> ValidateCreateRequestAsync(CreateCustomerQuoteRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.CustomerId <= 0)
            errors[nameof(request.CustomerId)] = [CustomerQuoteError.CustomerIdRequired];

        if (request.UserId <= 0)
            errors[nameof(request.UserId)] = [CustomerQuoteError.UserIdRequired];

        ValidateDetails(request.Details, errors, nameof(request.Details));

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        var customerExists = await _context.Customers.AnyAsync(c => c.Id == request.CustomerId);
        if (!customerExists)
            errors[nameof(request.CustomerId)] = [CustomerQuoteError.CustomerNotFound];

        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
        if (!userExists)
            errors[nameof(request.UserId)] = [CustomerQuoteError.UserNotFound];

        var productsValidation = await ValidateProductsAsync(request.Details.Select(d => d.ProductId).ToList());
        if (!productsValidation.IsSuccess)
            errors[nameof(request.Details)] = [CustomerQuoteError.InvalidProducts];

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }

    private async Task<Result> ValidateUpdateRequestAsync(UpdateCustomerQuoteRequestDto request, int currentCustomerId, int currentUserId)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.CustomerId <= 0)
            errors[nameof(request.CustomerId)] = [CustomerQuoteError.CustomerIdRequired];

        if (request.UserId <= 0)
            errors[nameof(request.UserId)] = [CustomerQuoteError.UserIdRequired];

        ValidateDetails(request.Details, errors, nameof(request.Details));

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        if (request.CustomerId != currentCustomerId)
        {
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == request.CustomerId);
            if (!customerExists)
                errors[nameof(request.CustomerId)] = [CustomerQuoteError.CustomerNotFound];
        }

        if (request.UserId != currentUserId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
                errors[nameof(request.UserId)] = [CustomerQuoteError.UserNotFound];
        }

        var productsValidation = await ValidateProductsAsync(request.Details.Select(d => d.ProductId).ToList());
        if (!productsValidation.IsSuccess)
            errors[nameof(request.Details)] = [CustomerQuoteError.InvalidProducts];

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }

    private static void ValidateDetails(List<CustomerQuoteDetailRequestDto> details, Dictionary<string, string[]> errors, string fieldName)
    {
        if (details == null || details.Count == 0)
        {
            errors[fieldName] = [CustomerQuoteError.DetailsRequired];
            return;
        }

        if (details.Any(d => d.Quantity <= 0))
            errors[$"{fieldName}.Quantity"] = [CustomerQuoteError.InvalidDetailQuantity];

        if (details.Any(d => d.Price < 0))
            errors[$"{fieldName}.Price"] = [CustomerQuoteError.InvalidDetailPrice];
    }

    private async Task<Result> ValidateProductsAsync(List<int> productIds)
    {
        var distinctProductIds = productIds.Distinct().ToList();

        if (distinctProductIds.Count == 0)
            return Result.Failure(CustomerQuoteError.InvalidProducts, ErrorType.Validation);

        var existingProducts = await _context.Products
            .CountAsync(p => distinctProductIds.Contains(p.Id));

        if (existingProducts != distinctProductIds.Count)
            return Result.Failure(CustomerQuoteError.InvalidProducts, ErrorType.Validation);

        return Result.Success();
    }

    private static decimal CalculateTotal(List<CustomerQuoteDetailRequestDto> details)
    {
        return details.Sum(d => d.Quantity * d.Price);
    }

    private async Task ExpireQuotesIfNeededAsync(int? customerId = null)
    {
        var utcNow = DateTime.UtcNow;
        var openQuotesQuery = _context.CustomerQuotes
            .Where(q => q.Status == CustomerQuote.QuoteStatus.Open);

        if (customerId.HasValue)
            openQuotesQuery = openQuotesQuery.Where(q => q.CustomerId == customerId.Value);

        var quotesToExpire = await openQuotesQuery
            .Where(q => q.Date.AddDays(QuoteValidityDays) < utcNow)
            .ToListAsync();

        if (quotesToExpire.Count == 0)
            return;

        foreach (var quote in quotesToExpire)
            quote.Status = CustomerQuote.QuoteStatus.Expired;

        await _context.SaveChangesAsync();
    }

    private async Task ExpireQuoteIfNeededAsync(CustomerQuote quote)
    {
        if (quote.Status == CustomerQuote.QuoteStatus.Expired)
            return;

        if (quote.Date.AddDays(QuoteValidityDays) >= DateTime.UtcNow)
            return;

        quote.Status = CustomerQuote.QuoteStatus.Expired;
        await _context.SaveChangesAsync();
    }
}