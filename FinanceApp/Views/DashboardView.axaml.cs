using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using FinanceApp.ViewModels;

namespace FinanceApp.Views;

public partial class DashboardView : UserControl
{
    private DashboardViewModel? _viewModel;
    
    public DashboardView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel viewModel)
        {
            _viewModel = viewModel;
            SubscribeToEvents(viewModel);
            UpdateUI();
        }
    }
    
    private void SubscribeToEvents(DashboardViewModel viewModel)
    {
        // 月份选择器变化事件
        MonthPicker.SelectedDateChanged += (s, e) =>
        {
            if (MonthPicker.SelectedDate.HasValue && _viewModel != null)
            {
                _viewModel.SelectedMonth = MonthPicker.SelectedDate.Value.DateTime;
                // 手动触发数据加载
                _viewModel.LoadDashboardDataCommand.Execute(null);
            }
        };
        
        // 导航按钮事件
        AddTransactionButton.Click += (s, e) => _viewModel?.NavigateToAddTransactionCommand.Execute(null);
        TransactionsButton.Click += (s, e) => _viewModel?.NavigateToTransactionsCommand.Execute(null);
        StatisticsButton.Click += (s, e) => _viewModel?.NavigateToStatisticsCommand.Execute(null);
        BillsButton.Click += (s, e) => _viewModel?.NavigateToBillsCommand.Execute(null);
        
        // 重新加载数据按钮
        ReloadButton.Click += (s, e) => _viewModel?.LoadDashboardDataCommand.Execute(null);
        
        // 监听 ViewModel 属性变化
        viewModel.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(DashboardViewModel.TotalIncome):
                case nameof(DashboardViewModel.TotalExpense):
                case nameof(DashboardViewModel.Balance):
                case nameof(DashboardViewModel.MonthDisplay):
                case nameof(DashboardViewModel.IsLoading):
                    UpdateUI();
                    break;
                case nameof(DashboardViewModel.SelectedMonth):
                    // 确保 UI 中的日期选择器与 ViewModel 同步
                    if (MonthPicker.SelectedDate != _viewModel?.SelectedMonth)
                    {
                        MonthPicker.SelectedDate = _viewModel?.SelectedMonth;
                    }
                    break;
            }
        };
    }
    
    private void UpdateUI()
    {
        if (_viewModel == null) return;
        
        // 更新月份显示
        MonthDisplayText.Text = _viewModel.MonthDisplay;
        
        // 更新财务数据
        IncomeText.Text = $"¥{_viewModel.TotalIncome:F2}";
        ExpenseText.Text = $"¥{_viewModel.TotalExpense:F2}";
        BalanceText.Text = $"¥{_viewModel.Balance:F2}";
        
        // 更新余额颜色
        BalanceText.Foreground = _viewModel.Balance >= 0 
            ? new SolidColorBrush(Colors.Green) 
            : new SolidColorBrush(Colors.Red);
        
        // 更新加载指示器
        LoadingIndicator.IsVisible = _viewModel.IsLoading;
        
        // 更新月份选择器
        if (MonthPicker.SelectedDate != _viewModel.SelectedMonth)
        {
            MonthPicker.SelectedDate = _viewModel.SelectedMonth;
        }
        
        // 输出调试信息
        Console.WriteLine($"UI更新: 收入={_viewModel.TotalIncome}, 支出={_viewModel.TotalExpense}, 余额={_viewModel.Balance}");
    }
}