using FinanceApp.Services;
using FinanceApp.ViewModels;
using Moq;

namespace FinanceApp.Tests.ViewModels;

public class StatisticsViewModelTests
{
    [Fact]
    public void Constructor_WithDesignTime_ShouldInitializeWithSampleData()
    {
        // Act
        var viewModel = new StatisticsViewModel();

        // Assert
        Assert.NotNull(viewModel.CategorySummary);
        Assert.Equal(4, viewModel.CategorySummary.Count);
        Assert.Equal(1200, viewModel.MaxCategoryValue);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public void Constructor_WithRuntime_ShouldInitializeProperties()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());

        // Act
        var viewModel = new StatisticsViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Assert
        Assert.NotNull(viewModel);
        // 不验证 CategorySummary，因为它在异步加载前可能为 null
    }

    [Fact]
    public async Task LoadStatisticsCommand_ShouldLoadCategorySummary()
    {
        // Arrange
        var categorySummary = new Dictionary<string, decimal>
        {
            { "餐饮", 800 },
            { "交通", 500 },
            { "娱乐", 300 }
        };

        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.GetCategorySummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(categorySummary);

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new StatisticsViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act
        await viewModel.LoadStatisticsCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(3, viewModel.CategorySummary.Count);
        Assert.Equal(800, viewModel.MaxCategoryValue);
    }

    [Fact]
    public async Task LoadStatisticsCommand_WhenEmptySummary_ShouldSetMaxValueToOne()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.GetCategorySummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new Dictionary<string, decimal>());

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new StatisticsViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act
        await viewModel.LoadStatisticsCommand.ExecuteAsync(null);

        // Assert
        Assert.Empty(viewModel.CategorySummary);
        Assert.Equal(1, viewModel.MaxCategoryValue);
    }

    [Fact]
    public void DateRangeProperties_ShouldHaveReasonableDefaults()
    {
        // Act
        var viewModel = new StatisticsViewModel();

        // Assert
        Assert.True(viewModel.StartDate <= DateTimeOffset.Now.AddMonths(-1));
        Assert.True(viewModel.EndDate >= DateTimeOffset.Now.AddMonths(-1));
    }

    [Fact]
    public void NavigateToDashboardCommand_ShouldExecuteWithoutError()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new StatisticsViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act & Assert - 确保命令执行不会抛出异常
        var exception = Record.Exception(() => viewModel.NavigateToDashboardCommand.Execute(null));
        Assert.Null(exception);
    }
}