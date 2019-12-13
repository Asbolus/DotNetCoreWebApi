using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly ContosouniversityContext _context;

        public DepartmentsController(ContosouniversityContext context)
        {
            _context = context;
        }

        // GET: api/Departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartment()
        {
            return await _context.Department.ToListAsync();
        }

        // GET: api/Departments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            var department = await _context.Department.FindAsync(id);

            // 標記刪除後，在 GET 資料的時候不能輸出該筆資料
            if (department == null || department.IsDeleted)
            {
                return NotFound();
            }

            return department;
        }

        // PUT: api/Departments/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        // 針對 Departments 表格的 CUD 操作需用到預存程序
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment(int id, Department department)
        {
            if (id != department.DepartmentId)
            {
                return BadRequest();
            }

            _context.Department.FromSqlRaw("[dbo].[[Department_Update]] @DepartmentID={0}, @Name={1}, @Budget={2}, @StartDate={3}, @InstructorID={4}, @RowVersion_Original={5}", department.DepartmentId, department.Name, department.Budget, department.StartDate, department.InstructorId, department.RowVersion).AsEnumerable().FirstOrDefault();

            // 自動更新該欄位為更新當下的時間
            department.DateModified = DateTime.Now;

            return NoContent();
        }

        // POST: api/Departments
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        // 針對 Departments 表格的 CUD 操作需用到預存程序
        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department)
        {
            _context.Department.FromSqlRaw("[dbo].[Department_Insert] @Name={0}, @Budget={1}, @StartDate={2}, @InstructorID={3}", department.Name, department.Budget, department.StartDate, department.InstructorId).AsEnumerable().FirstOrDefault();

            return CreatedAtAction("GetDepartment", new { id = department.DepartmentId }, department);
        }

        // DELETE: api/Departments/5
        // 針對 Departments 表格的 CUD 操作需用到預存程序
        [HttpDelete("{id}")]
        public async Task<ActionResult<Department>> DeleteDepartment(int id)
        {
            var department = await _context.Department.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            //// 針對 Departments 表格的 CUD 操作需用到預存程序
            //_context.Department.FromSqlRaw("[dbo].[Department_Delete] @DepartmentID={0}, @RowVersion_Original={1}", department.DepartmentId, department.RowVersion).AsEnumerable().FirstOrDefault();

            // 不能真的刪除資料，而是標記刪除即可
            department.IsDeleted = true;

            return department;
        }

        private bool DepartmentExists(int id)
        {
            return _context.Department.Any(e => e.DepartmentId == id);
        }



        // 用 Raw SQL Query 的方式查詢 vwDepartmentCourseCount 檢視表的內容

        [HttpGet("VwDepartmentCourseCount")]
        public async Task<IEnumerable<VwDepartmentCourseCount>> VwDepartmentCourseCount()
        {
            return await _context.VwDepartmentCourseCount.FromSqlRaw("SELECT * FROM [dbo].[vwDepartmentCourseCount] GO").ToListAsync();
        }
    }
}
