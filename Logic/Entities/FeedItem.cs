using System;
using System.Collections.Generic;

namespace Logic.Entities
{
    
    public class FeedItem
    {
        public static List<FeedItem> FeedItemList = new List<FeedItem>();
        public string Id { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }

        public List<FeedItem> getFeedItems()
        {
            return FeedItemList;
        }
    }
}