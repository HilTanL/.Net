using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SportsStore.Models.Respository
{
    public class EFDbContext:DbContext
    {
        public DbSet<Product> Proudcts { get; set; }
    }
}