using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceApp.Models;
using FinanceApp.Services;

namespace FinanceApp.ViewModels;

public partial class BillViewModel : ViewModelBase
{
    private readonly ITransactionService? _transactionService;
    private readonly MainWindowViewModel? _mainViewModel;

    [ObservableProperty]
    private int _selectedYear = DateTime.Now.Year;

    [ObservableProperty]
    private decimal _yearlyIncome;

    [ObservableProperty]
    private decimal _yearlyExpense;

    [ObservableProperty]
    private decimal _yearlyBalance;

    [ObservableProperty]
    private ObservableCollection<MonthlySummary> _monthlySummaries = new();

    [ObservableProperty]
    private bool _isLoading;

    public List<int> AvailableYears { get; } = new List<int> 
    { 
        DateTime.Now.Year, 
        DateTime.Now.Year - 1, 
        DateTime.Now.Year - 2 
    };

    // 设计时构造函数
    public BillViewModel()
    {
        // 设计时数据
        YearlyIncome = 50000;
        YearlyExpense = 32000;
        YearlyBalance = 18000;
        
        MonthlySummaries = new ObservableCollection<MonthlySummary>
        {
            new MonthlySummary { Month = "1月", Income = 4200, Expense = 2800, Balance = 1400 },
            new MonthlySummary { Month = "2月", Income = 3800, Expense = 3200, Balance = 600 },
            new MonthlySummary { Month = "3月", Income = 4500, Expense = 2900, Balance = 1600 }
        };
    }

    // 运行时构造函数
    public BillViewModel(ITransactionService transactionService, MainWindowViewModel mainViewModel)
    {
        _transactionService = transactionService;
        _mainViewModel = mainViewModel;
        _ = LoadBillDataAsync();
    }

    [RelayCommand]
    private async Task LoadBillDataAsync()
    {
        if (_transactionService == null) return;

        try
        {
            IsLoading = true;

            // 计算年度的开始和结束日期
            var startDate = new DateTime(SelectedYear, 1, 1);
            var endDate = new DateTime(SelectedYear, 12, 31);

            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);
            
            // 计算年度总额
            YearlyIncome = transactions.Where(t => t.Type == "收入").Sum(t => t.Amount);
            YearlyExpense = transactions.Where(t => t.Type == "支出").Sum(t => t.Amount);
            YearlyBalance = YearlyIncome - YearlyExpense;

            // 计算月度汇总
            CalculateMonthlySummaries(transactions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载年度账单错误: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CalculateMonthlySummaries(List<Transaction> transactions)
    {
        MonthlySummaries.Clear();

        var currentMonth = DateTime.Now.Month;
        var monthsToShow = SelectedYear == DateTime.Now.Year ? currentMonth : 12;

        for (int month = 1; month <= monthsToShow; month++)
        {
            var monthTransactions = transactions.Where(t => t.Date.Month == month).ToList();
            var income = monthTransactions.Where(t => t.Type == "收入").Sum(t => t.Amount);
            var expense = monthTransactions.Where(t => t.Type == "支出").Sum(t => t.Amount);
            var balance = income - expense;

            MonthlySummaries.Add(new MonthlySummary
            {
                Month = $"{month}月",
                Income = income,
                Expense = expense,
                Balance = balance
            });
        }
    }

    // 当选择的年份改变时重新加载数据
    partial void OnSelectedYearChanged(int value)
    {
        _ = LoadBillDataAsync();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        _mainViewModel?.NavigateToDashboardCommand.Execute(null);
    }
}

public class MonthlySummary
{
    public string Month { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Balance { get; set; }
}