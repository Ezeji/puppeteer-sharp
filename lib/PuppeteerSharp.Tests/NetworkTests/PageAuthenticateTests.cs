using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Nunit;
using NUnit.Framework;

namespace PuppeteerSharp.Tests.NetworkTests
{
    public class PageAuthenticateTests : PuppeteerPageBaseTest
    {
        public PageAuthenticateTests(): base()
        {
        }

        [PuppeteerTest("network.spec.ts", "Page.authenticate", "should work")]
        [Skip(SkipAttribute.Targets.Firefox)]
        public async Task ShouldWork()
        {
            Server.SetAuth("/empty.html", "user", "pass");

            var response = await Page.GoToAsync(TestConstants.EmptyPage);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.Status);

            await Page.AuthenticateAsync(new Credentials
            {
                Username = "user",
                Password = "pass"
            });

            response = await Page.ReloadAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.Status);
        }

        [PuppeteerTest("network.spec.ts", "Page.authenticate", "should fail if wrong credentials")]
        [Skip(SkipAttribute.Targets.Firefox)]
        public async Task ShouldFailIfWrongCredentials()
        {
            Server.SetAuth("/empty.html", "user2", "pass2");

            await Page.AuthenticateAsync(new Credentials
            {
                Username = "foo",
                Password = "bar"
            });

            var response = await Page.GoToAsync(TestConstants.EmptyPage);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.Status);
        }

        [PuppeteerTest("network.spec.ts", "Page.authenticate", "should allow disable authentication")]
        [Skip(SkipAttribute.Targets.Firefox)]
        public async Task ShouldAllowDisableAuthentication()
        {
            Server.SetAuth("/empty.html", "user3", "pass3");

            await Page.AuthenticateAsync(new Credentials
            {
                Username = "user3",
                Password = "pass3"
            });

            var response = await Page.GoToAsync(TestConstants.EmptyPage);
            Assert.AreEqual(HttpStatusCode.OK, response.Status);

            await Page.AuthenticateAsync(null);

            response = await Page.GoToAsync(TestConstants.CrossProcessUrl + "/empty.html");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.Status);
        }

        [PuppeteerTest("network.spec.ts", "Page.authenticate", "should not disable caching")]
        [Skip(SkipAttribute.Targets.Firefox)]
        public async Task ShouldNotDisableCaching()
        {
            Server.SetAuth("/cached/one-style.css", "user4", "pass4");
            Server.SetAuth("/cached/one-style.html", "user4", "pass4");

            await Page.AuthenticateAsync(new Credentials
            {
                Username = "user4",
                Password = "pass4"
            });

            var responses = new Dictionary<string, IResponse>();
            Page.Response += (_, e) => responses[e.Response.Url.Split('/').Last()] = e.Response;

            await Page.GoToAsync(TestConstants.ServerUrl + "/cached/one-style.html");
            await Page.ReloadAsync();

            Assert.AreEqual(HttpStatusCode.NotModified, responses["one-style.html"].Status);
            Assert.False(responses["one-style.html"].FromCache);
            Assert.AreEqual(HttpStatusCode.OK, responses["one-style.css"].Status);
            Assert.True(responses["one-style.css"].FromCache);
        }
    }
}
