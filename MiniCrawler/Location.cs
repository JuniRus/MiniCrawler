using System;

namespace MiniCrawler
{
    public class Location
    {
        public string Uri { get; set; }
        public int CurLocAbs { get; set; }
        public int CurLocRel { get; set; }

        public Location(string uri, int cla, int clr)
        {
            Uri = uri;
            CurLocAbs = cla;
            CurLocRel = clr;
        }
    }
}
