using Spider.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using System.ComponentModel;
namespace Spider
{
    public class DummyService : IMusicService
    {
        public static String CONNECTION_PATH = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=myspotify.mdb;Jet OLEDB:Database Password=;";
        public OleDbConnection MakeConnection()
        {
            return new OleDbConnection(CONNECTION_PATH);
        }
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
            get { return "spotify"; }
        }
        /// <summary>
        /// Simulate the track playback by a timer
        /// </summary>
        System.Windows.Forms.Timer timer;
        public string Name
        {
            get { return "Spotify"; }
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
            DataSet artists = MakeDataSet("SELECT * FROM artist WHERE identifier= '" + identifier + "'");
            DataRow artistRow = artists.Tables[0].Rows[0];
            

            return ArtistFromDataSet(artistRow);
        }


        public Playlist PlaylistFromRow(DataRow row)
        {
            Playlist playlist = new Playlist(this);
            playlist.Name = (String)row["title"];
            playlist.Description = (String)row["playlist.description"];
            playlist.Image = (String)row["playlist.image"];
            playlist.Status = Resource.State.Available;
            playlist.User = new User(this)
            {
                Name = (String)row["users.identifier"],
                Identifier = (String)row["users.identifier"]
            };
            playlist.Identifier =(String)row["playlist.identifier"];
            return playlist;
        }
        public Playlist LoadPlaylist(string username, string identifier)
        {
            String query = "SELECT * FROM users, playlist WHERE playlist.user = users.id AND users.identifier = '" + username + "' AND playlist.identifier = '" + identifier + "'";
            DataSet dr = MakeDataSet(query);
            Playlist pl = PlaylistFromRow(dr.Tables[0].Rows[0]);
            return pl;
        }
        public Dictionary<String, Track> Cache = new Dictionary<string, Track>();
        public SearchResult Find(string query, int maxResults, int page)
        {
            // Find songs
            DataSet tracksResult = MakeDataSet("SELECT * FROM track,artist, release WHERE artist.id = track.artist AND track.album = release.id AND (track.title LIKE '%" + query + "%' OR track.title LIKE '%%') AND (release.title LIKE '%" + query + "%' OR release.title LIKE '%%') AND (artist.title LIKE '%" + query + "%' OR artist.title = '%%')");
            SearchResult sr = new SearchResult(this);
            sr.Tracks = new TrackCollection(this, sr, new List<Track>());
            foreach (DataRow dr in tracksResult.Tables[0].Rows)
            {
                
                sr.Tracks.Add(TrackFromDataRow(dr));
            }

            // Find artists
            DataSet artistsResult = MakeDataSet("SELECT * FROM artist WHERE title LIKE '%" + query + "%'");
            ArtistCollection ac = new ArtistCollection(this, new List<Artist>());
            foreach (DataRow dr in artistsResult.Tables[0].Rows)
            {
                ac.Add(ArtistFromDataSet(dr));
            }
            sr.Artists = ac;

            // Find albums
            DataSet albumsResult = MakeDataSet("SELECT * FROM release, artist WHERE artist.id = release.artist AND release.title LIKE '%" + query + "%'");
            ReleaseCollection rc = new ReleaseCollection(this, new List<Release>());
            foreach (DataRow dr in albumsResult.Tables[0].Rows)
            {
                rc.Add(ReleaseFromDataRow(dr));
            }
            sr.Albums = rc;
            return sr;
        }

        public Artist ArtistFromDataSet(DataRow artistRow)
        {
            Artist artist = new Artist(this);
            artist.Name = (String)artistRow["title"];
            artist.Image = (String)artistRow["image"];
            artist.Identifier = (String)artistRow["identifier"];
            return artist;
        }
        public Release ReleaseFromDataRow(DataRow releaseRow)
        {
            Release r = new Release(this);
            r.Name = (String)releaseRow["release.title"];
            r.Image = (String)releaseRow["release.image"];
            r.Identifier = (String)releaseRow["release.identifier"];
            r.ReleaseDate = (DateTime)releaseRow["release_date"];
                r.Status = (Spider.Media.Resource.State)releaseRow["release.status"];
            
            r.Artist = new Artist(this)
            {
                Name = (String)releaseRow["artist.title"],
                Identifier = (String)releaseRow["artist.identifier"]
            };
            return r;
        }
        public Track TrackFromDataRow(DataRow dr)
        {
            Track t = new Track(this)
            {
                Name = (String)dr["track.title"],
                Identifier = (String)dr["track.identifier"],
                Artists = new Artist[] {
                        new Artist(this) {
                            Name = (String)dr["artist.title"],
                            Identifier = (String)dr["artist.identifier"]
                        }
                    },
                Album = new Release(this)
                {
                    Identifier = (String)dr["release.identifier"],
                    Name = (String)dr["release.title"],
                    Status = (Spider.Media.Resource.State)dr["release.status"],

                }
            };
            var pop = ((decimal)dr["track.popularity"]);
            t.Popularity = (float)pop / 100;
            return t;
        }

        public ReleaseCollection LoadReleasesForGivenArtist(Artist artist, ReleaseType type, int page)
        {
            List<Release> items = new List<Release>();
            DataSet dsReleases = MakeDataSet("SELECT *, release.status FROM release, artist  WHERE artist.id = release.artist AND artist.identifier = '" + artist.Identifier + "' AND type = " + ((int)type).ToString() + " AND release.status = 0 ORDER BY release_date DESC");
           ReleaseCollection rc = new ReleaseCollection(this, items);
            foreach (DataRow releaseRow in dsReleases.Tables[0].Rows)
            {
                Release r = ReleaseFromDataRow(releaseRow);
                items.Add(r);
                
            }
            return rc;
        }

        public TrackCollection LoadTracksForPlaylist(Playlist playlist)
        {
            Thread.Sleep(1000);
            TrackCollection tc = new TrackCollection(this, playlist, new List<Track>());
            for (var i = 0; i < 3; i++)
            {
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
            OleDbConnection conn = MakeConnection();
            String sql = "SELECT * FROM release, artist WHERE artist.id = release.artist AND release.identifier = '" + identifier + "'";
            OleDbDataAdapter oda = new OleDbDataAdapter(sql, conn);
            DataSet ds = new DataSet();
            oda.Fill(ds);
            Release r = ReleaseFromDataRow(ds.Tables[0].Rows[0]);
            conn.Close();
            return r;
        }


        public DataSet MakeDataSet(String sql)
        {
            OleDbConnection conn = MakeConnection();
            conn.Open();
            OleDbDataAdapter oda = new OleDbDataAdapter(sql, conn);
            DataSet ds = new DataSet();
            oda.Fill(ds);
            conn.Close();
            return ds;
        }

        public TrackCollection LoadTracksForGivenRelease(Release release)
        {
            String sql = "SELECT * FROM track, release, artist WHERE (release.ID = track.album) AND (artist.ID = track.artist) AND (artist.ID = track.artist)  AND " +
                "release.identifier = '" + release.Identifier + "'";
            DataSet result = MakeDataSet(sql);
            TrackCollection tc = new TrackCollection(this, release, new List<Track>());
            foreach (DataRow row in result.Tables[0].Rows)
            {
                Track t = TrackFromDataRow(row);
                tc.Add(t);
            }

            return tc;

        }


        public Track LoadTrack(string identifier)
        {
            if (Cache.ContainsKey(identifier))
            {
                return Cache[identifier];
            }
            DataSet trackSet = MakeDataSet("SELECT * " +
                "FROM (artist INNER JOIN release ON artist.ID = release.artist) INNER JOIN track ON (release.ID = track.album) AND (artist.ID = track.artist) AND (artist.ID = track.artist)" + 
                "WHERE (((track.identifier)=\"" + identifier + "\"));");
            Track t = new Track(this);
            DataRow track = trackSet.Tables[0].Rows[0];
            t.Name = (String)track["track.title"];
            t.Identifier = identifier;
            var pop = (decimal)track["track.popularity"];
            t.Popularity = (float)pop / 100f;
            t.Status = (Spider.Media.Resource.State)track["track.status"];
            t.Artists = new Artist[] { new Artist(this) { Name = (String)track["artist.title"], Identifier = (String)track["artist.identifier"] } };
            t.Album = new Release(this);
            t.Duration = (int)track[25];
            t.Album = new Release(this) 
            {
                Name = (String)track["release.title"],
                Identifier = (String)track["release.identifier"]
            };


            try
            {
                Cache.Add(identifier, t);
            }
            catch (Exception e)
            {
                try
                {
                    return Cache[identifier];
                }
                catch (Exception ex)
                {
                }
            }
            return t;
        }


        public TopList LoadTopListForResource(Resource res)
        {
            TopList t = new TopList(this);
            if (res.GetType() == typeof(Country))
            {
                DataSet topTracks = MakeDataSet("SELECT TOP 100  * FROM track, artist, release WHERE track.artist = artist.id AND track.album = release.id ORDER BY track.popularity DESC");
                t.TopTracks = new TrackCollection(this, t, new List<Track>());
                t.TopAlbums = new ReleaseCollection(this,  new List<Release>());
                foreach (DataRow row in topTracks.Tables[0].Rows)
                {
                    Track _track = TrackFromDataRow(row);
                    t.TopTracks.Add(_track);
                }
                DataSet topAlbums= MakeDataSet("SELECT TOP 100 * FROM artist, release WHERE release.artist = artist.id ORDER BY release.popularity DESC");
                foreach (DataRow row in topTracks.Tables[0].Rows)
                {
                    Release _album = ReleaseFromDataRow(row);
                    t.TopAlbums.Add(_album);
                }
                return t;
            }
            return null;
            

        }
        public Country GetCurrentCountry()
        {
            return new Country(this, "Sweden");
        }
        public User MakeUserFromRow(DataRow dr)
        {
            User user = new User(this);
            user.Name = (String)dr["canoncialName"];
            user.Image = (String)dr["image"];
            try {
                DataRow artist = MakeDataSet("SELECT * FROM artist WHERE artist.user = " + ((int)dr["id"]).ToString()).Tables[0].Rows[0];
                Artist art = ArtistFromDataSet(artist);
                user.Artist = art;
            } catch (Exception e) {
            }
            return user;
        }
        public User LoadUser(string identifier)
        {
            DataSet ds = MakeDataSet("SELECT * FROM users WHERE identifier = '" + identifier + "'");
            if (ds.Tables[0].Rows.Count < 1)
            {
                return new User(this)
                {
                    Name = identifier,
                    CanoncialName = "unknown"
                };
            }
           return MakeUserFromRow(ds.Tables[0].Rows[0]);


           
        }


        public SessionState SessionState
        {
            get { return SessionState.LoggedIn; }
        }

        public LogInResult LogIn(string userName, string passWord)
        {
            return LogInResult.Success;
        }

        public User GetCurrentUser()
        {
            return new User(this)
            {
                Name = "Test"
            };
        }


        public bool InsertTrack(Playlist playlist, Track track, int pos)
        {
            DataSet t = MakeDataSet("SELECT * FROM playlist WHERE identifier = '" + playlist.Identifier + "'");
            DataRow _playlist = t.Tables[0].Rows[0];
            String tracks = (String)_playlist["data"];

            List<String> Rows = new List<string>(tracks.Split('&')); 

            Rows.Insert(pos, "spotify:track:" + track.Identifier + ":user:drsounds");
            
            OleDbConnection conn = MakeConnection();
            conn.Open();
            OleDbCommand command = new OleDbCommand("UPDATE playlist SET data = '" + String.Join("&", Rows.ToArray()) + "' WHERE playlist.identifier = '" + playlist.Identifier + "'", conn);
            command.ExecuteNonQuery();
            conn.Close();


            return true;   
        }

        public bool ReorderTracks(Playlist playlist, int startPos, int[] ltracks, int newPos)
        {
            DataSet t = MakeDataSet("SELECT * FROM playlist WHERE identifier = '" + playlist.Identifier + "'");
            DataRow _playlist = t.Tables[0].Rows[0];
            String tracks = (String)_playlist["data"];

            List<String> Rows = new List<string>(tracks.Split('&'));
            List<Track> movingTracks = new List<Track>();
            foreach(int i in ltracks)
            {
                movingTracks.Add(playlist.Tracks.ElementAt(i));
                Rows.RemoveAt(i);

            }

            int x = 0;
            foreach (Track _track in movingTracks)
            {
                Rows.Insert(newPos + x, "spotify:track:" + _track.Identifier + ":user:drsounds");
                x++;
            }
            
            OleDbConnection conn = MakeConnection();
            conn.Open();
            OleDbCommand command = new OleDbCommand("UPDATE playlist SET data = '" + String.Join("&", Rows.ToArray()) + "' WHERE playlist.identifier = '" + playlist.Identifier + "'", conn);
            command.ExecuteNonQuery();
            conn.Close();
            return true;
        }

        public bool DeleteTrack(Playlist playlist, Track track)
        {
            return true;
        }
        public Track MakeUserTrackFromString(String row)
        {
            String[] parts = row.Split(':');
            Track track = new Track(this,  parts[2]);
            track.LoadAsync(track.Identifier);
            return track;
        }
        public TrackCollection GetPlaylistTracks(Playlist playlist, int revision)
        {
            DataSet ds = MakeDataSet("SELECT data FROM [playlist], [users] WHERE [users].[id] = [playlist].[user] AND [playlist].[identifier] = '" + playlist.Identifier + "' AND [users].[identifier] = '" + playlist.User.Identifier + "'");
            String d = "";
            try
            {
                d = (String)ds.Tables[0].Rows[0]["data"];
            }
            catch (Exception e)
            {
            }
            
            String[] tracks = d.Split('&');
            TrackCollection tc = new TrackCollection(this, playlist, new List<Track>());
            if (String.IsNullOrEmpty(d))
                return tc;
            foreach (String strtrack in tracks)
            {
                if (String.IsNullOrEmpty(strtrack))
                    continue;
                Track pt = MakeUserTrackFromString(strtrack);
                tc.Add((Track)pt);
            }
            return tc;
        }

        public event TrackChangedEventHandler TrackDeleted;

        public event TrackChangedEventHandler TrackAdded;

        public event TrackChangedEventHandler TrackReordered;


        public bool DeleteTracks(Playlist playlist, int[] indexes)
        {
            DataSet t = MakeDataSet("SELECT * FROM playlist WHERE identifier = '" + playlist.Identifier + "'");
            DataRow _playlist = t.Tables[0].Rows[0];
            String tracks = (String)_playlist["data"];

            List<String> Rows = new List<string>(tracks.Split('&'));
            List<Track> movingTracks = new List<Track>();
            foreach (int i in indexes)
            {
                movingTracks.Add(playlist.Tracks.ElementAt(i));
                Rows.RemoveAt(i);

            }

            OleDbConnection conn = MakeConnection();
            conn.Open();
            OleDbCommand command = new OleDbCommand("UPDATE playlist SET data = '" + String.Join("&", Rows.ToArray()) + "' WHERE playlist.identifier = '" + playlist.Identifier + "'");
            command.ExecuteNonQuery();
            conn.Close();
            return true;
        }

        /// <summary>
        /// NOTE: This method MUST return the WHOLE URI, not the identifier only!
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string NewPlaylist(string name)
        {
            String identifier = name.Trim().Replace(" ", "");
            OleDbConnection conn = MakeConnection();
            conn.Open();
            OleDbCommand command = new OleDbCommand("INSERT INTO playlist (identifier,  title, data, description, [image], [user]) VALUES ('" + identifier + "', '" + name + "', '', '', '', 1)", conn);
            command.ExecuteNonQuery();
            conn.Close();
            return "spotify:user:drsounds:playlist:" + identifier;
        }


        public event UserObjectsventHandler ObjectsDelivered;

        public void RequestUserObjects()
        {
            
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted+=bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.ObjectsDelivered != null)
            {
                this.ObjectsDelivered(this, new UserObjectsEventArgs()
                {
                    Objects = bgw
                });
            }
        }
        String[] bgw;
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(100);
            DataSet ds = MakeDataSet("SELECT favorites FROM users WHERE users.identifier = 'drsounds'");
            String d = "";
            try
            {
                d = (String)ds.Tables[0].Rows[0]["favorites"];
            }
            catch (Exception ex)
            {
            }
            if(String.IsNullOrEmpty(d))
            {
                bgw = new String[]{};
                return;
            }
            bgw = d.Split('&');
           
        }


        public void MoveUserObject(int oldPos, int newPos)
        {
             Thread t = new Thread(_moveUserObject);
           
           
            
        }
        private void _moveUserObject(object parameters)
        {
           
            int _oldPos = ((int[])parameters)[0];
            int _newPos = ((int[])parameters)[1];
            DataSet ds = MakeDataSet("SELECT * FROM user WHERE identifier = 'drsounds'");
            DataRow _playlist = ds.Tables[0].Rows[0];
            String tracks = (String)_playlist["favorites"];

            List<String> Rows = new List<string>(tracks.Split('&'));
            String c = Rows[_oldPos];
            Rows.RemoveAt(_oldPos);
            Rows.Insert(_newPos, c);

            OleDbConnection conn = MakeConnection();
            conn.Open();
            OleDbCommand command = new OleDbCommand("UPDATE user SET favorites = '" + String.Join("&", Rows.ToArray()) + "' WHERE user.identifier = 'drsounds'", conn);
            command.ExecuteNonQuery();
            conn.Close();
        }

        public void insertUserObject(string uri, int pos)
        {
            Thread t = new Thread(_insertUserObject);
        }
        private void _insertUserObject(object parameters)
        {
            String uri = (String)((Object[])parameters)[0];
            int pos = (int)((Object[])parameters)[1];
            DataSet ds = MakeDataSet("SELECT * FROM user WHERE identifier = 'drsounds'");
            DataRow _playlist = ds.Tables[0].Rows[0];
            String tracks = (String)_playlist["favorites"];

            List<String> Rows = new List<string>(tracks.Split('&'));

            Rows.Insert(pos, uri);

            OleDbConnection conn = MakeConnection();
            conn.Open();
            OleDbCommand command = new OleDbCommand("UPDATE user SET favorites = '" + String.Join("&", Rows.ToArray()) + "' WHERE user.identifier = 'drsounds'", conn);
            command.ExecuteNonQuery();
            conn.Close();
        }
    }
}
