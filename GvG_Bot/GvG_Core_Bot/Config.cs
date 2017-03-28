using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GvG_Core_Bot
{
    public class Config
    {
        public string token { get; set; }
        public IList<ulong> owner_ids { get; set; }
        public char char_prefix { get; set; }
        public string pub_gvg_chan_name {get;set;}
        public string oc_chan_name { get; set; }
        public string gaia_chan_name { get; set; }
        public string guardian_chan_name { get; set; }
    }
}
