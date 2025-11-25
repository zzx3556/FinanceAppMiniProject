using Microsoft.EntityFrameworkCore;
using FinanceApp.Models;

namespace FinanceApp.Data;

public class FinanceDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }

    // 添加无参数构造函数用于依赖注入
    public FinanceDbContext()
    {
    }

    // 添加带参数的构造函数
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // 只有当没有被配置时才使用这里的连接字符串
            var connectionString = "Server=localhost;Database=FinanceApp;Uid=root;Pwd=zmr20050122@;Port=3306;";
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }
}