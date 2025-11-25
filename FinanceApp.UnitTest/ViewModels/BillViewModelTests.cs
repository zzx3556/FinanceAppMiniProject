using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.ViewModels;
using Moq;

namespace FinanceApp.Tests.ViewModels;

public class BillViewModelTests
{
    [Fact]
    public void Constructor_WithDesignTime_ShouldInitializeWithSampleData()
    {
        // Act
        var viewModel = new BillViewModel();

        // Assert
        Assert.Equal(50000, viewModel.YearlyIncome);
        Assert.Equal(32000, viewModel.YearlyExpense);
        Assert.Equal(18000, viewModel.YearlyBalance);
        Assert.Equal(3, viewModel.MonthlySummaries.Count);
        Assert.Equal("1月", viewModel.MonthlySummaries[0].Month);
        Assert.Equal(4200, viewModel.MonthlySummaries[0].Income);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public void Constructor_WithRuntime_ShouldInitializeAndLoadData()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());

        // Act
        var viewModel = new BillViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Assert
        Assert.NotNull(viewModel);
        Assert.Equal(DateTime.Now.Year, viewModel.SelectedYear);
    }

    [Fact]
    public void AvailableYears_ShouldReturnCorrectYears()
    {
        // Arrange
        var viewModel = new BillViewModel();
        var currentYear = DateTime.Now.Year;

        // Act & Assert
        Assert.Equal(3, viewModel.AvailableYears.Count);
        Assert.Equal(currentYear, viewModel.AvailableYears[0]);
        Assert.Equal(currentYear - 1, viewModel.AvailableYears[1]);
        Assert.Equal(currentYear - 2, viewModel.AvailableYears[2]);
    }

    [Fact]
    public async Task LoadBillDataCommand_ShouldCalculateYearlyAndMonthlyTotals()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction { Date = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero), Amount = 5000, Type = "收入" },
            new Transaction { Date = new DateTimeOffset(2024, 1, 16, 0, 0, 0, TimeSpan.Zero), Amount = 1000, Type = "支出" },
            new Transaction { Date = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero), Amount = 3000, Type = "收入" },
            new Transaction { Date = new DateTimeOffset(2024, 2, 15, 0, 0, 0, TimeSpan.Zero), Amount = 500, Type = "支出" }
        };

        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.GetTransactionsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new BillViewModel(transactionServiceMock.Object, mainViewModelMock.Object)
        {
            SelectedYear = 2024
        };

        // Act
        await viewModel.LoadBillDataCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(8000, viewModel.YearlyIncome);
        Assert.Equal(1500, viewModel.YearlyExpense);
        Assert.Equal(6500, viewModel.YearlyBalance);
        
        // Check monthly summaries - 只检查有数据的月份
        var january = viewModel.MonthlySummaries.First(m => m.Month == "1月");
        Assert.Equal(5000, january.Income);
        Assert.Equal(1000, january.Expense);
        Assert.Equal(4000, january.Balance);
        
        var february = viewModel.MonthlySummaries.First(m => m.Month == "2月");
        Assert.Equal(3000, february.Income);
        Assert.Equal(500, february.Expense);
        Assert.Equal(2500, february.Balance);
    }

    [Fact]
    public async Task LoadBillDataCommand_WhenNoTransactions_ShouldSetZeroTotals()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.GetTransactionsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Transaction>());

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new BillViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act
        await viewModel.LoadBillDataCommand.ExecuteAsync(null);

        // Assert - 修正断言：MonthlySummaries 不为空，但所有值应为0
        Assert.Equal(0, viewModel.YearlyIncome);
        Assert.Equal(0, viewModel.YearlyExpense);
        Assert.Equal(0, viewModel.YearlyBalance);
        
        // 检查所有月份的汇总数据都是0
        Assert.All(viewModel.MonthlySummaries, summary =>
        {
            Assert.Equal(0, summary.Income);
            Assert.Equal(0, summary.Expense);
            Assert.Equal(0, summary.Balance);
        });
    }

    [Fact]
    public void SelectedYearChanged_ShouldUpdateSelectedYear()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new BillViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        
        var newYear = 2023;

        // Act
        viewModel.SelectedYear = newYear;

        // Assert
        Assert.Equal(2023, viewModel.SelectedYear);
    }

    [Fact]
    public void NavigateToDashboardCommand_ShouldExecuteWithoutError()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new BillViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act & Assert - 确保命令执行不会抛出异常
        var exception = Record.Exception(() => viewModel.NavigateToDashboardCommand.Execute(null));
        Assert.Null(exception);
    }
}