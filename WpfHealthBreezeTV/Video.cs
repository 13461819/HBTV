using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfHealthBreezeTV
{
    public class Video
    {
        public string code { get; set; }
        public string description { get; set; }
        public long id { get; set; }
        public int playtime { get; set; }
        public string thumbnail { get; set; }
        public string title { get; set; }
        public DateTime updated { get; set; }
        public Grid searchGrid { get; set; }
        private readonly List<Grid> _channelGrid = new List<Grid>();
        public List<Grid> channelGrid { get { return _channelGrid; } }
        public string testStr { get; set; }
        public BitmapImage img { get; set; }
        public byte[] imageBytes { get; set; }
    }
}
