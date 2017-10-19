using System;
using System.Collections.Generic;

namespace Logic.Entities
{
    public class Feed : IEntity
    {
        public Guid Id { get; set; }
        public List<FeedItem> Items { get; set; }
    }
}