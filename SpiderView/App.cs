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
    /// <summary>
    /// An application inside the Spider Markup Domain
    /// </summary>
    public partial class App : UserControl
    {
        public SpiderHost Host { get; set; }
        public App(SpiderHost host)
        {
            
            InitializeComponent();
            this.Host = host;
            this.spiderView = new SpiderView();
            //this.board =  new Board(this);
            this.Controls.Add(spiderView);
            this.spiderView.Dock = DockStyle.Fill;
            spiderView.Dock = DockStyle.Fill;
            spiderView.Navigate += spiderView_Navigate;
        }

        void spiderView_Navigate(object sender, SpiderView.NavigateEventArgs e)
        {
            Host.Navigate(e.Uri.ToString());
        }
        public App()
        {
            InitializeComponent();
            

        }
        /// <summary>
        /// Loads the app
        /// </summary>
        /// <param name="arguments"></param>
        public void Navigate(String[] arguments)
        {
            
        }
        /// <summary>
        /// Gets the spider view
        /// </summary>
        public SpiderView Spider
        {
            get
            {
                return this.spiderView;
            }
        }
        private SpiderView spiderView;
        private void App_Load(object sender, EventArgs e)
        {
           
        }
    }
}
