using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using FinanceApp.ViewModels;
using FinanceApp.Models;
using System;
using System.Collections.Generic;
using Avalonia;

namespace FinanceApp.Views;

public partial class TransactionsView : UserControl
{
    private TransactionsViewModel? _viewModel;
    
    public TransactionsView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }
    
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TransactionsViewModel viewModel)
        {
            _viewModel = viewModel;
            SubscribeToEvents(viewModel);
            LoadTransactions();
        }
    }
    
    private void SubscribeToEvents(TransactionsViewModel viewModel)
    {
        // 搜索按钮
        SearchButton.Click += (_, _) =>
        {
            if (_viewModel != null)
            {
                _viewModel.SearchKeyword = SearchTextBox.Text ?? string.Empty;
                _viewModel.SearchTransactionsCommand.Execute(null);
            }
        };
        
        // 重置按钮
        ResetButton.Click += (_, _) =>
        {
            SearchTextBox.Text = string.Empty;
            if (_viewModel != null)
            {
                _viewModel.SearchKeyword = string.Empty;
                _viewModel.LoadTransactionsCommand.Execute(null);
            }
        };
        
        // 返回首页按钮
        BackToDashboardButton.Click += (_, _) =>
        {
            _viewModel?.NavigateToDashboardCommand.Execute(null);
        };
        
        // 监听 ViewModel 属性变化
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TransactionsViewModel.Transactions))
            {
                UpdateTransactionsList();
            }
            else if (e.PropertyName == nameof(TransactionsViewModel.IsLoading))
            {
                LoadingText.IsVisible = _viewModel?.IsLoading ?? false;
            }
        };
    }
    
    private void LoadTransactions()
    {
        _viewModel?.LoadTransactionsCommand.Execute(null);
    }
    
    private void UpdateTransactionsList()
    {
        if (_viewModel == null) return;
        
        // 手动更新记录数量
        RecordCountText.Text = $"共找到 {_viewModel.Transactions.Count} 条记录";
        
        // 手动创建交易列表项
        TransactionsListBox.Items.Clear();
        
        foreach (var transaction in _viewModel.Transactions)
        {
            // 调试输出，检查数据
            Console.WriteLine($"交易: {transaction.Date:MM-dd} {transaction.Type} {transaction.Amount} {transaction.Category} {transaction.Account} {transaction.Description}");
            
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(10, 8)
            };
            
            var grid = new Grid();
            
            // 定义列 - 调整列宽确保所有信息都能显示
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });    // 日期
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });    // 类型
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });   // 金额
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });   // 分类
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });   // 支付方式
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // 备注
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });    // 删除按钮
            
            // 日期
            var dateText = new TextBlock
            {
                Text = transaction.Date.ToString("MM-dd"),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };
            Grid.SetColumn(dateText, 0);
            grid.Children.Add(dateText);
            
            // 类型 - 修复：确保收入类型也能正确显示
            var typeText = new TextBlock
            {
                Text = transaction.Type ?? "未知",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeight.Bold,
                FontSize = 12,
                Foreground = (transaction.Type == "支出") ? Brushes.Red : Brushes.Green
            };
            Grid.SetColumn(typeText, 1);
            grid.Children.Add(typeText);
            
            // 金额 - 修复：收入显示正数，支出显示负数
            var amountText = new TextBlock
            {
                Text = transaction.Type == "收入" ? $"¥{transaction.Amount:F2}" : $"¥-{transaction.Amount:F2}",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeight.Bold,
                FontSize = 12,
                Foreground = (transaction.Type == "支出") ? Brushes.Red : Brushes.Green
            };
            Grid.SetColumn(amountText, 2);
            grid.Children.Add(amountText);
            
            // 分类
            var categoryText = new TextBlock
            {
                Text = transaction.Category ?? "未分类",
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };
            Grid.SetColumn(categoryText, 3);
            grid.Children.Add(categoryText);
            
            // 支付方式 - 修复：确保显示Account字段
            var accountText = new TextBlock
            {
                Text = transaction.Account ?? "未知账户",
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };
            Grid.SetColumn(accountText, 4);
            grid.Children.Add(accountText);
            
            // 备注
            var descriptionText = new TextBlock
            {
                Text = transaction.Description ?? "无备注",
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontSize = 12
            };
            Grid.SetColumn(descriptionText, 5);
            grid.Children.Add(descriptionText);
            
            // 删除按钮
            var deleteButton = new Button
            {
                Content = "删除",
                Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Foreground = Brushes.White,
                Padding = new Thickness(8, 3),
                FontSize = 11
            };
            deleteButton.Click += (_, _) => OnDeleteButtonClick(transaction);
            Grid.SetColumn(deleteButton, 6);
            grid.Children.Add(deleteButton);
            
            border.Child = grid;
            TransactionsListBox.Items.Add(border);
        }
    }
    
    // 删除按钮点击事件处理
    private async void OnDeleteButtonClick(Transaction transaction)
    {
        if (_viewModel != null)
        {
            // 确认删除对话框
            var dialog = new Window()
            {
                Title = "确认删除",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            var stackPanel = new StackPanel 
            { 
                Margin = new Thickness(20),
                Spacing = 10
            };
            
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = $"确定要删除这条交易记录吗？",
                TextWrapping = TextWrapping.Wrap
            });
            
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = $"{transaction.Date:MM-dd} {transaction.Type} {transaction.Amount}元",
                FontWeight = FontWeight.Bold
            });
            
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 10
            };
            
            var confirmButton = new Button 
            { 
                Content = "确定删除",
                Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Foreground = Brushes.White
            };
            
            var cancelButton = new Button 
            { 
                Content = "取消",
                Background = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                Foreground = Brushes.White
            };
            
            confirmButton.Click += (_, _) =>
            {
                _viewModel.DeleteTransactionCommand.Execute(transaction.Id);
                dialog.Close();
            };
            
            cancelButton.Click += (_, _) => dialog.Close();
            
            buttonPanel.Children.Add(confirmButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);
            
            dialog.Content = stackPanel;
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}