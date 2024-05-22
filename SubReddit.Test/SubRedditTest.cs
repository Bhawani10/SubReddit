using SubReddit.DataModel.Model.Response;
using SubRedditAPI.Interface;
using SubRedditAPI.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubReddit.Test
{
    [TestFixture]
    public class SubRedditTest
    {
        private IAuthentication auth;
        ISubReddit subRedditService;

         [SetUp]
        public void Setup()
        {
            auth = OAuthAthentication.GetAuthenticationService();
            subRedditService = new SubRedditAPI.Services.SubReddit(auth);
        }

        [Test]
        public async Task CheckPosts()
        {
            //Since the credentials are given internally in Authenticate method, it can be tested by passing them from the main method.
            SubRedditPostsResponse postsResponse = await subRedditService.GetPostsWithMostUpVotesAsync();
            Assert.IsNotNull(postsResponse);
        }
        [Test]
        public async Task CheckUsers()
        {
            //Since the credentials are given internally in Authenticate method, it can be tested by passing them from the main method.
            SubRedditUsersResponse postsResponse = await subRedditService.GetUsersWithMostPostsAsync();
            Assert.IsNotNull(postsResponse);
        }
    }
}
