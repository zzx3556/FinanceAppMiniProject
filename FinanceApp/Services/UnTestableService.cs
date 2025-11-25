using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using FinanceApp.Models;

namespace FinanceApp.Services;

public class UnTestableService : IUnTestableService
{
    private readonly HttpClient _httpClient;
    private readonly Random _random;

    public UnTestableService()
    {
        _httpClient = new HttpClient();
        _random = new Random();
    }

    public string GenerateTransactionId()
    {
        var timestamp = DateTime.Now.Ticks;
        var randomPart = _random.Next(1000, 9999);
        return $"TXN_{timestamp}_{randomPart}";
    }
    
    public DateTimeOffset GetServerTime()
    {
        return DateTimeOffset.Now;
    }
    
    public decimal CalculateTax(decimal amount, string region)
    {
        return region?.ToUpper() switch
        {
            "CN" => amount * 0.06m,
            "US" => amount * 0.08m,
            "EU" => amount * 0.2m,
            "UK" => amount * 0.2m,
            _ => amount * 0.1m
        };
    }

    public async Task<string> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"https://api.exchangerate.host/latest?base={fromCurrency}&symbols={toCurrency}");
            var json = JsonDocument.Parse(response);
            return json.RootElement.GetProperty("rates").GetProperty(toCurrency).GetDecimal().ToString("F4");
        }
        catch
        {
            return (1.0m + (decimal)(_random.NextDouble() * 0.2 - 0.1)).ToString("F4");
        }
    }

    // 更合理的重复交易检测
    public bool CheckPotentialDuplicate(Transaction newTransaction, Transaction lastTransaction)
    {
        if (lastTransaction == null) return false;

        // 计算相似度分数
        int similarityScore = 0;

        // 1. 金额相同 +30分
        if (newTransaction.Amount == lastTransaction.Amount)
            similarityScore += 30;

        // 2. 描述相似 +20分
        if (!string.IsNullOrEmpty(newTransaction.Description) && 
            !string.IsNullOrEmpty(lastTransaction.Description) &&
            newTransaction.Description.ToLower() == lastTransaction.Description.ToLower())
            similarityScore += 20;

        // 3. 类型相同 +10分
        if (newTransaction.Type == lastTransaction.Type)
            similarityScore += 10;

        // 4. 分类相同 +10分
        if (newTransaction.Category == lastTransaction.Category)
            similarityScore += 10;

        // 5. 账户相同 +10分
        if (newTransaction.Account == lastTransaction.Account)
            similarityScore += 10;

        // 6. 同一天 +20分
        if (newTransaction.Date.Date == lastTransaction.Date.Date)
            similarityScore += 20;

        // 只有当相似度超过80分才认为是潜在重复
        // 这意味着需要：金额相同+描述相同+同一天 (30+20+20=70)，还需要其他一些匹配
        return similarityScore >= 80;
    }
}