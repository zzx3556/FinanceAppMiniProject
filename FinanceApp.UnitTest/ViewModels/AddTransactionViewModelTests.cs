using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.ViewModels;
using Moq;

namespace FinanceApp.Tests.ViewModels;

public class AddTransactionViewModelTests
{
    [Fact]
    public void Constructor_WithDesignTime_ShouldInitializeWithDefaultValues()
    {
        // Act
        var viewModel = new AddTransactionViewModel();

        // Assert
        Assert.Equal(100, viewModel.Amount);
        Assert.Equal("餐饮", viewModel.Category);
        Assert.Equal("微信", viewModel.Account);
        Assert.Equal("午餐", viewModel.Description);
    }

    [Fact]
    public void Constructor_WithRuntime_ShouldInitializeProperties()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());

        // Act
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Assert
        Assert.NotNull(viewModel);
        Assert.Equal(0, viewModel.Amount);
        Assert.Equal(string.Empty, viewModel.Description);
        Assert.Equal(string.Empty, viewModel.Type);
        Assert.Equal(string.Empty, viewModel.Category);
        Assert.Equal(string.Empty, viewModel.Account);
    }

    [Fact]
    public async Task CheckDuplicateCommand_WhenAmountIsZero_ShouldNotCallService()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        
        // 先创建 ViewModel，然后设置 Amount 为 0
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        viewModel.Amount = 0;

        // 等待所有属性变更事件完成
        await Task.Delay(100);

        // Act - 手动执行检查命令
        await viewModel.CheckDuplicateCommand.ExecuteAsync(null);

        // Assert
        transactionServiceMock.Verify(x => x.CheckDuplicateTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task CheckDuplicateCommand_WhenRequiredFieldsMissing_ShouldNotCallService()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        
        // 只设置金额，不设置其他必填字段
        viewModel.Amount = 100;
        
        // 等待所有属性变更事件完成
        await Task.Delay(100);

        // Act
        await viewModel.CheckDuplicateCommand.ExecuteAsync(null);

        // Assert
        transactionServiceMock.Verify(x => x.CheckDuplicateTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task CheckDuplicateCommand_WhenValidTransaction_ShouldCallService()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.CheckDuplicateTransactionAsync(It.IsAny<Transaction>()))
            .ReturnsAsync(false);

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        
        // 先创建 ViewModel
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        
        // 一次性设置所有属性，减少触发次数
        viewModel.Amount = 100;
        viewModel.Type = "支出";
        viewModel.Category = "餐饮";
        viewModel.Account = "微信";
        viewModel.Description = "午餐";
        
        // 等待所有属性变更事件完成
        await Task.Delay(100);

        // 重置 Mock 的调用计数
        transactionServiceMock.Invocations.Clear();

        // Act - 现在手动执行检查命令
        await viewModel.CheckDuplicateCommand.ExecuteAsync(null);

        // Assert - 应该只调用一次
        transactionServiceMock.Verify(x => x.CheckDuplicateTransactionAsync(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task AddTransactionCommand_WhenAmountIsZero_ShouldNotCallService()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        viewModel.Amount = 0;

        // Act
        await viewModel.AddTransactionCommand.ExecuteAsync(null);

        // Assert
        transactionServiceMock.Verify(x => x.AddTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task AddTransactionCommand_WhenInvalidType_ShouldNotCallService()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        viewModel.Amount = 100;
        viewModel.Type = "无效类型";

        // Act
        await viewModel.AddTransactionCommand.ExecuteAsync(null);

        // Assert
        transactionServiceMock.Verify(x => x.AddTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task AddTransactionCommand_WhenMissingRequiredFields_ShouldNotCallService()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        viewModel.Amount = 100;
        viewModel.Type = "支出";
        // 不设置 Category 和 Account

        // Act
        await viewModel.AddTransactionCommand.ExecuteAsync(null);

        // Assert
        transactionServiceMock.Verify(x => x.AddTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task AddTransactionCommand_WhenValidTransaction_ShouldCallService()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.AddTransactionAsync(It.IsAny<Transaction>()))
            .ReturnsAsync(new Transaction { Id = 1 });

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        viewModel.Amount = 100;
        viewModel.Type = "支出";
        viewModel.Category = "餐饮";
        viewModel.Account = "微信";
        viewModel.Description = "午餐";

        // Act
        await viewModel.AddTransactionCommand.ExecuteAsync(null);

        // Assert
        transactionServiceMock.Verify(x => x.AddTransactionAsync(It.IsAny<Transaction>()), Times.Once);
    }
}