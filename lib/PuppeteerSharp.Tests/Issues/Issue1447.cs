using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PuppeteerSharp.Tests.Attributes;

namespace PuppeteerSharp.Tests.Issues
{
    public class Issue1447 : PuppeteerPageBaseTest
    {
        public Issue1447(): base() { }

        [Ignore("It's an example")]
        public async Task Example()
        {
            var opts = new LaunchOptions
            {
                Headless = false,
                DefaultViewport = null,
                IgnoredDefaultArgs = new[] { "--enable-automation" }
            };

            await using (var browser = await new Launcher().LaunchAsync(opts))
            {
                var pages = await browser.PagesAsync();

                var page = pages.ElementAt(0);

                for (int i = 0; i < 20; i++)
                {
                    await Navigate(page, "https://distilnetworks.com");
                    await Navigate(page, "https://mail.com");
                    await Navigate(page, "https://distilnetworks.com");
                    await Navigate(page, "https://vk.com");
                    await Navigate(page, "https://distilnetworks.com");
                    await Navigate(page, "https://mail.com");
                    await Navigate(page, "https://distilnetworks.com");
                    await Navigate(page, "https://mail.com");
                    await Navigate(page, "about:blank");
                }
            }
        }

        public Task<IResponse> Navigate(IPage page, string url)
        {
            return page.MainFrame.GoToAsync(
                url,
                new NavigationOptions { Timeout = 0, WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded } });
        }
    }
}
