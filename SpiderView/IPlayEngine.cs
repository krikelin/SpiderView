using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spider.Media
{
    /// <summary>
    /// A resource is a certain resource
    /// </summary>
    public abstract class Resource
    {
        public String Uri
        {
            get
            {
                return this.Service.Namespace + ":" + this.GetType().Name.ToLower().Replace("release", "album") + ":" + Identifier;
            }
        }
        public String Identifier { get; set; }
        public String Name { get; set; }
        public IMusicService  Service { get; set; }
        public String Image { get; set; }
        
        /// <summary>
        /// Creates a new resource
        /// </summary>
        /// <param name="service"></param>
        public Resource(IMusicService service)
        {
            this.Service = service;
            this.Image = "http://o.scdn.co/300/69bfa0d62f92a60ffbc98a7c3df87928da6d5c39";
        }
    }

    /// <summary>
    /// Artist
    /// </summary>
    public class Artist : Resource
    {
        public String Biography { get; set; }
        public int Year { get; set; }
        public ReleaseCollection Albums;
        public ReleaseCollection Singles;
        
        /// <summary>
        /// Loads releases. This method blocks the curren thread
        /// </summary>
        public void LoadReleases()
        {
            this.Albums = this.Service.LoadReleasesForGivenArtist(this, ReleaseType.Album, 1);
            this.Singles = this.Service.LoadReleasesForGivenArtist(this, ReleaseType.Single, 1);

        }
        public Artist(IMusicService service)
            : base(service)
        {
        }
       
    }
    public class Track : Resource
    {
        public track Element { get; set; }
        public Artist[] Artists { get; set; }
        public Release Album { get; set; }
        public int Duration { get; set; }
        public bool Loaded { get; set; }
        public bool Playing
        {
            get
            {
                return this.Service.NowPlayingTrack == this;
            }
        }
        public class TrackLoadEventArgs
        {
            public Object Data;
            public Track Track;
        }
        public delegate void TrackLoadEventHandler(object sender, TrackLoadEventArgs e);
        public event TrackLoadEventHandler TrackLoaded;

        /// <summary>
        /// Load async
        /// </summary>
        public void LoadAsync(object data)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync(data);
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Loaded = true;
            if (TrackLoaded != null)
            {
                TrackLoaded(this, new TrackLoadEventArgs() { Data = e.Result, Track = this });
            }
            
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Track t = this.Service.LoadTrack(this.Identifier);
            this.Duration = t.Duration;
            this.Artists = t.Artists;
            this.Album = t.Album;
            this.Name = t.Name;
            e.Result = t;
           
        }
        /// <summary>
        /// Play the track
        /// </summary>
        public void Play()
        {
          
            this.Service.Play(this);
            if (Element != null)
            {
                Element.Board.Invalidate(new Rectangle(Element.X, Element.Y, Element.Width, Element.Height));
            }
        }
        public Track(IMusicService service)
            : base(service)
        {
            this.Duration = 3;
        }
        public Track(IMusicService service, String identifier)
            : base(service)
        {
            this.Identifier = identifier;
            this.Duration = 3;
        }
    }
    public class Release : Context
    {
        public Artist Artist { get; set; }
        
        private TrackCollection tracks;
        public new TrackCollection Tracks
        {
            get
            {
                return tracks;
            }
        }
        private bool isTracksLoaded;
        public bool IsTracksLoaded
        {
            get
            {
                return isTracksLoaded;
            }
        }
       
        public void LoadTracks()
        {
            tracks = this.Service.LoadTracksForGivenRelease(this);
        }
        public Release(IMusicService service)
            : base(service)
        {
        }
    }

    /// <summary>
    /// A collection of resources for a given context
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResourceCollection<T> : Resource, IList<T>
    {
        public List<T> Items
        {
            get
            {
                return this.items;
            }
        }
        private List<T> items;
        public ResourceCollection(IMusicService service, List<T> items)
            : base(service)
        {
            this.items = items;
        }

        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return items[index];
            }
            set
            {
                items[index] = value;
            }
        }

        public void Add(T item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return items.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
    public class PlaylistCollection : ResourceCollection<Playlist>
    {
        public PlaylistCollection(IMusicService service, List<Playlist> playlists)
            : base(service, playlists)
        {
        }
    }
    public class ArtistCollection : ResourceCollection<Artist>
    {
        public ArtistCollection(IMusicService service, List<Artist> artists)
            : base(service, artists)
        {
        }
    }
    public class TrackCollection : ResourceCollection<Track>
    {
        private Context context;
        public Context Context
        {
            get
            {
                return this.context;
            }
        }
        public TrackCollection(IMusicService service, Context context, List<Track> tracks)
            : base(service, tracks)
        {
            this.context = context;
        }
        public void LoadTracksForCurrentContext()
        {
        }
    }
    public class ReleaseCollection : ResourceCollection<Release>
    {
        public ReleaseCollection(IMusicService service, List<Release> releases)
            : base(service, releases)
        {
        }
    }
   
    public class Context : Resource
    {
        public List<Track> Tracks { get; set; }
        public Context(IMusicService service)
            : base(service)
        {
            
            Tracks = new List<Track>();
        }
    }
    /// <summary>
    /// Search result
    /// </summary>
    public class SearchResult : Context
    {
        public SearchResult(IMusicService service)
            : base(service)
        {
        }
        public ArtistCollection Artists { get; set; }
        public ReleaseCollection Albums { get; set; }
        public ReleaseCollection Singles { get; set; }
        public PlaylistCollection Playlists { get; set; }
        public TrackCollection Tracks { get; set; }
        public int Pages { get; set; }
      
    }
    /// <summary>
    /// A playlist
    /// </summary>
    public class Playlist : Context
    {
        public Playlist(IMusicService service)
            :base(service)
        {

        }
    }
    public enum ReleaseType {
        Single, Album, EP, Compilation
    }
    public delegate void PlayStateChangedEventHandler(object sender, EventArgs e);
    /// <summary>
    /// An engine
    /// </summary>
    public interface IMusicService
    {
        
        event PlayStateChangedEventHandler PlaybackFinished;
        /// <summary>
        /// Namespace of the service
        /// </summary>
        String Namespace { get; }

        /// <summary>
        /// Name of the service
        /// </summary>
        String Name { get; }
        /// <summary>
        /// Plays the track
        /// </summary>
        /// <param name="identifier"></param>
        void Play(Track track);

        /// <summary>
        /// Stops the song
        /// </summary>
        void Stop();

        /// <summary>
        /// Pauses the song
        /// </summary>
        void Pause();

        /// <summary>
        /// Seeks within a song
        /// </summary>
        /// <param name="pos"></param>
        void Seek(int pos);

        /// <summary>
        /// Loads an artist. This method is synchronisly.
        /// </summary>
        /// <remarks>This method only loads the meta-data of the artist.</remarks>
        /// <param name="identifier"></param>
        Artist LoadArtist(String identifier);
        Track LoadTrack(String identifier);
        ReleaseCollection LoadReleasesForGivenArtist(Artist artist, ReleaseType type, int page);
        TrackCollection LoadTracksForPlaylist(Playlist playlist);
        /// <summary>
        /// Loads an album, with a list of songs.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Release LoadRelease(String identifier);

        TrackCollection LoadTracksForGivenRelease(Release release);


        /// <summary>
        /// Loads a playlist
        /// </summary>
        /// <param name="username"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Playlist LoadPlaylist(String username, String identifier);

        /// <summary>
        /// Find the songs
        /// </summary>
        /// <param name="query"></param>
        /// <param name="maxResults"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        SearchResult Find(String query, int maxResults, int page);
        Track NowPlayingTrack { get; }
        
    }
    public class DummyService : IMusicService
    {
        public DummyService()
        {

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += timer_Tick;
        }
        private Track nowPlayingTrack;
        public Track NowPlayingTrack
        {
            get
            {
                return nowPlayingTrack;
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (NowPlayingTrack == null)
                return;

            position++;
            if (position >= nowPlayingTrack.Duration)
            {
                if (PlaybackFinished != null)
                {
                    timer.Stop();
                    position = 0; // Reset the position
                    PlaybackFinished(this, new EventArgs());
               //     nowPlayingTrack = null;
                }
                
            }
        }
        public event PlayStateChangedEventHandler PlaybackFinished;
        public string Namespace
        {
            get { return "dummy"; }
        }
        /// <summary>
        /// Simulate the track playback by a timer
        /// </summary>
        System.Windows.Forms.Timer timer;
        public string Name
        {
            get { return "Dumify"; }
        }
        int position = 0;
        private Track CurrentTrack;
        public void Play(Track track)
        {
            this.nowPlayingTrack = track;
            
            timer.Start();
            
        }

        public void Stop()
        {
            if (nowPlayingTrack == null)
                return;
            timer.Stop(); // Stop "playback"
            position = 0; // Reset position
            nowPlayingTrack = null;
        }

        public void Pause()
        {
            if (nowPlayingTrack == null)
                return;
            timer.Stop();
        }

        public void Seek(int pos)
        {
            if (CurrentTrack == null)
                return;
            position = pos;
            if (position >= CurrentTrack.Duration)
            {
                if (PlaybackFinished != null)
                {
                    timer.Stop();
                    position = 0; // Reset the position
                    PlaybackFinished(this, new EventArgs());
                    nowPlayingTrack = null;
                }

            }
        }

        public Artist LoadArtist(string identifier)
        {
            Thread.Sleep(1000);
            Artist t = new Artist(this);
            t.Identifier = identifier;
            t.Service = this;
            t.Year = new DateTime().Year;
            t.Name = identifier;
            return t;
        }

       

        public Playlist LoadPlaylist(string username, string identifier)
        {
            Thread.Sleep(100); // simulate connection elapse
            Playlist playlist = new Playlist(this);
            playlist.Name = username + " : " + identifier;
            playlist.Identifier = identifier;
            return playlist;
        }

        public SearchResult Find(string query, int maxResults, int page)
        {
            Thread.Sleep(100); // simulate connection elapse
            SearchResult sr = new SearchResult(this);
            sr.Albums = new ReleaseCollection(this, new List<Release>());
            sr.Albums.Add(new Release(this) { Service = this, Name = query + "'1", Identifier = "r" });
            sr.Tracks = new TrackCollection(this, sr, new List<Track>());
            return sr;
        }


        public ReleaseCollection LoadReleasesForGivenArtist(Artist artist, ReleaseType type, int page)
        {
            List<Release> items = new List<Release>();
            for (var i = 0; i < 2; i++)
            {
                var item = new Release(this) { Name = "In and Out of Love - Version " + i.ToString(), Artist = new Artist(this) { Name = "Armin Van Buuren", Identifier = "41241242", Service=this }, Identifier="AIMO" };
                items.Add(item);
            }
            ReleaseCollection rc = new ReleaseCollection(this, items);
            rc.Items.AddRange(items);
            return rc;
        }

        public TrackCollection LoadTracksForPlaylist(Playlist playlist)
        {
            Thread.Sleep(1000);
            TrackCollection tc = new TrackCollection(this, playlist, new List<Track>());
            for(var i = 0; i < 3; i++) {
                Track track = new Track(this)
                {
                    Identifier = "5124525ffs12",
                    Name = "Test",
                    Artists = new Artist[] { new Artist(this) { Name = "TestArtist", Identifier = "2FOU" } }
                };
                tc.Add(track);
            }
            return tc;
        }

        public Release LoadRelease(string identifier)
        {
            Release release = new Release(this);
            release.Identifier = identifier;
            release.Name = identifier;
            release.Service = this;
            release.Artist = new Artist(this) { Name = "Armin Van Buuren", Identifier = "2fafaefr" };
            return release;

        }

      


        public TrackCollection LoadTracksForGivenRelease(Release release)
        {
            Thread.Sleep(100);
            TrackCollection tc = new TrackCollection(this, release, new List<Track>());
            for (var i = 0; i < 10; i++)
            {
                Track track = new Track(this);
                track.Name = "Track " + i.ToString();
                track.Identifier = "track";
                track.Album = release;
                track.Artists = new Artist[] { new Artist(this) { Name = "Test", Identifier = "test" } };
                tc.Add(track);
            }
            return tc;

        }


        public Track LoadTrack(string identifier)
        {
            Thread.Sleep((int)Math.Round((double)new Random().Next(1,3000)));
            return new Track(this, identifier)
            {
                Name = "Test",
                Artists = new Artist[] {
                   new Artist(this) {
                       Name = "Test",
                       Identifier = "rom"
                   }
                },
                Album = new Release(this)
                {

                    Artist = new Artist(this)
                    {
                        Identifier = "rom",
                        Name = "Test"
                    },
                    Name = "Test",
                    Identifier = "Test"
                }
            };
        }
    }

}
