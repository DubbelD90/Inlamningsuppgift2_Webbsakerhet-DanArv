using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Inlamningsuppgift2_Webbsakerhet_DanArv.Models;

namespace Inlamningsuppgift2_Webbsakerhet_DanArv.Data
{
    public class Inlamningsuppgift2_Webbsakerhet_DanArvContext : DbContext
    {
        public Inlamningsuppgift2_Webbsakerhet_DanArvContext (DbContextOptions<Inlamningsuppgift2_Webbsakerhet_DanArvContext> options)
            : base(options)
        {
        }

        public DbSet<Inlamningsuppgift2_Webbsakerhet_DanArv.Models.Comment> Comment { get; set; }
    }
}
