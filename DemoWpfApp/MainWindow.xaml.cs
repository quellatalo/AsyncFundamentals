﻿using System.Diagnostics;
using System.Windows;

namespace DemoWpfApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow() => InitializeComponent();

    async void BtnCorrect_OnClick(object sender, RoutedEventArgs e)
    {
        BtnCorrect.Content = "Retrieving...";

        // by default, it's .ConfigureAwait(true)
        BtnCorrect.Content = await RetrieveStringFromBackEnd();
    }

    // Same as: UpdateUiContent_NotContinueOnCapturedContext_Exception
    async void BtnUi_OnClick(object sender, RoutedEventArgs e)
    {
        BtnUi.Content = "Retrieving...";

        // Error!
        BtnUi.Content = await RetrieveStringFromBackEnd().ConfigureAwait(false);
    }

    // Same as: SyncContext_Slow_ContinueOnCapturedContext(true)
    async void BtnBe_OnClick(object sender, RoutedEventArgs e)
    {
        BtnBe.Content = "Retrieving...";
        BtnBe.Content = await RetrieveStringFromBackEnd(true);
    }

    static async Task<string> RetrieveStringFromBackEnd(bool backEndAwaitConfiguration = false)
    {
        var sw = Stopwatch.StartNew();
        await Task.Delay(10).ConfigureAwait(backEndAwaitConfiguration);
        SomethingTimeConsuming();
        return sw.Elapsed.ToString();
    }

    static void SomethingTimeConsuming() => Thread.Sleep(4000);
}
