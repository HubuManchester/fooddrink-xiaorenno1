using System.Net.Http.Json;
using System.Text.Json;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

// Data access layer that serves food and drink records from one of two sources:
//   1. mockapi.io REST endpoint (when configured)
//   2. A local in-memory list that acts as a fallback during demos or offline runs.
// All methods are static so pages can call them directly without injection setup.
public static class FoodCatalogService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Seed data so the app shows meaningful content even without an API key.
    private static readonly List<FoodItem> LocalFallbackItems =
    [
        new()
        {
            Name = "Berry Yogurt Bowl",
            Category = "Breakfast",
            Description = "Greek yogurt with mixed berries, oats, and a small drizzle of honey. A refreshing and protein-packed way to start your day.",
            Calories = 340,
            Protein = 24,
            Carbs = 42,
            Fat = 8,
            AllergyNote = "Contains dairy and gluten.",
            Tags = "healthy breakfast yogurt berries",
            ImageUrl = "Resources/Images/berry_yogurt_bowl.jpg"
        },
        new()
        {
            Name = "Chicken Brown Rice Box",
            Category = "Lunch",
            Description = "Grilled chicken breast with brown rice, spinach, cucumber, and lemon dressing. Perfect for meal prep.",
            Calories = 520,
            Protein = 38,
            Carbs = 58,
            Fat = 14,
            AllergyNote = "No common allergens recorded.",
            Tags = "meal prep protein lunch",
            ImageUrl = "Resources/Images/chicken_brown_rice_box.jpg"
        },
        new()
        {
            Name = "Iced Matcha Latte",
            Category = "Drink",
            Description = "Matcha, milk, and ice. A lower-sugar version is recommended. Rich in antioxidants and gives a gentle energy boost.",
            Calories = 180,
            Protein = 8,
            Carbs = 22,
            Fat = 6,
            AllergyNote = "Contains dairy unless plant-based milk is selected.",
            Tags = "drink caffeine matcha latte",
            ImageUrl = "Resources/Images/iced_matcha_latte.jpg"
        },
        new()
        {
            Name = "Tomato Wholegrain Pasta",
            Category = "Dinner",
            Description = "Wholegrain pasta with tomato sauce, basil, and roasted vegetables. A comforting Italian-inspired evening meal.",
            Calories = 610,
            Protein = 18,
            Carbs = 92,
            Fat = 16,
            AllergyNote = "Contains gluten.",
            Tags = "vegetarian dinner pasta",
            ImageUrl = "Resources/Images/tomato_wholegrain_pasta.jpg"
        },
        new()
        {
            Name = "Avocado Toast",
            Category = "Breakfast",
            Description = "Sourdough toast topped with smashed avocado, cherry tomatoes, poached egg, and a sprinkle of chilli flakes.",
            Calories = 380,
            Protein = 14,
            Carbs = 36,
            Fat = 20,
            AllergyNote = "Contains gluten and eggs.",
            Tags = "breakfast avocado trendy healthy",
            ImageUrl = "Resources/Images/avocado_toast.jpg"
        },
        new()
        {
            Name = "Salmon Sashimi Bowl",
            Category = "Lunch",
            Description = "Fresh salmon sashimi over sushi rice with edamame, seaweed salad, wasabi, and soy sauce dressing.",
            Calories = 450,
            Protein = 34,
            Carbs = 42,
            Fat = 16,
            AllergyNote = "Contains fish and soy.",
            Tags = "japanese seafood rice bowl lunch",
            ImageUrl = "Resources/Images/salmon_sashimi_bowl.jpg"
        },
        new()
        {
            Name = "Mango Smoothie",
            Category = "Drink",
            Description = "Ripe mango blended with Greek yogurt, a dash of vanilla, and a pinch of turmeric for golden colour.",
            Calories = 220,
            Protein = 6,
            Carbs = 44,
            Fat = 3,
            AllergyNote = "Contains dairy.",
            Tags = "smoothie fruit mango refreshing drink",
            ImageUrl = "Resources/Images/mango_smoothie.jpg"
        },
        new()
        {
            Name = "Beef Burger Deluxe",
            Category = "Dinner",
            Description = "Angus beef patty with aged cheddar, caramelised onions, lettuce, tomato, and special sauce on a brioche bun.",
            Calories = 750,
            Protein = 38,
            Carbs = 52,
            Fat = 42,
            AllergyNote = "Contains gluten, dairy, and eggs.",
            Tags = "burger beef cheese indulgent dinner",
            ImageUrl = "Resources/Images/beef_burger_deluxe.jpg"
        }
    ];

    // Items loaded from either the API or local seed; used by SearchAsync and GetByIdAsync.
    private static List<FoodItem> cachedItems = new(LocalFallbackItems);

    // Lets the UI display a "Loaded from mockapi.io" vs "Loaded from fallback" message.
    public static bool LastLoadUsedMockApi { get; private set; }

    // Filters the full catalogue by name, category, description, or tags.
    // Returns everything sorted alphabetically when query is empty.
    public static async Task<IReadOnlyList<FoodItem>> SearchAsync(string? query)
    {
        var items = await GetAllAsync();

        if (string.IsNullOrWhiteSpace(query))
        {
            return items.OrderBy(item => item.Name).ToList();
        }

        var normalised = query.Trim();
        return items
            .Where(item =>
                item.Name.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Category.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Tags.Contains(normalised, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Name)
            .ToList();
    }

    // Tries the API first if configured; otherwise falls back to the in-memory cache.
    public static async Task<FoodItem?> GetByIdAsync(string id)
    {
        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var item = await HttpClient.GetFromJsonAsync<FoodItem>(
                    $"{MockApiConfig.EndpointUrl.TrimEnd('/')}/{Uri.EscapeDataString(id)}",
                    JsonOptions);

                if (item is not null)
                {
                    return item;
                }
            }
            catch
            {
                // The cache below keeps things working when the network is unreachable.
            }
        }

        return cachedItems.FirstOrDefault(item => item.Id == id);
    }

    // Posts a new item to the API if configured and adds it to the local cache either way.
    public static async Task<FoodItem> AddAsync(FoodItem item)
    {
        if (MockApiConfig.IsConfigured)
        {
            var response = await HttpClient.PostAsJsonAsync(MockApiConfig.EndpointUrl, item, JsonOptions);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<FoodItem>(JsonOptions);
            if (created is not null)
            {
                cachedItems.Add(created);
                return created;
            }
        }

        cachedItems.Add(item);
        return item;
    }

    // Internal fetch that populates the cache from the remote API on first call,
    // then keeps returning the latest snapshot from memory.
    private static async Task<IReadOnlyList<FoodItem>> GetAllAsync()
    {
        if (!MockApiConfig.IsConfigured)
        {
            LastLoadUsedMockApi = false;
            return cachedItems;
        }

        try
        {
            var items = await HttpClient.GetFromJsonAsync<List<FoodItem>>(MockApiConfig.EndpointUrl, JsonOptions);
            if (items is { Count: > 0 })
            {
                cachedItems = items;
                LastLoadUsedMockApi = true;
                return cachedItems;
            }
        }
        catch
        {
            // Silently falls back so the app never crashes just because a demo machine
            // has no network connection.
        }

        LastLoadUsedMockApi = false;
        return cachedItems;
    }
}
