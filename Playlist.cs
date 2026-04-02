using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistManager
{
    public class Playlist
    {
        public SongNode? Head { get; set; }
        public SongNode? Tail { get; set; }
        public int Count { get; private set; }

        public void Append(Song song)
        {
            var node = new SongNode(song);

            if (Head == null)
            {
                Head = Tail = node;
                Count = 1;
                return;
            }

            Tail!.Next = node;
            node.Prev = Tail;
            Tail = node;
            Count++;
        }

        private static string Normalise(string s)
        {
            if (s == null) return "";
            return s.Trim().ToLowerInvariant();
        }


        public void DisplayAll()
        {
            if (Head == null)
            {
                Console.WriteLine("Playlist is empty");
                return;
            }

            int index = 0;
            var current = Head;
            while (current != null)
            {
                Console.WriteLine($"{index}: {current.Data}");
                current = current.Next;
                index++;
            }
        }

        public int FindIndexByTitle(string title)
        {
            string q = Normalise(title);

            int index = 0;
            var current = Head;

            while (current != null)
            {
                if (Normalise(current.Data.Title) == q)
                    return index;

                current = current.Next;
                index++;
            }

            return -1;
        }

        public bool DeleteByTitle(string title)
        {
            string q = Normalise(title);

            var current = Head;
            while (current != null)
            {
                if (Normalise(current.Data.Title) == q)
                {
                    DeleteNode(current);
                    return true;
                }
                current = current.Next;
            }

            return false;
        }

        public bool DeleteByIndex(int index)
        {
            if (index < 0 || index >= Count) return false;

            int i = 0;
            var current = Head;

            while (current != null)
            {
                if (i == index)
                {
                    DeleteNode(current);
                    return true;
                }
                current = current.Next;
                i++;
            }
            return false;
        }

        public List<(int Index, Song Song)> SearchTitlesContaining(string text)
        {
            string q = Normalise(text);
            var results = new List<(int, Song)>();

            int index = 0;
            var current = Head;

            while (current != null)
            {
                if (Normalise(current.Data.Title).Contains(q))
                    results.Add((index, current.Data));

                current = current.Next;
                index++;
            }

            return results;
        }

        public void AddSongToEnd(string title, string artist, string album, int durationSeconds, string genre)
        {
            Append(new Song(title, artist, album, durationSeconds, genre));
        }


        private void DeleteNode(SongNode node)
        {
            if (Count == 0) return;

            if (node.Prev != null) node.Prev.Next = node.Next;
            else Head = node.Next;

            if (node.Next != null) node.Next.Prev = node.Prev;
            else Tail = node.Prev;

            Count--;

        } 
    }
}

        
