using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class AddItemPage : ContentPage
{
    public AddItemPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            var validationMessage = ValidateForm(out var calories, out var protein, out var carbs, out var fat);
            if (validationMessage is not null)
            {
                ShowValidation(validationMessage);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250));
                return;
            }

            var item = new FoodItem
            {
                Name = NameEntry.Text!.Trim(),
                Category = CategoryPicker.SelectedItem?.ToString() ?? "Snack",
                Description = DescriptionEditor.Text!.Trim(),
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                AllergyNote = string.IsNullOrWhiteSpace(AllergyEntry.Text)
                    ? "No allergy note provided."
                    : AllergyEntry.Text.Trim(),
                Tags = $"{NameEntry.Text} {CategoryPicker.SelectedItem} {DescriptionEditor.Text}",
                ImageUrl = ImageUrlEntry.Text?.Trim() ?? string.Empty
            };

            await FoodCatalogService.AddAsync(item);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce("Food record saved.");

            await DisplayAlert(
                "Saved",
                MockApiConfig.IsConfigured
                    ? "The record has been saved to mockapi.io."
                    : "The record has been saved to local fallback data.",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ShowValidation($"The record could not be saved: {ex.Message}");
        }
    }

    private string? ValidateForm(out int calories, out int protein, out int carbs, out int fat)
    {
        calories = protein = carbs = fat = 0;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            return "Please enter a food or drink name.";
        }

        if (NameEntry.Text!.Trim().Length > 100)
        {
            return "Food name is too long (maximum 100 characters).";
        }

        if (CategoryPicker.SelectedIndex < 0)
        {
            return "Please choose a category.";
        }

        if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
        {
            return "Please add a short description.";
        }

        if (DescriptionEditor.Text!.Trim().Length > 500)
        {
            return "Description is too long (maximum 500 characters).";
        }

        var imageUrl = ImageUrlEntry.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(imageUrl) &&
            !imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
            !imageUrl.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase))
        {
            return "Image URL must start with http://, https://, or Resources/.";
        }

        if (imageUrl?.Length > 500)
        {
            return "Image URL is too long (maximum 500 characters).";
        }

        return TryReadNumber(CaloriesEntry.Text, "calories", out calories, 5000)
            ?? TryReadNumber(ProteinEntry.Text, "protein", out protein, 999)
            ?? TryReadNumber(CarbsEntry.Text, "carbs", out carbs, 999)
            ?? TryReadNumber(FatEntry.Text, "fat", out fat, 999);
    }

    private static string? TryReadNumber(string? value, string fieldName, out int number, int max = int.MaxValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            number = 0;
            return null;
        }

        if (!int.TryParse(value, out number))
        {
            number = 0;
            return $"Please enter a valid whole number for {fieldName}.";
        }

        if (number < 0)
        {
            return $"{fieldName} cannot be negative.";
        }

        if (number > max)
        {
            return $"{fieldName} seems too high (maximum {max}).";
        }

        return null;
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationPanel.IsVisible = true;
        SemanticScreenReader.Announce(message);
    }
}
