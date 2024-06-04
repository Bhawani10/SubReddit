// See https://aka.ms/new-console-template for more information
using SubReddit.Common;
using SubReddit.DataModel.Model;
using SubReddit.DataModel.Model.Response;
using SubRedditAPI.Interface;
using SubRedditAPI.Services;
using System.Collections.Concurrent;
using System.Configuration;
using System.Diagnostics;
using Unity;

IUnityContainer container = new UnityContainer();

container.RegisterType<IAuthentication, OAuthAthentication>();

ISubReddit subRedditService = container.Resolve<SubRedditAPI.Services.SubReddit>();

SubRedditPostsResponse postsResponse = new SubRedditPostsResponse();
SubRedditUsersResponse usersResponse = new SubRedditUsersResponse();

await StartProcess();

async Task StartProcess()
{
    var post = Task.Run(() => GetPost());
    var user = Task.Run(() => GetUsers());
    await Task.WhenAll(post, user);
}

async Task GetPost()
{
    await foreach (var subRedditResponse in GetRealTimePosts())
    {
        List<Posts> response = subRedditService.UpdatedPosts(subRedditResponse);
        Posts[] posts = response.OrderByDescending(p => p.ups).Take(10).ToArray();
        WriteToExcelEvery15Mins(posts, ConfigurationManager.AppSettings["PostsPath"]);
    }
}

async Task GetUsers()
{
    await foreach (var subRedditResponse in GetRealTimeUsers())
    {
        List<User> response = subRedditService.UpdatedUsers(subRedditResponse);
        User[] users = response.GroupBy(u => u.author_fullname).Where(uc => uc.Count() > 1).SelectMany(s => s).Distinct().Take(10).ToArray();
        WriteToExcelEvery15Mins(users, ConfigurationManager.AppSettings["UsersPath"]);
    }
}

async IAsyncEnumerable<SubRedditPostsResponse> GetRealTimePosts()
{
    var semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 60);
    var total = 0;
    var stopwatch = Stopwatch.StartNew();
    var completionTimes = new ConcurrentQueue<TimeSpan>();
    while (true)
    {
        await semaphore.WaitAsync();

        if (Interlocked.Increment(ref total) > 60)
        {
            completionTimes.TryDequeue(out var earliest);
            var elapsed = stopwatch.Elapsed - earliest;
            var delay = TimeSpan.FromSeconds(60) - elapsed;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay);
        }
        try
        {
            SubRedditPostsResponse postsResponse = await subRedditService.GetPostsWithMostUpVotesAsync();
            yield return postsResponse;
        }
        finally
        {
            completionTimes.Enqueue(stopwatch.Elapsed);
            semaphore.Release();
        }
    }
}
async IAsyncEnumerable<SubRedditUsersResponse> GetRealTimeUsers()
{
    var semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 60);
    var total = 0;
    var stopwatch = Stopwatch.StartNew();
    var completionTimes = new ConcurrentQueue<TimeSpan>();
    while (true)
    {
        await semaphore.WaitAsync();

        if (Interlocked.Increment(ref total) > 60)
        {
            completionTimes.TryDequeue(out var earliest);
            var elapsed = stopwatch.Elapsed - earliest;
            var delay = TimeSpan.FromSeconds(60) - elapsed;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay);
        }
        try
        {
            SubRedditUsersResponse usersResponse = await subRedditService.GetUsersWithMostPostsAsync();
            yield return usersResponse;

        }
        finally
        {
            completionTimes.Enqueue(stopwatch.Elapsed);
            semaphore.Release();
        }
    }
}

void WriteToExcelEvery15Mins<T>(T[] data, string path)
{
    var startTimeSpan = TimeSpan.Zero;
    var periodTimeSpan = TimeSpan.FromMinutes(15);

    var timer = new Timer((e) =>
    {
        ExportToCSV.ExportPosts(data, path);

    }, null, startTimeSpan, periodTimeSpan);
}