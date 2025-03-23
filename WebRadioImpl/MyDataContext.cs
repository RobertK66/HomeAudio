using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRadioImpl {
    public class MyDataContext : DbContext {
        private static int instancecount;
        private ILogger? Log;

        
        public MyDataContext(DbContextOptions<MyDataContext> options, ILogger<MyDataContext>? log) : base(options) {
            Log = log;
            Log?.LogInformation("************** DbContext nr " + instancecount++ + " constructed ************");
        }

        public DbSet<WebRadio> WebRadios { get; set; }
    }
}
