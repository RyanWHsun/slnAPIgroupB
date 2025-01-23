using Microsoft.EntityFrameworkCore;

namespace prjGroupB.Models;
public partial class dbGroupBContext : DbContext
{
    public dbGroupBContext() { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfigurationRoot Config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseSqlServer(Config.GetConnectionString("dbGroupB"));
        }
    }
}

