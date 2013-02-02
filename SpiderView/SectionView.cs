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
           
        }

        void SectionView_Scroll(object sender, ScrollEventArgs e)
        {
            this.Invalidate();
        }
        private void SectionView_Load(object sender, EventArgs e)
        {

        }
    }
}
