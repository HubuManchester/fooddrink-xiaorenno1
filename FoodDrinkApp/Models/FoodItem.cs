using System.Text.Json.Serialization;

namespace FoodDrinkApp.Models;

public sealed class FoodItem
{
    // Each item gets a unique id so it can be referenced in navigation and API calls.
    // Serialized as "id" to match the mockapi.io schema.
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    // Ingredients, flavour notes, or meal context written by the user.
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    // Energy value in kilocalories. Used in the detail page header and card badges.
    [JsonPropertyName("calories")]
    public int Calories { get; set; }

    [JsonPropertyName("protein")]
    public int Protein { get; set; }

    [JsonPropertyName("carbs")]
    public int Carbs { get; set; }

    [JsonPropertyName("fat")]
    public int Fat { get; set; }

    [JsonPropertyName("allergyNote")]
    public string AllergyNote { get; set; } = string.Empty;

    // Space-separated keywords used by the search bar for matching.
    [JsonPropertyName("tags")]
    public string Tags { get; set; } = string.Empty;

    // Can be a remote URL or a local Resources/ path.
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    // Computed labels that the XAML bindings refer to directly.
    [JsonIgnore]
    public string CaloriesLabel => $"{Calories} kcal";

    [JsonIgnore]
    public string MacroSummary => $"Protein {Protein}g, carbs {Carbs}g, fat {Fat}g";

    // Concise sentence used by the screen reader and text-to-speech on the detail page.
    [JsonIgnore]
    public string AccessibleSummary => $"{Name}. {Category}. {Calories} kcal. {MacroSummary}. {AllergyNote}";
}
