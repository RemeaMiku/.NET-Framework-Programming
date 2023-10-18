using static System.Console;

namespace PhoneNumberCrawler.Shared;

partial class Program
{
    #region Private Methods

    static void Main(string[] args)
    {
        var crawler = new BingPhoneNumberCrawler(100, 2000);
        crawler.SearchResultsChanged += (object? s, BingPhoneNumberCrawler.SearchResultsChangedEventArgs e) =>
        {
            Clear();
            WriteLine(e.Progress);
        };
        var res = crawler.Search("手机号");
        if (res is not null)
        {
            foreach ((var phone, var source) in res)
            {
                WriteLine(phone);
                foreach (var uri in source)
                    WriteLine(uri);
                WriteLine();
            }
            WriteLine(res.Count);
        }
    }

    #endregion Private Methods
}
