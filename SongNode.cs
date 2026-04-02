using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistManager
{
    public class SongNode
    {
        public Song Data { get; set; }
        public SongNode? Next { get; set; }
        public SongNode? Prev {  get; set; }

        public SongNode(Song data)
        {
            Data = data;
        }
    }
}
