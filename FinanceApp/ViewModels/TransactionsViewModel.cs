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

public partial class TransactionsViewModel : ViewModelBase
{
    private readonly ITransactionService? _transactionService;
    private readonly MainWindowViewModel? _mainViewModel;
    private List<Transaction> _allTransactions = new();

    [ObservableProperty]
    private ObservableCollection<Transaction> _transactions = new();

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    // 设计时构造函数
    public TransactionsViewModel()
    {
        // 设计时数据
        Transactions = new ObservableCollection<Transaction>
        {
            new Transaction { Id = 1, Date = DateTimeOffset.Now, Description = "购物", Category = "日常", Amount = 150, Type = "支出", Account = "现金" },
            new Transaction { Id = 2, Date = DateTimeOffset.Now.AddDays(-1), Description = "工资", Category = "收入", Amount = 5000, Type = "收入", Account = "银行卡" }
        };
    }

    // 运行时构造函数
    public TransactionsViewModel(ITransactionService transactionService, MainWindowViewModel mainViewModel)
    {
        _transactionService = transactionService;
        _mainViewModel = mainViewModel;
        _ = LoadTransactionsAsync();
    }

    [RelayCommand]
    private async Task LoadTransactionsAsync()
    {
        if (_transactionService == null) return;

        try
        {
            IsLoading = true;
            _allTransactions = await _transactionService.GetTransactionsAsync();
            ApplySearchFilter();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载交易记录错误: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SearchTransactions()
    {
        ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchKeyword))
        {
            Transactions = new ObservableCollection<Transaction>(_allTransactions);
        }
        else
        {
            var keyword = SearchKeyword.ToLower();
            var filtered = _allTransactions
                .Where(t => 
                    (t.Category?.ToLower().Contains(keyword) ?? false) ||
                    (t.Account?.ToLower().Contains(keyword) ?? false) ||
                    (t.Description?.ToLower().Contains(keyword) ?? false) ||
                    (t.Type?.ToLower().Contains(keyword) ?? false) ||
                    (t.Amount.ToString().Contains(keyword))
                )
                .ToList();
                
            Transactions = new ObservableCollection<Transaction>(filtered);
        }
    }

    [RelayCommand]
    private async Task DeleteTransactionAsync(int id)
    {
        if (_transactionService == null) return;
        
        await _transactionService.DeleteTransactionAsync(id);
        await LoadTransactionsAsync();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        _mainViewModel?.NavigateToDashboardCommand.Execute(null);
    }
}