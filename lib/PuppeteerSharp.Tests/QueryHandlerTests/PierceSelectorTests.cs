﻿using System;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Nunit;
using NUnit.Framework;

namespace PuppeteerSharp.Tests.QueryHandlerTests
{
    public class PierceSelectorTests : PuppeteerPageBaseTest
    {
        public PierceSelectorTests(): base()
        {
        }

        [SetUp]
        public async Task SetDefaultContentAsync()
        {
            await Page.SetContentAsync(@"
                <script>
                const div = document.createElement('div');
                const shadowRoot = div.attachShadow({mode: 'open'});
                const div1 = document.createElement('div');
                div1.textContent = 'Hello';
                div1.className = 'foo';
                const div2 = document.createElement('div');
                div2.textContent = 'World';
                div2.className = 'foo';
                shadowRoot.appendChild(div1);
                shadowRoot.appendChild(div2);
                document.documentElement.appendChild(div);
                </script>
                ");
        }

        [TearDown]
        public void ClearCustomQueryHandlers()
        {
            Browser.ClearCustomQueryHandlers();
        }

        [PuppeteerTest("queryhandler.spec.ts", "Pierce selectors", "should find first element in shadow")]
        [PuppeteerTimeout]
        public async Task ShouldFindFirstElementInShadow()
        {
            var div = await Page.QuerySelectorAsync("pierce/.foo");
            var text = await div.EvaluateFunctionAsync<string>(@"(element) => {
                return element.textContent;
            }");
            Assert.AreEqual("Hello", text);
        }

        [PuppeteerTest("queryhandler.spec.ts", "Pierce selectors", "should find all elements in shadow")]
        [PuppeteerTimeout]
        public async Task ShouldFindAllElementsInShadow()
        {
            var divs = await Page.QuerySelectorAllAsync("pierce/.foo");
            var text = await Task.WhenAll(
                divs.Select(div => {
                    return div.EvaluateFunctionAsync<string>(@"(element) => {
                        return element.textContent;
                    }");
                }));
            Assert.AreEqual("Hello World", string.Join(" ", text));
        }

        [PuppeteerTest("queryhandler.spec.ts", "Pierce selectors", "should find first child element")]
        [PuppeteerTimeout]
        public async Task ShouldFindFirstChildElement()
        {
            var parentElement = await Page.QuerySelectorAsync("html > div");
            var childElement = await parentElement.QuerySelectorAsync("pierce/div");
            var text = await childElement.EvaluateFunctionAsync<string>(@"(element) => {
                return element.textContent;
            }");
            Assert.AreEqual("Hello", text);
        }

        [PuppeteerTest("queryhandler.spec.ts", "Pierce selectors", "should find all child elements")]
        [PuppeteerTimeout]
        public async Task ShouldFindAllChildElements()
        {
            var parentElement = await Page.QuerySelectorAsync("html > div");
            var childElements = await parentElement.QuerySelectorAllAsync("pierce/div");
            var text = await Task.WhenAll(
                childElements.Select(div => {
                    return div.EvaluateFunctionAsync<string>(@"(element) => {
                        return element.textContent;
                    }");
                }));
            Assert.AreEqual("Hello World", string.Join(" ", text));
        }
    }
}
