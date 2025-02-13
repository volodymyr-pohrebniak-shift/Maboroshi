namespace Maboroshi.TemplateEngine.FunctionResolvers;

public interface IFunctionResolver
{
    ReturnType? TryResolve(string functionName, params ReturnType[] additionalArguments);
}
