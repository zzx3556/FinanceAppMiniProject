using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceApp.Models;
using FinanceApp.Services;

namespace FinanceApp.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly ITransactionService? _transactionService;
    private readonly MainWindowViewModel? _mainViewModel;

    [ObservableProperty]
    private decimal _totalIncome;

    [ObservableProperty]
    private decimal _totalExpense;

    [ObservableProperty]
    private decimal _balance;

    [ObservableProperty]
    private DateTime _selectedMonth = DateTime.Now;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _monthDisplay = string.Empty;

    // 设计时构造函数
    public DashboardViewModel()
    {
        // 设计时数据
        TotalIncome = 5000;
        TotalExpense = 3200;
        Balance = 1800;
        MonthDisplay = DateTime.Now.ToString("yyyy年MM月");
    }

    // 运行时构造函数
    public DashboardViewModel(ITransactionService transactionService, MainWindowViewModel mainViewModel)
    {
        _transactionService = transactionService;
        _mainViewModel = mainViewModel;
        
        // 初始化月份显示
        MonthDisplay = SelectedMonth.ToString("yyyy年MM月");
        
        _ = LoadDashboardDataAsync();
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        if (_transactionService == null) return;

        try
        {
            IsLoading = true;

            // 计算所选月份的开始和结束日期
            var startDate = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1); // 当月最后一天

            Console.WriteLine($"加载仪表板数据: {startDate:yyyy-MM-dd} 到 {endDate:yyyy-MM-dd}");

            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);
            
            // 计算总额
            TotalIncome = transactions.Where(t => t.Type == "收入").Sum(t => t.Amount);
            TotalExpense = transactions.Where(t => t.Type == "支出").Sum(t => t.Amount);
            Balance = TotalIncome - TotalExpense;

            // 更新月份显示
            MonthDisplay = SelectedMonth.ToString("yyyy年MM月");

            Console.WriteLine($"数据加载完成: 收入={TotalIncome}, 支出={TotalExpense}, 余额={Balance}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载仪表板错误: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // 当选择的月份改变时重新加载数据
    partial void OnSelectedMonthChanged(DateTime value)
    {
        Console.WriteLine($"月份改变为: {value:yyyy-MM}");
        _ = LoadDashboardDataAsync();
    }

    [RelayCommand]
    private void NavigateToAddTransaction()
    {
        _mainViewModel?.NavigateToAddTransactionCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToTransactions()
    {
        _mainViewModel?.NavigateToTransactionsCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToStatistics()
    {
        _mainViewModel?.NavigateToStatisticsCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToBills()
    {
        _mainViewModel?.NavigateToBillsCommand.Execute(null);
    }
}