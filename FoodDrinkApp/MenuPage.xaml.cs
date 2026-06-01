using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

// Menu builder page. Users search for dishes from the catalogue and add them
// to a running list with adjustable quantities and a running calorie total.
// All menu state lives in the singleton MenuService so it persists across tabs.
public partial class MenuPage : ContentPage
{
    private List<FoodItem>? _allItems;
    private readonly MenuService _menu = MenuService.Instance;

    public MenuPage()
    {
        InitializeComponent();
        // React to external changes (e.g. if the menu is modified from another page).
        _menu.PropertyChanged += OnMenuChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        RefreshMenuView();
    }

    // MenuService raises PropertyChanged when items are added, removed, or quantities change.
    private void OnMenuChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(RefreshMenuView);
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await SearchAsync(e.NewTextValue);
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await SearchAsync(SearchBar.Text);
    }

    // Filters the full data catalogue by name, category, description, or tags
    // and shows matching results above the current menu list.
    private async Task SearchAsync(string? query)
    {
        try
        {
            _allItems ??= (await FoodCatalogService.SearchAsync(null)).ToList();

            List<FoodItem> results;
            if (string.IsNullOrWhiteSpace(query))
            {
                results = _allItems;
                SearchResults.IsVisible = false;
                NoResultsLabel.IsVisible = false;
                ResultCountLabel.Text = string.Empty;
                return;
            }

            var normalised = query.Trim();
            results = _allItems
                .Where(item =>
                    item.Name.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                    item.Category.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                    item.Description.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                    item.Tags.Contains(normalised, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Name)
                .ToList();

            if (results.Count > 0)
            {
                SearchResults.ItemsSource = results;
                SearchResults.IsVisible = true;
                NoResultsLabel.IsVisible = false;
                ResultCountLabel.Text = $"{results.Count} dish{(results.Count > 1 ? "es" : "")} found";
            }
            else
            {
                SearchResults.IsVisible = false;
                NoResultsLabel.IsVisible = true;
                ResultCountLabel.Text = "0 dishes found";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Search error", ex.Message, "OK");
        }
    }

    // Looks up the tapped item by id and adds it to the menu service.
    private async void OnAddToMenuClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            var item = await FoodCatalogService.GetByIdAsync(id);
            if (item is not null)
            {
                _menu.AddItem(item);
                SemanticScreenReader.Announce($"{item.Name} added to menu");
            }
        }
    }

    private void OnIncrementClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is FoodItem item)
        {
            _menu.IncrementQuantity(item);
        }
    }

    // Decrement removes the entry entirely when quantity would drop below one.
    private void OnDecrementClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is FoodItem item)
        {
            _menu.DecrementQuantity(item);
        }
    }

    // Prompts for confirmation before clearing the whole menu.
    private async void OnClearMenuClicked(object? sender, EventArgs e)
    {
        if (!_menu.HasItems) return;

        var confirmed = await DisplayAlert("Clear Menu",
            "Remove all items from your menu?", "Yes, clear", "Cancel");
        if (confirmed)
        {
            _menu.Clear();
            SemanticScreenReader.Announce("Menu cleared");
        }
    }

    // Toggles between the empty banner and the full menu list depending on state.
    private void RefreshMenuView()
    {
        var hasItems = _menu.HasItems;

        EmptyMenuBanner.IsVisible = !hasItems;
        MenuCollection.IsVisible = hasItems;
        TotalCard.IsVisible = hasItems;

        if (hasItems)
        {
            MenuCollection.ItemsSource = _menu.Entries.ToList();
            TotalCaloriesLabel.Text = $"{_menu.TotalCalories}";
            TotalKcalLabel.Text = _menu.TotalCalories == 1 ? "kcal" : "kcal total";
            TotalItemsLabel.Text = $"{_menu.ItemCount} item{(_menu.ItemCount > 1 ? "s" : "")}";
        }
    }
}
