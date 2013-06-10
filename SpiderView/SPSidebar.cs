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
    public partial class SPSidebar : UserControl
    {
        public SPListView ListView { get; set; }
        public SPSidebar()
        {
            InitializeComponent();
            
        }
        public SPSidebar(SpiderHost host, Skinning.Style style) {
            InitializeComponent();
            this.ListView = new SPListView(style, host);
        }
                
        private void Siderbar_Load(object sender, EventArgs e)
        {

        }
    }
}
