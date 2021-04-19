using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBeePlugin
{
    public class MusicInfo
    {
        public string Artist { get; set; }
        public string TrackArtist { get; set; }
        public string TrackTitle { get; set; }
        public string Album { get; set; }
        public string Duration { get; set; }

        public int Volume { get; set; }
        public int Position { get; set; }

    }
}
