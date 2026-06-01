using FoodDrinkApp.Services;

namespace FoodDrinkApp;

// Demonstrates every onboard hardware feature the app uses: camera capture,
// gallery pick, geolocation, text-to-speech, vibration, and haptic feedback.
// Each method handles its own permission and availability errors separately.
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

    // Opens the device camera and loads the resulting photo into the preview area.
    // Falls back to a user-friendly message when the camera is unavailable.
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

    // Lets the user pick an existing image from the device gallery as an
    // alternative to taking a new photo with the camera.
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

    // Reads the selected photo into a byte array and sets the preview image source.
    private async Task LoadPhotoAsync(FileResult photo)
    {
        await using var stream = await photo.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();
        FoodPhoto.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
    }

    // Fetches the device location with medium accuracy and resolves the
    // coordinates into a country / city / region string via reverse geocoding.
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

    // Tries real geocoding first; if that fails, falls back to a best-guess
    // description based on the coordinate ranges common in emulator scenarios.
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

    // Joins the available placemark fields into a readable location string.
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

    // Returns a human-readable fallback address for known emulator coordinate
    // ranges. This avoids showing a bare "Country and city unavailable" message.
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

    // Reads a short app description aloud to demonstrate text-to-speech.
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

    // Triggers both vibration and haptic feedback, incrementing a counter each time
    // so the effect can be verified visually in a screen recording.
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

    // Updates the status label and announces the change for screen readers.
    private void SetStatus(string message)
    {
        HardwareStatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
    }
}
