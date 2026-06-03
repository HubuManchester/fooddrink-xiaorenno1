# NutriBite — Food & Drink Tracker

NutriBite is a cross-platform .NET MAUI application developed for the **6G6Z0014 Mobile Computing** module (Manchester Metropolitan University). It allows users to browse, search, and log food and drink records, view nutrition details, and interact with onboard mobile hardware such as the camera, location services, and text-to-speech.

## Features

- **Food catalogue** — browse pre-loaded items with photos, nutrition breakdown, and allergy notes
- **Search & filter** — real‑time text search across names, categories, descriptions, and tags
- **Add record form** — input name, category, description, macros, and optional image URL with validation
- **Detail page** — full nutrition card, about section, speech read‑out, and vibration reminder
- **Menu builder** — search dishes, add/remove items, track total calories and item count
- **Hardware demo page** — camera capture, gallery pick, geolocation with reverse geocoding, text‑to‑speech, haptic feedback, and vibration
- **Settings** — system/light/dark theme picker, large text toggle for accessibility
- **Help & About** — in‑app guide covering all features, accessibility (WCAG 2.1), and hardware notes

## Accessibility (WCAG 2.1)

- Semantic heading levels (`Level1`, `Level2`) on every page
- `SemanticProperties.Description`, `Hint`, and `HeadingLevel` across all interactive elements
- `SemanticScreenReader.Announce` used for validation, save confirmations, and status changes
- Dark/Light theme support via `AppThemeBinding`
- Large text mode (1.22× scale) applied to labels, buttons, entries, editors, pickers, and search bars
- High-contrast colour palette and consistent layout

## Mobile Hardware Used

| Feature | Implementation |
|---|---|
| Camera | `MediaPicker.Default.CapturePhotoAsync()` with permission handling |
| Gallery | `MediaPicker.Default.PickPhotoAsync()` |
| Location | `Geolocation.Default.GetLocationAsync()` + `Geocoding.Default.GetPlacemarksAsync()` |
| Text‑to‑Speech | `SpeechService.SpeakAsync()` on detail and help pages |
| Vibration | `Vibration.Default.Vibrate()` for reminders and confirmations |
| Haptic Feedback | `HapticFeedback.Default.Perform()` on save, capture, and test |

## Project Structure

```
FoodDrinkApp/
├── Models/
│   ├── FoodItem.cs          # Food/drink data model
│   └── MenuEntry.cs         # Menu entry with quantity
├── Services/
│   ├── AccessibilityService.cs  # Font scaling across pages
│   ├── FoodCatalogService.cs    # Local + mockAPI data layer
│   ├── MenuService.cs           # Menu state management
│   ├── MockApiConfig.cs         # API endpoint configuration
│   └── SpeechService.cs         # Text-to-speech wrapper
├── AppShell.xaml             # Shell tab navigation
├── MainPage.xaml             # Food catalogue list
├── AddItemPage.xaml          # Add record form
├── FoodDetailPage.xaml       # Nutrition details
├── MenuPage.xaml             # Menu builder
├── HardwarePage.xaml         # Hardware demo
├── SettingsPage.xaml         # Theme & accessibility
├── HelpPage.xaml             # Help & about
└── Platforms/                # Platform-specific configs
```

## Running the App

### Prerequisites

- .NET 9 SDK with MAUI workload
- Visual Studio 2022 (recommended)

### Commands

```powershell
# Windows
dotnet build -f net9.0-windows10.0.19041.0

# Android
dotnet build -f net9.0-android
```

Open `FoodDrinkApp.csproj` in Visual Studio and deploy to an Android emulator, Android device, or Windows Machine target.

## Development Plan

1. Initial project setup with Shell navigation and data models
2. MainPage with CollectionView, search, and tap-to-detail
3. AddItemPage with form validation and save flow
4. FoodDetailPage with nutrition display and speech
5. MenuPage with search, add/remove, and total counter
6. HardwarePage with camera, location, speech, and haptic feedback
7. SettingsPage with theme picker and large text toggle
8. HelpPage with feature guide and accessibility documentation
9. Accessibility pass (semantic properties, WCAG compliance)
10. README, deployment testing, and screencast preparation

## Module

- **Module:** 6G6Z0014 – Mobile Computing
- **Institution:** Manchester Metropolitan University
- **Assessment:** 1CWK100
- **Name:** Baichuan Jiang
