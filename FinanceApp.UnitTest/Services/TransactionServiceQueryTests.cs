using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Services;
using Moq;

namespace FinanceApp.Tests.Services;

public class TransactionServiceQueryTests : IDisposable
{
    private readonly FinanceDbContext _context;
    private readonly Mock<IUnTestableService> _unTestableServiceMock;
    private readonly TransactionService _transactionService;

    public TransactionServiceQueryTests()
    {
        // 使用内存数据库进行测试
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FinanceDbContext(options);
        _unTestableServiceMock = new Mock<IUnTestableService>();
        _transactionService = new TransactionService(_context, _unTestableServiceMock.Object);

        // 初始化测试数据
        SeedTestData();
    }

    private void SeedTestData()
    {
        var transactions = new List<Transaction>
        {
            new Transaction 
            { 
                Id = 1, 
                Date = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero),
                Amount = 1000m, 
                Type = "收入", 
                Category = "工资", 
                Account = "银行卡",
                Description = "月薪"
            },
            new Transaction 
            { 
                Id = 2, 
                Date = new DateTimeOffset(2024, 1, 16, 0, 0, 0, TimeSpan.Zero),
                Amount = 50m, 
                Type = "支出", 
                Category = "餐饮", 
                Account = "微信",
                Description = "午餐"
            },
            new Transaction 
            { 
                Id = 3, 
                Date = new DateTimeOffset(2024, 1, 17, 0, 0, 0, TimeSpan.Zero),
                Amount = 200m, 
                Type = "支出", 
                Category = "购物", 
                Account = "支付宝",
                Description = "衣服"
            },
            new Transaction 
            { 
                Id = 4, 
                Date = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero),
                Amount = 1500m, 
                Type = "收入", 
                Category = "奖金", 
                Account = "银行卡",
                Description = "年终奖"
            }
        };

        _context.Transactions.AddRange(transactions);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldReturnAllTransactionsOrderedByDateDesc()
    {
        // Act
        var result = await _transactionService.GetTransactionsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        Assert.Equal(4, result[0].Id); // 最新的交易应该是ID=4
        Assert.Equal(3, result[1].Id);
        Assert.Equal(2, result[2].Id);
        Assert.Equal(1, result[3].Id);
    }

    [Fact]
    public async Task GetTransactionsByDateRangeAsync_ShouldReturnFilteredTransactions()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        // Act
        var result = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count); // 1月有3笔交易
        Assert.All(result, t => 
        {
            Assert.True(t.Date >= new DateTimeOffset(startDate));
            Assert.True(t.Date <= new DateTimeOffset(endDate.AddDays(1).AddTicks(-1)));
        });
    }

    [Fact]
    public async Task GetTransactionsByDateRangeAsync_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var startDate = new DateTime(2023, 12, 1);
        var endDate = new DateTime(2023, 12, 31);

        // Act
        var result = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCategorySummaryAsync_ShouldReturnCorrectCategoryTotals()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        // Act
        var result = await _transactionService.GetCategorySummaryAsync(startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // 餐饮和购物两个支出分类
        Assert.Equal(50m, result["餐饮"]);
        Assert.Equal(200m, result["购物"]);
    }

    [Fact]
    public async Task GetCategorySummaryAsync_WithNoExpenses_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var startDate = new DateTime(2024, 2, 1);
        var endDate = new DateTime(2024, 2, 29);

        // Act
        var result = await _transactionService.GetCategorySummaryAsync(startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result); // 2月只有收入，没有支出
    }

    [Fact]
    public async Task GetLastTransactionAsync_ShouldReturnMostRecentTransaction()
    {
        // Act
        var result = await _transactionService.GetLastTransactionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Id); // 最新的交易是ID=4
        Assert.Equal("奖金", result.Category);
    }

    [Fact]
    public async Task GetLastTransactionAsync_WithEmptyDatabase_ShouldReturnNull()
    {
        // Arrange
        _context.Transactions.RemoveRange(_context.Transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _transactionService.GetLastTransactionAsync();

        // Assert
        Assert.Null(result);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}