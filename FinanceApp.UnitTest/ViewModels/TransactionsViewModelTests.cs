using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.ViewModels;
using Moq;

namespace FinanceApp.Tests.ViewModels;

public class TransactionsViewModelTests
{
    [Fact]
    public void Constructor_WithDesignTime_ShouldInitializeWithSampleData()
    {
        // Act
        var viewModel = new TransactionsViewModel();

        // Assert
        Assert.NotNull(viewModel.Transactions);
        Assert.Equal(2, viewModel.Transactions.Count);
    }

    [Fact]
    public void Constructor_WithRuntime_ShouldInitializeProperties()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());

        // Act
        var viewModel = new TransactionsViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Assert
        Assert.NotNull(viewModel);
        Assert.NotNull(viewModel.Transactions);
    }

    [Fact]
    public async Task LoadTransactionsCommand_ShouldLoadTransactions()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction { Id = 1, Description = "交易1", Date = DateTimeOffset.Now },
            new Transaction { Id = 2, Description = "交易2", Date = DateTimeOffset.Now.AddDays(-1) }
        };

        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.GetTransactionsAsync())
            .ReturnsAsync(transactions);

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        
        // 创建新的 Mock 来确保干净的调用计数
        var cleanTransactionServiceMock = new Mock<ITransactionService>();
        cleanTransactionServiceMock.Setup(x => x.GetTransactionsAsync())
            .ReturnsAsync(transactions);
            
        var viewModel = new TransactionsViewModel(cleanTransactionServiceMock.Object, mainViewModelMock.Object);

        // Act
        await viewModel.LoadTransactionsCommand.ExecuteAsync(null);

        // Assert - 验证被调用，但不限制次数
        cleanTransactionServiceMock.Verify(x => x.GetTransactionsAsync(), Times.AtLeastOnce());
    }

    [Fact]
    public void SearchTransactionsCommand_WithMatchingKeyword_ShouldFilterTransactions()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new TransactionsViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        
        // 使用反射设置私有字段 _allTransactions，避免 null 异常
        var allTransactionsField = typeof(TransactionsViewModel).GetField("_allTransactions", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var testTransactions = new List<Transaction>
        {
            new Transaction { Id = 1, Description = "午餐", Category = "餐饮", Account = "现金", Type = "支出", Amount = 50 },
            new Transaction { Id = 2, Description = "工资", Category = "收入", Account = "银行卡", Type = "收入", Amount = 5000 },
            new Transaction { Id = 3, Description = "晚餐", Category = "餐饮", Account = "微信", Type = "支出", Amount = 80 }
        };
        
        allTransactionsField?.SetValue(viewModel, testTransactions);
        viewModel.SearchKeyword = "餐";

        // Act
        viewModel.SearchTransactionsCommand.Execute(null);

        // Assert - 验证过滤后的交易数量正确
        Assert.Equal(2, viewModel.Transactions.Count); // 应该找到2个包含"餐"的交易
        Assert.All(viewModel.Transactions, t => 
            Assert.True(t.Description?.Contains("餐") == true));
    }

    [Fact]
    public void SearchTransactionsCommand_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new TransactionsViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        
        // 使用反射设置私有字段 _allTransactions
        var allTransactionsField = typeof(TransactionsViewModel).GetField("_allTransactions", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var testTransactions = new List<Transaction>
        {
            new Transaction { Id = 1, Description = "午餐", Category = "餐饮", Account = "现金", Type = "支出", Amount = 50 },
            new Transaction { Id = 2, Description = "工资", Category = "收入", Account = "银行卡", Type = "收入", Amount = 5000 }
        };
        
        allTransactionsField?.SetValue(viewModel, testTransactions);
        viewModel.SearchKeyword = "不存在的关键词";

        // Act
        viewModel.SearchTransactionsCommand.Execute(null);

        // Assert - 验证没有找到匹配的交易
        Assert.Empty(viewModel.Transactions);
    }

    [Fact]
    public void SearchTransactionsCommand_WithEmptyKeyword_ShouldReturnAllTransactions()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new TransactionsViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        
        // 使用反射设置私有字段 _allTransactions
        var allTransactionsField = typeof(TransactionsViewModel).GetField("_allTransactions", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var testTransactions = new List<Transaction>
        {
            new Transaction { Id = 1, Description = "午餐", Category = "餐饮", Account = "现金", Type = "支出", Amount = 50 },
            new Transaction { Id = 2, Description = "工资", Category = "收入", Account = "银行卡", Type = "收入", Amount = 5000 }
        };
        
        allTransactionsField?.SetValue(viewModel, testTransactions);
        viewModel.SearchKeyword = "";

        // Act
        viewModel.SearchTransactionsCommand.Execute(null);

        // Assert - 验证返回所有交易
        Assert.Equal(2, viewModel.Transactions.Count);
    }

    [Fact]
    public void SearchTransactionsCommand_WithEmptyAllTransactions_ShouldNotThrow()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new TransactionsViewModel(transactionServiceMock.Object, mainViewModelMock.Object);
        
        // 设置 _allTransactions 为空列表而不是 null
        var allTransactionsField = typeof(TransactionsViewModel).GetField("_allTransactions", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        allTransactionsField?.SetValue(viewModel, new List<Transaction>());
        
        viewModel.SearchKeyword = "test";

        // Act & Assert - 确保命令执行不会抛出异常
        var exception = Record.Exception(() => viewModel.SearchTransactionsCommand.Execute(null));
        Assert.Null(exception);
    }

    [Fact]
    public async Task DeleteTransactionCommand_ShouldCallService()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        transactionServiceMock.Setup(x => x.DeleteTransactionAsync(1))
            .ReturnsAsync(true);

        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        
        // 创建新的 Mock 来确保干净的调用计数
        var cleanTransactionServiceMock = new Mock<ITransactionService>();
        cleanTransactionServiceMock.Setup(x => x.DeleteTransactionAsync(1))
            .ReturnsAsync(true);
            
        var viewModel = new TransactionsViewModel(cleanTransactionServiceMock.Object, mainViewModelMock.Object);

        // Act
        await viewModel.DeleteTransactionCommand.ExecuteAsync(1);

        // Assert
        cleanTransactionServiceMock.Verify(x => x.DeleteTransactionAsync(1), Times.Once);
    }

    [Fact]
    public void NavigateToDashboardCommand_ShouldExecuteWithoutError()
    {
        // Arrange
        var transactionServiceMock = new Mock<ITransactionService>();
        var mainViewModelMock = new Mock<MainWindowViewModel>(Mock.Of<ITransactionService>());
        var viewModel = new TransactionsViewModel(transactionServiceMock.Object, mainViewModelMock.Object);

        // Act & Assert - 确保命令执行不会抛出异常
        var exception = Record.Exception(() => viewModel.NavigateToDashboardCommand.Execute(null));
        Assert.Null(exception);
    }
}