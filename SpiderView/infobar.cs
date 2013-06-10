using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Spider.Skinning;

namespace Spider
{
     public enum NotificationType {
        Info, Warning, Error
    }
      public class NotificationEventArgs
        {
            public String Text;
            public NotificationType Type;

        }
        public delegate void NotificationEventHandler(object sender, NotificationEventArgs e);
        
    public partial class infobar : UserControl
    {
        public infobar()
        {
            InitializeComponent();

        }
        private Style stylesheet;
        public String Title;
        public Block InfoBlock;
        public NotificationType Type = NotificationType.Info;
        BufferedGraphicsContext bgc = new BufferedGraphicsContext();
       
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            BufferedGraphics bg = bgc.Allocate(e.Graphics, e.ClipRectangle);
            Draw(bg.Graphics);
            bg.Render();
        }
        private Timer blinkTimer = new Timer();
        public bool inverse = false;
        public void Draw(Graphics g)
        {
            g.DrawImage(this.InfoBlock.BackgroundImage, new Rectangle(0, 0, (int)((float)this.Width * 3.3f), this.Height));
            stylesheet.DrawString(g, Text, this.InfoBlock.Font, new SolidBrush(InfoBlock.ForeColor), new Rectangle(this.InfoBlock.Padding.Left, this.InfoBlock.Padding.Top, this.Width - this.InfoBlock.Padding.Right * 2, this.Height - this.InfoBlock.Padding.Bottom * 2), true);
        }
        public infobar(Style style)
        {
            stylesheet = style;
            this.InfoBlock = (Block)style.Blocks["infobar::info"].Clone();
            this.Height = this.InfoBlock.Height;
            blinkTimer.Tick += blinkTimer_Tick;
            blinkTimer.Interval = 50;
            
        }
        int count = 0;
        void blinkTimer_Tick(object sender, EventArgs e)
        {
            if (count > 4)
            {
                blinkTimer.Stop();
                count = 0;
                Draw(this.CreateGraphics());
                return;
            }
            if (inverse)
            {
                this.CreateGraphics().FillRectangle(new SolidBrush(Color.FromArgb(127, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));
            }
            else
            {
                Draw(this.CreateGraphics());
            }
            inverse = !inverse;
            count++;
        }
        private void infobar_Load(object sender, EventArgs e)
        {
            this.Paint += infobar_Paint;
        }
        public void ShowMessage(String text, NotificationType type) {
            this.Type = type;
            this.Text = text;
            this.Show();
            blinkTimer.Start();
        }
        void infobar_Paint(object sender, PaintEventArgs e)
        {

            BufferedGraphics bg = bgc.Allocate(e.Graphics, e.ClipRectangle);
            Draw(bg.Graphics);
            bg.Render();
        }
    }
}
