using System;
using System.Threading.Tasks;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Nunit;
using NUnit.Framework;

namespace PuppeteerSharp.Tests.QuerySelectorTests
{
    public class ElementHandleQuerySelectorEvalTests : PuppeteerPageBaseTest
    {
        public ElementHandleQuerySelectorEvalTests(): base()
        {
        }

        async Task Usage(Browser browser)
        {
            #region Evaluate
            await using var page = await browser.NewPageAsync();
            var seven = await page.EvaluateExpressionAsync<int>("4 + 3");
            var someObject = await page.EvaluateFunctionAsync<dynamic>("(value) => ({a: value})", 5);
            Console.WriteLine(someObject.a);
            #endregion
        }

        [PuppeteerTest("queryselector.spec.ts", "ElementHandle.$eval", "should work")]
        [PuppeteerTimeout]
        public async Task QuerySelectorShouldWork()
        {
            await Page.SetContentAsync("<html><body><div class='tweet'><div class='like'>100</div><div class='retweets'>10</div></div></body></html>");
            var tweet = await Page.QuerySelectorAsync(".tweet");
            var content = await tweet.QuerySelectorAsync(".like")
                .EvaluateFunctionAsync<string>("node => node.innerText");
            Assert.AreEqual("100", content);
        }

        [PuppeteerTest("queryselector.spec.ts", "ElementHandle.$eval", "should retrieve content from subtree")]
        [PuppeteerTimeout]
        public async Task QuerySelectorShouldRetrieveContentFromSubtree()
        {
            var htmlContent = "<div class='a'>not-a-child-div</div><div id='myId'><div class='a'>a-child-div</div></div>";
            await Page.SetContentAsync(htmlContent);
            var elementHandle = await Page.QuerySelectorAsync("#myId");
            var content = await elementHandle.QuerySelectorAsync(".a")
                .EvaluateFunctionAsync<string>("node => node.innerText");
            Assert.AreEqual("a-child-div", content);
        }

        [PuppeteerTest("queryselector.spec.ts", "ElementHandle.$eval", "should throw in case of missing selector")]
        [PuppeteerTimeout]
        public async Task QuerySelectorShouldThrowInCaseOfMissingSelector()
        {
            var htmlContent = "<div class=\"a\">not-a-child-div</div><div id=\"myId\"></div>";
            await Page.SetContentAsync(htmlContent);
            var elementHandle = await Page.QuerySelectorAsync("#myId");
            var exception = Assert.ThrowsAsync<SelectorException>(
                () => elementHandle.QuerySelectorAsync(".a").EvaluateFunctionAsync<string>("node => node.innerText")
            );
            Assert.AreEqual("Error: failed to find element matching selector", exception.Message);
        }
    }
}
