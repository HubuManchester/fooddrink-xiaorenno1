using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class HelpPage : ContentPage
{
    public HelpPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    protected override void OnDisappearing()
    {
        SpeechService.Stop();
        base.OnDisappearing();
    }

    private async void OnReadPageClicked(object? sender, EventArgs e)
    {
        try
        {
            const string text = "NutriBite helps you track foods and drinks. " +
                "Browse the Foods tab and search by name or category. " +
                "Tap View Full Details for nutrition info. " +
                "Use the Menu tab to build a meal. " +
                "The Hardware tab demonstrates the camera, location, text to speech, vibration, and haptic feedback. " +
                "Settings lets you switch between light and dark themes and toggle large text for better readability. " +
                "This app follows WCAG 2.1 accessibility guidelines.";

            await SpeechService.SpeakAsync(text);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Speech unavailable", ex.Message, "OK");
        }
    }

    private void OnStopSpeechClicked(object? sender, EventArgs e)
    {
        SpeechService.Stop();
        SemanticScreenReader.Announce("Reading stopped.");
    }
}
