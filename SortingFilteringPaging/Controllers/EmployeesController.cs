using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SortingFilteringPaging.Models;

namespace SortingFilteringPaging.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ListBaseController
    {
        private AppDbContext _context;

        public EmployeesController(
            AppDbContext context, 
            IHttpContextAccessor httpContextAccessor)
            :base(httpContextAccessor)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery]int page, [FromQuery]string lastName)
        {
            int limit = 10;
            var total = await _context.Employees
                .Where(x => x.LastName.Contains(lastName))
                .CountAsync();
            var emps = await _context.Employees
                .Where(x => x.LastName.Contains(lastName) )
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
            this.SetTotalCountHeader(total);
            return Ok(emps);
        }
    }
}
