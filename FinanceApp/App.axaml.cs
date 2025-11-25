using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using FinanceApp.ViewModels;
using FinanceApp.Views;
using FinanceApp.Services;
using FinanceApp.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace FinanceApp;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 配置依赖注入容器
        var services = new ServiceCollection();
        
        // 注册服务
        services.AddSingleton<IUnTestableService, UnTestableService>();
        services.AddScoped<ITransactionService, TransactionService>();
        
        // 注册 DbContext
        services.AddScoped<FinanceDbContext>(provider =>
        {
            var connectionString = "Server=localhost;Database=FinanceApp;Uid=root;Pwd=zmr20050122@;Port=3306;";
            var optionsBuilder = new DbContextOptionsBuilder<FinanceDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            return new FinanceDbContext();
        });
        
        // 注册 ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AddTransactionViewModel>();
        services.AddTransient<TransactionsViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<BillViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        // 设置主窗口
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}