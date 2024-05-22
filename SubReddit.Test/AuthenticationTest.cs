using Newtonsoft.Json.Linq;
using RestSharp.Authenticators;
using SubReddit.DataModel.Model;
using SubRedditAPI;
using SubRedditAPI.Interface;
using SubRedditAPI.Services;
using System.Configuration;
using System.Net.Sockets;

namespace SubReddit.Test
{
    [TestFixture]
    public class Tests
    {
        private IAuthentication auth;
        [SetUp]
        public void Setup()
        {
            auth =  OAuthAthentication.GetAuthenticationService();
        }

        [Test]
        public async Task CheckAccessToken()
        {
            var token = await auth.Authenticate(); //Since the credentials are given internally in Authenticate method, it can be tested by passing them from the main method.
            Assert.IsNotNull(token);
        }
        
    }
}