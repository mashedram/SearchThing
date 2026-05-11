using System.Collections.Concurrent;
using System.Diagnostics;
using Il2CppSLZ.Marrow.Warehouse;
using MelonLoader;
using SearchThing.Search.Containers;
using SearchThing.Search.CrateData;
using SearchThing.Search.Sorting;
using SearchThing.Util;

namespace SearchThing.Search.Search;

public record ScoredCrate(ISearchEntry Crate, int Score) : ISearchOrderable
{
    public ISearchEntry Source => Crate;
    public IEnumerable<IFuzzySearchable> SearchFields { get; } = Crate.SearchFields;
    public int Salt => Crate.Salt;

    public static ScoredCrate ScoreCrate(ISearchEntry crate, string preprocessedQuery)
    {
        return new ScoredCrate(crate, SearchManager.ScoreCrate(preprocessedQuery, crate));
    }
}

internal interface ISearchTask
{
    string Query { get; }
    ISearchableCrateList<ISearchEntry> PureSourceList { get; }
    ISearchOrder SearchOrder { get; }
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Checks if the crate matches the task's filter criteria. This is used to pre-filter crates before scoring, to avoid unnecessary scoring of irrelevant crates.
    /// </summary>
    /// <param name="crate"></param>
    /// <returns></returns>
    bool Filter(ISearchEntry crate);

    /// <summary>
    /// Calls the OnComplete callback with the generic search results
    /// </summary>
    /// <remarks>Runs on the Search Thread</remarks>
    /// <param name="results">The results to process</param>
    void RunAndStore(IEnumerable<ISearchEntry> results);
}

public record SearchTask<TCrate>(
    string Query,
    ISearchableCrateList<TCrate> SourceList,
    Func<TCrate, bool> TypedFilter,
    ISearchOrder SearchOrder,
    Action<SearchResults<TCrate>> OnComplete,
    CancellationToken CancellationToken
) : ISearchTask where TCrate : class, ISearchEntry
{
    public ISearchableCrateList<ISearchEntry> PureSourceList => SourceList;

    public bool Filter(ISearchEntry crate)
    {
        return crate is TCrate typedCrate && TypedFilter(typedCrate);
    }

    public void RunAndStore(IEnumerable<ISearchEntry> results)
    {
        var typedResults = results.OfType<TCrate>().ToList();
        var searchResults = new SearchResults<TCrate>(typedResults);
        ThreadUtils.RunOnMainThread(() => OnComplete(searchResults));
    }
}

public static class SearchManager
{
    private const int RequiredMatchRate = 75;

    private static readonly object SearchLock = new();
    private static CancellationTokenSource? _lastSearchCts = new();

    private static Thread? _searchThread;
    private static readonly BlockingCollection<ISearchTask> SearchQueue = new();
    private static readonly CancellationTokenSource SearchThreadCts = new();


    public static void InitializeSearchThread()
    {
        _searchThread = new Thread(SearchThreadWorker)
        {
            IsBackground = true,
            Name = "SearchWorker"
        };
        _searchThread.Start();
    }

    public static void ShutdownSearchThread()
    {
        SearchThreadCts.Cancel();
        SearchQueue.CompleteAdding();
        _searchThread?.Join(TimeSpan.FromMilliseconds(500));
    }

    private static void SearchThreadWorker()
    {
        try
        {
            foreach (var task in SearchQueue.GetConsumingEnumerable(SearchThreadCts.Token))
            {
                ExecuteSearch(task);
            }
        }
        catch (OperationCanceledException)
        {
            // Thread is being shut down
        }
    }

    private static void ExecuteSearch(ISearchTask task)
    {
#if DEBUG
        var stopwatch = Stopwatch.StartNew();
#endif

        var searchableCrates = task.PureSourceList.GetCrates();

        try
        {

            if (string.IsNullOrWhiteSpace(task.Query) || !AssetWarehouse.ready)
            {
                var emptyResult =
                    searchableCrates
                        .AsParallel()
                        .WithDegreeOfParallelism(Math.Max(1, Environment.ProcessorCount / 2))
                        .WithCancellation(task.CancellationToken)
                        .Where(task.Filter)
                        .Select(c => new ScoredCrate(c, 0))
                        .OrderByDescending(task.SearchOrder.Order)
                        .ThenByDescending(c => c.Salt)
                        .Select(c => c.Crate); // Tie-breaker

                task.RunAndStore(emptyResult);
                return;
            }

            var preprocessedQuery = SearchTag.Preprocess(task.Query);

            var result = searchableCrates
                .AsParallel()
                // Avoid starving the game thread of cores
                .WithDegreeOfParallelism(Math.Max(1, Environment.ProcessorCount / 2))
                .WithCancellation(task.CancellationToken)
                .Where(task.Filter)
                .Select(crate =>
                    new ScoredCrate(crate, ScoreCrate(preprocessedQuery, crate))
                )
                .Where(c => c.Score >= RequiredMatchRate)
                .OrderByDescending(task.SearchOrder.Order)
                .ThenByDescending(c => c.Crate.Salt) // Tie-breaker
                .Select(c => c.Crate);

            task.RunAndStore(result);
        }
        catch (OperationCanceledException)
        {
            // Search was canceled
        }
        catch (Exception ex)
        {
            MelonLogger.Error("Search failed: {0}", ex);
            // TODO : Pass an error page to the search page
        }
#if DEBUG
        finally
        {
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedTicks * (1_000 / (double)Stopwatch.Frequency);
            MelonLogger.Msg($"Search completed in {elapsedMilliseconds:F0} milliseconds");
        }
#endif
    }

    public static void SearchAsync<TCrate>(string query, ISearchableCrateList<TCrate> crateList, Func<TCrate, bool> filter, ISearchOrder searchOrder,
        Action<SearchResults<TCrate>> onComplete)
        where TCrate : class, ISearchEntry
    {
        lock (SearchLock)
        {
            _lastSearchCts?.Cancel();
            _lastSearchCts?.Dispose();
            _lastSearchCts = new CancellationTokenSource();
        }

        var searchTask = new SearchTask<TCrate>(
            query,
            crateList,
            filter,
            searchOrder,
            onComplete,
            _lastSearchCts.Token
        );

        SearchQueue.Add(searchTask);
    }

    public static int ScoreCrate(string preprocessedQuery, ISearchEntry crate)
    {
        return crate.SearchFields.Select(field => field.PartialRatio(preprocessedQuery))
            .DefaultIfEmpty(0)
            .Max();
    }
}