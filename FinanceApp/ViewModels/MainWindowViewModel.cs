using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceApp.Services;

namespace FinanceApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;

    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    [ObservableProperty]
    private string _currentPageTitle = "财务概览";

    public MainWindowViewModel(ITransactionService transactionService)
    {
        _transactionService = transactionService;
        CurrentViewModel = new DashboardViewModel(_transactionService, this);
    }

    [RelayCommand]
    public void NavigateToDashboard()
    {
        CurrentViewModel = new DashboardViewModel(_transactionService, this);
        CurrentPageTitle = "财务概览";
    }

    [RelayCommand]
    public void NavigateToAddTransaction()
    {
        CurrentViewModel = new AddTransactionViewModel(_transactionService, this);
        CurrentPageTitle = "添加交易";
    }

    [RelayCommand]
    public void NavigateToTransactions()
    {
        CurrentViewModel = new TransactionsViewModel(_transactionService, this);
        CurrentPageTitle = "交易明细";
    }

    [RelayCommand]
    public void NavigateToStatistics()
    {
        CurrentViewModel = new StatisticsViewModel(_transactionService, this);
        CurrentPageTitle = "统计分析";
    }

    [RelayCommand]
    public void NavigateToBills()
    {
        CurrentViewModel = new BillViewModel(_transactionService, this);
        CurrentPageTitle = "年度账单";
    }
}