using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeadWallet.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DeadWallet.DAL
{
    public class DeadWalletContext : DbContext
    {
        public DeadWalletContext (DbContextOptions<DeadWalletContext> options)
            : base(options)
        {
        }
        public DbSet<DeadWalletUser> DeadWalletUsers { get; set; }
    }
}
