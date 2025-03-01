using System.Threading.Tasks;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Nunit;
using NUnit.Framework;

namespace PuppeteerSharp.Tests.NavigationTests
{
    public class PageGoBackTests : PuppeteerPageBaseTest
    {
        public PageGoBackTests(): base()
        {
        }

        //TODO: This is working in puppeteer. I don't know why is hanging here.
        [PuppeteerTest("navigation.spec.ts", "Page.goBack", "should work")]
        [Skip(SkipAttribute.Targets.Firefox)]
        public async Task ShouldWork()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            await Page.GoToAsync(TestConstants.ServerUrl + "/grid.html");

            var response = await Page.GoBackAsync();
            Assert.True(response.Ok);
            Assert.AreEqual(TestConstants.EmptyPage, response.Url);

            response = await Page.GoForwardAsync();
            Assert.True(response.Ok);
            StringAssert.Contains("grid", response.Url);

            response = await Page.GoForwardAsync();
            Assert.Null(response);
        }

        [PuppeteerTest("navigation.spec.ts", "Page.goBack", "should work with HistoryAPI")]
        [Skip(SkipAttribute.Targets.Firefox)]
        public async Task ShouldWorkWithHistoryAPI()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            await Page.EvaluateExpressionAsync(@"
              history.pushState({ }, '', '/first.html');
              history.pushState({ }, '', '/second.html');
            ");
            Assert.AreEqual(TestConstants.ServerUrl + "/second.html", Page.Url);

            await Page.GoBackAsync();
            Assert.AreEqual(TestConstants.ServerUrl + "/first.html", Page.Url);
            await Page.GoBackAsync();
            Assert.AreEqual(TestConstants.EmptyPage, Page.Url);
            await Page.GoForwardAsync();
            Assert.AreEqual(TestConstants.ServerUrl + "/first.html", Page.Url);
        }
    }
}
