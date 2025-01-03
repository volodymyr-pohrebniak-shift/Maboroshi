using System.Text.Json.Serialization;

namespace Maboroshi.Web.Models;

[method: JsonConstructor]
public class Root(IEnumerable<Environment> environments)
{
    public IEnumerable<Environment> Environments { get; } = environments;
}
