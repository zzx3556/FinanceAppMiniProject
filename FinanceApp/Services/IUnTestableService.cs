using System;
using System.Threading.Tasks;
using FinanceApp.Models;

namespace FinanceApp.Services;

public interface IUnTestableService
{
    string GenerateTransactionId();
    DateTimeOffset GetServerTime();
    decimal CalculateTax(decimal amount, string region);
    Task<string> GetExchangeRateAsync(string fromCurrency, string toCurrency);
    
    // 重复检测
    bool CheckPotentialDuplicate(Transaction newTransaction, Transaction lastTransaction);
}