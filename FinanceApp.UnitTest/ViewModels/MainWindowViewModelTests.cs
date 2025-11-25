using FinanceApp.Services;
using FinanceApp.ViewModels;
using Moq;

namespace FinanceApp.Tests.ViewModels;

public class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDashboardViewModel()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();

        // Act
        var viewModel = new MainWindowViewModel(transactionServiceMock.Object);

        // Assert
        Assert.NotNull(viewModel.CurrentViewModel);
        Assert.IsType<DashboardViewModel>(viewModel.CurrentViewModel);
        Assert.Equal("财务概览", viewModel.CurrentPageTitle);
    }

    [Fact]
    public void NavigateToDashboard_ShouldSetCorrectViewModelAndTitle()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var viewModel = new MainWindowViewModel(transactionServiceMock.Object);

        // Act
        viewModel.NavigateToDashboard();

        // Assert
        Assert.IsType<DashboardViewModel>(viewModel.CurrentViewModel);
        Assert.Equal("财务概览", viewModel.CurrentPageTitle);
    }

    [Fact]
    public void NavigateToAddTransaction_ShouldSetCorrectViewModelAndTitle()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var viewModel = new MainWindowViewModel(transactionServiceMock.Object);

        // Act
        viewModel.NavigateToAddTransaction();

        // Assert
        Assert.IsType<AddTransactionViewModel>(viewModel.CurrentViewModel);
        Assert.Equal("添加交易", viewModel.CurrentPageTitle);
    }

    [Fact]
    public void NavigateToTransactions_ShouldSetCorrectViewModelAndTitle()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var viewModel = new MainWindowViewModel(transactionServiceMock.Object);

        // Act
        viewModel.NavigateToTransactions();

        // Assert
        Assert.IsType<TransactionsViewModel>(viewModel.CurrentViewModel);
        Assert.Equal("交易明细", viewModel.CurrentPageTitle);
    }

    [Fact]
    public void NavigateToStatistics_ShouldSetCorrectViewModelAndTitle()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var viewModel = new MainWindowViewModel(transactionServiceMock.Object);

        // Act
        viewModel.NavigateToStatistics();

        // Assert
        Assert.IsType<StatisticsViewModel>(viewModel.CurrentViewModel);
        Assert.Equal("统计分析", viewModel.CurrentPageTitle);
    }

    [Fact]
    public void NavigateToBills_ShouldSetCorrectViewModelAndTitle()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var viewModel = new MainWindowViewModel(transactionServiceMock.Object);

        // Act
        viewModel.NavigateToBills();

        // Assert
        Assert.IsType<BillViewModel>(viewModel.CurrentViewModel);
        Assert.Equal("年度账单", viewModel.CurrentPageTitle);
    }

    [Fact]
    public void NavigationCommands_ShouldUpdateCurrentViewModel()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var viewModel = new MainWindowViewModel(transactionServiceMock.Object);

        // Act & Assert - Test all navigation commands
        viewModel.NavigateToAddTransaction();
        Assert.IsType<AddTransactionViewModel>(viewModel.CurrentViewModel);

        viewModel.NavigateToTransactions();
        Assert.IsType<TransactionsViewModel>(viewModel.CurrentViewModel);

        viewModel.NavigateToStatistics();
        Assert.IsType<StatisticsViewModel>(viewModel.CurrentViewModel);

        viewModel.NavigateToBills();
        Assert.IsType<BillViewModel>(viewModel.CurrentViewModel);

        viewModel.NavigateToDashboard();
        Assert.IsType<DashboardViewModel>(viewModel.CurrentViewModel);
    }
}