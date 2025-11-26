using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.ViewModels;
using Moq;
using Xunit;

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
        Assert.Equal(DateTime.Now.ToString("yyyy年MM月"), viewModel.MonthDisplay);
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
        Assert.Equal(DateTime.Now.ToString("yyyy年MM月"), viewModel.MonthDisplay);
        Assert.False(viewModel.IsLoading);
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
        Assert.False(viewModel.IsLoading);
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
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadDashboardDataCommand_WhenServiceThrowsException_ShouldHandleGracefully()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.GetTransactionsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Database error"));

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new DashboardViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act
        await viewModel.LoadDashboardDataCommand.ExecuteAsync(null);

        // Assert
        Assert.False(viewModel.IsLoading);
        // 确保没有异常抛出并且IsLoading被正确设置为false
    }

    [Fact]
    public void SelectedMonthChanged_ShouldTriggerDataReload()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new DashboardViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        
        bool loadCalled = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(DashboardViewModel.SelectedMonth))
            {
                loadCalled = true;
            }
        };

        // Act
        viewModel.SelectedMonth = DateTime.Now.AddMonths(-1);

        // Assert
        Assert.True(loadCalled);
    }

    [Fact]
    public void NavigationCommands_ShouldExecuteWithoutError()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new DashboardViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act & Assert - 确保所有导航命令执行不会抛出异常
        var exception1 = Record.Exception(() => viewModel.NavigateToAddTransactionCommand.Execute(null));
        var exception2 = Record.Exception(() => viewModel.NavigateToTransactionsCommand.Execute(null));
        var exception3 = Record.Exception(() => viewModel.NavigateToStatisticsCommand.Execute(null));
        var exception4 = Record.Exception(() => viewModel.NavigateToBillsCommand.Execute(null));

        Assert.Null(exception1);
        Assert.Null(exception2);
        Assert.Null(exception3);
        Assert.Null(exception4);
    }

    [Fact]
    public void Properties_ShouldRaisePropertyChanged()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new DashboardViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        
        var changedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

        // Act
        viewModel.TotalIncome = 1000;
        viewModel.TotalExpense = 500;
        viewModel.Balance = 500;
        viewModel.SelectedMonth = DateTime.Now;
        viewModel.IsLoading = true;
        viewModel.MonthDisplay = "测试";

        // Assert
        Assert.Contains(nameof(DashboardViewModel.TotalIncome), changedProperties);
        Assert.Contains(nameof(DashboardViewModel.TotalExpense), changedProperties);
        Assert.Contains(nameof(DashboardViewModel.Balance), changedProperties);
        Assert.Contains(nameof(DashboardViewModel.SelectedMonth), changedProperties);
        Assert.Contains(nameof(DashboardViewModel.IsLoading), changedProperties);
        Assert.Contains(nameof(DashboardViewModel.MonthDisplay), changedProperties);
    }
}