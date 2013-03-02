using System.Collections.Generic;
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

        public IEnumerable<string> GetUsersForSite()
        {
            using (SPSite site = new SPSite(siteUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    List<string> users = new List<string>();

                    for (int i = 0; i < web.Users.Count; i++)
                    {
                        users.Add(web.Users[i].LoginName);
                    }

                    return users;
                }
            }
        }
    }
}
