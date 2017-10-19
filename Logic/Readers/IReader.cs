using System;
using System.Collections.Generic;
using Logic.Entities;

namespace Logic.Readers
{
    public interface IReader
    {
        IEnumerable<FeedItem> Read(string url);
    }
}