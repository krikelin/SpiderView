using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spider
{
    public partial class SPCoverBar : UserControl
    {
        public SPCoverBar()
        {
            InitializeComponent();
        }
        public SPCoverBar(SpiderHost host, Skinning.Style style)
        {
            this.Host = host;
            this.Style = style;

        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
                
        }
        private void SPCoverBar_Load(object sender, EventArgs e)
        {

        }

        public SpiderHost Host { get; set; }

        public Skinning.Style Style { get; set; }
    }
}
