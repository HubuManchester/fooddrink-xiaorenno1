using FoodDrinkApp.Services;

namespace FoodDrinkApp;

// Main food catalogue page. Shows a scrollable list of FoodItem cards with
// search, pull-to-refresh, and navigation to the detail and add pages.
// Card widths are constrained to 83 % of the screen for a polished look on tablets.
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        await LoadFoodItemsAsync(SearchFoodBar.Text);
        ApplyCardMargins();
    }

    // Responds to layout changes (e.g. rotation) by recalculating card width.
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        ApplyCardMargins(width);
    }

    // Limits the card area to 83 % of the available width so the list
    // does not stretch edge-to-edge on wide screens like tablets.
    private void ApplyCardMargins(double? pageWidth = null)
    {
        var w = pageWidth;
        if (w == null || w <= 0)
        {
            try { w = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density; }
            catch { return; }
        }
        var targetWidth = w.Value * 0.83;
        FoodRefreshView.HorizontalOptions = LayoutOptions.Center;
        FoodRefreshView.WidthRequest = targetWidth;
    }

    // Reloads the collection from the data service, optionally filtered by a search query.
    private async Task LoadFoodItemsAsync(string? query = null)
    {
        FoodCollection.ItemsSource = await FoodCatalogService.SearchAsync(query);
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddItemPage));
    }

    // The detail page is opened by tapping the "View Full Details" button or the card image.
    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            await Shell.Current.GoToAsync($"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(id)}");
        }
    }

    // Tapping the card image navigates to the detail page and gives haptic feedback.
    private async void OnImageTapped(object? sender, TappedEventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is FoodDrinkApp.Models.FoodItem item)
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            await Shell.Current.GoToAsync($"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(item.Id)}");
        }
    }

    // Triggers a new search every time the user types in the search bar.
    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadFoodItemsAsync(e.NewTextValue);
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    // Pull-to-refresh reloads the data and announces the source (API vs. local) so the
    // user knows where the records are coming from.
    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
        FoodRefreshView.IsRefreshing = false;
        var source = FoodCatalogService.LastLoadUsedMockApi ? "mockapi.io" : "local fallback data";
        SemanticScreenReader.Announce($"Food and drink list refreshed. Current source: {source}.");
    }
}
