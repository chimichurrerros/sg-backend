using BackEnd.DTOs.Requests.Pagination;
using System;

namespace BackEnd.DTOs.Requests.PurchaseReturn;

public class PurchaseReturnQueryDto : PaginationRequestDto
{
    public string? Number { get; set; } // CODIGO/NUMBER GENERADO POR EL BACKEND
    public DateTime? Date { get; set; } // FECHA DE DEVOLUCION
    public int? ReasonId { get; set; } // MOTIVO ID
    public string? ReasonName { get; set; } // MOTIVO NOMBRE
    public string? CustomerName { get; set; } // CLIENTE NOMBRE
    public string? SupplierName { get; set; } // PROVEEDOR NOMBRE (ASOCIADO A LA COMPRA)
}
