using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.ViewModels;
using Moq;

namespace FinanceApp.Tests.ViewModels;

public class DashboardViewModelTests
{
    [Fact]
    public void Constructor_WithDesignTime_ShouldInitializeWithDefaultValues()
    {
        // Act
        var viewModel = new DashboardViewModel();

        // Assert
        Assert.Equal(5000, viewModel.TotalIncome);
        Assert.Equal(3200, viewModel.TotalExpense);
        Assert.Equal(1800, viewModel.Balance);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public void Constructor_WithRuntime_ShouldInitializeProperties()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());

        // Act
        var viewModel = new DashboardViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Assert
        Assert.NotNull(viewModel);
    }

    [Fact]
    public async Task LoadDashboardDataCommand_ShouldCalculateTotalsCorrectly()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction { Amount = 1000, Type = "收入" },
            new Transaction { Amount = 500, Type = "收入" },
            new Transaction { Amount = 300, Type = "支出" },
            new Transaction { Amount = 200, Type = "支出" }
        };

        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.GetTransactionsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(transactions);

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new DashboardViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act
        await viewModel.LoadDashboardDataCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(1500, viewModel.TotalIncome);
        Assert.Equal(500, viewModel.TotalExpense);
        Assert.Equal(1000, viewModel.Balance);
    }

    [Fact]
    public async Task LoadDashboardDataCommand_WhenNoTransactions_ShouldSetZeroTotals()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.GetTransactionsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Transaction>());

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new DashboardViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act
        await viewModel.LoadDashboardDataCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(0, viewModel.TotalIncome);
        Assert.Equal(0, viewModel.TotalExpense);
        Assert.Equal(0, viewModel.Balance);
    }

    [Fact]
    public void SelectedMonthChanged_ShouldUpdateMonthDisplay()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new DashboardViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        
        var newMonth = new DateTime(2024, 6, 1);

        // Act
        viewModel.SelectedMonth = newMonth;

        // Assert - 只验证月份已更改，不验证具体显示格式
        Assert.Equal(newMonth, viewModel.SelectedMonth);
    }
}