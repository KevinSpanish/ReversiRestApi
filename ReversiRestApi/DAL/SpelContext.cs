using Microsoft.EntityFrameworkCore;
using ReversiRestApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiRestApi.DAL
{
    public class SpelContext : DbContext
    {
        public SpelContext(DbContextOptions<SpelContext> options) : base(options) { }
        public DbSet<Spel> Spel { get; set; }
    }
}
