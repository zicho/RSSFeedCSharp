using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class Reader
    {
        public async Task<string> DownloadFeed(string url) //method to get all the data from an RSS feed
        {
            return await Task.Run(() =>
            {
                String text = null;

                using (var client = new System.Net.WebClient())
                {
                    try
                    {
                        client.Encoding = Encoding.UTF8;
                        text = client.DownloadString(url);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Something went wrong.");
                    }
                }
                return text;
            });
        }
    }
}
