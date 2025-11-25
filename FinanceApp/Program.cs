using Avalonia;
using System;

namespace FinanceApp;

class Program
{
    // 应用程序的初始入口点。 Avalonia 配置
    // 在此处由应用程序完成。
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia 配置，别着急，在准备阶段使用。
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}