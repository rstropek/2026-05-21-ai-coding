using System.Text.Json;
using Microsoft.Extensions.Options;
using PizzaChef.Core.Models;

namespace PizzaChef.Core.Storage;

public sealed class FileMenuRepository : IMenuRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly StorageOptions _options;

    public FileMenuRepository(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<Menu> GetMenuAsync(CancellationToken cancellationToken = default)
    {
        var dataDir = Path.IsPathRooted(_options.DataDirectory)
            ? _options.DataDirectory
            : Path.Combine(AppContext.BaseDirectory, _options.DataDirectory);
        var path = Path.Combine(dataDir, _options.MenuFileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Menu file not found at '{path}'.", path);
        }

        await using var stream = File.OpenRead(path);
        var menu = await JsonSerializer.DeserializeAsync<Menu>(stream, JsonOptions, cancellationToken);
        return menu ?? throw new InvalidOperationException("Menu file is empty or invalid.");
    }
}
