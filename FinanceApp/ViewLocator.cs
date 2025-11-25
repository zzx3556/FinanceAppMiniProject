using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FinanceApp.ViewModels;
using System;

namespace FinanceApp;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var name = data?.GetType().FullName?.Replace("ViewModel", "View");
        if (name == null) return new TextBlock { Text = "数据为空" };
        
        var type = Type.GetType(name);
        if (type != null)
        {
            try
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            catch
            {
                return new TextBlock { Text = $"创建失败: {name}" };
            }
        }
        
        return new TextBlock { Text = $"未找到: {name}" };
    }

    public bool Match(object? data) => data is ViewModelBase;
}