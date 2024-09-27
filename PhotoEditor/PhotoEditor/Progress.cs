using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoEditor
{
    public partial class Progress : Form
    {
        public int imageSize;
        public int pixels;

        private double onePercent;
        private int totalPercent = 0;
        public Progress(Bitmap bitmap)
        {
            InitializeComponent();

            CalculatePercent(bitmap);
        }

        private void CalculatePercent(Bitmap bitmap)
        {
            imageSize = bitmap.Height * bitmap.Width;
            onePercent = imageSize / 100; 
        }

        public void UpdateProgress(int _pixels) // _pixels comes from transformation methods
        {
            pixels = _pixels;
            double currentProgress = pixels / onePercent;
            if (currentProgress > 1 && currentProgress > (double)totalPercent)
            {
                ++totalPercent;
                progressBar1.Value = totalPercent;
            }      
        }
    }
}
