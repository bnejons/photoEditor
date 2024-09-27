using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoEditor
{
    public partial class EditForm : Form
    {
        string fileName;
        public EditForm(string fileName)
        {
            InitializeComponent();

            this.fileName = fileName;

            pictureBox1.Image = Bitmap.FromFile(fileName);
        }

        private void button1_Click(object sender, EventArgs e) // Invert Colors
        {
            Transform(sender);
        }


        private async Task<int> InvertColorsAsync(Bitmap transformedBitmap, Progress progress)
        {
            double onePercent = (transformedBitmap.Height * transformedBitmap.Width) / 100;
            await Task.Run(() =>
            {
                for (int y = 0; y < transformedBitmap.Height; y++)
                {
                    for (int x = 0; x < transformedBitmap.Width; x++)
                    {
                        int currentPixel = y * x;

                        var color = transformedBitmap.GetPixel(x, y);
                        int newRed = Math.Abs(color.R - 255);
                        int newGreen = Math.Abs(color.G - 255);
                        int newBlue = Math.Abs(color.B - 255);
                        Color newColor = Color.FromArgb(newRed, newGreen, newBlue);
                        transformedBitmap.SetPixel(x, y, newColor);

                        if (currentPixel % onePercent == 0)
                        {
                            Invoke(() => // Takes too long??
                            {
                                progress.UpdateProgress(currentPixel);
                            });
                        }
                    }
                }
            });
            return 3;
        }

        private void button2_Click(object sender, EventArgs e) // Change Colors
        {

            Transform(sender);
        }

        private async Task<int> ChangeColorsAsync(Bitmap transformedBitmap, Color transformColor, Progress progress) 
        {
            double onePercent = (transformedBitmap.Height * transformedBitmap.Width) / 100;
            await Task.Run(() =>
            {
                for (int y = 0; y < transformedBitmap.Height; y++)
                {
                    for (int x = 0; x < transformedBitmap.Width; x++)
                    {
                        int currentPixel = y * x;

                        var color = transformedBitmap.GetPixel(x, y);
                        var newColor = Blend(color, transformColor);
                        transformedBitmap.SetPixel(x, y, newColor);

                        if (currentPixel % onePercent == 0)
                        {
                            Invoke(() => // Takes too long??
                            {
                                progress.UpdateProgress(currentPixel);
                            });
                        }
                    }
                }
            });
            return 3;
        }

        // I got following method from Timwi 
        // https://stackoverflow.com/questions/3722307/is-there-an-easy-way-to-blend-two-system-drawing-color-values
        // I tweaked it to match how it should work, but still does not look like the McCown example
        public Color Blend(Color color, Color backColor)
        {
            byte r = (byte)((color.R + backColor.R) / 2);
            byte g = (byte)((color.G + backColor.G) / 2);
            byte b = (byte)((color.B + backColor.B) / 2);
            return Color.FromArgb(r, g, b);
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            // Change brightness based on value
        }


        // Got the following method from Ian at
        // https://stackoverflow.com/questions/16062934/disable-complete-form-while-other-form-is-shown
        public void ToggleControls()
        {
            foreach (Control c in this.Controls)
            {
                c.Enabled = !c.Enabled;
            }
        }

        private async void Transform(object sender) // Performs all transformations
        {
            // Register event handler for form closed?
            var transformedBitmap = new Bitmap(pictureBox1.Image);
            Progress progress = new Progress(transformedBitmap);

            ToggleControls();

            if (sender == button1)
            {
                progress.Show();
                await InvertColorsAsync(transformedBitmap, progress);
                
            }
            
            else if (sender == button2)
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    progress.Show();
                    await ChangeColorsAsync(transformedBitmap, colorDialog1.Color, progress);
                }
            }

            // else if brightness method
            progress.Close();
            ToggleControls();
            // If not canceled, picture box = transformed 
            pictureBox1.Image = transformedBitmap;
        }
        private void EditForm_Load(object sender, EventArgs e)
        {
            // Ignore
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Ignore
        }
    }
}
