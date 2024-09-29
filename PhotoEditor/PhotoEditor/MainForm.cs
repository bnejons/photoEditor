using Microsoft.VisualBasic.ApplicationServices;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PhotoEditor
{
    // Need to implement requirement 3 (menu bar - file explorer and change root folder, also about)
    public partial class MainForm : Form
    {
        private string photoRootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public MainForm()
        {
            InitializeComponent();

            ListDirectory(photoRootDirectory);

            listView1.View = View.Details;
            imageList1.ImageSize = new Size(75, 75);
            listView1.SmallImageList = imageList1;

        }
        
        /*----------------------------------------------------------------------------------------
                 Got both methods below from 
        https://stackoverflow.com/questions/6239544/populate-treeview-with-file-system-directory-structure
                 -Alex Aza June 4, 2011
                 I did modify them to help create tags and add images to imageList1 in following method*/

        private void ListDirectory(string path)
        {
            treeView1.Nodes.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView1.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo));
            treeView1.Nodes[0].Tag = path;
            AddImages(rootDirectoryInfo);
        }

        private TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNode(directoryInfo.Name);

            try
            {
                foreach (var directory in directoryInfo.GetDirectories()) //recursive, tags each Node with directory name, adds to directoryNode
                {
                    TreeNode node = CreateDirectoryNode(directory);
                    node.Tag = directory.FullName;
                    directoryNode.Nodes.Add(node);
                }
            }
            catch (UnauthorizedAccessException)
            {

            }

            return directoryNode;
        }

        private void AddImages(DirectoryInfo directoryInfo)
        {
            try
            {
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    AddImages(directory);
                }
            }
            catch (UnauthorizedAccessException)
            {

            }

            try
            {
                foreach (var file in directoryInfo.GetFiles()) //adds each photo to imageList
                {
                    if (isImageFile(file))
                    {
                        //uses file.FullName as a key to access later in listView

                        byte[] bytes = System.IO.File.ReadAllBytes(file.FullName);
                        MemoryStream ms = new MemoryStream(bytes);
                        try
                        {
                            Image img = Image.FromStream(ms);
                            //I used this link to learn getting thumbnail images
                            //https://learn.microsoft.com/en-us/dotnet/api/system.drawing.image.getthumbnailimage?view=net-8.0
                            Image.GetThumbnailImageAbort callback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
                            imageList1.Images.Add(file.FullName, img.GetThumbnailImage(1000, 1000, callback, IntPtr.Zero));
                        }
                        catch (ArgumentException)
                        {

                        }

                    }
                }
            }
            catch (UnauthorizedAccessException)
            {

            }
        }

        public bool ThumbnailCallback() // Only used for getting thumbnail image, not important
        {
            return false;
        }

        /*----------------------------------------------------------------------------------------------*/
        //I used 
        //https://learn.microsoft.com/en-us/dotnet/api/system.io.filesysteminfo.extension?view=net-8.0#system-io-filesysteminfo-extension
        //to implement the extension check in isImageFile

        private bool isImageFile(FileInfo file) //tests if file is a .jpg file
        {
            if (file.Extension == ".jpg")
            {
                return true;
            }
            return false;
        }

        private async void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string path = treeView1.SelectedNode.Tag.ToString();

            var currentDirectoryInfo = new DirectoryInfo(path);
            await ShowListViewAsync(currentDirectoryInfo);
        }

        private async Task ShowListViewAsync(DirectoryInfo directoryInfo) 
        {
            listView1.Clear();

            await Task.Run(() =>
            {
                Invoke(() =>
                {
                    listViewProgressBar.Visible = true;
                });

                try
                {
                    foreach (var file in directoryInfo.GetFiles()) //adds to imageList
                    {
                        if (isImageFile(file))
                        {
                            ListViewItem item1 = new ListViewItem(file.Name);    // Text and image index    imageList1.Images.IndexOfKey(file.FullName)
                            item1.SubItems.Add(file.LastWriteTime.ToString());      // Column 3
                            item1.SubItems.Add(file.Length.ToString());             // Column 4
                            item1.Tag = file.FullName;                              //sending to editForm

                            /*I used
        https://stackoverflow.com/questions/27826992/how-to-index-image-list-by-name-instead-of-position
                            -DWZA April 26, 2016
                            to figure out how to locate index of image based on key*/
                            item1.ImageIndex = imageList1.Images.IndexOfKey(item1.Tag.ToString());

                            Invoke(() =>
                            {
                                // Add the items to the list view
                                listView1.Items.Add(item1);
                            });
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {

                }
            });
            // Create columns (Width of -2 indicates auto-size)
            listView1.Columns.Add("Name", 250, HorizontalAlignment.Left);
            listView1.Columns.Add("Date", 170, HorizontalAlignment.Left);
            listView1.Columns.Add("Size", -2, HorizontalAlignment.Left);

            Invoke(() =>
            {
                listViewProgressBar.Visible = false;
            });

        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //Ignore
        }

        private void listView1_ItemActivate_1(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                var editForm = new EditForm(item.Tag.ToString());
                DialogResult result = editForm.ShowDialog();

                //check result, if ok, save, not, dont save
            }
        }

        private void locateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("First select an image, then choose this option to see " +
                    "its location on disk", "No Image Selected", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                // I used Jamie Penney at this link to know how to open file explorer
                // https://stackoverflow.com/questions/1746079/how-can-i-open-windows-explorer-to-a-certain-directory-from-within-a-wpf-app
                // I used Samuel Yang at this link to know how to have a selected file
                // https://stackoverflow.com/questions/334630/opening-a-folder-in-explorer-and-selecting-a-file

                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    string path = "/select,\"" + item.Tag.ToString() + "\"";
                    Process.Start("explorer.exe", path);
                }
            }
        }

        private async void selectRootFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                var path = folderBrowserDialog1.SelectedPath;
                ListDirectory(path);
                var currentDirectoryInfo = new DirectoryInfo(path);
                await ShowListViewAsync(currentDirectoryInfo);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutInfo aboutInfo = new AboutInfo();
            aboutInfo.ShowDialog();
        }

        // Changing the listView icon size from Shiroy
        //https://stackoverflow.com/questions/30035814/c-sharp-listview-image-icon-size

        private async void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Get path of selected treeNode then call ShowListView
            string path = treeView1.SelectedNode.Tag.ToString();

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            listView1.View = View.Details;
            imageList1.ImageSize = new Size(75, 75);
            listView1.SmallImageList = imageList1;

            AddImages(directoryInfo);

            var currentDirectoryInfo = new DirectoryInfo(path);
            await ShowListViewAsync(currentDirectoryInfo);
        }

        private async void smallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get path of selected treeNode then call ShowListView
            string path = treeView1.SelectedNode.Tag.ToString();

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            listView1.View = View.SmallIcon;
            imageList1.ImageSize = new Size(75, 75);
            listView1.SmallImageList = imageList1;

            AddImages(directoryInfo);

            var currentDirectoryInfo = new DirectoryInfo(path);
            await ShowListViewAsync(currentDirectoryInfo);
        }

        private async void largeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get path of selected treeNode then call ShowListView
            string path = treeView1.SelectedNode.Tag.ToString();

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            listView1.View = View.LargeIcon;
            imageList1.ImageSize = new Size(145, 145);
            listView1.LargeImageList = imageList1;

            AddImages(directoryInfo);

            var currentDirectoryInfo = new DirectoryInfo(path);
            await ShowListViewAsync(currentDirectoryInfo);
        }

    }
}