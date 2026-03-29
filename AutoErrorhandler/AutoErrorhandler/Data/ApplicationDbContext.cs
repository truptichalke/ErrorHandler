using AutoErrorhandler.Model;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;

namespace AutoErrorhandler.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<AutoErrorHandlerRequest> tbl_AutoErrorHandlers { get; set; }
        public DbSet<ErrorDetail> ErrorDetails { get; set; }
    }
}
