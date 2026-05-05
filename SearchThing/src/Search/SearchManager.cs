using System.Collections.Concurrent;
using System.Diagnostics;
using FuzzySharp;
using FuzzySharp.PreProcess;
using Il2CppSLZ.Marrow.Warehouse;
using MelonLoader;
using SearchThing.Util;

namespace SearchThing.Search;

public record ScoredCrate(ISearchableCrate Crate, int Score) : ISearchableCrate
{
    public SearchTag Name => Crate.Name;
    public SearchTag PalletName => Crate.PalletName;
    public SearchTag Author => Crate.Author;
    public SearchTag[] Tags => Crate.Tags;
    public string Description => Crate.Description;
    public bool Redacted => Crate.Redacted;
    public CrateType CrateType => Crate.CrateType;
    public int Salt => Crate.Salt;
    public Barcode Barcode => Crate.Barcode;
    public DateTime DateAdded => Crate.DateAdded;

    public static ScoredCrate ScoreCrate(ISearchableCrate crate, string preprocessedQuery)
    {
        return new ScoredCrate(crate, SearchManager.ScoreCrate(preprocessedQuery, crate));
    }
}

public record SearchTask(
    string Query,
    ISearchableCrateList SearchableCrateCrates,
    Func<ISearchableCrate, bool> Filter,
    ISearchOrder SearchOrder,
    Action<SearchResults> OnComplete,
    CancellationToken CancellationToken
);

public static class SearchManager
{
    private const int RequiredMatchRate = 75;

    private static readonly object SearchLock = new();
    private static CancellationTokenSource? _lastSearchCts = new();

    private static Thread? _searchThread;
    private static readonly BlockingCollection<SearchTask> SearchQueue = new();
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

    private static void ExecuteSearch(SearchTask task)
    {
#if DEBUG
        var stopwatch = Stopwatch.StartNew();
#endif
            
        var searchableCrates = task.SearchableCrateCrates.GetCrates();

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
                        .OrderByDescending(task.SearchOrder.Order)
                        .ThenByDescending(c => c.Salt) // Tie-breaker
                        .ToSearchResults();

                task.OnComplete.RunOnMainThread(emptyResult);
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
                .Select(c => c.Crate)
                .ToSearchResults();

            task.OnComplete.RunOnMainThread(result);
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

    public static void SearchAsync(string query, ISearchableCrateList crateList, Func<ISearchableCrate, bool> filter, ISearchOrder searchOrder, Action<SearchResults> onComplete)
    {
        lock (SearchLock)
        {
            _lastSearchCts?.Cancel();
            _lastSearchCts?.Dispose();
            _lastSearchCts = new CancellationTokenSource();
        }

        var searchTask = new SearchTask(
            query,
            crateList,
            filter,
            searchOrder,
            onComplete,
            _lastSearchCts.Token
        );

        SearchQueue.Add(searchTask);
    }
    
    public static int ScoreCrate(string preprocessedQuery, ISearchableCrate crate)
    {
        var nameScore = crate.Name.PartialRatio(preprocessedQuery);
        var palletScore = crate.PalletName.PartialRatio(preprocessedQuery);
        var authorScore = crate.Author.PartialRatio(preprocessedQuery);
        var tagScore = crate.Tags.Any(t => t.PartialRatio(preprocessedQuery) > 80) ? 90 : 0;
    
        return new [] { nameScore, palletScore, authorScore, tagScore }.Max();
    }
}