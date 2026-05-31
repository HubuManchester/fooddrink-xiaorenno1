using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class MenuPage : ContentPage
{
    private List<FoodItem>? _allItems;
    private readonly MenuService _menu = MenuService.Instance;

    public MenuPage()
    {
        InitializeComponent();
        _menu.PropertyChanged += OnMenuChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        RefreshMenuView();
    }

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

    private void OnDecrementClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is FoodItem item)
        {
            _menu.DecrementQuantity(item);
        }
    }

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
