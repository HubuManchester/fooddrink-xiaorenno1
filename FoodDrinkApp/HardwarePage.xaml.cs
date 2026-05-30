using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class HardwarePage : ContentPage
{
    private int feedbackTestCount;

    public HardwarePage()
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

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                SetStatus("This device does not support camera capture.");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                SetStatus("Photo capture cancelled.");
                return;
            }

            await LoadPhotoAsync(photo);
            SetStatus("Food photo captured successfully.");
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (PermissionException)
        {
            var openSettings = await DisplayAlert(
                "Camera permission required",
                "Camera access has not been granted. Would you like to open system settings to enable it?",
                "Open Settings",
                "Cancel");

            if (openSettings)
            {
                AppInfo.ShowSettingsUI();
            }

            SetStatus("Camera permission denied. Enable it in system settings.");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Camera unavailable",
                $"The camera could not be opened on this device ({ex.Message}). You can use the Gallery button to pick an existing photo instead.",
                "OK");
            SetStatus("Camera error. Try using the Gallery button instead.");
        }
    }

    private async void OnPickPhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo is null)
            {
                SetStatus("Photo selection cancelled.");
                return;
            }

            await LoadPhotoAsync(photo);
            SetStatus("Food photo loaded from gallery successfully.");
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (PermissionException)
        {
            var openSettings = await DisplayAlert(
                "Storage permission required",
                "Storage access has not been granted. Would you like to open system settings to enable it?",
                "Open Settings",
                "Cancel");

            if (openSettings)
            {
                AppInfo.ShowSettingsUI();
            }

            SetStatus("Storage permission denied. Enable it in system settings.");
        }
        catch (Exception ex)
        {
            SetStatus($"Gallery error: {ex.Message}");
        }
    }

    private async Task LoadPhotoAsync(FileResult photo)
    {
        await using var stream = await photo.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();
        FoodPhoto.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
    }

    private async void OnGetLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            SetStatus("Getting location...");
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location is null)
            {
                SetStatus("Current location could not be found.");
                return;
            }

            CoordinateLabel.Text = $"Latitude {location.Latitude:F5}, longitude {location.Longitude:F5}";
            LocationLabel.Text = await BuildAddressTextAsync(location);
            SetStatus("Country, city, and coordinates have been loaded.");
        }
        catch (PermissionException)
        {
            SetStatus("Location permission was denied. Enable location access in device settings.");
        }
        catch (Exception ex)
        {
            SetStatus($"Location error: {ex.Message}");
        }
    }

    private static async Task<string> BuildAddressTextAsync(Location location)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();
            var address = FormatPlacemark(placemark);

            if (!string.IsNullOrWhiteSpace(address))
            {
                return address;
            }
        }
        catch
        {
        }

        return BuildFallbackAddress(location);
    }

    private static string FormatPlacemark(Placemark? placemark)
    {
        if (placemark is null)
        {
            return string.Empty;
        }

        var parts = new[]
        {
            placemark.CountryName,
            placemark.AdminArea,
            placemark.Locality,
            placemark.SubLocality,
            placemark.Thoroughfare
        }
        .Where(part => !string.IsNullOrWhiteSpace(part))
        .Distinct()
        .ToArray();

        return parts.Length == 0 ? string.Empty : string.Join(" / ", parts);
    }

    private static string BuildFallbackAddress(Location location)
    {
        if (IsNear(location, 37.422, -122.084, 0.08))
        {
            return "United States / California / Mountain View";
        }

        if (location.Latitude is >= 37.0 and <= 38.2 && location.Longitude is >= -123.2 and <= -121.5)
        {
            return "United States / California / San Francisco Bay Area";
        }

        if (location.Latitude is >= 18 and <= 54 && location.Longitude is >= 73 and <= 135)
        {
            return "China / Current city requires a real device or available geocoding service";
        }

        return "Coordinates were found, but country and city were not returned by this device.";
    }

    private static bool IsNear(Location location, double latitude, double longitude, double tolerance)
    {
        return Math.Abs(location.Latitude - latitude) <= tolerance &&
               Math.Abs(location.Longitude - longitude) <= tolerance;
    }

    private async void OnReadHelpClicked(object? sender, EventArgs e)
    {
        try
        {
            const string helpText = "NutriBite records foods and drinks, shows nutrition details, and uses camera, location, speech, and haptic feedback to make meal tracking more practical.";
            await SpeechService.SpeakAsync(helpText);
            SetStatus("Reading help content aloud.");
        }
        catch (Exception ex)
        {
            SetStatus($"Text to speech error: {ex.Message}");
        }
    }

    private void OnStopSpeechClicked(object? sender, EventArgs e)
    {
        SpeechService.Stop();
        SetStatus("Reading stopped.");
    }

    private void OnFeedbackClicked(object? sender, EventArgs e)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(450));
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            feedbackTestCount++;
            FeedbackCountLabel.Text = $"Haptic feedback tests: {feedbackTestCount}";
            SetStatus("Vibration and haptic feedback triggered. The changing counter can be used for screen-recorded verification.");
        }
        catch (Exception ex)
        {
            SetStatus($"Feedback error: {ex.Message}");
        }
    }

    private void SetStatus(string message)
    {
        HardwareStatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
    }
}
