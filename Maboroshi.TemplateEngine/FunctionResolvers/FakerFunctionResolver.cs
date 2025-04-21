namespace Maboroshi.TemplateEngine.FunctionResolvers;

internal class FakerFunctionResolver(IFakerAdapter fakerAdapter) : IFunctionResolver
{
    private readonly IFakerAdapter _fakerAdapter = fakerAdapter;

    public ReturnType? TryResolve(string functionName, params ReturnType[] additionalArguments)
    {
        return functionName switch
        {
            "faker" => GetFakerValue(additionalArguments),

            "number" => _fakerAdapter.GetFakeValue("number", additionalArguments),
            "int" => _fakerAdapter.GetFakeValue("int", additionalArguments),
            "float" => _fakerAdapter.GetFakeValue("float", additionalArguments),
            "boolean" => _fakerAdapter.GetFakeValue("bool", additionalArguments),
            "title" => _fakerAdapter.GetFakeValue("name.jobtitle", additionalArguments),
            "firstname" => _fakerAdapter.GetFakeValue("name.firstname", additionalArguments),
            "lastname" => _fakerAdapter.GetFakeValue("name.lastname", additionalArguments),
            "company" => _fakerAdapter.GetFakeValue("company.companyname", additionalArguments),
            "domain" => _fakerAdapter.GetFakeValue("internet.domainname", additionalArguments),
            "tld" => _fakerAdapter.GetFakeValue("internet.domainsuffix", additionalArguments),
            "email" => _fakerAdapter.GetFakeValue("internet.email", additionalArguments),
            "street" => _fakerAdapter.GetFakeValue("address.streetname", additionalArguments),
            "city" => _fakerAdapter.GetFakeValue("address.city", additionalArguments),
            "country" => _fakerAdapter.GetFakeValue("address.country", additionalArguments),
            "countrycode" => _fakerAdapter.GetFakeValue("address.countrycode", additionalArguments),
            "zipcode" => _fakerAdapter.GetFakeValue("address.zipcode", additionalArguments),
            "postcode" => _fakerAdapter.GetFakeValue("address.zipcode", additionalArguments),
            "lat" => _fakerAdapter.GetFakeValue("address.latitude", additionalArguments),
            "long" => _fakerAdapter.GetFakeValue("address.longitude", additionalArguments),
            "phone" => _fakerAdapter.GetFakeValue("phone.phonenumber", additionalArguments),
            "color" => _fakerAdapter.GetFakeValue("internet.color", additionalArguments),
            "hexcolor" => _fakerAdapter.GetFakeValue("internet.color", additionalArguments),
            "guid" => _fakerAdapter.GetFakeValue("guid", additionalArguments),
            "uuid" => _fakerAdapter.GetFakeValue("uuid", additionalArguments),
            "ipv4" => _fakerAdapter.GetFakeValue("internet.ip", additionalArguments),
            "ipv6" => _fakerAdapter.GetFakeValue("internet.ipv6", additionalArguments),
            "lorem" => _fakerAdapter.GetFakeValue("lorem.text", additionalArguments),


            _ => null,
        };
    }

    private ReturnType? GetFakerValue(ReturnType[] additionalArguments)
    {
        if (additionalArguments == null || additionalArguments.Length == 0)
        {
            throw new Exception("faker function requires a path");
        }

        if (additionalArguments[0] is StringReturn path)
            return _fakerAdapter.GetFakeValue(path, additionalArguments.Skip(1).ToArray());
        else
            throw new Exception("faker function requires a path");
    }
}
