using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PhoneNumberCrawler.Services;

public partial class BingPhoneNumberCrawlerService
{
    #region Public Events

    public event EventHandler<UriChangedEventArgs>? UriChanged;

    public event EventHandler<NewPhoneNumberSearchedEventArgs>? NewPhoneNumberSearched;

    public event EventHandler<SourcesUpdatedEventArgs>? SourcesUpdated;

    #endregion Public Events

    #region Public Properties

    public int TargetCount { get; private set; } = 100;

    public int MaxUriCount { get; private set; } = 100_000;

    public AggregateException? Exception { get; private set; }

    #endregion Public Properties

    #region Public Methods

    public BingPhoneNumberCrawlerService Reset()
    {
        _searchResults.Clear();
        _visitedUris.Clear();
        Exception = default;
        return this;
    }

    public record class SearchResult(string PhoneNumber, HashSet<Uri> Sources);
    public IEnumerable<SearchResult>? Search(string keyword, int targetCount, int maxUriCount)
    {
        ValidateArguments(targetCount, maxUriCount);
        TargetCount = targetCount;
        MaxUriCount = maxUriCount;
        var initSearchUrl = _initBingSearchLink + keyword;
        using var client = new HttpClient();
        _tokenSource = new CancellationTokenSource();
        var exceptions = new ConcurrentQueue<Exception>();
        try
        {
            var r = Parallel.For(0, MaxUriCount, new ParallelOptions
            {
                CancellationToken = _tokenSource.Token,
                MaxDegreeOfParallelism = 39
            }, (i) =>
            {
                try
                {
                    var currentUri = new Uri($"{initSearchUrl}&first={i}0");
                    var response = client.GetAsync(currentUri).Result;
                    if (!response.IsSuccessStatusCode)
                        return;
                    var html = client.GetStringAsync(currentUri).Result;
                    ExtractPhoneNumbersFromHtml(currentUri, html, _tokenSource);
                    foreach (var uri in ExtractUrisFromHtml(html, _tokenSource))
                        ExtractPhoneNumbersFromHtml(uri, html, _tokenSource);
                }
                catch (AggregateException ae)
                {
                    foreach (var ex in ae.InnerExceptions)
                    {
                        if (ex is HttpRequestException)
                        {
                            exceptions.Enqueue(ex);
                            if (!_tokenSource.IsCancellationRequested)
                                _tokenSource.Cancel();
                        }
                        else if (ex is not TaskCanceledException)
                            throw ex;
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                    if (!_tokenSource.IsCancellationRequested)
                        _tokenSource.Cancel();
                }
            });
            return EnumerateSearchResultsAndCheckExceptions(exceptions);
        }
        catch (OperationCanceledException)
        {
            return EnumerateSearchResultsAndCheckExceptions(exceptions);
        }
        finally
        {
            _tokenSource.Dispose();
            _tokenSource = null;
        }
    }

    public void Cancel()
    {
        if (_tokenSource is null)
            throw new InvalidOperationException("Search not yet started.");
        _tokenSource.Cancel();
    }

    public IEnumerable<Uri> GetSources(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentNullException(nameof(phoneNumber), "The phone number is null or empty.");
        if (!_searchResults.TryGetValue(phoneNumber, out var sources))
            throw new InvalidOperationException("The specific phone number could not be found");
        else
            return sources;
    }

    #endregion Public Methods

    #region Public Classes

    public class NewPhoneNumberSearchedEventArgs(int progress, string phoneNumber) : EventArgs
    {
        #region Public Properties

        public int Progress { get; } = progress;
        public string PhoneNumber { get; } = phoneNumber;

        #endregion Public Properties
    }

    public class UriChangedEventArgs(Uri currentUri) : EventArgs
    {
        #region Public Properties

        public Uri CurrentUri { get; } = currentUri;

        #endregion Public Properties
    }

    public class SourcesUpdatedEventArgs(string phoneNumber, Uri newSource) : EventArgs
    {
        #region Public Properties

        public string PhoneNumber { get; } = phoneNumber;
        public Uri NewSource { get; } = newSource;

        #endregion Public Properties
    }

    #endregion Public Classes

    #region Private Fields

    static readonly Regex _phoneRegex = PhoneNumberRegex();

    static readonly Regex _httpUrlRegex = HttpUrlRegex();

    static readonly string _initBingSearchLink = "https://bing.com/search?q=";

    readonly ConcurrentDictionary<string, HashSet<Uri>> _searchResults = new();

    readonly ConcurrentBag<Uri> _visitedUris = new();

    CancellationTokenSource? _tokenSource;

    #endregion Private Fields

    #region Private Methods

    [GeneratedRegex("(13[0-9]|14[01456879]|15[0-35-9]|16[2567]|17[0-8]|18[0-9]|19[0-35-9])\\d{8}")]
    private static partial Regex PhoneNumberRegex();

    [GeneratedRegex("https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b([-a-zA-Z0-9()@:%_\\+.~#?&//=]*)")]
    private static partial Regex HttpUrlRegex();

    void ValidateArguments(int targetCount, int maxUriCount)
    {
        if (targetCount <= 0 || targetCount > 999)
            throw new ArgumentOutOfRangeException(nameof(targetCount), "The target count of phone numbrs should be greater than 1.");
        if (maxUriCount <= 0 || maxUriCount > 999_999)
            throw new ArgumentOutOfRangeException(nameof(maxUriCount), "The maximize count of uri should be greater than 1.");
    }

    IEnumerable<SearchResult> EnumerateSearchResultsAndCheckExceptions(ConcurrentQueue<Exception> exceptions)
    {
        if (!exceptions.IsEmpty)
            Exception = new AggregateException(exceptions);
        var count = 1;
        foreach ((var phoneNumber, var source) in _searchResults)
        {
            yield return new(phoneNumber, source);
            if (count == TargetCount)
                yield break;
        }
    }

    void ValidateHtml(string html)
    {
        if (html is null || string.IsNullOrWhiteSpace(html))
            throw new ArgumentException("The html is null or empty.", nameof(html));
    }

    bool CheckVisitedUris(Uri uri, CancellationTokenSource tokenSource)
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
        ValidateHtml(html);
        if (!CheckVisitedUris(currentUri, tokenSource))
            return;
        UriChanged?.Invoke(this, new(currentUri));
        var matches = _phoneRegex.Matches(html);
        foreach (var match in matches.Cast<Match>())
        {
            if (_searchResults.Count >= TargetCount && !tokenSource.IsCancellationRequested)
                tokenSource.Cancel();
            var phoneNumber = match.Value;
            lock (_searchResults)
            {
                var sources = _searchResults.GetOrAdd(phoneNumber, new HashSet<Uri>());
                lock (sources)
                {
                    if (sources.Count == 0)
                        NewPhoneNumberSearched?.Invoke(this, new(_searchResults.Count, phoneNumber));
                    else
                        SourcesUpdated?.Invoke(this, new(phoneNumber, currentUri));
                    sources.Add(currentUri);
                }
            }
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
