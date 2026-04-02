using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistManager
{
    public class Song
    {
        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public int DurationSeconds { get; }  
        public string Genre { get; }

        public Song(string title, string artist, string album, int durationSeconds, string genre)
        {
            Title = title;
            Artist = artist;
            Album = album;
            DurationSeconds = durationSeconds;
            Genre = genre;
        }

        public override string ToString()
        {
            var mins = DurationSeconds / 60;
            var secs = DurationSeconds % 60;
            return $"{Title} | {Artist} | {Album} | {mins:D2}:{secs:D2} | {Genre}";
        }
    }
}
