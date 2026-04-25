using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.BillType;

public class BillTypeResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class BillTypeWrapperDto
{
    public BillTypeResponseDto BillType { get; set; } = null!;
}

public class ListBillTypesWrapperDto
{
    public List<BillTypeResponseDto> BillTypes { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}