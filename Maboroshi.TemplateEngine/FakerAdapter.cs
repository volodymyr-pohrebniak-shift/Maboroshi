using Bogus;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Maboroshi.TemplateEngine;

interface IFakerAdapter
{
    public ReturnType? GetFakeValue(string name, params ReturnType[] args);
}

internal class StaticFakerAdapter : IFakerAdapter
{
    private static readonly Faker _faker = new();

    public ReturnType? GetFakeValue(string name, params ReturnType[] args)
    {
        object? result = name.ToLowerInvariant() switch
        {
            // Address
            "address.zipcode" => _faker.Address.ZipCode(),
            "address.city" => _faker.Address.City(),
            "address.streetaddress" => _faker.Address.StreetAddress(),
            "address.cityprefix" => _faker.Address.CityPrefix(),
            "address.citysuffix" => _faker.Address.CitySuffix(),
            "address.streetname" => _faker.Address.StreetName(),
            "address.buildingnumber" => _faker.Address.BuildingNumber(),
            "address.streetsuffix" => _faker.Address.StreetSuffix(),
            "address.secondaryaddress" => _faker.Address.SecondaryAddress(),
            "address.county" => _faker.Address.County(),
            "address.country" => _faker.Address.Country(),
            "address.fulladdress" => _faker.Address.FullAddress(),
            "address.countrycode" => _faker.Address.CountryCode(),
            "address.state" => _faker.Address.State(),
            "address.stateabbr" => _faker.Address.StateAbbr(),
            "address.latitude" => _faker.Address.Latitude(),
            "address.longitude" => _faker.Address.Longitude(),
            "address.direction" => _faker.Address.Direction(),
            "address.cardinaldirection" => _faker.Address.CardinalDirection(),
            "address.ordinaldirection" => _faker.Address.OrdinalDirection(),

            // Commerce
            "commerce.department" => _faker.Commerce.Department(),
            "commerce.price" => Math.Round(_faker.Random.Decimal(1, 1000), 2),
            "commerce.categories" => _faker.Commerce.Categories(1)[0],
            "commerce.productname" => _faker.Commerce.ProductName(),
            "commerce.color" => _faker.Commerce.Color(),
            "commerce.product" => _faker.Commerce.Product(),
            "commerce.productadjective" => _faker.Commerce.ProductAdjective(),
            "commerce.productmaterial" => _faker.Commerce.ProductMaterial(),
            "commerce.ean8" => _faker.Commerce.Ean8(),
            "commerce.ean13" => _faker.Commerce.Ean13(),

            // Company
            "company.companysuffix" => _faker.Company.CompanySuffix(),
            "company.companyname" => _faker.Company.CompanyName(),
            "company.catchphrase" => _faker.Company.CatchPhrase(),
            "company.bs" => _faker.Company.Bs(),

            // Database
            "database.column" => _faker.Database.Column(),
            "database.type" => _faker.Database.Type(),
            "database.collation" => _faker.Database.Collation(),
            "database.engine" => _faker.Database.Engine(),

            // Date
            "date.past" => _faker.Date.Past(IntArg(args, 0, 1), DateArg(args, 1)),

            "date.future" => _faker.Date.Future(IntArg(args, 0, 1), DateArg(args, 1)),

            "date.between" => args.Length switch
            {
                0 => _faker.Date.Between(DateTime.UtcNow.AddDays(-10), DateTime.UtcNow),
                1 => throw new TemplateRuntimeException("date.between needs both the start and the end date"),
                _ => _faker.Date.Between(DateArg(args, 0)!.Value, DateArg(args, 1)!.Value),
            },

            "date.recent" => _faker.Date.Recent(IntArg(args, 0, 1)),

            "date.soon" => _faker.Date.Soon(IntArg(args, 0, 1)),

            // Finance
            "finance.account" => _faker.Finance.Account(),
            "finance.accountname" => _faker.Finance.AccountName(),
            "finance.amount" => _faker.Finance.Amount(),
            "finance.transactiontype" => _faker.Finance.TransactionType(),
            "finance.currency" => _faker.Finance.Currency().Code,
            "finance.creditcardnumber" => _faker.Finance.CreditCardNumber(),
            "finance.creditcardcvv" => _faker.Finance.CreditCardCvv(),
            "finance.bitcoinaddress" => _faker.Finance.BitcoinAddress(),
            "finance.ethereumaddress" => _faker.Finance.EthereumAddress(),
            "finance.routingnumber" => _faker.Finance.RoutingNumber(),
            "finance.bic" => _faker.Finance.Bic(),
            "finance.iban" => _faker.Finance.Iban(),

            // Hacker
            "hacker.abbreviation" => _faker.Hacker.Abbreviation(),
            "hacker.adjective" => _faker.Hacker.Adjective(),
            "hacker.noun" => _faker.Hacker.Noun(),
            "hacker.verb" => _faker.Hacker.Verb(),
            "hacker.ingverb" => _faker.Hacker.IngVerb(),
            "hacker.phrase" => _faker.Hacker.Phrase(),

            // Internet
            "internet.avatar" => _faker.Internet.Avatar(),
            "internet.email" => _faker.Internet.Email(),
            "internet.exampleemail" => _faker.Internet.ExampleEmail(),
            "internet.username" => _faker.Internet.UserName(),
            "internet.usernameunicode" => _faker.Internet.UserNameUnicode(),
            "internet.domainname" => _faker.Internet.DomainName(),
            "internet.domainword" => _faker.Internet.DomainWord(),
            "internet.domainsuffix" => _faker.Internet.DomainSuffix(),
            "internet.ip" => _faker.Internet.Ip(),
            "internet.port" => _faker.Internet.Port(),
            "internet.ipaddress" => _faker.Internet.IpAddress().ToString(),
            "internet.ipendpoint" => _faker.Internet.IpEndPoint().ToString(),
            "internet.ipv6" => _faker.Internet.Ipv6(),
            "internet.ipv6address" => _faker.Internet.Ipv6Address().ToString(),
            "internet.ipv6endpoint" => _faker.Internet.Ipv6EndPoint().ToString(),
            "internet.useragent" => _faker.Internet.UserAgent(),
            "internet.mac" => _faker.Internet.Mac(),
            "internet.password" => _faker.Internet.Password(),
            "internet.color" => _faker.Internet.Color(),
            "internet.protocol" => _faker.Internet.Protocol(),
            "internet.url" => _faker.Internet.Url(),
            "internet.urlwithpath" => _faker.Internet.UrlWithPath(),
            "internet.urlrootedpath" => _faker.Internet.UrlRootedPath(),

            // Lorem
            "lorem.word" => _faker.Lorem.Word(),
            "lorem.words" => string.Join(" ", _faker.Lorem.Words()),
            "lorem.letter" => _faker.Lorem.Letter(),
            "lorem.sentence" => _faker.Lorem.Sentence(),
            "lorem.sentences" => _faker.Lorem.Sentences(),
            "lorem.paragraph" => _faker.Lorem.Paragraph(),
            "lorem.paragraphs" => _faker.Lorem.Paragraphs(),
            "lorem.text" => _faker.Lorem.Text(),
            "lorem.lines" => _faker.Lorem.Lines(),
            "lorem.slug" => _faker.Lorem.Slug(),

            // Name
            "name.firstname" => _faker.Name.FirstName(),
            "name.lastname" => _faker.Name.LastName(),
            "name.fullname" => _faker.Name.FullName(),
            "name.prefix" => _faker.Name.Prefix(),
            "name.suffix" => _faker.Name.Suffix(),
            "name.findname" => _faker.Name.FindName(),
            "name.jobtitle" => _faker.Name.JobTitle(),
            "name.jobdescriptor" => _faker.Name.JobDescriptor(),
            "name.jobarea" => _faker.Name.JobArea(),
            "name.jobtype" => _faker.Name.JobType(),

            // Phone
            "phone.phonenumber" => _faker.Phone.PhoneNumber(),
            "phone.phonenumberformat" => _faker.Phone.PhoneNumberFormat(),

            // Random
            "number" => GetRandomNumber(args),
            "digits" => int.Parse(_faker.Random.Digits(1)[0].ToString()),
            "even" => args.Length == 0 ? _faker.Random.Even() : _faker.Random.Even(IntArg(args, 0, 0), IntArg(args, 1, int.MaxValue)),
            "odd" => args.Length == 0 ? _faker.Random.Odd() : _faker.Random.Odd(IntArg(args, 0, 0), IntArg(args, 1, int.MaxValue)),
            "double" => _faker.Random.Double(NumberArg(args, 0, 0), NumberArg(args, 1, 1)),
            "decimal" => _faker.Random.Decimal((decimal)NumberArg(args, 0, 0), (decimal)NumberArg(args, 1, 1)),
            "float" => _faker.Random.Float((float)NumberArg(args, 0, 0), (float)NumberArg(args, 1, 1)),
            "byte" => _faker.Random.Byte(),
            "sbyte" => _faker.Random.SByte(),
            "int" => _faker.Random.Int(IntArg(args, 0, int.MinValue), IntArg(args, 1, int.MaxValue)),
            "uint" => _faker.Random.UInt(),
            "long" => _faker.Random.Long(-NumberReturn.MaxSafeInteger, NumberReturn.MaxSafeInteger),
            "ulong" => _faker.Random.ULong(0, NumberReturn.MaxSafeInteger),
            "short" => _faker.Random.Short(),
            "ushort" => _faker.Random.UShort(),
            "char" => _faker.Random.Char('a', 'z'),
            "chars" => new string(_faker.Random.Chars('a', 'z', 5)),
            "string" => _faker.Random.String2(10),
            "string2" => _faker.Random.String2(10),
            "hash" => _faker.Random.Hash(),
            "bool" => _faker.Random.Bool(),

            // Misc
            "word" => _faker.Random.Word(),
            "words" => _faker.Random.Words(args.Length > 0 ? WordsCount(args) : null),
            "wordsarray" => string.Join(" ", _faker.Random.WordsArray(3)),
            "guid" => _faker.Random.Guid().ToString(),
            "uuid" => _faker.Random.Uuid().ToString(),
            "randomlocale" => _faker.Random.RandomLocale(),
            "alphanumeric" => _faker.Random.AlphaNumeric(10),

            _ => null
        };

        return result switch
        {
            double d => new NumberReturn(d),
            float f => new NumberReturn(f),
            int i => new NumberReturn(i),
            long l => new NumberReturn(l),
            decimal m => new NumberReturn((double)m),
            byte or sbyte or short or ushort or uint or ulong => new NumberReturn(Convert.ToDouble(result, CultureInfo.InvariantCulture)),
            char c => new StringReturn(c.ToString()),
            string s => new StringReturn(s),
            bool b => new BoolReturn(b),
            DateTime d => new StringReturn(ToIsoUtc(d)),
            _ => null
        };
    }

    // the same format as {{now}}; Bogus dates are local unless a reference date is given
    private static string ToIsoUtc(DateTime date) =>
        (date.Kind == DateTimeKind.Local ? date.ToUniversalTime() : DateTime.SpecifyKind(date, DateTimeKind.Utc))
            .ToString("o", CultureInfo.InvariantCulture);

    private static double NumberArg(ReturnType[] args, int index, double defaultValue)
    {
        if (args.Length <= index)
            return defaultValue;

        return args[index].TryGetNumber(out var value)
            ? value
            : throw new TemplateRuntimeException($"argument {index + 1} should be a number, but got '{args[index]}'");
    }

    // a missing argument falls back to the default, an invalid one is an error
    private static int IntArg(ReturnType[] args, int index, int defaultValue)
    {
        if (args.Length <= index)
            return defaultValue;

        return args[index].TryGetInt(out var value)
            ? value
            : throw new TemplateRuntimeException($"argument {index + 1} should be an integer, but got '{args[index]}'");
    }

    private static DateTime? DateArg(ReturnType[] args, int index)
    {
        if (args.Length <= index)
            return null;

        return TryParseDate(args[index].ToString())
            ?? throw new TemplateRuntimeException($"argument {index + 1} should be a date, but got '{args[index]}'");
    }

    // the count may come from the request, so it's checked before Bogus builds the words; each word takes at least two characters
    private static int WordsCount(ReturnType[] args)
    {
        var count = IntArg(args, 0, 1);
        return count <= TemplateCompilationOptions.DefaultMaxOutputLength / 2
            ? count
            : throw new TemplateLimitExceededException($"words count can't exceed {TemplateCompilationOptions.DefaultMaxOutputLength / 2}");
    }

    private static DateTime? TryParseDate(string? input)
    {
        return DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt) ? dt : null;
    }

    private static int GetRandomNumber(ReturnType[] args) => args.Length == 0
        ? _faker.Random.Number()
        : _faker.Random.Number(IntArg(args, 0, 0), IntArg(args, 1, int.MaxValue));
}

internal class FakerDynamicResolver : IFakerAdapter
{
    private static readonly Faker _faker = new();
    private static readonly ConcurrentDictionary<string, Func<object, object>> _propertiesCache = [];

    public ReturnType? GetFakeValue(string path, params ReturnType[] additionalArguments)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path to faker property cannot be  null or empty");
        }

        if (!_propertiesCache.TryGetValue(path, out var compiledDelegate))
        {
            compiledDelegate = CompileDelegate(path);
            _propertiesCache[path] = compiledDelegate;
        }

        return new StringReturn((string) compiledDelegate(_faker));
    }

    private static Func<object, object> CompileDelegate(string path) {
        var pathParts = path.Split('.');

        var parameter = Expression.Parameter(typeof(object), "faker");
        Expression currentExpression = Expression.Convert(parameter, typeof(Faker));

        foreach (var pathPart in pathParts)
        {
            var currentType = currentExpression.Type;

            var property = currentType.GetProperty(pathPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property != null)
            {
                currentExpression = Expression.Property(currentExpression, property);
            } else
            {
                var method = currentType.GetMethod(pathPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    var argumentExpressions = new List<Expression>();

                    foreach (var param in parameters)
                    {
                        if (param.HasDefaultValue)
                        {
                            argumentExpressions.Add(Expression.Constant(param.DefaultValue, param.ParameterType));
                        }
                        else
                        {
                            throw new InvalidOperationException($"Faker property ${path} requires parameters. It's not supported yet");
                        }
                    }

                    currentExpression = Expression.Call(currentExpression, method, argumentExpressions);
                }
                else
                {
                    throw new InvalidOperationException($"'{pathPart}' is not a valid property or method of '{currentType.Name}'.");
                }
            }
        }

        currentExpression = Expression.Convert(currentExpression, typeof(object));

        return Expression.Lambda<Func<object, object>>(currentExpression, parameter).Compile();
    }
}
