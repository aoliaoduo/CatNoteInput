using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CatNoteInput;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel(new SettingsService(), new CatNoteApiClient());
        DataContext = _viewModel;
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ApplyBackdrop();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        await _viewModel.PersistSettingsAsync();
    }

    private void ApplyBackdrop()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var backdrop = (int)DwmSystemBackdropType.Mica;
        _ = DwmSetWindowAttribute(hwnd, DwmWindowAttribute.DwmwaSystemBackdropType, ref backdrop, sizeof(int));
    }

    private enum DwmWindowAttribute
    {
        DwmwaSystemBackdropType = 38
    }

    private enum DwmSystemBackdropType
    {
        Auto = 0,
        None = 1,
        Mica = 2,
        Acrylic = 3,
        MicaAlt = 4
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attribute, ref int pvAttribute, int cbAttribute);
}
