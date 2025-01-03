using System.Text.Json.Serialization;

namespace Maboroshi.Web.Models;

public class Environment
{
    public string Name { get; }
    public bool IsActive { get; }
    public IEnumerable<MockedRoute> Routes { get; }

    [JsonConstructor]
    public Environment(string name, IEnumerable<MockedRoute> routes, bool isActive)
    {
        Name = name;
        Routes = routes ?? [];
        IsActive = isActive;
    }
}
