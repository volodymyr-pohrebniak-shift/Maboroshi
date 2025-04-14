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
        object? result = name switch
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
            "commerce.price" => double.TryParse(_faker.Commerce.Price(), out var price) ? price : _faker.Commerce.Price(),
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
            "date.past" => args.Length == 2 && args[0] is NumberReturn y1 && args[1] is StringReturn ds1 && TryParseDate(ds1.Value) is DateTime d1
                ? _faker.Date.Past((int)y1.Value, d1)
                : _faker.Date.Past(),

            "date.future" => args.Length == 2 && args[0] is NumberReturn y2 && args[1] is StringReturn ds2 && TryParseDate(ds2.Value) is DateTime d2
                ? _faker.Date.Future((int)y2.Value, d2)
                : _faker.Date.Future(),

            "date.between" => args.Length == 2 && args[0] is StringReturn ds3 && args[1] is StringReturn ds4 && TryParseDate(ds3.Value) is DateTime d3 && TryParseDate(ds4.Value) is DateTime d4
                ? _faker.Date.Between(d3, d4)
                : _faker.Date.Between(DateTime.Now.AddDays(-10), DateTime.Now),

            "date.recent" => args.Length == 1 && args[0] is NumberReturn days
                ? _faker.Date.Recent((int)days.Value)
                : _faker.Date.Recent(),

            "date.soon" => args.Length == 1 && args[0] is NumberReturn soonDays
                ? _faker.Date.Soon((int)soonDays.Value)
                : _faker.Date.Soon(),

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
            "number" => _faker.Random.Number(),
            "digits" => int.Parse(_faker.Random.Digits(1)[0].ToString()),
            "even" => _faker.Random.Even(),
            "odd" => _faker.Random.Odd(),
            "double" => _faker.Random.Double(),
            "decimal" => (double)_faker.Random.Decimal(),
            "float" => _faker.Random.Float(),
            "byte" => _faker.Random.Byte(),
            "sbyte" => _faker.Random.SByte(),
            "int" => _faker.Random.Int(),
            "uint" => _faker.Random.UInt(),
            "long" => _faker.Random.Long(),
            "ulong" => _faker.Random.ULong(),
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
            "words" => _faker.Random.Words(),
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
            string s => new StringReturn(s),
            bool b => new BoolReturn(b),
            _ => null
        };
    }

    private static DateTime? TryParseDate(string? input)
    {
        return DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : null;
    }
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
