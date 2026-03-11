using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DisputeAutomation.DAL.Database
{
    public class DBConfiguration: DbContext
    {
        public DBConfiguration(DbContextOptions<DBConfiguration> options) : base(options)
        { 
        }
    }
}
