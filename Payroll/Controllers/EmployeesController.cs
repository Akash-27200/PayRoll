using Microsoft.AspNetCore.Mvc;
using Payroll.Services;

namespace Payroll.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeesController(IEmployeeService service)
            => _service = service;

        /// <summary>Returns all active employees.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _service.GetAllEmployeesAsync();
            return Ok(employees);
        }
    }
}
