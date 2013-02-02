using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Spider.Skinning;
namespace Spider
{
    [Serializable]
    public partial class SPListView : UserControl
    {
        public SpiderHost Host { get; set; }
        public Block Block { get; set; }
        public Block SelectedBlock { get; set; }
      
        public Style stylesheet;
        public SPListView(Spider.Skinning.Style stylesheet, SpiderHost host)
        {
            this.Host = host;
            InitializeComponent();
            this.Items = new List<SPListItem>();

            this.stylesheet = stylesheet;
            this.SelectedBlock = stylesheet.Blocks["::selection"];
            this.Block = stylesheet.Blocks["ListView"];
        }
        public int GetIndexByName(String name)
        {
            int i = 0;
            foreach (SPListItem item in Items)
            {
                if (item.Text == name) return i;
                i++;
            }
            return -1;
        }
        private int scrollY = 0;
        /// <summary>
        /// Event args
        /// </summary>
        public class SPListItemEventArgs
        {
            public SPListItem Item { get; set; }
        }
        public delegate void SPListItemMouseEventHandler(object Sender, SPListItemEventArgs e);
        public event SPListItemMouseEventHandler ItemSelected;


        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            Draw(e.Graphics);
        }
     
        /// <summary>
        /// Draw item and sub-Items
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="pos"></param>
        /// <param name="level"></param>
        private void drawItem(Graphics g, SPListItem Item, ref int pos, ref int level)
        {
          
                Item.Draw(g, ref level, ref pos);

               // pos += 16;
                // If has subitems draw them
#if(False)
                if (Item.Expanded)
                    foreach (SPListItem subItem in Item.Children)
                    {
                        level += 16;
                        drawItem(g, subItem, ref pos, ref level);
                        level -= 16;

                        if (Item.Selected)
                        {

                            // g.DrawImage(Properties.Resources.menu_selection, 0, pos, this.Width * 500, Properties.Resources.menu_selection.Height);
                            g.FillRectangle(new SolidBrush(SelectedStyle.BackColor), new Rectangle(0, pos, this.Width * 500, 16));

                            foreColor = SelectedStyle.ForeColor;
                        }
                        else if (Item.Touched)
                        {
                            g.FillRectangle(new SolidBrush(Color.Gray), 0, pos, this.Width, ItemHeight);
                        }
                        else
                        {
                            g.DrawString(Item.Text, new Font("MS Sans Serif", 8), new SolidBrush(Color.FromArgb(10, 10, 10)), new Point(level + 32, pos + 3));
                        }
                        g.DrawString(Item.Text, new Font("MS Sans Serif", 8), new SolidBrush(foreColor), new Point(level + 32, pos + 2));
                        if (Item.Icon != null)
                        {
                            g.DrawImage(Item.Icon, level + 16, pos + 1, 16, 16);
                        }
                        // If has subItems create expander
                        if (Item.Children.Count > 0)
                        {
                            // Image expander = Item.Expanded ? Properties.Resources.ic_expander_open : Properties.Resources.ic_expander_closed;
                            // g.DrawImage(expander, level, pos, 16, 16);
                        }
                    }
#endif
                pos += ItemHeight;

                // If has subitems draw them
                if (Item.Expanded)
                    foreach (SPListItem subItem in Item.Children)
                    {
                        level += 16;
                        drawItem(g, subItem, ref pos, ref level);
                        level -= 16;
                    }
            

        }
        public int ListHeight
        { get; set; }
        public int ScrollOverflow
        {
            get
            {
                int height = ListHeight - this.Height;
                return height > 0 ? height : 0;
            }
        }
        float scrollScale = 0;
        private float myScroll = 0;
        public void Draw(Graphics gr)
        {

            BufferedGraphicsContext c = new BufferedGraphicsContext();
            BufferedGraphics bg = c.Allocate(gr, new Rectangle(0, 0, this.Width, this.Height));
            Graphics g = bg.Graphics;
            g.FillRectangle(new SolidBrush(Block.BackColor), 0, 0, this.Width, this.Height);
            int pos = -ScrollY;
            int level = 0;           
            if (Items != null)
                foreach (SPListItem Item in Items)
                {
                    drawItem(g, Item, ref pos, ref level);

                }
            ListHeight = pos + scrollY + 128;
#if(scrollbar)
            g.DrawLine(new Pen(Color.Black), new Point(this.Width  -1, 0), new Point(this.Width -1, this.Height));

            // Paint scrollbar
            g.DrawImage(Properties.Resources.scrollbar_space, new Rectangle(this.Width - Properties.Resources.scrollbar_top.Width, 0 ,15, this.Height*30)); 
            g.DrawImage(Properties.Resources.scrollbar_top, new Rectangle(this.Width - Properties.Resources.scrollbar_top.Width, 0, 15, 22));
            g.DrawImage(Properties.Resources.scrollbar_bottom, new Rectangle(this.Width - Properties.Resources.scrollbar_bottom.Width, this.Height-21, 15, 22));

            // Draw thumb
            int thumbHeight = (this.Height -(Properties.Resources.scrollbar_top.Height + Properties.Resources.scrollbar_bottom.Height)) * (this.ScrollOverflow / this.Height) + 8;
            int thumbPosition = 
            // Draw thumb over
            g.DrawImage(Properties.Resources.scrollbar_thumb,new Rectangle(this.Width - Properties.Resources.scrollbar_thumb.Width,thumbHeight+
#else
            scrollScale = (float)(((float)this.ListHeight + 1) / ((float)this.Height + 1));
            float f = (float)(((float)this.Height + 1) / ((float)this.ListHeight + 1));
            if (f < 1)
                g.FillRectangle(new SolidBrush(Color.FromArgb(127, 255, 255, 255)), this.Width - 8, (this.ScrollY / scrollScale), 7, (int)(f * (float)this.Height));
#endif
            g.DrawLine(new Pen(Color.Black), new Point(this.Width - 1, 0), new Point(this.Width - 1, this.Height));
            bg.Render();





        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int pos = -scrollY;
            // Draw all list items

        }
        public int ItemHeight = 18;
        public SPListView()
        {

            InitializeComponent();
            this.Items = new List<SPListItem>();
            this.BackColor = Block.BackColor;
            this.ForeColor = Block.ForeColor;

        }
        public SPListItem GetAppByUri(Uri uri)
        {
            foreach (SPListItem item in this.Items)
            {
                if (item.Uri.ToString() == (uri.ToString()))
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Check if the uri is in the list
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public bool HasURI(Uri uri)
        {
            bool positive = false;
            foreach (SPListItem item in this.Items)
            {
                if (item.Uri.ToString().Contains(uri.ToString()))
                {
                    positive = true;
                }
                else
                {
                    item.HasUri(uri, ref positive);
                }
            }
            return positive;
        }
        public SPListItem AddItem(Uri uri)
        {
            SPListItem c = new SPListItem(this, uri.ToString());
           

            this.Items.Add(c);
            return c;
        }
        public SPListItem AddItem(String text, Uri uri)
        {
            SPListItem c = new SPListItem(this, uri.ToString(), text);
            c.Text = text;
            c.Uri = uri;
            this.Items.Add(c);
            this.Refresh();
            return c;
        }
        public SPListItem AddItem(String text, Uri uri, Spider.SPListItem.ListIcon icon)
        {
            SPListItem c = new SPListItem(this);
            c.Text = text;
            c.Uri = uri;
            c.Icon = icon;
            this.Items.Add(c);
            this.Refresh();
            return c;
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.stylesheet != null)
                this.Draw(CreateGraphics());
        }
        public List<SPListItem> Items { get; set; }
        private void ucMenu_Load(object sender, EventArgs e)
        {

        }

        private void SPListView_MouseMove(object sender, MouseEventArgs e)
        {
            /*    float f = (float)(((float)this.ListHeight + 1) / (float)this.Height + 1) ;
                this.ScrollY = (int)(f * e.Y);*/
            if (this.scrollScale > 0)
            {
                this.Draw(CreateGraphics());
            }
        }
        private void deselectItem(SPListItem item)
        {
            item.Selected = false;
            foreach (SPListItem subItem in item.Children)
            {
                deselectItem(subItem);
            }
        }
        private void checkItem(SPListItem Item, MouseEventArgs e, ref int level, ref int pos)
        {

            if (e.Y > pos && e.Y < pos + ItemHeight)
            {




                Item.Selected = true;
                SPListItemEventArgs args = new SPListItemEventArgs();
                args.Item = Item;
                this.ItemSelected(this, args);
            }
            pos += ItemHeight;
            // If has subitems draw them
            if (Item.Expanded)
                foreach (SPListItem subItem in Item.Children)
                {
                    level += 16;
                    checkItem(subItem, e, ref level, ref pos);
                    level -= 16;
                }
        }
        private void SPListView_MouseDown(object sender, MouseEventArgs e)
        {

            int pos = -ScrollY;
            int level = 0;
            // Draw all list items
            if (e.X > this.Width - 122)
            {
                myScroll = scrollScale * e.Y;
                return;
            }
            foreach (SPListItem Item in Items)
            {
                deselectTouchItem(Item);
            }
            if (Items != null)

                foreach (SPListItem Item in Items)
                {

                    touchItem(Item, e, ref level, ref pos);

                }


            this.Draw(this.CreateGraphics());
        }
        private bool expanding = false;
        private void touchItem(SPListItem Item, MouseEventArgs e, ref int level, ref int pos)
        {
            if (e.Y > pos && e.Y < pos + ItemHeight)
            {

                // If clicked on expander
                if (e.X < level + 17 && Item.Children.Count > 0)
                {
                    Item.Expanded = !Item.Expanded;
                    pos += ItemHeight;
                    expanding = true;
                    return;
                }



                Item.Touched = true;
            }
            pos += ItemHeight;
            // If has subitems draw them
            if (Item.Expanded)
                foreach (SPListItem subItem in Item.Children)
                {
                    level += 16;
                    touchItem(subItem, e, ref level, ref pos);
                    level -= 16;
                }
        }

        private void deselectTouchItem(SPListItem item)
        {
            item.Touched = false;
            foreach (SPListItem subItem in item.Children)
            {
                deselectTouchItem(subItem);
            }
        }

        private void SPListView_MouseUp(object sender, MouseEventArgs e)
        {
            myScroll = 0;
            try
            {
                int pos = -scrollY;
                int level = 0;
                // Draw all list items
                if (!expanding)
                {
                    foreach (SPListItem Item in Items)
                    {
                        deselectItem(Item);
                    }
                    if (Items != null)

                        foreach (SPListItem Item in Items)
                        {

                            checkItem(Item, e, ref level, ref pos);

                        }
                }
                expanding = false;

            }
            catch (Exception ex)
            {
            }
            this.Draw(this.CreateGraphics());
        }

        public int ScrollY { get; set; }
    }

}
