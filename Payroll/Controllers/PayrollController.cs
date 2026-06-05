using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payroll.DTO;
using Payroll.Services;

namespace Payroll.Controllers
{
    [ApiController]
    [Route("api/payroll")]
    [Produces("application/json")]
    public sealed class PayrollController : ControllerBase
    {
        private readonly IPayrollService _service;

        public PayrollController(IPayrollService service)
            => _service = service;

        // ─────────────────────────────────────────────────────
        // POST /api/payroll/run
        // Trigger payroll for the given month/year.
        // Returns 201 on success, 409 if already exists, 400 on bad input.
        // ─────────────────────────────────────────────────────
        [HttpPost("run")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]   // BONUS
        public async Task<IActionResult> RunPayroll([FromBody] PayrollRunRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (run, error, isDuplicate) =
                await _service.RunPayrollAsync(request.Month, request.Year);

            if (isDuplicate)
                return Conflict(new { message = error });

            if (error is not null)
                return BadRequest(new { message = error });

            return StatusCode(StatusCodes.Status201Created, run);
        }

        // ─────────────────────────────────────────────────────
        // GET /api/payroll/{month}/{year}?page=1&pageSize=10
        // Fetch the saved payroll run with all employee details.
        // Supports optional pagination (BONUS).
        // ─────────────────────────────────────────────────────
        [HttpGet("{month:int}/{year:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPayrollRun(
            int month, int year,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 0)   // 0 = return full run, >0 = paged
        {
            if (pageSize > 0)
            {
                // Paged mode (BONUS)
                var paged = await _service.GetPayrollPagedAsync(month, year, page, pageSize);
                if (paged.TotalCount == 0)
                    return NotFound(new { message = $"No payroll run found for {month:D2}/{year}." });
                return Ok(paged);
            }

            // Full run with details
            var run = await _service.GetPayrollRunAsync(month, year);
            if (run is null)
                return NotFound(new { message = $"No payroll run found for {month:D2}/{year}." });

            return Ok(run);
        }

        // ─────────────────────────────────────────────────────
        // GET /api/payroll/{runId}/slip/{employeeId}
        // Return an individual payslip.
        // ─────────────────────────────────────────────────────
        [HttpGet("{runId:int}/slip/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPayslip(int runId, int employeeId)
        {
            var slip = await _service.GetPayslipAsync(runId, employeeId);
            if (slip is null)
                return NotFound(new
                {
                    message = $"Payslip not found for runId={runId}, employeeId={employeeId}."
                });

            return Ok(slip);
        }
    }
}
