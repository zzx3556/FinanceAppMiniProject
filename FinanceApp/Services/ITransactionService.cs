using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinanceApp.Models;

namespace FinanceApp.Services;

public interface ITransactionService
{
    Task<List<Transaction>> GetTransactionsAsync();
    Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end);
    Task<Transaction> AddTransactionAsync(Transaction transaction);
    Task<bool> DeleteTransactionAsync(int id);
    Task<Dictionary<string, decimal>> GetCategorySummaryAsync(DateTime start, DateTime end);
    Task<decimal> CalculateTransactionWithTaxAsync(decimal amount, string region);
    
    // 获取最近交易用于比较
    Task<Transaction> GetLastTransactionAsync();
    Task<bool> CheckDuplicateTransactionAsync(Transaction transaction);
}