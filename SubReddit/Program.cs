// See https://aka.ms/new-console-template for more information
using SubReddit.Common;
using SubReddit.DataModel.Model;
using SubReddit.DataModel.Model.Response;
using SubRedditAPI.Interface;
using SubRedditAPI.Services;
using System.Configuration;
using Unity;

IUnityContainer container = new UnityContainer();
container.RegisterType<IAuthentication, OAuthAthentication>();
container.RegisterType<ISubReddit, SubRedditAPI.Services.SubReddit>();


IAuthentication auth = container.Resolve<IAuthentication>();
RedditToken token = await auth.Authenticate();

SubRedditPostsResponse postsResponse = new SubRedditPostsResponse();
SubRedditUsersResponse usersResponse = new SubRedditUsersResponse();

ISubReddit subRedditService = container.Resolve<ISubReddit>();

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
        //new Timer(WriteToExcelEvery15Mins, response.OrderByDescending(p => p.ups).Take(10).ToArray(), 0, 15000);
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
    while (true)
    {
        SubRedditPostsResponse postsResponse = await subRedditService.GetPostsWithMostUpVotesAsync();
        yield return postsResponse;
    }
}
async IAsyncEnumerable<SubRedditUsersResponse> GetRealTimeUsers()
{
    while (true)
    {
        SubRedditUsersResponse usersResponse = await subRedditService.GetUsersWithMostPostsAsync();
        yield return usersResponse;
    }
}

void WriteToExcelEvery15Mins<T>(T[] data, string path)
{
    var startTimeSpan = TimeSpan.Zero;
    var periodTimeSpan = TimeSpan.FromMinutes(1);

    var timer = new Timer((e) =>
    {
        ExportToCSV.ExportPosts(data, path);

    }, null, startTimeSpan, periodTimeSpan);
}