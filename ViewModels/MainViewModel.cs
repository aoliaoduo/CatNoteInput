using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CatNoteInput;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private const int MaxContentLength = 5000;
    private const string ReadyMessage = "\u51C6\u5907\u5C31\u7EEA.";
    private const string EmptyContentMessage = "\u8BF7\u8F93\u5165\u7B14\u8BB0\u5185\u5BB9.";
    private const string EmptySecretMessage = "\u8BF7\u8F93\u5165 API \u5BC6\u94A5.";
    private const string InvalidSecretMessage = "\u8BF7\u8F93\u5165\u6709\u6548\u7684 API \u5BC6\u94A5\u6216\u5B8C\u6574\u5730\u5740.";
    private const string MissingSecretMessage = "API \u5730\u5740\u4E2D\u6CA1\u6709\u627E\u5230\u5BC6\u94A5.";
    private const string TooLongMessage = "\u5185\u5BB9\u8D85\u8FC7 5000 \u5B57.";
    private const string SendingMessage = "\u6B63\u5728\u53D1\u9001...";
    private const string SuccessMessage = "\u5DF2\u5F55\u5165.";
    private const string TimeoutMessage = "\u8D85\u65F6, \u8BF7\u68C0\u67E5\u7F51\u7EDC\u540E\u91CD\u8BD5.";
    private const string NetworkErrorMessage = "\u7F51\u7EDC\u9519\u8BEF, \u8BF7\u7A0D\u540E\u91CD\u8BD5.";
    private const string FailedMessagePrefix = "\u5F55\u5165\u5931\u8D25";
    private const string SendText = "\u53D1\u9001";
    private const string SendingText = "\u6B63\u5728\u53D1\u9001...";
    private const string ApiPathMarker = "/sapi/";

    private readonly SettingsService _settingsService;
    private readonly CatNoteApiClient _apiClient;

    private string _contentText = string.Empty;
    private string _apiSecret = string.Empty;
    private bool _rememberSecret = true;
    private bool _isBusy;
    private string _statusMessage = ReadyMessage;
    private Brush _statusBrush = Brushes.Gray;

    public MainViewModel(SettingsService settingsService, CatNoteApiClient apiClient)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        SendCommand = new RelayCommand(SendAsync, () => CanSend);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public RelayCommand SendCommand { get; }

    public string ContentText
    {
        get => _contentText;
        set
        {
            if (SetProperty(ref _contentText, value))
            {
                OnPropertyChanged(nameof(ContentLengthDisplay));
                OnPropertyChanged(nameof(ContentLengthBrush));
                SendCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ApiSecret
    {
        get => _apiSecret;
        set
        {
            if (SetProperty(ref _apiSecret, value))
            {
                SendCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool RememberSecret
    {
        get => _rememberSecret;
        set => SetProperty(ref _rememberSecret, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(SendButtonText));
                SendCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public Brush StatusBrush
    {
        get => _statusBrush;
        private set => SetProperty(ref _statusBrush, value);
    }

    public string ContentLengthDisplay => $"{ContentLength} / {MaxContentLength}";

    public Brush ContentLengthBrush => ContentLength > MaxContentLength ? Brushes.IndianRed : Brushes.Gray;

    public string SendButtonText => IsBusy ? SendingText : SendText;

    private int ContentLength => string.IsNullOrEmpty(ContentText) ? 0 : ContentText.Length;

    private bool CanSend =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(ContentText) &&
        !string.IsNullOrWhiteSpace(ApiSecret) &&
        ContentLength <= MaxContentLength;

    public async Task InitializeAsync()
    {
        var settings = await _settingsService.LoadAsync();
        RememberSecret = settings.RememberSecret;

        if (RememberSecret)
        {
            ApiSecret = settings.ApiSecret ?? string.Empty;
        }

        UpdateStatus(ReadyMessage, Brushes.Gray);
    }

    public Task PersistSettingsAsync()
    {
        var settings = new AppSettings
        {
            RememberSecret = RememberSecret,
            ApiSecret = RememberSecret ? (ApiSecret ?? string.Empty).Trim() : string.Empty
        };

        return _settingsService.SaveAsync(settings);
    }

    private async Task SendAsync()
    {
        var content = ContentText ?? string.Empty;

        if (string.IsNullOrWhiteSpace(content))
        {
            UpdateStatus(EmptyContentMessage, Brushes.IndianRed);
            return;
        }

        if (content.Length > MaxContentLength)
        {
            UpdateStatus(TooLongMessage, Brushes.IndianRed);
            return;
        }

        if (!TryResolveSecret(ApiSecret, out var secret, out var secretError))
        {
            UpdateStatus(secretError, Brushes.IndianRed);
            return;
        }

        IsBusy = true;
        UpdateStatus(SendingMessage, Brushes.DodgerBlue);

        try
        {
            var result = await _apiClient.SendAsync(secret, content, CancellationToken.None);
            if (result.IsSuccess)
            {
                UpdateStatus(SuccessMessage, Brushes.ForestGreen);
            }
            else
            {
                var message = $"{FailedMessagePrefix} (HTTP {result.StatusCode}).";
                UpdateStatus(message, Brushes.IndianRed);
            }

            await PersistSettingsAsync();
        }
        catch (TaskCanceledException)
        {
            UpdateStatus(TimeoutMessage, Brushes.IndianRed);
        }
        catch (HttpRequestException)
        {
            UpdateStatus(NetworkErrorMessage, Brushes.IndianRed);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateStatus(string message, Brush brush)
    {
        StatusMessage = message;
        StatusBrush = brush;
    }

    private bool TryResolveSecret(string? rawInput, out string secret, out string errorMessage)
    {
        secret = string.Empty;
        errorMessage = string.Empty;

        var trimmed = (rawInput ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            errorMessage = EmptySecretMessage;
            return false;
        }

        var hasApiPath = trimmed.Contains("api.catnote.cn/sapi/", StringComparison.OrdinalIgnoreCase);
        var looksLikeUrl = trimmed.StartsWith("http", StringComparison.OrdinalIgnoreCase);

        if (!looksLikeUrl && !hasApiPath)
        {
            secret = trimmed;
            return true;
        }

        var candidate = trimmed;
        if (!looksLikeUrl)
        {
            candidate = $"https://{trimmed.TrimStart('/')}";
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            errorMessage = InvalidSecretMessage;
            return false;
        }

        var path = uri.AbsolutePath ?? string.Empty;
        var markerIndex = path.IndexOf(ApiPathMarker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            errorMessage = InvalidSecretMessage;
            return false;
        }

        var remainder = path[(markerIndex + ApiPathMarker.Length)..];
        var extracted = remainder.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (extracted.Length == 0)
        {
            errorMessage = MissingSecretMessage;
            return false;
        }

        secret = extracted[0];
        return !string.IsNullOrWhiteSpace(secret);
    }

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return false;
        }

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
