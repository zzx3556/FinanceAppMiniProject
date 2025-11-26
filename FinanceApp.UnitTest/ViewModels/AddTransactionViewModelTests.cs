using System;
using System.Threading.Tasks;
using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.ViewModels;
using Moq;
using Xunit;

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
        Assert.Equal("✅ 准备就绪", viewModel.DuplicateMessage);
        Assert.True(viewModel.TransactionDate.Date == DateTimeOffset.Now.Date);
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
        Assert.Equal("✅ 准备就绪", viewModel.DuplicateMessage);
    }

    [Fact]
    public async Task CheckDuplicateCommand_WhenAmountIsZero_ShouldSetReadyMessage()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object)
        {
            Amount = 0
        };

        // Act
        await viewModel.CheckDuplicateCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("✅ 准备就绪", viewModel.DuplicateMessage);
        transactionServiceMock.Verify(x => x.CheckDuplicateTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task CheckDuplicateCommand_WhenRequiredFieldsMissing_ShouldSetInfoMessage()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object)
        {
            Amount = 100,
            Type = "",
            Category = "",
            Account = ""
        };

        // Act
        await viewModel.CheckDuplicateCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("ℹ️ 请填写完整信息", viewModel.DuplicateMessage);
        transactionServiceMock.Verify(x => x.CheckDuplicateTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task CheckDuplicateCommand_WhenDuplicateDetected_ShouldSetWarningMessage()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.CheckDuplicateTransactionAsync(It.IsAny<Transaction>()))
            .ReturnsAsync(true);

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object)
        {
            Amount = 100,
            Type = "支出",
            Category = "餐饮",
            Account = "微信",
            Description = "午餐"
        };

        // Act
        await viewModel.CheckDuplicateCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("⚠️ 检测到与最近交易相似，请确认", viewModel.DuplicateMessage);
        transactionServiceMock.Verify(x => x.CheckDuplicateTransactionAsync(It.IsAny<Transaction>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckDuplicateCommand_WhenNoDuplicate_ShouldSetSuccessMessage()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.CheckDuplicateTransactionAsync(It.IsAny<Transaction>()))
            .ReturnsAsync(false);

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object)
        {
            Amount = 100,
            Type = "支出",
            Category = "餐饮",
            Account = "微信",
            Description = "午餐"
        };

        // Act
        await viewModel.CheckDuplicateCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("✅ 无重复交易，可以保存", viewModel.DuplicateMessage);
        transactionServiceMock.Verify(x => x.CheckDuplicateTransactionAsync(It.IsAny<Transaction>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckDuplicateCommand_WhenServiceThrowsException_ShouldSetErrorMessage()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.CheckDuplicateTransactionAsync(It.IsAny<Transaction>()))
            .ThrowsAsync(new Exception("Service error"));

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object)
        {
            Amount = 100,
            Type = "支出",
            Category = "餐饮",
            Account = "微信",
            Description = "午餐"
        };

        // Act
        await viewModel.CheckDuplicateCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("❌ 检测失败，请重试", viewModel.DuplicateMessage);
    }

    [Fact]
    public async Task AddTransactionCommand_WhenAmountIsZero_ShouldSetErrorMessage()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object)
        {
            Amount = 0
        };

        // Act
        await viewModel.AddTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("❌ 金额必须大于0", viewModel.DuplicateMessage);
        transactionServiceMock.Verify(x => x.AddTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task AddTransactionCommand_WhenInvalidType_ShouldSetErrorMessage()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object)
        {
            Amount = 100,
            Type = "无效类型"
        };

        // Act
        await viewModel.AddTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("❌ 类型必须是'收入'或'支出'", viewModel.DuplicateMessage);
        transactionServiceMock.Verify(x => x.AddTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task AddTransactionCommand_WhenMissingRequiredFields_ShouldSetErrorMessage()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object)
        {
            Amount = 100,
            Type = "支出",
            Category = "",
            Account = ""
        };

        // Act
        await viewModel.AddTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("❌ 请填写分类和账户", viewModel.DuplicateMessage);
        transactionServiceMock.Verify(x => x.AddTransactionAsync(It.IsAny<Transaction>()), Times.Never);
    }

    [Fact]
    public async Task AddTransactionCommand_WhenValidTransaction_ShouldSaveAndResetFields()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.AddTransactionAsync(It.IsAny<Transaction>()))
            .ReturnsAsync(new Transaction { Id = 1 });

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object)
        {
            Amount = 100,
            Type = "支出",
            Category = "餐饮",
            Account = "微信",
            Description = "午餐"
        };

        // Act
        await viewModel.AddTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("✅ 交易保存成功！", viewModel.DuplicateMessage);
        Assert.Equal(0, viewModel.Amount);
        Assert.Equal(string.Empty, viewModel.Description);
        Assert.Equal(string.Empty, viewModel.Category);
        Assert.Equal(string.Empty, viewModel.Account);
        Assert.Equal(string.Empty, viewModel.Type);
        transactionServiceMock.Verify(x => x.AddTransactionAsync(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task AddTransactionCommand_WhenServiceThrowsException_ShouldSetErrorMessage()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.AddTransactionAsync(It.IsAny<Transaction>()))
            .ThrowsAsync(new Exception("Database error"));

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object)
        {
            Amount = 100,
            Type = "支出",
            Category = "餐饮",
            Account = "微信",
            Description = "午餐"
        };

        // Act
        await viewModel.AddTransactionCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("❌ 保存失败，请重试", viewModel.DuplicateMessage);
        transactionServiceMock.Verify(x => x.AddTransactionAsync(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public void CancelCommand_ShouldExecuteWithoutError()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act & Assert
        var exception = Record.Exception(() => viewModel.CancelCommand.Execute(null));
        Assert.Null(exception);
    }

    [Fact]
    public void NavigateToDashboardCommand_ShouldExecuteWithoutError()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act & Assert
        var exception = Record.Exception(() => viewModel.NavigateToDashboardCommand.Execute(null));
        Assert.Null(exception);
    }

    [Fact]
    public void Properties_ShouldRaisePropertyChanged()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new AddTransactionViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        
        var changedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);

        // Act
        viewModel.TransactionDate = DateTimeOffset.Now.AddDays(1);
        viewModel.Amount = 100;
        viewModel.Type = "收入";
        viewModel.Category = "测试";
        viewModel.Account = "测试账户";
        viewModel.Description = "测试描述";
        viewModel.DuplicateMessage = "测试消息";

        // Assert
        Assert.Contains(nameof(AddTransactionViewModel.TransactionDate), changedProperties);
        Assert.Contains(nameof(AddTransactionViewModel.Amount), changedProperties);
        Assert.Contains(nameof(AddTransactionViewModel.Type), changedProperties);
        Assert.Contains(nameof(AddTransactionViewModel.Category), changedProperties);
        Assert.Contains(nameof(AddTransactionViewModel.Account), changedProperties);
        Assert.Contains(nameof(AddTransactionViewModel.Description), changedProperties);
        Assert.Contains(nameof(AddTransactionViewModel.DuplicateMessage), changedProperties);
    }
}