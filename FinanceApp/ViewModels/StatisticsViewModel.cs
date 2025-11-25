using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceApp.Services;

namespace FinanceApp.ViewModels;

public partial class StatisticsViewModel : ViewModelBase
{
    private readonly ITransactionService? _transactionService;
    private readonly MainWindowViewModel? _mainViewModel;

    [ObservableProperty]
    private Dictionary<string, decimal> _categorySummary = new();

    [ObservableProperty]
    private DateTimeOffset _startDate = DateTimeOffset.Now.AddMonths(-1);

    [ObservableProperty]
    private DateTimeOffset _endDate = DateTimeOffset.Now;

    [ObservableProperty]
    private decimal _maxCategoryValue;

    [ObservableProperty]
    private bool _isLoading;

    // 设计时构造函数
    public StatisticsViewModel()
    {
        // 设计时数据
        CategorySummary = new Dictionary<string, decimal>
        {
            { "日常", 1200 },
            { "餐饮", 800 },
            { "交通", 500 },
            { "娱乐", 300 }
        };
        MaxCategoryValue = 1200;
    }

    // 运行时构造函数
    public StatisticsViewModel(ITransactionService transactionService, MainWindowViewModel mainViewModel)
    {
        _transactionService = transactionService;
        _mainViewModel = mainViewModel;
        _ = LoadStatisticsAsync();
    }

    [RelayCommand]
    private async Task LoadStatisticsAsync()
    {
        if (_transactionService == null) return;

        try
        {
            IsLoading = true;
            CategorySummary = await _transactionService.GetCategorySummaryAsync(StartDate.DateTime, EndDate.DateTime);
            
            // 计算最大值用于进度条
            MaxCategoryValue = CategorySummary.Values.Any() ? CategorySummary.Values.Max() : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载统计错误: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        _mainViewModel?.NavigateToDashboardCommand.Execute(null);
    }
}