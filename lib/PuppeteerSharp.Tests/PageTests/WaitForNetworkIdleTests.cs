using System;
using System.Threading.Tasks;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Nunit;
using NUnit.Framework;

namespace PuppeteerSharp.Tests.PageTests
{
    public class WaitForNetworkIdleTests : PuppeteerPageBaseTest
    {
        public WaitForNetworkIdleTests(): base()
        {
        }

        [PuppeteerTest("page.spec.ts", "Page.waitForNetworkIdle", "should work")]
        [PuppeteerTimeout]
        public async Task ShouldWork()
        {
            var t1 = DateTime.UtcNow;
            var t2 = DateTime.UtcNow;

            await Page.GoToAsync(TestConstants.EmptyPage);
            var task = Page.WaitForNetworkIdleAsync()
                .ContinueWith(x =>
                {
                    if (x.IsFaulted) throw x.Exception;
                    return t1 = DateTime.UtcNow;
                });

            await Task.WhenAll(
                task,
                Page.EvaluateFunctionAsync(@"async () => {
                    await Promise.all([
                            fetch('/digits/1.png'),
                            fetch('/digits/2.png'),
                        ]);
                    await new Promise((resolve) => setTimeout(resolve, 200));
                    await fetch('/digits/3.png');
                    await new Promise((resolve) => setTimeout(resolve, 200));
                    await fetch('/digits/4.png');
                }").ContinueWith(x =>
                {
                    if (x.IsFaulted) throw x.Exception;
                    t2 = DateTime.UtcNow;
                })
            );

            Assert.True(t1 > t2);
            Assert.True((t1 - t2).TotalMilliseconds >= 400);
        }

        [PuppeteerTest("page.spec.ts", "Page.waitForNetworkIdle", "should respect timeout")]
        [PuppeteerTimeout]
        public async Task ShouldRespectTimeout()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);
            var exception = Assert.ThrowsAsync<TimeoutException>(async () =>
                await Page.WaitForNetworkIdleAsync(new WaitForNetworkIdleOptions { Timeout = 1 }));

            StringAssert.Contains("Timeout of 1 ms exceeded", exception.Message);
        }

        // This should work on Firefox, this ignore should be temporal
        // PRs are welcome :)
        [PuppeteerTest("page.spec.ts", "Page.waitForNetworkIdle", "should respect idleTime")]
        [Skip(SkipAttribute.Targets.Firefox)]
        public async Task ShouldRespectIdleTimeout()
        {
            var t1 = DateTime.UtcNow;
            var t2 = DateTime.UtcNow;

            await Page.GoToAsync(TestConstants.EmptyPage);
            var task = Page.WaitForNetworkIdleAsync(new WaitForNetworkIdleOptions { IdleTime = 10 })
                .ContinueWith(x =>
                {
                    if (x.IsFaulted) throw x.Exception;
                    return t1 = DateTime.UtcNow;
                });

            await Task.WhenAll(
                task,
                Page.EvaluateFunctionAsync(@"async () => {
                    await Promise.all([
                    fetch('/digits/1.png'),
                    fetch('/digits/2.png'),
                    ]);
                    await new Promise((resolve) => setTimeout(resolve, 250));
                }").ContinueWith(x =>
                {
                    if (x.IsFaulted) throw x.Exception;
                    return t2 = DateTime.UtcNow;
                })
            );

            Assert.True(t2 > t1);
        }

        [PuppeteerTest("page.spec.ts", "Page.waitForNetworkIdle", "should work with no timeout")]
        [PuppeteerTimeout]
        public async Task ShouldWorkWithNoTimeout()
        {
            await Page.GoToAsync(TestConstants.EmptyPage);

            await Task.WhenAll(
                Page.WaitForNetworkIdleAsync(new WaitForNetworkIdleOptions { Timeout = 0 }),
                Page.EvaluateFunctionAsync(@"() => setTimeout(() => {
                        fetch('/digits/1.png');
                        fetch('/digits/2.png');
                        fetch('/digits/3.png');
                    }, 50)")
            );
        }
    }
}
