using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.Customer;
using BackEnd.DTOs.Responses.Customer;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
namespace BackEnd.Services;

public class CustomerService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListCustomersWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var customersQuery = _context.Customers
            .AsNoTracking()
            .Include(c => c.Entity)
                .ThenInclude(e => e.LegalPerson);

        var totalElements = await customersQuery.CountAsync();

        var customers = await customersQuery
            .OrderBy(c => c.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<CustomerResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var paginationData = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListCustomersWrapperDto>.Success(new ListCustomersWrapperDto
        {
            Customers = customers,
            Pagination = paginationData
        });
    }

    public async Task<Result<CustomerWrapperDto>> GetByIdAsync(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .ProjectTo<CustomerWrapperDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (customer == null)
            return Result<CustomerWrapperDto>.Failure(CustomerError.CustomerNotFound, ErrorType.NotFound);

        return Result<CustomerWrapperDto>.Success(customer);
    }

    public async Task<Result<CustomerWrapperDto>> CreateAsync(CreateCustomerRequestDto request)
    {
        var validationResult = await ValidateCreateRequestAsync(request);
        if (!validationResult.IsSuccess)
            return Result<CustomerWrapperDto>.Failure(
                validationResult.ErrorMessage!,
                validationResult.Errors!,
                ErrorType.Validation);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var entity = new Entity
            {
                EntityTypeId = (int)EntityPersonType.Legal,
                DocumentNumber = request.DocumentNumber,
                Phone = request.Phone,
                Address = request.Address,
                Email = request.Email,
                IsActive = request.IsActive
            };

            _context.Entities.Add(entity);
            await _context.SaveChangesAsync();

            var customer = new Customer
            {
                EntityId = entity.Id,
                CreditLimit = request.CreditLimit
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var legalPerson = new LegalPerson
            {
                EntityId = entity.Id,
                BussinessName = request.BusinessName,
                FantasyName = request.FantasyName
            };

            _context.LegalPersons.Add(legalPerson);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var createdCustomer = await _context.Customers
                .Where(c => c.Id == customer.Id)
                .ProjectTo<CustomerWrapperDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return Result<CustomerWrapperDto>.Success(createdCustomer!);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<CustomerWrapperDto>> UpdateAsync(int id, UpdateCustomerRequestDto request)
    {
        var customer = await _context.Customers
            .Include(c => c.Entity)
                .ThenInclude(e => e.LegalPerson)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return Result<CustomerWrapperDto>.Failure(CustomerError.CustomerNotFound, ErrorType.NotFound);

        var validationResult = await ValidateUpdateRequestAsync(request, customer.EntityId);
        if (!validationResult.IsSuccess)
            return Result<CustomerWrapperDto>.Failure(
                validationResult.ErrorMessage!,
                validationResult.Errors!,
                ErrorType.Validation);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            customer.CreditLimit = request.CreditLimit;

            customer.Entity.DocumentNumber = request.DocumentNumber;
            customer.Entity.Phone = request.Phone;
            customer.Entity.Address = request.Address;
            customer.Entity.Email = request.Email;
            customer.Entity.IsActive = request.IsActive;

            if (customer.Entity.LegalPerson == null)
            {
                var legalPerson = new LegalPerson
                {
                    EntityId = customer.Entity.Id,
                    BussinessName = request.BusinessName,
                    FantasyName = request.FantasyName
                };
                _context.LegalPersons.Add(legalPerson);
            }
            else
            {
                customer.Entity.LegalPerson.BussinessName = request.BusinessName;
                customer.Entity.LegalPerson.FantasyName = request.FantasyName;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var updatedCustomer = await _context.Customers
                .Where(c => c.Id == id)
                .ProjectTo<CustomerWrapperDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return Result<CustomerWrapperDto>.Success(updatedCustomer!);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<Result> ValidateCreateRequestAsync(CreateCustomerRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            errors["DocumentNumber"] = [CustomerError.DocumentNumberRequired];
        }
        else
        {
            var documentExists = await _context.Entities.AnyAsync(e => e.DocumentNumber == request.DocumentNumber);
            if (documentExists) errors["DocumentNumber"] = [CustomerError.DocumentNumberAlreadyExists];
        }

        if (string.IsNullOrWhiteSpace(request.BusinessName))
            errors["BusinessName"] = [CustomerError.BusinessNameRequired];

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }

    private async Task<Result> ValidateUpdateRequestAsync(UpdateCustomerRequestDto request, int currentEntityId)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            errors["DocumentNumber"] = [CustomerError.DocumentNumberRequired];
        }
        else
        {
            var documentExists = await _context.Entities.AnyAsync(e =>
                e.DocumentNumber == request.DocumentNumber && e.Id != currentEntityId);
            if (documentExists) errors["DocumentNumber"] = [CustomerError.DocumentNumberAlreadyExists];
        }

        if (string.IsNullOrWhiteSpace(request.BusinessName))
            errors["BusinessName"] = [CustomerError.BusinessNameRequired];

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }
}
