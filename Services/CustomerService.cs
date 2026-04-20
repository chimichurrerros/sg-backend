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
        var customersQuery = _context.Customers.AsNoTracking();

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

        try
        {
            var customer = new Customer
            {
                Name = request.Name,
                Ruc = request.Ruc
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var createdCustomer = await _context.Customers
                .Where(c => c.Id == customer.Id)
                .ProjectTo<CustomerWrapperDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return Result<CustomerWrapperDto>.Success(createdCustomer!);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<CustomerWrapperDto>> UpdateAsync(int id, UpdateCustomerRequestDto request)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return Result<CustomerWrapperDto>.Failure(CustomerError.CustomerNotFound, ErrorType.NotFound);

        var validationResult = await ValidateUpdateRequestAsync(request, customer.Id);
        if (!validationResult.IsSuccess)
            return Result<CustomerWrapperDto>.Failure(
                validationResult.ErrorMessage!,
                validationResult.Errors!,
                ErrorType.Validation);

        try
        {
            customer.Name = request.Name;
            customer.Ruc = request.Ruc;

            await _context.SaveChangesAsync();

            var updatedCustomer = await _context.Customers
                .Where(c => c.Id == id)
                .ProjectTo<CustomerWrapperDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return Result<CustomerWrapperDto>.Success(updatedCustomer!);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<Result> ValidateCreateRequestAsync(CreateCustomerRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors[nameof(request.Name)] = [CustomerError.NameRequired];

        if (string.IsNullOrWhiteSpace(request.Ruc))
            errors[nameof(request.Ruc)] = [CustomerError.RucRequired];
        else
        {
            var rucExists = await _context.Customers.AnyAsync(c => c.Ruc == request.Ruc);
            if (rucExists) errors[nameof(request.Ruc)] = [CustomerError.RucAlreadyExists];
        }

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

        if (string.IsNullOrWhiteSpace(request.Name))
            errors[nameof(request.Name)] = [CustomerError.NameRequired];

        if (string.IsNullOrWhiteSpace(request.Ruc))
            errors[nameof(request.Ruc)] = [CustomerError.RucRequired];
        else
        {
            var rucExists = await _context.Customers.AnyAsync(c =>
                c.Ruc == request.Ruc && c.Id != currentEntityId);
            if (rucExists) errors[nameof(request.Ruc)] = [CustomerError.RucAlreadyExists];
        }

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors.Values.SelectMany(v => v));
            return Result.Failure(errorMessage, errors, ErrorType.Validation);
        }

        return Result.Success();
    }
}
