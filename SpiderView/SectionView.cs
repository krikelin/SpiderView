using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Spider.Media;

namespace Spider
{
    public class Overflow : Panel
    {
        public CListView listView;
        public Overflow(CListView listView)
        {
            this.listView = listView;
            this.Paint += Overflow_Paint;
        }

        void Overflow_Paint(object sender, PaintEventArgs e)
        {
            Rectangle bounds = new Rectangle(0, 0, listView.Width, 16);
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            listView.DrawToBitmap(bitmap, bounds);
            e.Graphics.DrawImage(bitmap, new Point(0, 0));
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle bounds = new Rectangle(0, 0, listView.Width, 16);
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            listView.DrawToBitmap(bitmap, bounds);
            e.Graphics.DrawImage(bitmap, new Point(0, 0));
        }
    }
    public partial class SectionView : UserControl
    {
        public Board Board;
        public SpiderView SpiderView;
        public bool IsPlaylist { get; set; }
        public CListView ListView { get; set; }

        public List<Track> Tracks
        {
            get
            {
                List<Track> playlist = new List<Track>();
                foreach (Element e in this.Board.Children)
                {
                    if (e.GetType() == typeof(track))
                    {
                        playlist.Add((e as track).Track);
                    }
                    foreach (track track in e.Tracks)
                    {
                        playlist.Add(track.Track);
                    }
                }

                return playlist;
            }
        }

        public List<Track> GetQueue(Track startingTrack)
        {
           
            
            List<Track> playlist = new List<Track>();
            int start = Tracks.IndexOf(startingTrack);
            if(start < 0)
                start = 0;
            foreach (Track track in Tracks.GetRange(start, Tracks.Count - start))
            {
                playlist.Add(track);
            }
            if(this.ListView != null)
            foreach (CListView.CListViewItem item in this.ListView.Items)
            {
                playlist.Add(item.Track);
            }
            return playlist;
            
        }

        public void SelectNext()
        {
            track lastTrack = null;

            for (int i = 0; i < Tracks.Count; i++)
            {
                track track = this.TrackElements[i];
                if (lastTrack != null)
                {
                    track.Selected = true;

                    foreach (track t in this.TrackElements)
                    {
                        if (t != track)
                        {
                            t.Selected = false;
                        }
                    }
                    if (track.Y + track.Height < this.VerticalScroll.Value)
                    {
                        this.VerticalScroll.Value = track.Y;
                    }
                    if (track.Y + track.Height > this.VerticalScroll.Value + this.Height)
                    {
                        this.VerticalScroll.Value = track.Y - track.Height;
                    }
                    return;
                }
                if (track.Selected)
                {
                    lastTrack = track;

                }

            }
        }
        public void SelectPrev()
        {
            track lastTrack = null;

            for (int i = Tracks.Count - 1; i >= 0; i--)
            {
                track track = this.TrackElements[i];
                if (lastTrack != null)
                {
                    track.Selected = true;
                    foreach (track t in this.TrackElements)
                    {
                        if (t != track)
                        {
                            t.Selected = false;
                        }
                    }
                    return;
                }
                if (track.Selected)
                {
                    lastTrack = track;

                }

            }
        }
        /// <summary>
        /// Tracklist
        /// </summary>
        public List<track> TrackElements
        {
            get
            {
                List<track> playlist = new List<track>();
                foreach (Element e in this.Board.Children)
                {
                    if (e.GetType() == typeof(track))
                    {
                        playlist.Add((e as track));
                    }
                    foreach (track track in e.Tracks)
                    {
                        playlist.Add(track);
                    }
                }
                return playlist;
            }
        }
        public List<track> SelectedTrackElements
        {
            get
            {
                List<track> playlist = new List<track>();
                foreach (track e in this.TrackElements)
                {
                    if (e.Selected)
                        playlist.Add(e);
                }

                return playlist;
            }
        }
        

        public SectionView()
        {
            InitializeComponent();
        }
        protected override void OnResize(EventArgs e)
        {
            if(this.Board != null)
            this.Board.Width = this.Width;
        }
        protected override Point ScrollToControl(Control activeControl)
        {
            return this.AutoScrollPosition;
        }
        public SectionView(Board board, SpiderView spiderView)
        {
            InitializeComponent();
            this.Board = board;
            this.Controls.Add(board);
            board.AutoResize();
            this.AutoScroll = true;
            board.AutoResize();
            this.SpiderView = spiderView;
            if (overflow == null)
            {
                overflow = new Overflow(ListView);
                overflow.Top = -1424;

                this.Controls.Add(overflow);

            }
            this.Scroll +=SectionView_Scroll;
            System.Windows.Forms.ScrollBar scrollBar = new System.Windows.Forms.VScrollBar();
         /*   scrollBar.Dock = DockStyle.Right;
            scrollBar.Scroll += scrollBar_Scroll;
            scrollBar.Maximum = this.Board.Height;
            this.Controls.Add(scrollBar);
            scrollBar.BringToFront();*/
            
        }

        void scrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if(this.ListView != null)
            this.ListView.Top = - e.NewValue;
            this.Board.Top =- e.NewValue;
            int x = 0, y = 0;
            this.Board.Draw(this.Board.CreateGraphics(), ref x, ref y, new Rectangle(0, 0, Board.Width, Board.Height), false);
        }
        public Overflow overflow;
        void SectionView_Scroll(object sender, ScrollEventArgs e)
        {
          this.Invalidate();
                        if (this.VerticalScroll.Value > this.Board.Height)
                        {
                            overflow.listView = ListView;
                            overflow.Show();
                
                            overflow.Left = 0;
                            overflow.Top =0;
                            overflow.Width = this.Width;
                            overflow.Height = 18;
                            overflow.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                            overflow.BringToFront();
                        }
                        else
                        {
                            if (overflow != null)
                                overflow.Hide();
                        }
        }
        private void SectionView_Load(object sender, EventArgs e)
        {

        }
    }
}
