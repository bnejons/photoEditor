﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace PhotoEditor
{
    public partial class EditForm : Form
    {
        public string fileName;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public Progress progress;
        public EditForm(string fileName)
        {
            InitializeComponent();

            this.fileName = fileName;

            pictureBox1.Image = Bitmap.FromFile(fileName);
        }

        private void button1_Click(object sender, EventArgs e) // Invert Colors
        {
            cancellationTokenSource = new CancellationTokenSource();

            Transform(sender);
        }


        private async Task InvertColorsAsync(Bitmap transformedBitmap, Progress progress)
        {
            double onePercent = (transformedBitmap.Height * transformedBitmap.Width) / 100;
            var token = cancellationTokenSource.Token;
            await Task.Run(() =>
            {
                for (int y = 0; y < transformedBitmap.Height; y++)
                {
                    for (int x = 0; x < transformedBitmap.Width; x++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        int currentPixel = y * x;

                        var color = transformedBitmap.GetPixel(x, y);
                        int newRed = Math.Abs(color.R - 255);
                        int newGreen = Math.Abs(color.G - 255);
                        int newBlue = Math.Abs(color.B - 255);
                        Color newColor = Color.FromArgb(newRed, newGreen, newBlue);
                        transformedBitmap.SetPixel(x, y, newColor);

                        if (currentPixel % onePercent == 0)
                        {
                            try
                            {
                                Invoke(() =>
                                {
                                    progress.UpdateProgress(currentPixel);
                                });
                            }
                            catch (ObjectDisposedException)
                            {
                                
                            }
                        }
                    }
                }
            }, token);
            
        }

        private void button2_Click(object sender, EventArgs e) // Change Colors
        {
            cancellationTokenSource = new CancellationTokenSource();
            Transform(sender);
        }

        private async Task ChangeColorsAsync(Bitmap transformedBitmap, Color transformColor, Progress progress) 
        {
            double onePercent = (transformedBitmap.Height * transformedBitmap.Width) / 100;
            var token = cancellationTokenSource.Token;

            await Task.Run(() =>
            {
                for (int y = 0; y < transformedBitmap.Height; y++)
                {
                    for (int x = 0; x < transformedBitmap.Width; x++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        int currentPixel = y * x;

                        var color = transformedBitmap.GetPixel(x, y);
                        var newColor = BlendColor(color, transformColor);
                        transformedBitmap.SetPixel(x, y, newColor);

                        if (currentPixel % onePercent == 0)
                        {
                            try
                            {
                                Invoke(() =>
                                {
                                    progress.UpdateProgress(currentPixel);
                                });
                            }
                            catch (ObjectDisposedException)
                            {

                            }
                        }
                    }
                }
            });
            
        }

        // I got following method from Timwi 
        // https://stackoverflow.com/questions/3722307/is-there-an-easy-way-to-blend-two-system-drawing-color-values
        // I tweaked it to match how it should work, but still does not look like the McCown example
        private Color BlendColor(Color color, Color backColor)
        {
            byte r = (byte)((color.R + backColor.R) / 2);
            byte g = (byte)((color.G + backColor.G) / 2);
            byte b = (byte)((color.B + backColor.B) / 2);
            return Color.FromArgb(r, g, b);
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();
            Transform(sender);
        }

        private async Task ChangeBrightnessAsync(Bitmap transformedBitmap, Progress progress)
        {
            double onePercent = (transformedBitmap.Height * transformedBitmap.Width) / 100;
            double amount = trackBar1.Value;
            bool blackOrWhite = true; // White
            var token = cancellationTokenSource.Token;

            if (amount < 50)
            {
                blackOrWhite = false;
            }

            await Task.Run(() =>
            {
                for (int y = 0; y < transformedBitmap.Height; y++)
                {
                    for (int x = 0; x < transformedBitmap.Width; x++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        int currentPixel = y * x;

                        var color = transformedBitmap.GetPixel(x, y);

                        Color newColor = BlendBright(blackOrWhite, color, amount);

                        transformedBitmap.SetPixel(x, y, newColor);

                        if (currentPixel % onePercent == 0)
                        {
                            try
                            {
                                Invoke(() =>
                                {
                                    progress.UpdateProgress(currentPixel);
                                });
                            }
                            catch (ObjectDisposedException)
                            {

                            }
                        }
                    }
                }
            });
            
            
        }

        private Color BlendBright(bool blackOrWhite, Color color, double amount)
        {
            if (blackOrWhite) // If brightening image
            {
                int r = (int)(color.R + (255 * (((amount - 50) * 2) / 100)));
                if (r > 255) { r = 255; }
                int b = (int)(color.B + (255 * (((amount - 50) * 2) / 100)));
                if (b > 255) { b = 255; }
                int g = (int)(color.G + (255 * (((amount - 50) * 2) / 100)));
                if (g > 255) { g = 255; }
                return Color.FromArgb(r, g, b);
            }
            else // If darkening image
            {
                int r = (int)(color.R - (255 * (((50 - amount) * 2) / 100)));
                if (r < 0) { r = 0; }
                int b = (int)(color.B - (255 * (((50 - amount) * 2) / 100)));
                if (b < 0) { b = 0; }
                int g = (int)(color.G - (255 * (((50 - amount) * 2) / 100)));
                if (g < 0) { g = 0; }
                return Color.FromArgb(r, g, b);
            }
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
            // Register event handler for form closed? if cancel, return
            var transformedBitmap = new Bitmap(pictureBox1.Image);
            progress = new Progress(transformedBitmap);
            progress.Cancel += ProgressForm_Close;

            ToggleControls();

            if (sender == button1) // Invert
            {
                // I learned how to have the start position at the parent of a modeless from Rotem in this link
                // https://stackoverflow.com/questions/8566582/how-to-centerparent-a-non-modal-form
                progress.StartPosition = FormStartPosition.Manual;
                progress.Location = new Point(this.Location.X + (this.Width - progress.Width) / 2, this.Location.Y + (this.Height - progress.Height) / 2);
                progress.Show(this);
                await InvertColorsAsync(transformedBitmap, progress);
                
            }
            
            else if (sender == button2) // Change Color
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    progress.StartPosition = FormStartPosition.Manual;
                    progress.Location = new Point(this.Location.X + (this.Width - progress.Width) / 2, this.Location.Y + (this.Height - progress.Height) / 2);
                    progress.Show(this);
                    await ChangeColorsAsync(transformedBitmap, colorDialog1.Color, progress);
                }
            }

            else if (sender == trackBar1)
            {
                progress.StartPosition = FormStartPosition.Manual;
                progress.Location = new Point(this.Location.X + (this.Width - progress.Width) / 2, this.Location.Y + (this.Height - progress.Height) / 2);
                progress.Show(this);
                await ChangeBrightnessAsync(transformedBitmap, progress);
            }

            progress.Close();
            ToggleControls();

            if (!cancellationTokenSource.IsCancellationRequested)
            {
                pictureBox1.Image = transformedBitmap;
            }
            else
            {
                trackBar1.Value = 50;
            }
        }


        private void EditForm_Load(object sender, EventArgs e)
        {
            // Ignore
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Ignore
        }

        private void EditForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            cancellationTokenSource.Cancel();

        }

        private void ProgressForm_Close()
        {
            cancellationTokenSource.Cancel();
        }

        private void button3_Click(object sender, EventArgs e) // Close form
        {
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image.Save("myphoto.jpg", ImageFormat.Jpeg);
                string currentDirectory = Environment.CurrentDirectory;
                MessageBox.Show("Photo was saved at " + currentDirectory, "Saved Photo Location", MessageBoxButtons.OK, MessageBoxIcon.Information); ;
            }
           catch (Exception)
            {
                MessageBox.Show("Error occured when saving", "hol up bro");
            }
        }
    }
}
