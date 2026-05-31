using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public partial class SupplierCategory
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public int ProductCategoryId { get; set; }

    public virtual ProductCategory ProductCategory { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;
}
