using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace FinanceApp.Views;

public partial class BillView : UserControl
{
    public BillView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext != null)
        {
            // 年份选择
            YearComboBox.Bind(ComboBox.ItemsSourceProperty, new Binding("AvailableYears"));
            YearComboBox.Bind(ComboBox.SelectedItemProperty, new Binding("SelectedYear"));
            
            // 年度汇总
            YearlyIncomeText.Bind(TextBlock.TextProperty, new Binding("YearlyIncome") { StringFormat = "¥{0:F2}" });
            YearlyExpenseText.Bind(TextBlock.TextProperty, new Binding("YearlyExpense") { StringFormat = "¥{0:F2}" });
            YearlyBalanceText.Bind(TextBlock.TextProperty, new Binding("YearlyBalance") { StringFormat = "¥{0:F2}" });
            
            // 月度汇总
            MonthlySummariesControl.Bind(ItemsControl.ItemsSourceProperty, new Binding("MonthlySummaries"));
            
            // 加载指示器
            LoadingIndicator.Bind(Border.IsVisibleProperty, new Binding("IsLoading"));
        }
    }
}