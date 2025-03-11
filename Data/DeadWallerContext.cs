using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DeadWaller.Data
{
    public class DeadWallerContext : DbContext
    {
        public DeadWallerContext (DbContextOptions<DeadWallerContext> options)
            : base(options)
        {
        }

        public DbSet<TestClass> TestClass { get; set; } = default!;
    }
}
