using System;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace FinanceApp.Views;

public static class StringConverters
{
    public static readonly IValueConverter ToExpenseColor = new FuncValueConverter<string, IBrush>(
        type => type == "支出" ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green)
    );
    
    public static readonly IValueConverter BooleanToColor = new FuncValueConverter<bool, IBrush>(
        isValid => isValid ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red)
    );
    
    // 检查字符串是否为null或空
    public static readonly IValueConverter IsNotNullOrEmpty = new FuncValueConverter<string, bool>(
        value => !string.IsNullOrEmpty(value)
    );
    
    // 根据重复消息内容返回背景色
    public static readonly IValueConverter DuplicateMessageToBackground = new FuncValueConverter<string, IBrush>(
        message =>
        {
            if (string.IsNullOrEmpty(message)) return new SolidColorBrush(Colors.Transparent);
            if (message.Contains("✅")) return new SolidColorBrush(Color.FromRgb(232, 245, 233)); // 浅绿色
            if (message.Contains("⚠️")) return new SolidColorBrush(Color.FromRgb(255, 243, 224)); // 浅橙色
            if (message.Contains("❌")) return new SolidColorBrush(Color.FromRgb(255, 235, 238)); // 浅红色
            return new SolidColorBrush(Colors.Transparent);
        }
    );
    
    // 根据重复消息内容返回前景色
    public static readonly IValueConverter DuplicateMessageToForeground = new FuncValueConverter<string, IBrush>(
        message =>
        {
            if (string.IsNullOrEmpty(message)) return new SolidColorBrush(Colors.Transparent);
            if (message.Contains("✅")) return new SolidColorBrush(Color.FromRgb(46, 125, 50)); // 深绿色
            if (message.Contains("⚠️")) return new SolidColorBrush(Color.FromRgb(245, 124, 0)); // 橙色
            if (message.Contains("❌")) return new SolidColorBrush(Color.FromRgb(198, 40, 40)); // 红色
            return new SolidColorBrush(Colors.Black);
        }
    );
}