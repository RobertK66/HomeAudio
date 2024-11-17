using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRadioImpl {
    public class MyDataContext : DbContext {
        public MyDataContext(DbContextOptions<MyDataContext> options) : base(options) { }

        public DbSet<WebRadio> WebRadios { get; set; }
    }
}
