using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class Product
{
    public int Id { get; set; }

    public int ProductCategoryId { get; set; }

    public int ProductBrandId { get; set; }
    
    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal Cost { get; set; }

    public decimal MinimumStock { get; set; }

    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();

    public virtual ICollection<CreditNoteDetail> CreditNoteDetails { get; set; } = new List<CreditNoteDetail>();

    public virtual ICollection<CustomerQuoteDetail> CustomerQuoteDetails { get; set; } = new List<CustomerQuoteDetail>();

    public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();

    public virtual ProductBrand ProductBrand { get; set; } = null!;

    public virtual ProductCategory ProductCategory { get; set; } = null!;

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    public virtual ICollection<PurchaseRequestDetail> PurchaseRequestDetails { get; set; } = new List<PurchaseRequestDetail>();

    public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();

    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();

    public virtual ICollection<SupplierQuoteDetail> SupplierQuoteDetails { get; set; } = new List<SupplierQuoteDetail>();

    public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();

    public virtual ICollection<TransferDetail> TransferDetails { get; set; } = new List<TransferDetail>();

    public virtual UnitsOfMeasurement UnitOfMeasurement { get; set; } = null!;
}
