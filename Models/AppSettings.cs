namespace CatNoteInput;

public sealed class AppSettings
{
    public string ApiSecret { get; set; } = string.Empty;

    public bool RememberSecret { get; set; } = true;
}
