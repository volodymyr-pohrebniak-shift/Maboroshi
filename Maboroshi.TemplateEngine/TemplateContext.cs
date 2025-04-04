namespace Maboroshi.TemplateEngine;

public class TemplateContext
{
    private readonly List<Dictionary<string, ReturnType>> _contexts = [[]];

    public void InitializeScope()
    {
        _contexts.Add([]);
    }

    public void ReleaseScope()
    {
        if (_contexts.Count <= 1)
            return;
        _contexts.RemoveAt(_contexts.Count - 1);
    }

    public ReturnType GetVariable(string name)
    {
        for (var i = _contexts.Count - 1; i >= 0; i--)
        {
            if (_contexts[i].TryGetValue(name, out var ret))
                return ret;
        }
        throw new Exception($"Variable {name} doesn't exist");
    }

    public void SetVariable(string name, ReturnType value)
    {
        _contexts[^1][name] = value;
    }
}
