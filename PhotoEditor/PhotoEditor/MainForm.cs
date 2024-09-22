using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PhotoEditor
{
    public partial class MainForm : Form
    {
        private string photoRootDirectory;        
        public MainForm()
        {
            InitializeComponent();

            photoRootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            ListDirectory(photoRootDirectory);

            listView1.StateImageList = imageList1;

        }
        //ADD IMAGES TO EACH ICON
        /*----------------------------------------------------------------------------------------
                 Got both methods below from 
        https://stackoverflow.com/questions/6239544/populate-treeview-with-file-system-directory-structure
                 -Alex Aza June 4, 2011
                 I did modify them to help create tags and add images to imageList1*/

        private void ListDirectory(string path)
        {
            treeView1.Nodes.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView1.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo));
            treeView1.Nodes[0].Tag = path;
        }

        private TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNode(directoryInfo.Name);

            foreach (var directory in directoryInfo.GetDirectories()) //recursive, tags each Node with directory name, adds to directoryNode
            {
                TreeNode node = CreateDirectoryNode(directory);
                node.Tag = directory.FullName;
                directoryNode.Nodes.Add(node);
            }

            foreach (var file in directoryInfo.GetFiles()) //adds each photo to imageList
            {
                if (isImageFile(file))
                {
                    //uses file.FullName as a key to access later in listView
                    imageList1.Images.Add(file.FullName, Bitmap.FromFile(file.FullName));
                }
            }
            return directoryNode;
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

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string path = treeView1.SelectedNode.Tag.ToString();

            var currentDirectoryInfo = new DirectoryInfo(path);
            showListView(currentDirectoryInfo);
        }

        private void showListView(DirectoryInfo directoryInfo)
        {
            listView1.Clear();
            foreach (var file in directoryInfo.GetFiles()) //adds to imageList
            {
                if (isImageFile(file))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(file.FullName);
                    MemoryStream ms = new MemoryStream(bytes);
                    Image img = Image.FromStream(ms);                   // idk if I need any of this

                    ListViewItem item1 = new ListViewItem(file.Name);    // Text and image index; replace item1 with name of photo
                    item1.SubItems.Add(file.LastWriteTime.ToString());      // Column 3
                    item1.SubItems.Add(file.Length.ToString());             // Column 4
                    item1.Tag = file.FullName;                              //sending to editForm

                    /*I used
https://stackoverflow.com/questions/27826992/how-to-index-image-list-by-name-instead-of-position
                    -DWZA April 26, 2016
                    to figure out how to locate index of image based on key*/
                    item1.StateImageIndex = imageList1.Images.IndexOfKey(item1.Tag.ToString());


                    // Add the items to the list view
                    listView1.Items.Add(item1);
                }
            }
            // Create columns (Width of -2 indicates auto-size)
            listView1.Columns.Add("Name", 250, HorizontalAlignment.Left);
            listView1.Columns.Add("Date", 170, HorizontalAlignment.Left);
            listView1.Columns.Add("Size", -2, HorizontalAlignment.Left);
            // Show default view
            listView1.View = View.Details;
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
                openFileDialog1 = new OpenFileDialog();
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    openFileDialog1.InitialDirectory = item.Tag.ToString();
                    openFileDialog1.ShowDialog();
                }

            }
        }
    }
}