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
    public record class SearchResult(string PhoneNumber, HashSet<Uri> Sources);

    public class SearchResultsChangedEventArgs(int progress, Uri currentUri, SearchResult result) : EventArgs
    {
        public int Progress { get; } = progress;
        public SearchResult Result { get; } = result;
        public Uri CurrentUri { get; } = currentUri;
    }

    public class UriChangedEventArgs(int progress, Uri currentUri) : EventArgs
    {
        public int Progress { get; } = progress;
        public Uri CurrentUri { get; } = currentUri;
    }

    public EventHandler<UriChangedEventArgs>? UriChanged;

    public EventHandler<SearchResultsChangedEventArgs>? SearchResultsChanged;

    public BingPhoneNumberCrawlerService(int targetCount, int maxUriCount)
    {
        ValidateArguments(targetCount, maxUriCount);
        TargetCount = targetCount;
        MaxUriCount = maxUriCount;
    }

    public int TargetCount { get; }

    public int MaxUriCount { get; }

    public IEnumerable<SearchResult>? Search(string keyword)
    {
        var initSearchLink = _initBingSearchLink + keyword;
        using var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
        using var tokenSource = new CancellationTokenSource();
        var exceptions = new ConcurrentQueue<Exception>();
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
            return EnumerateSearchResultsIfHasNoException(exceptions);
        }
        catch (OperationCanceledException)
        {
            return EnumerateSearchResultsIfHasNoException(exceptions);
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

    static readonly Regex _phoneRegex = PhoneNumberRegex();

    static readonly Regex _httpUrlRegex = HttpUrlRegex();

    static readonly string _initBingSearchLink = "https://cn.bing.com/search?q=";

    readonly ConcurrentDictionary<string, HashSet<Uri>> _searchResults = new();

    readonly ConcurrentBag<Uri> _visitedUris = new();

    [GeneratedRegex("(13[0-9]|14[01456879]|15[0-35-9]|16[2567]|17[0-8]|18[0-9]|19[0-35-9])\\d{8}")]
    private static partial Regex PhoneNumberRegex();

    [GeneratedRegex("https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b([-a-zA-Z0-9()@:%_\\+.~#?&//=]*)")]
    private static partial Regex HttpUrlRegex();

    void ValidateArguments(int targetCount, int maxUriCount)
    {
        if (targetCount <= 0 || targetCount > 999)
            throw new ArgumentOutOfRangeException(nameof(targetCount), "The target count of phone numbrs should be greater than 1.");
        if (maxUriCount <= 0 || maxUriCount > 99_9999)
            throw new ArgumentOutOfRangeException(nameof(maxUriCount), "The maximize count of uri should be greater than 1.");
    }

    IEnumerable<SearchResult> EnumerateSearchResultsIfHasNoException(ConcurrentQueue<Exception> exceptions)
    {
        if (!exceptions.IsEmpty)
            throw new AggregateException(exceptions);
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
        UriChanged?.Invoke(this, new(_visitedUris.Count, currentUri));
        var matches = _phoneRegex.Matches(html);
        foreach (var match in matches.Cast<Match>())
        {
            if (_searchResults.Count >= TargetCount && !tokenSource.IsCancellationRequested)
                tokenSource.Cancel();
            var phoneNumber = match.Value;
            var sourcesChanged = false;
            var sources = _searchResults.AddOrUpdate(phoneNumber, new HashSet<Uri>() { currentUri }, (p, l) =>
            {
                sourcesChanged = l.Add(currentUri);
                return l;
            });
            if (sourcesChanged || sources.Count == 1)
            {
                SearchResultsChanged?.Invoke(this, new(_searchResults.Count, currentUri, new(phoneNumber, _searchResults[phoneNumber])));
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

}
