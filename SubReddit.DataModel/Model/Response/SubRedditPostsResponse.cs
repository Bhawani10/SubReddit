namespace SubReddit.DataModel.Model.Response
{
    public class SubRedditPostsResponse
    {
        public SubRedditPostsResponse()
        {
            RedditPosts = new List<Posts>();
        }
        public List<Posts> RedditPosts { get; set; }
    }
}
