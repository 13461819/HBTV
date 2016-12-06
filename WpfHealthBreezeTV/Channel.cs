using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfHealthBreezeTV
{
    class Channel
    {
        public string userId { get; set; }
        private List<string> _videos = new List<string>();
        public List<string> videos
        {
            get { return _videos; }
        }
    }
}
