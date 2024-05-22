using Newtonsoft.Json;
using SubReddit.DataModel.Model;
using SubRedditAPI.Interface;
using System.Configuration;
using System.Net.Http.Headers;
using System.Text;

namespace SubRedditAPI.Services
{
    public class OAuthAthentication : IAuthentication
    {
        private static volatile OAuthAthentication oAuthAthentication;
        public RedditToken token = new RedditToken();
        private static readonly string REDDIT_AUTH_URL = "https://www.reddit.com/api/v1/access_token";
        string appId = ConfigurationManager.AppSettings["AppId"];
        string secret = ConfigurationManager.AppSettings["Secret"];
        string userName = ConfigurationManager.AppSettings["Username"];
        string password = ConfigurationManager.AppSettings["Password"];


        public static IAuthentication GetAuthenticationService()
        {
            if (oAuthAthentication == null)
            {
                oAuthAthentication = new OAuthAthentication();
            }
            return oAuthAthentication;
        }

            public async Task<RedditToken> Authenticate()
            {
                try
                {
                    var x = await GetAccessToken(userName, password, appId, secret);
                    token = new RedditToken()
                    {
                        access_token = x.access_token,
                        token_type = x.token_type,
                        expires_in = x.expires_in
                    };
                    return token;
                }
                catch (Exception ex)
                {
                    ExceptionLogging.SendErrorToText(ex); ;
                    return new RedditToken();
                }
            }

        private async Task<RedditToken> GetAccessToken(string userName, string password, string appId, string secret)
        {
            try
            {
                var url = $"{REDDIT_AUTH_URL}?grant_type=password&username={userName}&password={password}";
                var byteArray = Encoding.ASCII.GetBytes($"{appId}:{secret}");
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "Console");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", Convert.ToBase64String(byteArray));
                var resp = await client.PostAsync(url, null);
                if (resp.IsSuccessStatusCode)
                {
                    var data = await resp.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<RedditToken>(data);
                }
                else
                {
                    throw new HttpRequestException("Error while Authenticating.");
                }
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
            }
            return new RedditToken();
        }
    }
}
