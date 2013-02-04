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
        public float Popularity { get; set; }
        public override bool Equals (object obj)
        {
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return ((Resource)obj).Uri == this.Uri;
        }
        public int RealStatus
        {
            get
            {
                int status = (int)Status;
                return status;
            }
        }
        public enum State 
        {
            Available = 0, BadKarma = 5, Removed = 1, PurchaseOnly = 2, PremiumOnly = 3, NotAvailable = 4
        }
        public bool Available
        {
            get
            {
                return this.Status == Resource.State.Available;
            }
        }
        private State status;
        public State Status
        {
            get
            {
                return this.status;
            }
            set
            {
               
                this.status = value;
            }
        }
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
    public class Country : Resource
    {
        public Country(IMusicService service, String name)
            : base(service)
        {
            this.Identifier = name;
        }
    }

    /// <summary>
    /// A Top List
    /// </summary>
    public class TopList : Context
    {
        public Resource BelongsTo { get; set; }
        public TrackCollection TopTracks
        {
            get;
            set;
        }
        public ReleaseCollection TopAlbums
        {
            get;
            set;
        }
        public TopList(IMusicService service)
            : base(service)
        {
        }
    }
    /// <summary>
    /// An user
    /// </summary>
    public class User : Resource
    {
        public String CanoncialName { get; set; }

        /// <summary>
        /// The artist he/she represents
        /// </summary>
        public Artist Artist { get; set; }
        public User(IMusicService service)
            : base(service)
        {

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
        /// The user the artist belongs to
        /// </summary>
        public User User { get; set; }
        
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
        public String Zerofill(int number)
        {
            return number > 10 ? number.ToString() : "0" + number.ToString();
        }
        
        public track Element { get; set; }
        public Artist[] Artists { get; set; }
        public Release Album { get; set; }
        private Spider.CListView.CListViewItem item;
        public Spider.CListView.CListViewItem Item
        {
            get
            {
                return item;
            }
            set
            {
                item = value;
                if (this.Item != null)
                {
                    this.Item.Name = this.Name;
                    if(this.Artists != null && this.Artists.Length > 0)
                    this.Item.SubItems.Add(this.Artists[0].Name);
                    this.Item.SubItems.Add(String.Format("{0}:{1}", (this.Length.Minutes), Zerofill(this.Length.Seconds)));
                    if(this.Album != null)
                    this.Item.SubItems.Add(this.Album.Name);
                    this.Item.Spawn.Refresh();
                    
                }
            }
        }
        public int Duration { get; set; }
        public TimeSpan Length
        {
            get
            {
                return new TimeSpan(0, 0, Duration);
            }
        }
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
            bw.DoWork += LoadData;
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

        public virtual void LoadData(object sender, DoWorkEventArgs e)
        {
            Track t = this.Service.LoadTrack(this.Identifier);
            this.Duration = t.Duration;
            this.Artists = t.Artists;
            this.Album = t.Album;
            this.Status = t.Status;
            this.Popularity = t.Popularity;
            this.Name = t.Name;
            e.Result = t;
           
        }
        /// <summary>
        /// Play the track
        /// </summary>
        public void Play()
        {
            if (this.Status != State.Available)
            {
                return;
            }
            this.Service.Play(this);
            if (Element != null)
            {
                Element.Board.Invalidate(new Rectangle(Element.X, Element.Y, Element.Width, Element.Height));
            }
            if (Item != null)
            {
                Item.Spawn.Section.SpiderView.Host.PlayContext = Item.Spawn.Section.Board;
                Item.Spawn.Invalidate();
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
        public DateTime ReleaseDate { get; set; }
        public int Year
        {
            get
            {
                return ReleaseDate.Year;
            }
        }
        private TrackCollection tracks;
        public new TrackCollection Tracks
        {
            get
            {
                return tracks;
            }
        }
        public ReleaseType Type { get; set; }
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
    public class UserCollection : ResourceCollection<User>
    {
        public UserCollection(IMusicService service, List<User> users) : base(service, users) {
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

    public class TrackChangedEventArgs
    {
        public Playlist Playlist { get; set; }
        public Track[] Tracks { get; set; }
        public Track Track { get; set; }
        public int Index { get; set; }
        public int NewIndex { get; set; }
        public int[] Indexes { get; set; }
    }
    public delegate void TrackChangedEventHandler(object sender, TrackChangedEventArgs e);
    /// <summary>
    /// A playlist
    /// </summary>
    public class Playlist : Context
    {
        public event TrackChangedEventHandler TrackDeleted;
        public event TrackChangedEventHandler TrackAdded;
        public event TrackChangedEventHandler TrackReordered;
        public void OnTrackDeleted(int[] indexes)
        {
            if (TrackDeleted != null)
            {
                TrackDeleted(this, new TrackChangedEventArgs() {Playlist=this, Indexes=indexes });

            }
        }
        public void OnTrackAdded(Track track, int pos)
        {
            if (TrackAdded != null)
            {
                TrackReordered(this, new TrackChangedEventArgs() { Index = pos, Track = track, NewIndex = pos, Playlist = this });

            }
        }
        public void OnTrackReordered(int pos, int newPos)
        {
            if (TrackReordered != null)
            {
                TrackReordered(this, new TrackChangedEventArgs() { Index = pos, Tracks = (Track[])this.Tracks.Items.GetRange(pos, newPos).ToArray(), NewIndex = newPos, Playlist = this });

            }
        }
        public void Add(Track track)
        {
            this.Service.InsertTrack(this, track, 0);
        }
        public void Insert(Track track, int pos)
        {
            this.Service.InsertTrack(this, track, pos);
        }
        public void Reorder(int oldPos, int[] tracks, int newPos)
        {
            this.Service.ReorderTracks(this, oldPos, tracks, newPos);
        }
        public void Delete(int[] indexes)
        {
            this.Service.DeleteTracks(this, indexes);

        }
        
        public String Description { get; set; }
        public User User { get; set; }
        /// <summary>
        /// LoadTracks must be called first before this will be available
        /// </summary>
        public TrackCollection Tracks { get; set; }
        public delegate decimal sum(Track track);
        public TimeSpan Duration
        {
            get
            {
                if (this.Tracks == null)
                    return new TimeSpan(0);

                return this.Tracks.Aggregate(TimeSpan.Zero, (subtotal, t) => subtotal.Add(new TimeSpan(0, 0, t.Duration)));
            }
        }
        public int Minutes {
            get {
                return this.Duration.Minutes;
            }}
        public void LoadTracks()
        {
            this.Tracks = Service.GetPlaylistTracks(this, 0);

        }

        public Playlist(IMusicService service)
            :base(service)
        {

        }
    }
    public enum SessionState
    {
        LoggedIn, Success, Failed, BadPassword
    }
    public enum LogInResult
    {
        Success, BadPassword, Failure, Banned
    }
    public enum ReleaseType {
        Single = 2, Album = 1, EP = 3, Compilation = 4
    }
    public class UserObjectsEventArgs {
        public String[] Objects;
    }
    public delegate void PlayStateChangedEventHandler(object sender, EventArgs e);
    public delegate void UserObjectsventHandler(object sender, UserObjectsEventArgs e);
    
    /// <summary>
    /// An engine
    /// </summary>
    public interface IMusicService
    {
        
        event TrackChangedEventHandler TrackDeleted;
        event TrackChangedEventHandler TrackAdded;
        event TrackChangedEventHandler TrackReordered;
        event PlayStateChangedEventHandler PlaybackFinished;
        event UserObjectsventHandler ObjectsDelivered;

        
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

        /// <summary>
        /// Loads metadata for the given song
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Track LoadTrack(String identifier);

        /// <summary>
        /// Loads a collection of releases
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        ReleaseCollection LoadReleasesForGivenArtist(Artist artist, ReleaseType type, int page);

        /// <summary>
        /// Loads an album, with a list of songs.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Release LoadRelease(String identifier);

        /// <summary>
        /// Load tracks for the given release
        /// </summary>
        /// <param name="release"></param>
        /// <returns></returns>
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

        /// <summary>
        /// The current now playing track
        /// </summary>
        Track NowPlayingTrack { get; }

        /// <summary>
        /// Get top lsit for resource
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        TopList LoadTopListForResource(Resource res);

        /// <summary>
        /// The user
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        User LoadUser(String identifier);

        /// <summary>
        /// State of the session
        /// </summary>
       
        SessionState SessionState { get; }
        
        /// <summary>
        /// Logs in to the player
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        LogInResult LogIn(string userName, String passWord);

        /// <summary>
        /// Gets the current user
        /// </summary>
        /// <returns></returns>
        User GetCurrentUser();

        /// <summary>
        /// This method can be run on a separate thread ae we assume the event happens asynchronizly
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="track"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool InsertTrack(Playlist playlist, Track track, int pos);

        /// <summary>
        /// This method can be run on a separate thread ae we assume the event happens asynchronizly
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="track"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool ReorderTracks(Playlist playlist, int startPos, int[] tracks, int newPos);

        /// <summary>
        /// This method can be run on a separate thread ae we assume the event happens asynchronizly
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="track"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        bool DeleteTracks(Playlist playlist, int[] indexes);


        /// <summary>
        /// Creates a new playlist
        /// </summary>
        /// <param name="name">The name of the playlist</param>
        /// <returns>The identifier for the new playlist</returns>
        String NewPlaylist(String name);
        /// <summary>
        /// Get tracks from the playlist
        /// </summary>
        /// <param name="playlist">The playlist</param>
        /// <param name="revision">The revision of the playlist</param>
        /// <returns></returns>
        TrackCollection GetPlaylistTracks(Playlist playlist, int revision);

        /// <summary>
        /// Get current country
        /// </summary>
        /// <returns></returns>
        Country GetCurrentCountry();

        /// <summary>
        /// Request user objects
        /// </summary>
        void RequestUserObjects();

        /// <summary>
        /// Move user object
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="pos"></param>       
        void MoveUserObject(int oldPos, int newPos);


        /// <summary>
        /// Insert an user object at the specified location
        /// </summary>
        /// <param name="oldPos"></param>
        /// <param name="newPos"></param>
        void insertUserObject(String uri, int pos);

    }
   

}
