using System;

namespace FinanceApp.Models;

public class Transaction
{
    public int Id { get; set; }
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;  // 改为 DateTimeOffset
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;  // 改为 DateTimeOffset
}