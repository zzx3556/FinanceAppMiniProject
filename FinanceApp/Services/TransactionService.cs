using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Models;
using FinanceApp.Data;

namespace FinanceApp.Services;

public class TransactionService : ITransactionService
{
    private readonly FinanceDbContext _context;
    private readonly IUnTestableService _unTestableService;

    public TransactionService(FinanceDbContext context, IUnTestableService unTestableService)
    {
        _context = context;
        _unTestableService = unTestableService;
    }

    public async Task<List<Transaction>> GetTransactionsAsync()
    {
        return await _context.Transactions
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end)
    {
        var startOffset = new DateTimeOffset(start.Date);
        var endOffset = new DateTimeOffset(end.Date.AddDays(1).AddTicks(-1));

        var transactions = await _context.Transactions
            .Where(t => t.Date >= startOffset && t.Date <= endOffset)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
            
        return transactions;
    }

    public async Task<Transaction> AddTransactionAsync(Transaction transaction)
    {
        transaction.CreatedAt = _unTestableService.GetServerTime();
        
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        return transaction;
    }

    public async Task<bool> DeleteTransactionAsync(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<Dictionary<string, decimal>> GetCategorySummaryAsync(DateTime start, DateTime end)
    {
        var startOffset = new DateTimeOffset(start.Date);
        var endOffset = new DateTimeOffset(end.Date.AddDays(1).AddTicks(-1));

        return await _context.Transactions
            .Where(t => t.Date >= startOffset && t.Date <= endOffset && t.Type == "支出")
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.Category, x => x.Total);
    }

    public async Task<decimal> CalculateTransactionWithTaxAsync(decimal amount, string region)
    {
        var tax = _unTestableService.CalculateTax(amount, region);
        return amount + tax;
    }

    // 获取最近一笔交易
    public async Task<Transaction> GetLastTransactionAsync()
    {
        return await _context.Transactions
            .OrderByDescending(t => t.Date)
            .FirstOrDefaultAsync();
    }

    // 重复交易检测
    public async Task<bool> CheckDuplicateTransactionAsync(Transaction transaction)
    {
        var lastTransaction = await GetLastTransactionAsync();
        return _unTestableService.CheckPotentialDuplicate(transaction, lastTransaction);
    }
}