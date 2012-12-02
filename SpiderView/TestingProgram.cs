using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aleros;
namespace Spider
{
    class TestingProgram
    {
        [STAThread]
        static void Main()
        {
            using(StreamReader SR = new StreamReader(@"testcss.css")) 
            {
                Aleros.CSS.Stylesheet stylesheet = new Aleros.CSS.Stylesheet(SR.ReadToEnd());
                SR.Close();
            }
        }
    }
}
