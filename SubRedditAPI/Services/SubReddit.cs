using Newtonsoft.Json;
using SubReddit.DataModel.Model;
using SubReddit.DataModel.Model.Response;
using SubRedditAPI.Interface;
using System.Net.Http.Headers;

namespace SubRedditAPI.Services
{
    public class SubReddit : ISubReddit
    {
        private IAuthentication _auth;
        private HttpClient _client;
        private static readonly string REDDIT_OAUTH_BASE_URL = "https://oauth.reddit.com";
        public static volatile SubRedditPostsResponse consolidatedResponse = new SubRedditPostsResponse();
        public static volatile SubRedditUsersResponse consolidatedUsersResponse = new SubRedditUsersResponse();

        public SubReddit(IAuthentication auth)
        {
            _auth = auth;
        }
        private void GetToken()
        {
            var _token = _auth.Authenticate();
            _client = new HttpClient();
            _client.BaseAddress = new Uri(REDDIT_OAUTH_BASE_URL);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(("application/json")));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_token.Result.token_type, _token.Result.access_token);
            _client.DefaultRequestHeaders.Add("User-Agent", "Console");
        }

        public async Task<SubRedditPostsResponse> GetPostsWithMostUpVotesAsync()
        {
            SubRedditPostsResponse posts = new SubRedditPostsResponse();    
            try
            {
                GetToken();  //instantiate Token
                HttpResponseMessage response = await _client.GetAsync($"/r/worldnews/new");
                var data = await response.Content.ReadAsStringAsync();
                posts = GeneratePostsResponse(data);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Error when getting Posts with most up votes.");
                }
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
            }
            return posts;
        }

        public async Task<SubRedditUsersResponse> GetUsersWithMostPostsAsync()
        {
            SubRedditUsersResponse users = new SubRedditUsersResponse();
            try
            {
                GetToken();  //instantiate Token
                HttpResponseMessage response = await _client.GetAsync($"/r/worldnews/new");
                var data = await response.Content.ReadAsStringAsync();
                users = GenerateUserResponse(data);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Error when getting Users with most posts.");
                }
                return users;
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
            }
            return users;
        }

        private SubRedditPostsResponse GeneratePostsResponse(string response)
        {
            try
            {
                var results = JsonConvert.DeserializeObject<dynamic>(response);
                List<Posts> posts = new List<Posts>();
                foreach (var post in results.data.children)
                {
                    posts.Add(new Posts()
                    {
                        CurrentDate = DateTime.Now,
                        ups = post.data.ups,
                        author = post.data.author,
                        author_fullname = post.data.author_fullname,
                        title = post.data.title,
                        subreddit = post.data.subreddit,
                        createdDt = GetLocalTime((int)post.data.created)
                    });
                }
                var resp = new SubRedditPostsResponse
                {
                    RedditPosts = posts.OrderByDescending(x => x.ups).Take(10).ToList()
                };
                return resp;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private SubRedditUsersResponse GenerateUserResponse(string response)
        {
            try
            {
                var results = JsonConvert.DeserializeObject<dynamic>(response);
                List<User> user = new List<User>();
                foreach (var post in results.data.children)
                {
                    user.Add(new User()
                    {
                        CurrentDate = DateTime.Now,
                        author = post.data.author,
                        author_fullname = post.data.author_fullname,
                        title = post.data.title,
                        subreddit = post.data.subreddit,
                        createdDt = GetLocalTime((int)post.data.created)
                    }); ;
                }
                var resp = new SubRedditUsersResponse
                {
                    RedditUsers = user.GroupBy(u => u.author_fullname).Where(uc => uc.Count() > 1).SelectMany(s => s).Distinct().Take(10).ToList()
                };

                return resp;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DateTime GetLocalTime(int created)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(created);
        }

        public List<Posts> UpdatedPosts(SubRedditPostsResponse response)
        {
            consolidatedResponse.RedditPosts.AddRange(response.RedditPosts);
            return consolidatedResponse.RedditPosts;    
        }

        public List<User> UpdatedUsers(SubRedditUsersResponse response)
        {
            consolidatedUsersResponse.RedditUsers.AddRange(response.RedditUsers);
            return consolidatedUsersResponse.RedditUsers;
        }
    }

}

