using Avalonia.Controls;
using Avalonia.Interactivity;
using FinanceApp.ViewModels;
using System;
using System.Globalization;

namespace FinanceApp.Views;

public partial class AddTransactionView : UserControl
{
    private bool _isAmountFocused;
    private string _lastAmountText = "0.00";
    
    public AddTransactionView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }
    
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is AddTransactionViewModel viewModel)
        {
            // 设置初始值
            DatePicker.SelectedDate = viewModel.TransactionDate;
            AmountTextBox.Text = "0.00"; // 初始显示0.00
            TypeTextBox.Text = viewModel.Type;
            CategoryTextBox.Text = viewModel.Category;
            AccountTextBox.Text = viewModel.Account;
            DescriptionTextBox.Text = viewModel.Description;
            
            // 订阅事件
            SubscribeToEvents(viewModel);
        }
    }
    
    private void SubscribeToEvents(AddTransactionViewModel viewModel)
    {
        // 日期变化
        DatePicker.SelectedDateChanged += (_, _) =>
        {
            if (DatePicker.SelectedDate.HasValue)
            {
                viewModel.TransactionDate = DatePicker.SelectedDate.Value;
            }
        };
        
        // 金额焦点事件
        AmountTextBox.GotFocus += (_, _) =>
        {
            _isAmountFocused = true;
            // 如果当前文本是"0.00"，则清空以便用户输入
            if (AmountTextBox.Text == "0.00")
            {
                AmountTextBox.Text = "";
            }
            _lastAmountText = AmountTextBox.Text ?? "0.00";
        };
        
        AmountTextBox.LostFocus += (_, _) =>
        {
            _isAmountFocused = false;
            FormatAmountTextBox(viewModel);
        };
        
        // 金额文本变化 - 只在失去焦点时更新ViewModel
        AmountTextBox.TextChanged += (_, _) =>
        {
            // 只有在失去焦点时才更新ViewModel和格式化
            if (!_isAmountFocused)
            {
                FormatAmountTextBox(viewModel);
            }
        };
        
        // 类型变化
        TypeTextBox.TextChanged += (_, _) =>
        {
            viewModel.Type = TypeTextBox.Text ?? "支出";
        };
        
        // 分类变化
        CategoryTextBox.TextChanged += (_, _) =>
        {
            viewModel.Category = CategoryTextBox.Text ?? string.Empty;
        };
        
        // 账户变化
        AccountTextBox.TextChanged += (_, _) =>
        {
            viewModel.Account = AccountTextBox.Text ?? string.Empty;
        };
        
        // 描述变化
        DescriptionTextBox.TextChanged += (_, _) =>
        {
            viewModel.Description = DescriptionTextBox.Text ?? string.Empty;
        };
        
        // 按钮命令
        CancelButton.Click += (_, _) => viewModel.CancelCommand.Execute(null);
        SaveButton.Click += (_, _) => 
        {
            // 保存前确保金额已格式化
            if (_isAmountFocused)
            {
                FormatAmountTextBox(viewModel);
            }
            
            // 保存前再次确认数据
            Console.WriteLine($"保存前确认 - 类型: {viewModel.Type}, 金额: {viewModel.Amount}, 分类: {viewModel.Category}");
            viewModel.AddTransactionCommand.Execute(null);
        };
        TopRightNavButton.Click += (_, _) => viewModel.NavigateToDashboardCommand.Execute(null);
        
        // 监听 ViewModel 属性变化来更新 UI
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == null) return;
            
            switch (e.PropertyName)
            {
                case nameof(AddTransactionViewModel.TransactionDate):
                    DatePicker.SelectedDate = viewModel.TransactionDate;
                    break;
                case nameof(AddTransactionViewModel.Amount):
                    // 只有当不在编辑状态时才更新UI
                    if (!_isAmountFocused && viewModel.Amount > 0)
                    {
                        AmountTextBox.Text = viewModel.Amount.ToString("F2", CultureInfo.InvariantCulture);
                    }
                    break;
                case nameof(AddTransactionViewModel.Type):
                    TypeTextBox.Text = viewModel.Type;
                    break;
                case nameof(AddTransactionViewModel.Category):
                    CategoryTextBox.Text = viewModel.Category;
                    break;
                case nameof(AddTransactionViewModel.Account):
                    AccountTextBox.Text = viewModel.Account;
                    break;
                case nameof(AddTransactionViewModel.Description):
                    DescriptionTextBox.Text = viewModel.Description;
                    break;
                case nameof(AddTransactionViewModel.DuplicateMessage):
                    // 重复交易信息会自动通过绑定更新
                    break;
            }
        };
    }
    
    private void FormatAmountTextBox(AddTransactionViewModel viewModel)
    {
        string text = AmountTextBox.Text ?? string.Empty;
        
        // 如果为空，设置为0.00
        if (string.IsNullOrWhiteSpace(text))
        {
            AmountTextBox.Text = "0.00";
            viewModel.Amount = 0;
            return;
        }
        
        // 尝试解析为decimal
        if (decimal.TryParse(text, out decimal amount))
        {
            // 确保金额非负
            if (amount < 0) amount = 0;
            
            // 格式化为两位小数
            AmountTextBox.Text = amount.ToString("F2", CultureInfo.InvariantCulture);
            viewModel.Amount = amount;
        }
        else
        {
            // 如果解析失败，恢复上一次的有效值
            AmountTextBox.Text = _lastAmountText;
        }
        
        _lastAmountText = AmountTextBox.Text;
    }
}