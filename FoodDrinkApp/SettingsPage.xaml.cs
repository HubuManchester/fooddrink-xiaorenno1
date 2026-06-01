using FoodDrinkApp.Services;

namespace FoodDrinkApp;

// Settings page for theme selection and accessibility options.
// Changes to large text mode are applied immediately through AccessibilityService
// and persist for the current session across all pages.
public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        ThemePicker.SelectedIndex = 0;
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
        ApplyLargeTextState();
    }

    // Maps the picker index to the MAUI AppTheme enum.
    // "System" (index 0) follows the device setting; Light and Dark override it.
    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Application.Current!.UserAppTheme = ThemePicker.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        Announce("Theme preference updated.");
    }

    // Toggles the global large-text flag and rescales the current page immediately.
    private void OnLargeTextToggled(object? sender, ToggledEventArgs e)
    {
        AccessibilityService.LargeTextEnabled = e.Value;
        ApplyLargeTextState();
        Announce(e.Value
            ? "Large text mode is on. Page text is now larger."
            : "Large text mode is off. Page text has returned to normal.");
    }

    // Refreshes the preview labels and re-applies the font scale so the user sees
    // the effect right away on the same page.
    private void ApplyLargeTextState()
    {
        AccessibilityService.ApplyFontScale(this);

        LargeTextPreviewTitle.Text = AccessibilityService.LargeTextEnabled
            ? "Large text preview: enlarged"
            : "Large text preview";
        LargeTextPreviewBody.Text = AccessibilityService.LargeTextEnabled
            ? "Text is now noticeably larger. The food and hardware pages will use the same setting."
            : "Turn on the switch to enlarge this preview and other page text.";
    }

    private async void OnHelpClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(HelpPage));
    }

    // Updates the status bar message and pushes it to screen readers.
    private void Announce(string message)
    {
        SettingsStatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
    }
}
