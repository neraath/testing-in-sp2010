using Microsoft.SharePoint;

namespace SharePoint.Library
{
    public class WebSiteManager
    {
        private string siteUrl;

        public WebSiteManager(string url)
        {
            this.siteUrl = url;
        }

        public string GetSiteName()
        {
            using (SPSite site = new SPSite(siteUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    return web.Title;
                }
            }
        }

        public int GetNumberOfSubSites()
        {
            using (SPSite site = new SPSite(siteUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    return web.Webs.Count;
                }
            }
        }
    }
}
