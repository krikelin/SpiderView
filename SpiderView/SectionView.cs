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

namespace Spider
{
    public class Overflow : Panel
    {
        public CListView listView;
        public Overflow(CListView listView)
        {
            this.listView = listView;
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (listView == null)
                return;
            BufferedGraphicsContext bgc = new BufferedGraphicsContext();
            BufferedGraphics g = bgc.Allocate(e.Graphics, new Rectangle(0, 0, this.Width, this.Height));
            Bitmap bitmap = new Bitmap(this.Width, 20);
            
            listView.DrawToBitmap(bitmap, new Rectangle(0, 0, this.Width, 18));
            g.Graphics.DrawImage(bitmap, new Point(0, 0));
            g.Render();
        }
    }
    public partial class SectionView : UserControl
    {
        public Board Board;
        public SpiderView SpiderView;
        public bool IsPlaylist { get; set; }
        public CListView ListView { get; set; }
        public SectionView()
        {
            InitializeComponent();
        }
        protected override void OnResize(EventArgs e)
        {
            if(this.Board != null)
            this.Board.Width = this.Width;
        }
        public SectionView(Board board, SpiderView spiderView)
        {
            InitializeComponent();
            this.Board = board;
            this.Controls.Add(board);
            board.AutoResize();
            this.Scroll += SectionView_Scroll;
            board.AutoResize();
            this.AutoScroll = true;
            this.SpiderView = spiderView;
            if (overflow == null)
            {
                overflow = new Overflow(ListView);
                overflow.Top = -1424;

                this.Controls.Add(overflow);

            }
        }
        public Overflow overflow;
        void SectionView_Scroll(object sender, ScrollEventArgs e)
        {
            this.Invalidate();
          /*  if (this.VerticalScroll.Value > this.Board.Height)
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
            }*/
        }
        private void SectionView_Load(object sender, EventArgs e)
        {

        }
    }
}
