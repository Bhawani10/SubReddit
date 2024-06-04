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
        private static readonly SemaphoreSlim accessTokenSemaphore;
        private static RedditToken token;
        private static readonly string REDDIT_AUTH_URL = ConfigurationManager.AppSettings["RedditAuthURL"];
        private static string appId = ConfigurationManager.AppSettings["AppId"];
        private static string secret = ConfigurationManager.AppSettings["Secret"];
        private static string userName = ConfigurationManager.AppSettings["Username"];
        private static string password = ConfigurationManager.AppSettings["Password"];

        static OAuthAthentication()
        {
            token = null!;
            accessTokenSemaphore = new SemaphoreSlim(1, 1);          
        }

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
                DateTime tokenExpiresTime;
                tokenExpiresTime = DateTime.Now.AddSeconds(x.expires_in);
                token = new RedditToken()
                {
                    access_token = x.access_token,
                    token_type = x.token_type,
                    expires_in = x.expires_in,
                    Expired = !(DateTime.Now < tokenExpiresTime)                  

            };
                return token;
            }
            catch (Exception ex)
            {
                ExceptionLogging.SendErrorToText(ex);
                return null;
            }
        }

        private async Task<RedditToken> GetAccessToken(string userName, string password, string appId, string secret)
        {
            try
            {
                await accessTokenSemaphore.WaitAsync();

                if (token is { Expired: false })
                {
                    return token;
                }

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
            finally
            {
                accessTokenSemaphore.Release(1);
            }
            return new RedditToken();
        }
    }
}
