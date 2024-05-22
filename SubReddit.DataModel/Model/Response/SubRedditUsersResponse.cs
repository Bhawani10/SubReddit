namespace SubReddit.DataModel.Model.Response
{
    public class SubRedditUsersResponse
    {
        public SubRedditUsersResponse()
        {
            RedditUsers = new List<User>();
        }
        public List<User> RedditUsers { get; set; }
    }
}
