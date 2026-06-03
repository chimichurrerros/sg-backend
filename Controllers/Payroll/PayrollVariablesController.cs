using BackEnd.Constants.Payroll;
using BackEnd.DTOs.Responses.PayrollVariable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Infrastructure.Authorization;
namespace BackEnd.Controllers.Payroll;

[Route("api/payroll-variables")]
[ApiController]
[Authorize]
public class PayrollVariablesController : ControllerBase
{
    [HttpGet]
    [HasPermission("payrollVariables.view")]
    public ActionResult<List<PayrollVariableResponseDto>> GetList()
    {
        return Ok(PayrollVariableCatalog.GetAll());
    }
}
