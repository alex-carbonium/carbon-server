using System;
using System.Collections.Generic;
using System.Linq;
using Bogus.DataSets;
using Bogus;

namespace Carbon.Business.Services
{
    public class DataService
    {
        private readonly BogusDataSet _defaultDataSet = new BogusDataSet();
        private readonly Dictionary<DataField, Func<BogusDataSet, DataContext, string>> _generators;
        private readonly Random _random = new Random();

        public DataService()
        {
            _generators = CreateGenerators();
        }

        public Dictionary<string, List<string>> Discover()
        {
            var result = new Dictionary<string, List<string>>();

            var fields = Enum.GetValues(typeof(DataField)).Cast<DataField>();
            var context = new DataContext();
            foreach (var field in fields)
            {
                var examples = new List<string>(2);
                for (int i = 0; i < 2; i++)
                {
                    examples.Add(_generators[field](_defaultDataSet, context));
                }
                result.Add(field.ToString(), examples);
            }

            return result;
        }

        public List<Dictionary<DataField, string>> Generate(string fieldsString, int rows)
        {
            var dataSet = _defaultDataSet;
            //if (!string.IsNullOrEmpty(seed))
            //{
            //    dataSet = new BogusDataSet(seed.GetHashCode());
            //}

            var fields = fieldsString.Split(',');
            var dataFields = new List<DataField>(fields.Length);
            foreach (var field in fields)
            {
                DataField dataField;
                if (Enum.TryParse(field, true, out dataField))
                {
                    dataFields.Add(dataField);
                }
            }

            var context = new DataContext();
            var result = new List<Dictionary<DataField, string>>();
            for (int i = 0; i < rows; i++)
            {
                result.Add(GenerateRow(dataFields, dataSet, context));
            }

            return result;
        }

        private Dictionary<DataField, string> GenerateRow(ICollection<DataField> fields, BogusDataSet dataSet, DataContext context)
        {
            var result = new Dictionary<DataField, string>();

            if (fields.Contains(DataField.FirstName))
            {
                context.FirstName = _generators[DataField.FirstName](dataSet, context);
            }
            if (fields.Contains(DataField.LastName))
            {
                context.LastName = _generators[DataField.LastName](dataSet, context);
            }

            foreach (var field in fields)
            {
                if (field == DataField.FirstName)
                {
                    result.Add(field, context.FirstName);
                }
                else if (field == DataField.LastName)
                {
                    result.Add(field, context.LastName);
                }
                else
                {
                    result.Add(field, _generators[field](dataSet, context));
                }
            }

            return result;
        }

        private Dictionary<DataField, Func<BogusDataSet, DataContext, string>> CreateGenerators()
        {
            return new Dictionary<DataField, Func<BogusDataSet, DataContext, string>>
            {
                { DataField.FirstName, (d, c) => d.Name.FirstName() },
                { DataField.LastName, (d, c) => d.Name.LastName() },
                { DataField.FullName, (d, c) => (c.FirstName ?? d.Name.FirstName()) + " " + (c.LastName ?? d.Name.LastName()) },
                { DataField.NamePrefix, (d, c) => d.Name.Prefix() },
                { DataField.NameSuffix, (d, c) => d.Name.Suffix() },

                { DataField.JobArea, (d, c) => d.Name.JobArea() },
                { DataField.JobDescription, (d, c) => d.Name.JobDescriptor() },
                { DataField.JobTitle, (d, c) => d.Name.JobTitle() },
                { DataField.JobType, (d, c) => d.Name.JobType() },

                { DataField.Username, (d, c) => d.Internet.UserName(c?.FirstName, c?.LastName) },
                { DataField.Email, (d, c) => d.Internet.Email(c?.FirstName, c?.LastName) },
                { DataField.Avatar, (d, c) => d.Internet.Avatar() },
                { DataField.DomainName, (d, c) => d.Internet.DomainName() },
                { DataField.Ip, (d, c) => d.Internet.Ip() },
                { DataField.Ipv6, (d, c) => d.Internet.Ipv6() },
                { DataField.Password, (d, c) => d.Internet.Password(memorable: true) },
                { DataField.Protocol, (d, c) => d.Internet.Protocol() },
                { DataField.Url, (d, c) => d.Internet.Url() },
                { DataField.UserAgent, (d, c) => d.Internet.UserAgent() },

                { DataField.Country, (d, c) => d.Address.Country() },
                { DataField.City, (d, c) => d.Address.City() },
                { DataField.State, (d, c) => d.Address.State() },
                { DataField.CountryCode, (d, c) => d.Address.CountryCode() },
                { DataField.StreetAddress, (d, c) => d.Address.StreetAddress() },
                { DataField.Street, (d, c) => d.Address.StreetName() },
                { DataField.BuildingNumber, (d, c) => d.Address.BuildingNumber() },
                { DataField.Latitude, (d, c) => d.Address.Latitude().ToString() },
                { DataField.Longitude, (d, c) => d.Address.Longitude().ToString() },
                { DataField.ZipCode, (d, c) => d.Address.ZipCode() },

                { DataField.PhoneNumber, (d, c) => d.PhoneNumbers.PhoneNumber() },

                { DataField.DateOfBirth, (d, c) => d.Date.Past(70, DateTime.Now.AddYears(-5)).ToShortDateString() },
                { DataField.DateOfBirthLong, (d, c) => d.Date.Past(70, DateTime.Now.AddYears(-5)).ToLongDateString() },
                { DataField.DateFuture, (d, c) => {
                    var refDate = c.PreviousDateFuture;
                    var newRefDate = d.Date.Future(refDate: refDate);
                    c.PreviousDateFuture = newRefDate;
                    return newRefDate.ToShortDateString();
                } },
                { DataField.DatePast, (d, c) => {
                    var refDate = c.PreviousDatePast;
                    var newRefDate = d.Date.Past(refDate: refDate);
                    c.PreviousDatePast = newRefDate;
                    return newRefDate.ToShortDateString();
                } },
                { DataField.DateRecent, (d, c) => d.Date.Recent().ToLongDateString() },
                { DataField.Ago, (d, c) => d.Ago(c) },
                { DataField.Time, (d, c) => d.Date.Random.Number(23) + ":" + d.Date.Random.Number(59) },
                { DataField.TimeAM, (d, c) => d.Date.Random.Number(12) + ":" + d.Date.Random.Number(59) + " AM" },
                { DataField.TimePM, (d, c) => d.Date.Random.Number(12) + ":" + d.Date.Random.Number(59) + " PM" },
                { DataField.Year, (d, c) => d.Date.Random.Number(1950, DateTime.Now.Year).ToString() },

                { DataField.Product, (d, c) => d.Commerce.Product() },
                { DataField.Department, (d, c) => d.Commerce.Department() },
                { DataField.Color, (d, c) => d.Commerce.Color() },
                { DataField.Price, (d, c) => d.Commerce.Price() },
                { DataField.ProductAdjective, (d, c) => d.Commerce.ProductAdjective() },
                { DataField.ProductMaterial, (d, c) => d.Commerce.ProductMaterial() },
                { DataField.ProductName, (d, c) => d.Commerce.ProductName() },
                { DataField.ProductCategories, (d, c) => string.Join(", ", d.Commerce.Categories(2)) },

                { DataField.Account, (d, c) => d.Finance.Account() },
                { DataField.AccountName, (d, c) => d.Finance.AccountName() },
                { DataField.Amount, (d, c) => d.Finance.Amount().ToString() },
                { DataField.Bic, (d, c) => d.Finance.Bic() },
                { DataField.Bitcoin, (d, c) => d.Finance.BitcoinAddress() },
                { DataField.CreditCardNumber, (d, c) => d.Finance.CreditCardNumber() },
                { DataField.CurrencyCode, (d, c) => d.Finance.Currency().Code },
                { DataField.Currency, (d, c) => d.Finance.Currency().Description },
                { DataField.Iban, (d, c) => d.Finance.Iban() },
                { DataField.TransactionType, (d, c) => d.Finance.TransactionType() },

                { DataField.CompanyName, (d, c) => d.Company.CompanyName() },
                { DataField.CatchPhrase, (d, c) => d.Company.CatchPhrase() },
                { DataField.BS, (d, c) => d.Company.Bs() },

                { DataField.FileExtension, (d, c) => d.System.CommonFileExt() },
                { DataField.FileName, (d, c) => d.System.CommonFileName() },
                { DataField.FileType, (d, c) => d.System.CommonFileType() },
                { DataField.Exception, (d, c) => d.System.Exception().ToString() },
                { DataField.MimeType, (d, c) => d.System.MimeType() },
                { DataField.Semver, (d, c) => d.System.Semver() },
                { DataField.Version, (d, c) => d.System.Version().ToString() },

                { DataField.LoremParagraph, (d, c) => d.Lorem.Paragraph() },
                { DataField.LoremSentence, (d, c) => d.Lorem.Sentence() },

                { DataField.NumberSeq, (d, c) => d.NumberSequential(c) },
                { DataField.NumberRand, (d, c) => _random.Next(1, 1000).ToString() }
            };
        }
    }

    public class DataContext
    {
        public DataContext()
        {
            PreviousDatePast = DateTime.Now.AddDays(-1);
            PreviousDateFuture = DateTime.Now.AddDays(1);
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int AgoStep { get; set; }
        public DateTime PreviousDatePast { get; set; }
        public DateTime PreviousDateFuture { get; set; }
        public int PreviousNumber { get; set; }
    }

    public enum DataField
    {
        FirstName,
        LastName,
        FullName,
        NamePrefix,
        NameSuffix,
        JobArea,
        JobDescription,
        JobTitle,
        JobType,
        Username,
        Email,
        Avatar,
        DomainName,
        Ip,
        Ipv6,
        Password,
        Protocol,
        Url,
        UserAgent,
        Country,
        City,
        State,
        CountryCode,
        StreetAddress,
        Street,
        BuildingNumber,
        Latitude,
        Longitude,
        ZipCode,
        PhoneNumber,
        DateOfBirth,
        DateFuture,
        DatePast,
        DateRecent,
        Ago,
        Time,
        TimeAM,
        TimePM,
        Year,
        Product,
        Department,
        Color,
        Price,
        ProductAdjective,
        ProductMaterial,
        ProductName,
        ProductCategories,
        Account,
        AccountName,
        Amount,
        Bic,
        Bitcoin,
        CreditCardNumber,
        CurrencyCode,
        Currency,
        Iban,
        TransactionType,
        CompanyName,
        CatchPhrase,
        BS,
        FileExtension,
        FileName,
        FileType,
        Exception,
        MimeType,
        Semver,
        Version,
        LoremParagraph,
        LoremSentence,
        DateOfBirthLong,
        NumberSeq,
        NumberRand
    }

    public class BogusDataSet
    {
        private List<DataSet> _dataSets;

        public Internet Internet { get; }
        public Name Name { get; }
        public Date Date { get; }
        public Address Address { get; }
        public Finance Finance { get; }
        public Commerce Commerce { get; }
        public Company Company { get; }
        public Bogus.DataSets.System System { get; }
        public PhoneNumbers PhoneNumbers { get; }
        public Lorem Lorem { get; }

        public BogusDataSet(int seed = 0, string locale = "en")
        {
            _dataSets = new List<DataSet>();

            Internet = new Internet(locale);
            _dataSets.Add(Internet);

            Name = new Name(locale);
            _dataSets.Add(Name);

            Address = new Address(locale);
            _dataSets.Add(Address);

            Date = new Date(locale);
            _dataSets.Add(Date);

            Finance = new Finance();
            _dataSets.Add(Finance);

            Commerce = new Commerce(locale);
            _dataSets.Add(Commerce);

            Company = new Company(locale);
            _dataSets.Add(Company);

            System = new Bogus.DataSets.System(locale);
            _dataSets.Add(System);

            PhoneNumbers = new PhoneNumbers(locale);
            _dataSets.Add(PhoneNumbers);

            Lorem = new Lorem(locale);
            _dataSets.Add(Lorem);

            if (seed != 0)
            {
                var randomizer = new DataRandomizer(new Random(seed));
                foreach(var ds in _dataSets)
                {
                    ds.Random = randomizer;
                }
            }
        }

        public string Ago(DataContext context)
        {
            // 3 seconds 5 minutes
            if (context.AgoStep == 0)
            {
                ++context.AgoStep;
                var n = Date.Random.Number(1, 59);
                return n <= 1 ? "1 second ago" : n + " seconds ago";
            }

            if (context.AgoStep < 3)
            {
                var n = Date.Random.Number(30 * (context.AgoStep - 1), 30 * (context.AgoStep) - 1);
                ++context.AgoStep;
                return n <= 1 ? "1 minute ago" : n + " minutes ago";
            }

            if (context.AgoStep < 6)
            {
                var n = Date.Random.Number(12 * (context.AgoStep - 4), 12 * (context.AgoStep - 3) - 1);
                ++context.AgoStep;
                return n <= 1 ? "1 hour ago" : n + " hours ago";
            }

            if (context.AgoStep < 8)
            {
                var n = Date.Random.Number(3 * (context.AgoStep - 6), 3 * (context.AgoStep - 5) - 1);
                ++context.AgoStep;
                return n <= 1 ? "1 day ago" : n + " days ago";
            }

            if (context.AgoStep < 10)
            {
                var n = Date.Random.Number(2 * (context.AgoStep - 8), 2 * (context.AgoStep - 7) - 1);
                ++context.AgoStep;
                return n <= 1 ? "1 week ago" : n + " weeks ago";
            }

            return context.AgoStep++ + " months ago";
        }

        public string NumberSequential(DataContext context)
        {
            return (++context.PreviousNumber).ToString();
        }
    }
}
