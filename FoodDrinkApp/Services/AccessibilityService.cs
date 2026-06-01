using System.Runtime.CompilerServices;

namespace FoodDrinkApp.Services;

// Applies a uniform font scale across every page so users can toggle larger text
// in Settings. The scale factor is applied on top of each control's original size,
// which is captured once and cached to avoid drift when toggling on and off.
public static class AccessibilityService
{
    // 1.22 × base size gives a noticeable but not breaking increase.
    private const double LargeTextScale = 1.22;
    private static readonly ConditionalWeakTable<BindableObject, FontSizeStore> OriginalFontSizes = new();

    public static bool LargeTextEnabled { get; set; }

    // Walks the visual tree of the given root and rescales every text-bearing control.
    public static void ApplyFontScale(Element root)
    {
        ApplyToElement(root);

        if (root is not IVisualTreeElement visualTreeElement)
        {
            return;
        }

        foreach (var child in visualTreeElement.GetVisualChildren().OfType<Element>())
        {
            ApplyFontScale(child);
        }
    }

    private static void ApplyToElement(Element element)
    {
        var scale = LargeTextEnabled ? LargeTextScale : 1.0;

        switch (element)
        {
            case Label label:
                label.FontSize = GetOriginalFontSize(label, label.FontSize) * scale;
                break;
            case Button button:
                button.FontSize = GetOriginalFontSize(button, button.FontSize) * scale;
                break;
            case Entry entry:
                entry.FontSize = GetOriginalFontSize(entry, entry.FontSize) * scale;
                break;
            case Editor editor:
                editor.FontSize = GetOriginalFontSize(editor, editor.FontSize) * scale;
                break;
            case Picker picker:
                picker.FontSize = GetOriginalFontSize(picker, picker.FontSize) * scale;
                break;
            case SearchBar searchBar:
                searchBar.FontSize = GetOriginalFontSize(searchBar, searchBar.FontSize) * scale;
                break;
        }
    }

    // Stores the first seen font size per control so we always scale from the original,
    // not from an already-scaled value.
    private static double GetOriginalFontSize(BindableObject control, double currentSize)
    {
        var store = OriginalFontSizes.GetOrCreateValue(control);
        if (!store.HasValue)
        {
            store.Value = currentSize > 0 ? currentSize : 14;
            store.HasValue = true;
        }

        return store.Value;
    }

    private sealed class FontSizeStore
    {
        public bool HasValue { get; set; }
        public double Value { get; set; }
    }
}
