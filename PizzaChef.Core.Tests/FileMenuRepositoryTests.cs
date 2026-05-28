using Microsoft.Extensions.Options;
using PizzaChef.Core.Storage;

namespace PizzaChef.Core.Tests;

public class FileMenuRepositoryTests : IDisposable
{
    private readonly string _tempDir;

    public FileMenuRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "pizzachef-menu-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    [Fact]
    public async Task GetMenu_ReadsAndDeserializesFile()
    {
        const string json = """
        {
          "schemaVersion": 1,
          "scrapedAt": "2026-05-28T08:14:22+02:00",
          "source": "test",
          "currency": "EUR",
          "categories": [
            { "id": "pizza", "name": "Pizza", "sortOrder": 10 }
          ],
          "items": [
            {
              "id": "pizza-margherita",
              "categoryId": "pizza",
              "name": "Pizza Margherita",
              "description": "Test",
              "price": 9.9,
              "variants": [],
              "allergens": ["A"],
              "available": true
            }
          ],
          "allergenLegend": { "A": "Glutenhaltiges Getreide" }
        }
        """;
        var path = Path.Combine(_tempDir, "speisekarte.json");
        await File.WriteAllTextAsync(path, json);

        var repo = new FileMenuRepository(
            Options.Create(new StorageOptions { DataDirectory = _tempDir, MenuFileName = "speisekarte.json" }));

        var menu = await repo.GetMenuAsync();

        Assert.Equal(1, menu.SchemaVersion);
        Assert.Single(menu.Categories);
        Assert.Single(menu.Items);
        Assert.Equal("Pizza Margherita", menu.Items[0].Name);
        Assert.Equal(9.9m, menu.Items[0].Price);
    }

    [Fact]
    public async Task GetMenu_MissingFile_Throws()
    {
        var repo = new FileMenuRepository(
            Options.Create(new StorageOptions { DataDirectory = _tempDir, MenuFileName = "missing.json" }));

        await Assert.ThrowsAsync<FileNotFoundException>(() => repo.GetMenuAsync());
    }
}
