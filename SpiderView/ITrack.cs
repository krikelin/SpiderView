using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BungaSpotify09.Models
{
    public class Artist
    {
        public Uri Uri { get; set; }
        public String Name { get; set; }
    }
    public class Album
    {
        public Uri Uri { get; set; }
        public String Name { get; set; }
    }
    public class SpotifyTrack : Track
    {
        private Uri uri;
        public SpotifyTrack(Uri uri)
        {
            this.uri = uri;
            this.Name = "Test track #" + uri.ToString().Split(':')[2];
            this.artists = new Artist[] { new Artist() { Name = "Test", Uri = new Uri("spotify:artist:test") } };
            this.Album = new Album() { Name = "Test", Uri = new Uri("spotify:album:test") };
            
        }

        public override void Play()
        {
             
        }

        public override void Seek(int pos)
        {
             
        }

        public override void Stop()
        {
            
        }

        public override bool Available
        {
            get
            {
                return true;
            }
        }
    }
    public abstract class Track
    {
        public abstract void Play();
        public abstract void Seek(int pos);
        public abstract void Stop();
        public abstract bool Available { get; }
        public String Name { get; set; }
        public String Version { get; set; }
        public Artist[] artists { get; set; }
        public Album Album { get; set; }

        public TimeSpan Position { get; set; }
        public TimeSpan Duration { get; set; }
        public delegate void MetadataLoadEventHandler(object sender);
        public event MetadataLoadEventHandler ArtistLoaded;
        public event MetadataLoadEventHandler AlbumLoaded;
        public event MetadataLoadEventHandler Loaded;

        
    }
}
