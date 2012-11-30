using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Spider
{
    [Serializable]
    [XmlRoot("skin")]
    public class Style
    {
        public Style()
        {
            ForeColor = Color.FromArgb(255, 211, 211, 211);
            AlternateColor = Color.Gray;
            BackColor = Color.FromArgb(255, 55, 55, 55);
            EntryColor = Color.FromArgb(255, 88, 88, 88);
            Font = new Font("MS Sans Serif", 11);
            Skin = Properties.Resources.skin;

            // Partialize skin
        }
        private Bitmap skin;
        public Bitmap Skin {
            get{
                return this.skin;
            }set{
                this.skin = value;
                Slice(skin);
            }
        }
        public void Slice(Bitmap bitmap)
        { 
            // Get tab bar
            ForeColor = bitmap.GetPixel(1, 0);
            BackColor = bitmap.GetPixel(0, 0);
            AlternateColor = bitmap.GetPixel(3, 0);
            EntryColor = bitmap.GetPixel(4, 0);
            SelectedBackColor = bitmap.GetPixel(6, 0);
            ListBackgroundColor = bitmap.GetPixel(7, 0);
            Parts.Add("TabBar", sliceBitmap(bitmap, new Rectangle(48, 1, 2, 28)));
            Parts.Add("TabBarActive", sliceBitmap(bitmap, new Rectangle(0, 1, 42, 28)));
            Parts.Add("Divider", sliceBitmap(bitmap, new Rectangle(0, 29, 50, 23)));
        }
        public Bitmap sliceBitmap(Bitmap src, Rectangle region)
        {
            Bitmap target = new Bitmap(region.Width, region.Height);
            Graphics g = Graphics.FromImage(src);
            Graphics targetGraphics = Graphics.FromImage(target);
            targetGraphics.DrawImage(src, 0, 0, region, GraphicsUnit.Pixel);
            return target;
        }

        [XmlElement("forecolor")]
        public Color ForeColor { get; set; }
        public Color EntryColor { get; set; }
        public Color BackColor { get; set; }
        public Color AlternateColor { get; set; }
        public Font Font { get; set; }
        
        public Dictionary<String, Image> Parts = new Dictionary<string,Image>();

        public Color SelectedBackColor { get; set; }

        public Color SelectedForeColor { get; set; }

        public Color ListBackgroundColor { get; set; }
    }
}
