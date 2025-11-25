using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinanceApp.Models;
using FinanceApp.Services;

namespace FinanceApp.ViewModels;

public partial class AddTransactionViewModel : ViewModelBase
{
    private readonly ITransactionService? _transactionService;
    private readonly MainWindowViewModel? _mainViewModel;

    [ObservableProperty]
    private DateTimeOffset _transactionDate = DateTimeOffset.Now;

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private string _type = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _account = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    // 重复检测消息
    [ObservableProperty]
    private string _duplicateMessage = "✅ 准备就绪";

    // 设计时构造函数
    public AddTransactionViewModel()
    {
        TransactionDate = DateTimeOffset.Now;
        Amount = 100;
        Category = "餐饮";
        Account = "微信";
        Description = "午餐";
    }

    // 运行时构造函数
    public AddTransactionViewModel(ITransactionService transactionService, MainWindowViewModel mainViewModel)
    {
        _transactionService = transactionService;
        _mainViewModel = mainViewModel;
    }

    [RelayCommand]
    private async Task CheckDuplicateAsync()
    {
        if (_transactionService == null || Amount <= 0) 
        {
            DuplicateMessage = "✅ 准备就绪";
            return;
        }

        // 检查必填字段
        if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(Category) || string.IsNullOrEmpty(Account))
        {
            DuplicateMessage = "ℹ️ 请填写完整信息";
            return;
        }

        var testTransaction = new Transaction
        {
            Amount = Amount,
            Description = Description,
            Date = TransactionDate,
            Type = Type,
            Category = Category,
            Account = Account
        };

        try
        {
            var isDuplicate = await _transactionService.CheckDuplicateTransactionAsync(testTransaction);
            
            if (isDuplicate)
            {
                DuplicateMessage = "⚠️ 检测到与最近交易相似，请确认";
            }
            else
            {
                DuplicateMessage = "✅ 无重复交易，可以保存";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"检测重复交易错误: {ex.Message}");
            DuplicateMessage = "❌ 检测失败，请重试";
        }
    }

    [RelayCommand]
    private async Task AddTransactionAsync()
    {
        if (_transactionService == null) return;
        if (Amount <= 0) 
        {
            DuplicateMessage = "❌ 金额必须大于0";
            return;
        }

        if (Type != "收入" && Type != "支出")
        {
            DuplicateMessage = "❌ 类型必须是'收入'或'支出'";
            return;
        }

        if (string.IsNullOrEmpty(Category) || string.IsNullOrEmpty(Account))
        {
            DuplicateMessage = "❌ 请填写分类和账户";
            return;
        }

        Console.WriteLine($"准备保存交易: 类型={Type}, 金额={Amount}, 分类={Category}, 账户={Account}, 描述={Description}");

        var transaction = new Transaction
        {
            Date = TransactionDate,
            Amount = Amount,
            Type = Type,
            Category = Category,
            Account = Account,
            Description = Description,
            CreatedAt = DateTimeOffset.Now
        };

        try
        {
            await _transactionService.AddTransactionAsync(transaction);
            Console.WriteLine("交易保存成功");
            
            // 清空输入
            Amount = 0;
            Description = string.Empty;
            Category = string.Empty;
            Account = string.Empty;
            Type = string.Empty;
            DuplicateMessage = "✅ 交易保存成功！";
            
            // 2秒后返回仪表板
            _ = Task.Delay(2000).ContinueWith(_ => 
            {
                _mainViewModel?.NavigateToDashboardCommand.Execute(null);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存交易错误: {ex.Message}");
            DuplicateMessage = "❌ 保存失败，请重试";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _mainViewModel?.NavigateToDashboardCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        _mainViewModel?.NavigateToDashboardCommand.Execute(null);
    }

    // 当任何字段变化时重新检测重复
    partial void OnAmountChanged(decimal value)
    {
        _ = CheckDuplicateAsync();
    }

    partial void OnDescriptionChanged(string value)
    {
        _ = CheckDuplicateAsync();
    }

    partial void OnTypeChanged(string value)
    {
        _ = CheckDuplicateAsync();
    }

    partial void OnCategoryChanged(string value)
    {
        _ = CheckDuplicateAsync();
    }

    partial void OnAccountChanged(string value)
    {
        _ = CheckDuplicateAsync();
    }

    partial void OnTransactionDateChanged(DateTimeOffset value)
    {
        _ = CheckDuplicateAsync();
    }
}