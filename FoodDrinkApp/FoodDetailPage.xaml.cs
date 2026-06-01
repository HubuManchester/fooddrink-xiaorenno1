using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

// Full nutrition breakdown for a single food or drink item.
// Receives the item id through Shell query parameters and loads
// the matching record on arrival.
[QueryProperty(nameof(ItemId), "id")]
public partial class FoodDetailPage : ContentPage
{
    private FoodItem? currentItem;

    public FoodDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    // Make sure speech stops when the user navigates away.
    protected override void OnDisappearing()
    {
        SpeechService.Stop();
        base.OnDisappearing();
    }

    // Called automatically by Shell when navigating with a query parameter.
    public string ItemId
    {
        set => _ = LoadItemAsync(value);
    }

    private async Task LoadItemAsync(string id)
    {
        currentItem = await FoodCatalogService.GetByIdAsync(id);
        BindingContext = currentItem;
        RenderItem();
    }

    // Writes the item data directly to named label controls.
    // The image and macro values are handled through XAML bindings instead.
    private void RenderItem()
    {
        if (currentItem is null)
        {
            NameLabel.Text = "Record not found";
            DescriptionLabel.Text = "The selected food or drink could not be loaded.";
            return;
        }

        NameLabel.Text = currentItem.Name;
        CategoryLabel.Text = currentItem.Category;
        CaloriesLabel.Text = currentItem.CaloriesLabel;
        DescriptionLabel.Text = currentItem.Description;
        AllergyLabel.Text = $"! {currentItem.AllergyNote}";

        // Override the default semantic description so screen readers
        // announce the full accessible summary instead of just the name.
        SemanticProperties.SetDescription(NameLabel, currentItem.AccessibleSummary);
    }

    // Uses the speech service to read the item summary aloud.
    private async void OnSpeakClicked(object? sender, EventArgs e)
    {
        if (currentItem is null)
        {
            await DisplayAlert("Missing record", "There is no nutrition summary to read.", "OK");
            return;
        }

        try
        {
            await SpeechService.SpeakAsync(currentItem.AccessibleSummary);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Text to speech unavailable", ex.Message, "OK");
        }
    }

    private void OnStopSpeechClicked(object? sender, EventArgs e)
    {
        SpeechService.Stop();
        SemanticScreenReader.Announce("Reading stopped.");
    }

    // Demonstrates both Vibration and HapticFeedback as a meal reminder.
    private async void OnVibrateClicked(object? sender, EventArgs e)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            await DisplayAlert("Reminder", "Vibration feedback has been triggered.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Vibration unavailable", ex.Message, "OK");
        }
    }
}
