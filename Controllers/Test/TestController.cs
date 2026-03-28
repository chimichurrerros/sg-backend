using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BackEnd.Infrastructure.Authorization;

namespace BackEnd.Controllers.Test;

[Route("api/test")]
[ApiController]
[Authorize]
public class TestController : ControllerBase
{
    [HttpGet]
    [HasPermission("Test.Get")] // Test.Get is a permission in a table name called Permissions
    public IActionResult Get()
    {
        return Ok(new { message = "Tienes el permiso Test.Get" });
    } 
}
