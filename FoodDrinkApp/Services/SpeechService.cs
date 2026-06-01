namespace FoodDrinkApp.Services;

// Thin wrapper around MAUI's TextToSpeech API that keeps a cancellation token
// so the user can stop playback at any time. The food detail page and the help
// page both use this for reading content aloud.
public static class SpeechService
{
    private static CancellationTokenSource? currentSpeech;

    public static async Task SpeakAsync(string text)
    {
        Stop();

        currentSpeech = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Volume = 0.9f,
            Pitch = 1.05f,
            Locale = await FindEnglishLocaleAsync()
        };

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, options, currentSpeech.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public static Task SpeakChineseAsync(string text) => SpeakAsync(text);

    // Cancels any active speech so a new one can start cleanly.
    public static void Stop()
    {
        if (currentSpeech is null)
        {
            return;
        }

        currentSpeech.Cancel();
        currentSpeech.Dispose();
        currentSpeech = null;
    }

    // Picks the first available English voice so pronunciation is consistent.
    private static async Task<Locale?> FindEnglishLocaleAsync()
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        return locales.FirstOrDefault(locale => locale.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));
    }
}
