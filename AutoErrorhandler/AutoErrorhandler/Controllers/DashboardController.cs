using AutoErrorhandler.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace AutoErrorhandler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ 1. Total Errors
        [HttpGet("total-errors")]
        public IActionResult GetTotalErrors()
        {
            var total = _context.ErrorDetails.Count();
            return Ok(total);
        }

        // ✅ 2. Errors by Language
        [HttpGet("errors-by-language")]
        public IActionResult GetErrorsByLanguage()
        {
            var data = _context.tbl_AutoErrorHandlers
                .GroupBy(x => x.language)
                .Select(g => new
                {
                    language = g.Key,
                    count = g.Count()
                })
                .ToList();

            return Ok(data);
        }

        // ✅ 3. Errors by Project
        [HttpGet("errors-by-project")]
        public IActionResult GetErrorsByProject()
        {
            var data = _context.tbl_AutoErrorHandlers
                .GroupBy(x => x.ProjectName)
                .Select(g => new
                {
                    project = g.Key,
                    count = g.Count()
                })
                .ToList();

            return Ok(data);
        }

        // ✅ 4. Join Data (IMPORTANT 🔥)
        [HttpGet("error-details")]
        public IActionResult GetErrorDetails()
        {
            var data = _context.ErrorDetails
                .Include(e => e.Request)
                .Select(e => new
                {
                    e.Id,
                    e.Exception,
                    e.description,
                    Project = e.Request.ProjectName,
                    Language = e.Request.language
                })
                .ToList();

            return Ok(data);
        }
        
        [HttpGet("top-exceptions")]
        public IActionResult GetTopExceptions()
        {
            var data = _context.ErrorDetails
                .GroupBy(e => e.Exception)
                .Select(g => new
                {
                    exception = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .Take(5)
                .ToList();

            return Ok(data);
        }
        [HttpGet("errors-by-file")]
        public IActionResult GetErrorsByFile()
        {
            var data = _context.ErrorDetails
                .Include(e => e.Request)
                .GroupBy(e => e.Request.Filename)
                .Select(g => new
                {
                    file = g.Key,
                    count = g.Count()
                })
                .ToList();

            return Ok(data);
        }
        [HttpGet("project-language-matrix")]
        public IActionResult GetProjectLanguageMatrix()
        {
            var data = _context.ErrorDetails
                .Include(e => e.Request)
                .GroupBy(e => new { e.Request.ProjectName, e.Request.language })
                .Select(g => new
                {
                    project = g.Key.ProjectName,
                    language = g.Key.language,
                    count = g.Count()
                })
                .ToList();

            return Ok(data);
        }
        [HttpGet("recent-errors")]
        public IActionResult GetRecentErrors()
        {
            var data = _context.ErrorDetails
                .Include(e => e.Request)
                .OrderByDescending(e => e.Id)
                .Take(10)
                .Select(e => new
                {
                    e.Exception,
                    e.description,
                    Project = e.Request.ProjectName
                })
                .ToList();

            return Ok(data);
        }
    }
}
