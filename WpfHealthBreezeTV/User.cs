using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfHealthBreezeTV
{
    public class User
    {
        public long uid { get; set; }
        public long did { get; set; }
        public string sessionKey { get; set; }
        public int authorization { get; set; }
        public List<long> downloads { get; set; }
        public string logos { get; set; }
    }
}
