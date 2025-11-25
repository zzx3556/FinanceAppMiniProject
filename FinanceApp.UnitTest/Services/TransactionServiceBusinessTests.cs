using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Services;
using Moq;

namespace FinanceApp.Tests.Services;

public class TransactionServiceBusinessTests : IDisposable
{
    private readonly FinanceDbContext _context;
    private readonly Mock<IUnTestableService> _unTestableServiceMock;
    private readonly TransactionService _transactionService;

    public TransactionServiceBusinessTests()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FinanceDbContext(options);
        _unTestableServiceMock = new Mock<IUnTestableService>();
        _transactionService = new TransactionService(_context, _unTestableServiceMock.Object);
    }

    [Fact]
    public async Task AddTransactionAsync_ShouldAddTransactionWithCreatedAt()
    {
        // Arrange
        var expectedTime = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        _unTestableServiceMock.Setup(x => x.GetServerTime()).Returns(expectedTime);

        var transaction = new Transaction
        {
            Date = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero),
            Amount = 100m,
            Type = "支出",
            Category = "测试",
            Account = "测试账户",
            Description = "测试交易"
        };

        // Act
        var result = await _transactionService.AddTransactionAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTime, result.CreatedAt);
        Assert.True(result.Id > 0);

        var savedTransaction = await _context.Transactions.FindAsync(result.Id);
        Assert.NotNull(savedTransaction);
        Assert.Equal(expectedTime, savedTransaction.CreatedAt);
    }

    [Fact]
    public async Task DeleteTransactionAsync_WithExistingId_ShouldRemoveTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            Date = DateTimeOffset.Now,
            Amount = 100m,
            Type = "支出",
            Category = "测试",
            Account = "测试账户",
            Description = "待删除交易"
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        var transactionId = transaction.Id;

        // Act
        var result = await _transactionService.DeleteTransactionAsync(transactionId);

        // Assert
        Assert.True(result);
        var deletedTransaction = await _context.Transactions.FindAsync(transactionId);
        Assert.Null(deletedTransaction);
    }

    [Fact]
    public async Task DeleteTransactionAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await _transactionService.DeleteTransactionAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CalculateTransactionWithTaxAsync_ShouldReturnAmountPlusTax()
    {
        // Arrange
        var amount = 100m;
        var region = "US";
        var expectedTax = 8m;
        _unTestableServiceMock.Setup(x => x.CalculateTax(amount, region)).Returns(expectedTax);

        // Act
        var result = await _transactionService.CalculateTransactionWithTaxAsync(amount, region);

        // Assert
        Assert.Equal(108m, result); // 100 + 8
        _unTestableServiceMock.Verify(x => x.CalculateTax(amount, region), Times.Once);
    }

    [Fact]
    public async Task CheckDuplicateTransactionAsync_WhenPotentialDuplicate_ShouldReturnTrue()
    {
        // Arrange
        var lastTransaction = new Transaction
        {
            Date = DateTimeOffset.Now,
            Amount = 100m,
            Type = "支出",
            Category = "餐饮",
            Account = "微信",
            Description = "午餐"
        };

        var newTransaction = new Transaction
        {
            Date = DateTimeOffset.Now,
            Amount = 100m,
            Type = "支出",
            Category = "餐饮",
            Account = "微信",
            Description = "午餐"
        };

        _context.Transactions.Add(lastTransaction);
        await _context.SaveChangesAsync();

        _unTestableServiceMock.Setup(x => x.CheckPotentialDuplicate(newTransaction, lastTransaction))
            .Returns(true);

        // Act
        var result = await _transactionService.CheckDuplicateTransactionAsync(newTransaction);

        // Assert
        Assert.True(result);
        _unTestableServiceMock.Verify(x => x.CheckPotentialDuplicate(newTransaction, lastTransaction), Times.Once);
    }

    [Fact]
    public async Task CheckDuplicateTransactionAsync_WhenNoLastTransaction_ShouldReturnFalse()
    {
        // Arrange
        var newTransaction = new Transaction
        {
            Date = DateTimeOffset.Now,
            Amount = 100m,
            Type = "支出",
            Category = "餐饮",
            Account = "微信",
            Description = "午餐"
        };

        // Act
        var result = await _transactionService.CheckDuplicateTransactionAsync(newTransaction);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckDuplicateTransactionAsync_WhenNotDuplicate_ShouldReturnFalse()
    {
        // Arrange
        var lastTransaction = new Transaction
        {
            Date = DateTimeOffset.Now.AddDays(-1),
            Amount = 50m,
            Type = "收入",
            Category = "工资",
            Account = "银行卡",
            Description = "月薪"
        };

        var newTransaction = new Transaction
        {
            Date = DateTimeOffset.Now,
            Amount = 100m,
            Type = "支出",
            Category = "餐饮",
            Account = "微信",
            Description = "午餐"
        };

        _context.Transactions.Add(lastTransaction);
        await _context.SaveChangesAsync();

        _unTestableServiceMock.Setup(x => x.CheckPotentialDuplicate(newTransaction, lastTransaction))
            .Returns(false);

        // Act
        var result = await _transactionService.CheckDuplicateTransactionAsync(newTransaction);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}