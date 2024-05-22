using SubReddit.DataModel.Model;

namespace SubRedditAPI.Interface
{
    public interface IAuthentication
    {
        Task<RedditToken> Authenticate();
    }
}
