using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;

namespace PlaylistManager
{upa
    internal class Program
    {
        static void Main()
        {
            string? csvPathUsed = null;

            var playlist = new Playlist();
            string defaultCsv = Path.Combine(AppContext.BaseDirectory, "songs_dataset.csv");
            Console.WriteLine("===Project 2: Music Playlist Manager ===\n");

            if (File.Exists(defaultCsv))
            {
                LoadFromCsvIntoPlaylist(defaultCsv, playlist);
                csvPathUsed = defaultCsv;
                Console.WriteLine($"Loaded CSV: {defaultCsv}\n");
            }
            else
            {
                Console.WriteLine("Could not find songs_dataset.csv in the output folder");
                Console.Write("Enter full path to your CSV (or press Enter to skip): ");
                var path = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    LoadFromCsvIntoPlaylist(path, playlist);
                    csvPathUsed = path;
                    Console.WriteLine($"Loaded CSV: {path}\n");
                }
                else
                {
                    Console.WriteLine("Skipping CSV load. Playlist will start empty.\n");
                }
            }

            SongNode? current = playlist.Head;

            while (true)
            {
                Console.WriteLine("\n--- Menu ---");
                Console.WriteLine("1) Display playlist");
                Console.WriteLine("2) Search by title (show position)");
                Console.WriteLine("3) Display sorted by artist");
                Console.WriteLine("4) Display sorted by duration");
                Console.WriteLine("5) Shuffle playlist (rebuild list)");
                Console.WriteLine("6) Playback: show current / next / previous");
                Console.WriteLine("7) Delete by title");
                Console.WriteLine("8) Delete by index");
                Console.WriteLine("9) Add a song");
                Console.WriteLine("10) Save playlist to csv");
                Console.WriteLine("0) Exit");
                Console.Write("Choose: ");

                var choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        playlist.DisplayAll();
                        break;

                    case "2":
                        SearchByTitle(playlist);
                        break;

                    case "3":
                        DisplaySortedByArtist(playlist);
                        break;

                    case "4":
                        DisplaySortedByDuration(playlist);
                        break;

                    case "5":
                        playlist = ShuffleRebuild(playlist);
                        current = playlist.Head;
                        Console.WriteLine("Shuffled playlist ✅");
                        break;

                    case "6":
                        current = PlaybackMenu(playlist, current);
                        break;

                    case "7":
                        DeleteByTitleMenu(playlist);
                        break;

                    case "8":
                        DeleteByIndexMenu(playlist);
                        break;

                    case "9":
                        AddSongMenu(playlist, csvPathUsed);
                        break;

                    case "10":
                        if (string.IsNullOrWhiteSpace(csvPathUsed))
                        {
                            Console.WriteLine("No CSV loaded yet, so it cannot be saved");
                        }
                        else
                        {
                            SavePlaylistToCsv(csvPathUsed, playlist);
                            Console.WriteLine($"Saved {csvPathUsed}");
                        }
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        private static void SavePlaylistToCsv(string csvPath, Playlist playlist)
        {
            static string Q(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";

            using var writer = new StreamWriter(csvPath, false);

            writer.WriteLine("Index, Title, Artist, Album, Duration, Genre");

            int i = 0;
            var node = playlist.Head;
            while (node != null)
            {
                var s = node.Data;
                string dur = $"{s.DurationSeconds / 60:D2}:{s.DurationSeconds % 60:D2}";

                writer.WriteLine($"{i},{Q(s.Title)}, {Q(s.Artist)}, {Q(s.Album)}, {Q(dur)}, {Q(s.Genre)}");
                node = node.Next;
                i++;

            }
        }

        private static void LoadFromCsvIntoPlaylist(string csvPath, Playlist playlist)
        {
            using var parser = new TextFieldParser(csvPath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            
            if (!parser.EndOfData)
            {
                _ = parser.ReadFields();
            }

            while (!parser.EndOfData)
            {
                var fields = parser.ReadFields();
                if (fields == null || fields.Length < 6) continue;

                
                string title = fields[1]?.Trim() ?? "";
                string artist = fields[2]?.Trim() ?? "";
                string album = fields[3]?.Trim() ?? "";
                string durationRaw = fields[4]?.Trim() ?? "";
                string genre = fields[5]?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(title)) continue;

                int durationSeconds = ParseDurationToSeconds(durationRaw);

                var song = new Song(title, artist, album, durationSeconds, genre);
                playlist.Append(song);
            }
        }

        private static int ParseDurationToSeconds(string duration)
        {
            if (string.IsNullOrWhiteSpace(duration)) return 0;

            duration = duration.Trim();

           
            if (duration.Contains(':'))
            {
                var parts = duration.Split(':');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int mins) &&
                    int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int secs))
                {
                    return Math.Max(0, mins) * 60 + Math.Max(0, secs);
                }
            }

            
            if (int.TryParse(duration, NumberStyles.Integer, CultureInfo.InvariantCulture, out int totalSecs))
                return Math.Max(0, totalSecs);

            return 0;
        }

        private static List<Song> ToSongList(Playlist playlist)
        {
            var list = new List<Song>(playlist.Count);
            var node = playlist.Head;

            while (node != null)
            {
                list.Add(node.Data);
                node = node.Next;
            }

            return list;
        }

        private static void LoopSongMenu(Playlist playlist, ref SongNode? current)
        {
            Console.WriteLine("Enter title to loop: ");
            var title = Console.ReadLine() ?? "";

            var node = playlist.Head;
            while (node != null && !node.Data.Title.Equals(title, StringComparison.OrdinalIgnoreCase))
                node = node.Next;

            if (node == null)
            {
                Console.WriteLine("Song not Found");
                return;
            }

            Console.WriteLine("How many times ? ");
            if (!int.TryParse(Console.ReadLine(), out int times) || times <= 0)
            {
                Console.WriteLine("Invalid number");
                return;
            }

            current = node;
            for (int i = 1; i <= times; i++)
                Console.WriteLine($"Loop/{times}: {current.Data}");
        }

        private static void AddSongMenu(Playlist playlist, string? csvPathUsed)
        {
            Console.WriteLine("Title: ");
            var title = Console.ReadLine() ?? "";

            Console.WriteLine("Artist: ");
            var artist = Console.ReadLine() ?? "";

            Console.WriteLine("Album: ");
            var album = Console.ReadLine() ?? "";

            Console.WriteLine("Duration (mm:ss or in seconds): ");
            var dur = Console.ReadLine() ?? "";
            int seconds = ParseDurationToSeconds(dur);

            Console.WriteLine("Genre: ");
            var genre = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("Title can't be empty.");
                return;
            }

            playlist.Append(new Song(title, artist, album, seconds, genre));
            Console.WriteLine("Song Added");

            if (!string.IsNullOrWhiteSpace(csvPathUsed))
            {
                SavePlaylistToCsv(csvPathUsed, playlist);
                Console.WriteLine($"Auto-saved to CSV ({csvPathUsed})");
            }
            else
            {
                Console.WriteLine("No CSV path available to auto-save. Use option 10 after loading a CSV.");
            }
        }



        private static void SearchByTitle(Playlist playlist)
        {
            Console.Write("Enter title (or part of title): ");
            var query = Console.ReadLine() ?? "";

            var results = playlist.SearchTitlesContaining(query);

            if (results.Count == 0)
            {
                Console.WriteLine("No matches found.");
                return;
            }

            Console.WriteLine($"\nMatches ({results.Count}):");
            foreach (var r in results)
            {
                Console.WriteLine($"{r.Index}: {r.Song}");
            }

            Console.WriteLine("\nTip: Use the INDEX shown above for Delete by index.");
        }


        private static void DeleteByTitleMenu(Playlist playlist)
        {
            Console.Write("Enter title to delete: ");
            var title = Console.ReadLine() ?? "";

            bool deleted = playlist.DeleteByTitle(title);
            Console.WriteLine(deleted ? "Deleted!" : "Title not found!");
        }

        private static void DeleteByIndexMenu(Playlist playlist)
        {
            Console.Write("Enter index to delete (use Display playlist indexes): ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out int index))
            {
                Console.WriteLine("Invalid number.");
                return;
            }

            
            var matches = playlist.SearchTitlesContaining(""); 
            var item = matches.FirstOrDefault(x => x.Index == index);

            bool deleted = playlist.DeleteByIndex(index);

            if (deleted)
            {
                Console.WriteLine(item.Song != null
                    ? $"Deleted! {item.Song.Title} by {item.Song.Artist}"
                    : "Deleted! ");
            }
            else
            {
                Console.WriteLine("Index out of range  ");
            }
        }


        private static void DisplaySortedByArtist(Playlist playlist)
        {
            var songs = ToSongList(playlist)
                .OrderBy(s => s.Artist, StringComparer.OrdinalIgnoreCase)
                .ThenBy(s => s.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (songs.Count == 0)
            {
                Console.WriteLine("Playlist is empty.");
                return;
            }

            Console.WriteLine("\n--- Sorted by Artist ---");
            for (int i = 0; i < songs.Count; i++)
                Console.WriteLine($"{i}: {songs[i]}");
        }

        private static void DisplaySortedByDuration(Playlist playlist)
        {
            var songs = ToSongList(playlist)
                .OrderBy(s => s.DurationSeconds)
                .ThenBy(s => s.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (songs.Count == 0)
            {
                Console.WriteLine("Playlist is empty.");
                return;
            }

            Console.WriteLine("\n--- Sorted by Duration ---");
            for (int i = 0; i < songs.Count; i++)
                Console.WriteLine($"{i}: {songs[i]}");
        }

       
        private static Playlist ShuffleRebuild(Playlist playlist)
        {
            var songs = ToSongList(playlist);

           
            var rng = new Random();
            for (int i = songs.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (songs[i], songs[j]) = (songs[j], songs[i]);
            }

            var shuffled = new Playlist();
            foreach (var s in songs)
                shuffled.Append(s);

            return shuffled;
        }

        

        private static SongNode? PlaybackMenu(Playlist playlist, SongNode? current)
        {
            if (playlist.Head == null)
            {
                Console.WriteLine("Playlist is empty.");
                return null;
            }

           
            current ??= playlist.Head;

            while (true)
            {
                Console.WriteLine("\nPlayback:");
                Console.WriteLine("1) Show current");
                Console.WriteLine("2) Next");
                Console.WriteLine("3) Previous");
                Console.WriteLine("0) Back to main menu");
                Console.Write("Choose: ");

                var c = Console.ReadLine()?.Trim();

                if (c == "0") return current;

                if (c == "1")
                {
                    Console.WriteLine($"Now at: {current!.Data}");
                }
                else if (c == "2")
                {
                    current = current!.Next ?? playlist.Head;
                    Console.WriteLine($"Next: {current.Data}");
                }
                else if (c == "3")
                {
                    current = current!.Prev ?? playlist.Tail;
                    Console.WriteLine($"Previous: {current!.Data}");
                }
                else
                {
                    Console.WriteLine("Invalid option.");
                }

            }
        }
    }
}





            

        
    
