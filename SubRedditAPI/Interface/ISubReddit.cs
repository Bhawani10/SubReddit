using SubReddit.DataModel.Model;
using SubReddit.DataModel.Model.Response;

namespace SubRedditAPI.Interface
{
    public interface ISubReddit
    {
        Task<SubRedditPostsResponse> GetPostsWithMostUpVotesAsync();
        Task<SubRedditUsersResponse> GetUsersWithMostPostsAsync();
        List<Posts> UpdatedPosts(SubRedditPostsResponse response);
        List<User> UpdatedUsers(SubRedditUsersResponse response);
    }
}
