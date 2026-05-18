using System;
using System.Collections.Generic;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.PurchaseRequest;

public class PurchaseRequestResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public PurchaseRequestStateEnum PurchaseRequestState { get; set; }
    public string? Observation { get; set; }

    public List<PurchaseRequestDetailResponseDto> Details { get; set; } = new();
}

public class PurchaseRequestWrapperDto
{
    public PurchaseRequestResponseDto PurchaseRequest { get; set; } = null!;
}

public class ListPurchaseRequestsWrapperDto
{
    public List<PurchaseRequestResponseDto> PurchaseRequests { get; set; } = new();
    public Pagination? Pagination { get; set; }
}
