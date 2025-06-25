using M_SAVA_DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Contexts
{
    public class BaseDataContext : DbContext
    {
        public BaseDataContext(DbContextOptions<BaseDataContext> options) : base(options)
        {
            //todo
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //todo
        }
        public DbSet<UserDB> Users { get; set; }
        public DbSet<SavedFileDB> SavedFiles { get; set; }
    }
}
