using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace PhoneNumberCrawler.Shared;

public partial class BingPhoneNumberCrawler
{
    #region Public Fields

    public EventHandler<SearchResultsChangedEventArgs>? SearchResultsChanged;

    #endregion Public Fields

    #region Public Constructors

    public BingPhoneNumberCrawler(int targetPhoneNumberCount, int maxUriCount)
    {
        ThrowArgumentOutOfRangeExceptionIfIsInvalid(targetPhoneNumberCount, maxUriCount);
        TargetPhoneNumberCount = targetPhoneNumberCount;
        MaxUriCount = maxUriCount;
    }

    #endregion Public Constructors

    #region Public Properties

    public int TargetPhoneNumberCount { get; }

    public int MaxUriCount { get; }

    #endregion Public Properties

    #region Public Methods

    public List<PhoneNumberSearchResult>? Search(string keyword)
    {
        var result = default(List<PhoneNumberSearchResult>);
        var initSearchLink = _initBingSearchLink + keyword;
        using var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
        using var tokenSource = new CancellationTokenSource();
        var exceptions = new ConcurrentQueue<Exception>();
        result = new();
        try
        {
            var r = Parallel.For(0, MaxUriCount, new ParallelOptions
            {
                CancellationToken = tokenSource.Token,
                MaxDegreeOfParallelism = 39
            }, (i) =>
            {
                try
                {
                    var currentUri = new Uri($"{initSearchLink}&first={i}0");
                    var html = client.GetStringAsync(currentUri).Result;
                    ExtractPhoneNumbersFromHtml(currentUri, html, tokenSource);
                    foreach (var uri in ExtractUrisFromHtml(html, tokenSource))
                        ExtractPhoneNumbersFromHtml(uri, html, tokenSource);
                }
                catch (HttpRequestException ex)
                {
                    exceptions.Enqueue(ex);
                }
                catch (TaskCanceledException)
                {
                }
                catch (AggregateException ae)
                {
                    foreach (var ex in ae.InnerExceptions)
                    {
                        if (ex is HttpRequestException)
                            exceptions.Enqueue(ex);
                        else if (ex is not TaskCanceledException)
                            throw ex;
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                    if (!tokenSource.IsCancellationRequested)
                        tokenSource.Cancel();
                }
            });
            return GetSearchResultsOrThrowAggregateExceptionIfHasException(exceptions);
        }
        catch (OperationCanceledException)
        {
            return GetSearchResultsOrThrowAggregateExceptionIfHasException(exceptions);
        }
        catch (Exception ex)
        {
            exceptions.Enqueue(ex);
            throw new AggregateException(exceptions);
        }
        finally
        {
            _searchResults.Clear();
            _visitedUris.Clear();
        }
    }

    #endregion Public Methods

    #region Public Classes

    public class SearchResultsChangedEventArgs(int progress, PhoneNumberSearchResult result) : EventArgs
    {
        #region Public Properties

        public int Progress { get; } = progress;
        public PhoneNumberSearchResult Result { get; } = result;

        #endregion Public Properties
    }

    #endregion Public Classes

    #region Private Fields

    static readonly Regex _phoneRegex = PhoneNumberRegex();

    static readonly Regex _httpUrlRegex = HttpUrlRegex();

    static readonly string _initBingSearchLink = "https://cn.bing.com/search?q=";

    readonly ConcurrentDictionary<string, HashSet<Uri>> _searchResults = new();

    readonly ConcurrentBag<Uri> _visitedUris = new();

    #endregion Private Fields

    #region Private Methods

    [GeneratedRegex("(13[0-9]|14[01456879]|15[0-35-9]|16[2567]|17[0-8]|18[0-9]|19[0-35-9])\\d{8}")]
    private static partial Regex PhoneNumberRegex();

    [GeneratedRegex("https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b([-a-zA-Z0-9()@:%_\\+.~#?&//=]*)")]
    private static partial Regex HttpUrlRegex();

    void ThrowArgumentOutOfRangeExceptionIfIsInvalid(int targetPhoneNumberCount, int maxUriCount)
    {
        if (targetPhoneNumberCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetPhoneNumberCount), "The target count of phone numbrs should be greater than 1.");
        }
        if (maxUriCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxUriCount), "The maximize count of uri should be greater than 1.");
        }
    }
    List<PhoneNumberSearchResult> GetSearchResultsOrThrowAggregateExceptionIfHasException(ConcurrentQueue<Exception> exceptions)
    {
        if (!exceptions.IsEmpty)
            throw new AggregateException(exceptions);
        var result = new List<PhoneNumberSearchResult>();
        foreach ((var phoneNumber, var source) in _searchResults)
        {
            result.Add(new(phoneNumber, source));
            if (result.Count == TargetPhoneNumberCount)
                break;
        }
        return result;
    }

    void ThrowArgumentExceptionIfHtmlIsNullOrWhiteSpace(string html)
    {
        if (html is null || string.IsNullOrWhiteSpace(html))
            throw new ArgumentException("The html is null or empty.", nameof(html));
    }

    bool ValadateVisitedUris(Uri uri, CancellationTokenSource tokenSource)
    {
        if (_visitedUris.Count >= MaxUriCount && !tokenSource.IsCancellationRequested)
        {
            tokenSource.Cancel();
            return false;
        }
        if (_visitedUris.Contains(uri))
            return false;
        _visitedUris.Add(uri);
        return true;
    }


    void ExtractPhoneNumbersFromHtml(Uri currentUri, string html, CancellationTokenSource tokenSource)
    {
        if (tokenSource.IsCancellationRequested)
            return;
        ThrowArgumentExceptionIfHtmlIsNullOrWhiteSpace(html);
        if (!ValadateVisitedUris(currentUri, tokenSource))
            return;
        var matches = _phoneRegex.Matches(html);
        foreach (var match in matches.Cast<Match>())
        {
            if (_searchResults.Count >= TargetPhoneNumberCount && !tokenSource.IsCancellationRequested)
                tokenSource.Cancel();
            var phoneNumber = match.Value;
            _searchResults.AddOrUpdate(phoneNumber, new HashSet<Uri>() { currentUri }, (p, l) =>
            {
                l.Add(currentUri);
                return l;
            });
            SearchResultsChanged?.Invoke(this, new(_searchResults.Count, new(phoneNumber, _searchResults[phoneNumber])));
        }
    }

    IEnumerable<Uri> ExtractUrisFromHtml(string html, CancellationTokenSource tokenSource)
    {
        if (tokenSource.IsCancellationRequested)
            yield break;
        var matches = _httpUrlRegex.Matches(html);
        foreach (var match in matches.Cast<Match>())
        {
            var uri = new Uri(match.Value);
            if (!_visitedUris.Contains(uri))
                yield return uri;
        }
    }

    #endregion Private Methods
}
