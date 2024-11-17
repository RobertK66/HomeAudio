using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRadioImpl {
    internal class DtHomeAudioContextFactory : IDesignTimeDbContextFactory<MyDataContext> {
        public MyDataContext CreateDbContext(string[] args) {
            var optionsBuilder = new DbContextOptionsBuilder<MyDataContext>();
            optionsBuilder.UseSqlServer("Server=THINKP-15\\DEVSERVER;Database=HomeAudio;Trusted_Connection=True;TrustServerCertificate=true;");

            return new MyDataContext(optionsBuilder.Options);
        }
    }


}

