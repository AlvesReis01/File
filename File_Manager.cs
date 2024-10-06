using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Web.UI.WebControls;
using TreeNodeWF = System.Windows.Forms.TreeNode;
using ImageWebControls = System.Web.UI.WebControls.Image;
using ImageDrawing = System.Drawing.Image;



namespace File_Maneger
{
    public partial class File_Manager : Form

    {
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        ImageList imageList1;

        public File_Manager()
        {
            InitializeComponent();
            LoadTreeView();
            LoadSizeBlocksFileManager();
            TreeViewMenu.AfterExpand += TreeViewMenu_AfterExpand;
            TreeViewMenu.AfterCollapse += TreeViewMenu_AfterCollapse;

            // Move the window
            flowLayoutPanel2.MouseDown += flowLayoutPanel2_MouseDown;
            flowLayoutPanel2.MouseMove += flowLayoutPanel2_MouseMove;
            flowLayoutPanel2.MouseUp += flowLayoutPanel2_MouseUp;
        }
        /// <summary>
        /// Function to drag the window(flowlayoutpanel)
        /// </summary>
        private void flowLayoutPanel2_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }
        private void flowLayoutPanel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }
        private void flowLayoutPanel2_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void TreeViewProprety()
        {
            TreeViewMenu.Visible = false;
        }
        private void MainShowTreeView()
        {
            if (TreeViewMenu.Visible == false) 
            {
                MainHideTreeView();
            }
        }
        private void MainHideTreeView()
        {

        }
        bool barExtend= false;
        private void LoadTreeView()
        {

            //Pegar uma imagem de um diretório para guarda na lista e após isso armazena
            ImageList imageList = new ImageList(); //objeto tipo imageList
            imageList.Images.Add(ImageDrawing.FromFile(@"E:\Pictures\folder1.png"));  //Imagem de um Diretório 
            TreeViewMenu.ImageList = imageList;

            //Guarda os diretórios de um local especifico em uma treeview, por meio de seus nodes
            DirectoryInfo info = new DirectoryInfo(@"E:\");
            
            try
            {
                if (Directory.Exists(info.FullName))
                {
                    TreeNodeWF DirectoryNode = new TreeNodeWF(info.Name, 0, 0);
                    TreeViewMenu.Nodes.Add(DirectoryNode);
                    LoadsubDirectory(info, DirectoryNode);
                    TreeViewMenu.Height += 20;
                }
            }
            catch 
            {
                //erro quando tiver um
            }
            
        }
        private void LoadsubDirectory(DirectoryInfo path, TreeNodeWF treeNode)// Carrega recursivamente os subdiretórios de um diretório específico e os adiciona a um nó da árvore
        {
            // Obtém uma lista de subdiretórios do diretório especificado
            string[] subdirectoryEntries = Directory.GetDirectories(path.FullName);
            foreach (string subdirctory in subdirectoryEntries)
            {    // Para cada subdiretório encontrado, cria um novo nó e chama a função recursivamente.
                DirectoryInfo Sub_Di = new DirectoryInfo(subdirctory);
                TreeNodeWF DirectoryNode = treeNode.Nodes.Add(Sub_Di.Name);
                LoadsubDirectory(Sub_Di, DirectoryNode);// Chamada recursiva para carregar subdiretórios.

            }
        }
        public void ShowDrivers()
        {
            TreeViewMenu.BeginUpdate();// Suspende a atualização do TreeView
            string[] drives = Directory.GetLogicalDrives();
            foreach (string adrive in drives)
            {    // Para cada unidade, cria um nó e chama o método AddDirs para carregar diretórios.
                TreeNodeWF tn = new TreeNodeWF(adrive);
                TreeViewMenu.Nodes.Add(tn);// Carrega os diretórios da unidade.
                AddDirs(tn);
            }
            TreeViewMenu.EndUpdate();// Retoma a atualização do TreeView.
        }
        public void ShowFiles() 
        {
            DirectoryInfo direct = new DirectoryInfo(TreeViewMenu.SelectedNode.FullPath);// Obtém o diretório selecionado a partir do TreeView.
            FileInfo[] fiarry = { };
            ListViewItem item;

            imageList1 = new ImageList();
            ListViewScreen1.Items.Clear();
            ListViewScreen1.SmallImageList = imageList1;
            if (direct.Exists)
            {
                fiarry = direct.GetFiles();
            }
            ListViewScreen1.BeginUpdate();
            foreach (FileInfo fi in fiarry)    // Para cada arquivo encontrado, cria um item no ListView e exibe informações adicionais.
            {
                Icon iconForFile;
                item = new ListViewItem(fi.Name);
                ListViewScreen1.Items.Add(item);

                iconForFile = SystemIcons.WinLogo; // Obtém o ícone associado ao tipo de arquivo.
                if (!imageList1.Images.ContainsKey(fi.Extension))
                {
                    // Extrai o ícone associado ao arquivo e o adiciona à ImageList se ainda não estiver presente.
                    iconForFile = System.Drawing.Icon.ExtractAssociatedIcon(fi.FullName);
                    imageList1.Images.Add(fi.Extension, iconForFile);
                }
                // Adiciona tamanho, data de modificação e atributos como subitens.
                item.ImageKey = fi.Extension;
                item.SubItems.Add(fi.Length.ToString() + " bytes");
                item.SubItems.Add(fi.LastWriteTime.ToString());
                item.SubItems.Add(Atributes(fi));
            }
            ListViewScreen1.EndUpdate();
        }
        public void AddDirs(TreeNodeWF tn)
        {
            string path = tn.FullPath;
            DirectoryInfo di = new DirectoryInfo(path);
            DirectoryInfo[] diarray = { };
            try
            {
                if (di.Exists)
                {
                    diarray = di.GetDirectories();
                }
            }
            catch
            {
                return;
            }
            foreach (DirectoryInfo d in diarray)
            {
                TreeNodeWF tndir = new TreeNodeWF(d.Name);
                tn.Nodes.Add(tndir);
            }
        }
        private string Atributes(FileInfo fi)
        {
            string atts = "";
            if ((fi.Attributes & FileAttributes.Archive) != 0)
                atts += "Archive";
            if ((fi.Attributes & FileAttributes.Hidden) != 0)
                atts += "Hidden";
            if ((fi.Attributes & FileAttributes.ReadOnly) != 0)
                atts += "ReadOnly";
            if ((fi.Attributes & FileAttributes.System) != 0)
                atts += "System";
            return atts;
        }
        private void listView1_DoubleClick(object sender, EventArgs e) // function open the file
        {
            string diskfile = TreeViewMenu.SelectedNode.FullPath;
            if (!diskfile.EndsWith("\\"))
                diskfile += "\\";
            diskfile += ListViewScreen1.FocusedItem.Text;
            if (File.Exists(diskfile))
                // Process.Start(diskfile);
                Process.Start(new ProcessStartInfo { FileName = diskfile, UseShellExecute = true });
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ShowDrivers();

        }
        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            ShowFiles();        
        }
        private void TreeViewMenu_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeViewMenu.BeginUpdate();
            foreach (TreeNodeWF tn in e.Node.Nodes)
            {
                AddDirs(tn);
            }
            TreeViewMenu.EndUpdate();
        }
        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
        }
        private void TreeViewMenu_AfterExpand(object sender, TreeViewEventArgs e)
        {
            AtualySizeTreewViewMenu();
        }
        private void TreeViewMenu_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            AtualySizeTreewViewMenu();
        }
        ///
        /// 
        /// Desing
        /// 
        /// 

        /// 
        /// Atualizar tamanho do TreeViewMenu
        ///

        private void AtualySizeTreewViewMenu()
        {
            // Obter a altura total dos nós visíveis expandidos
            int totalHeight = 0;
            int CountHeight = 0;
            foreach (TreeNodeWF node in  TreeViewMenu.Nodes)
            {
                totalHeight += GetNodeHeight(node);
                CountHeight = GetNodeHeight(node)+1;
            }

            // Ajustar a altura do TreeView
            TreeViewMenu.Height = CountHeight*35;
        }
        private int GetNodeHeight(TreeNodeWF node)
        {
            int height = node.Bounds.Height; // Altura do nó atual
            int CountHeight = 1;
            if (node.IsExpanded)
            {
                foreach (TreeNodeWF childNode in node.Nodes)
                {
                    height += GetNodeHeight(childNode); // Recursivamente obter a altura de filhos
                    CountHeight++;
                }
            }
            return CountHeight;

        }

        // Mudanças no Layout
        private void btnMenuConfig_Click(object sender, EventArgs e)
        {
            var menuConfig = new Configuração(this);
            menuConfig.Show();
        }
        public void AplMod(int R, int G, int B, int RS, int GS, int BS)
        {
            TreeViewMenu.ForeColor = Color.FromArgb(R, G, B);
            TreeViewMenu.BackColor = Color.FromArgb(RS, GS, BS);
            ListCompontsPanel.ForeColor = Color.FromArgb(R, G, B);
            ListCompontsPanel.BackColor = Color.FromArgb(RS, GS, BS);
            flowLayoutPanel2.ForeColor = Color.FromArgb(R, G, B);
            flowLayoutPanel2.BackColor = Color.FromArgb(RS, GS, BS);
            ControlScreenButtons.ForeColor = Color.FromArgb(R, G, B);
            ControlScreenButtons.EnableCloseColor = Color.FromArgb(R, G, B);
            ControlScreenButtons.EnableMaximizeColor = Color.FromArgb(R, G, B);
            ControlScreenButtons.EnableMinimizeColor = Color.FromArgb(R, G, B);
            ControlScreenButtons.BackColor = Color.FromArgb(RS, GS, BS);
            ListViewScreen1.ForeColor = Color.FromArgb(R, G, B);
            ListViewScreen1.BackColor = Color.FromArgb(RS, GS, BS);
            splitter1.BackColor = Color.FromArgb(RS/2, GS/2, BS/2);
            splitter2.BackColor = Color.FromArgb(RS/2, GS/2, BS/2);
            splitter3.BackColor = Color.FromArgb(RS/2, GS/2, BS/2);
            splitter4.BackColor = Color.FromArgb(RS / 2, GS / 2, BS / 2);
        }

        DirectoryInfo selectedpath = new DirectoryInfo(@"D:\arquivos criados"); //treeView1.SelectedNode.FullPath

        private void create_configs_button_Click(object sender, EventArgs e)
        {
            Create_Config config = new Create_Config(selectedpath);
            config.Show();
        }

        // Functions to move the window(flowlayoutopanel

            /// <Resize>
            /// Redicionamento dos componentes conforme o File_Manaer 
            /// </summary>
                public class AutoSizeFileManagerBlocksProperty
                {
                    public Size FileManagerOriginal;
                    public Rectangle TreeViewMenuSize;

                    /// <summary>
                    /// Componentes do ListCompontsPanel 
                    /// </summary>
                        public Rectangle ListCompontsPanelSize;
                        ///
                        ///Limitadores de espaçamento do ListCompontsPanel
                        ///
                        public Rectangle Limiter1Size;
                        public Rectangle Limiter2Size;
                        public Rectangle Limiter3Size;

                        ///
                        /// Linha de componentes do Game
                        ///
                        public Rectangle PanelGameSize;
                        public Rectangle GameTxtBoxSize;
                        public Rectangle SeparatorButtonGameSize;
                        public Rectangle OpenButtonGameSize;

                        ///
                        ///  Linha de componentes do Menu
                        ///
                        public Rectangle PanelMenuSIze;
                        public Rectangle MenuTxtBoxSize;
                        public Rectangle SeparatorButtonMenuSize;
                        public Rectangle OpenButtonMenu;
                        ///
                        /// Buttons
                        ///
                        public Rectangle ButtonConfigGeralSIze;
                        public Rectangle ListViewScreen1Size;
                }
                AutoSizeFileManagerBlocksProperty SASFMBP = new AutoSizeFileManagerBlocksProperty();
            
                private void LoadSizeBlocksFileManager()
                {
                    this.Resize += File_Manager_Resize;
                    SASFMBP.FileManagerOriginal = this.Size;

                ///
                /// 
                /// Panel Geral da TreeView
                ///
                    SASFMBP.ListCompontsPanelSize = new Rectangle(ListCompontsPanel.Location, ListCompontsPanel.Size);
                    ///
                    /// Limitadores de espaçamento do ListCompontsPanel
                    ///
                    SASFMBP.Limiter1Size = new Rectangle(Limiter1.Location, Limiter1.Size);
                    SASFMBP.Limiter2Size = new Rectangle(Limiter2.Location,Limiter2.Size);
                    SASFMBP.Limiter3Size = new Rectangle(Limiter3.Location,Limiter3.Size);
                    ///
                    /// Gamer Componentes
                    ///
                    SASFMBP.PanelGameSize = new Rectangle(PanelGames.Location,PanelGames.Size);
                    SASFMBP.GameTxtBoxSize = new Rectangle(GamesTxtBox.Location, GamesTxtBox.Size);
                    SASFMBP.SeparatorButtonGameSize = new Rectangle(SeparatorButtonGames.Location,SeparatorButtonGames.Size);  
                    SASFMBP.OpenButtonGameSize = new Rectangle(OpenButtonMenu.Location, OpenButtonMenu.Size);
                    ///
                    /// Menu Componentes
                    ///
                    SASFMBP.PanelMenuSIze = new Rectangle(PanelMenu.Location, PanelMenu.Size);
                    SASFMBP.MenuTxtBoxSize = new Rectangle(MenuTxtBox.Location, MenuTxtBox.Size);
                    SASFMBP.SeparatorButtonMenuSize = new Rectangle(SeparatorButtonMenu.Location,SeparatorButtonMenu.Size);
                    SASFMBP.OpenButtonGameSize = new Rectangle(OpenButtonGames.Location, OpenButtonGames.Size);
                        ///
                        /// Treeview Componente
                        ///
                        SASFMBP.TreeViewMenuSize = new Rectangle(TreeViewMenu.Location, TreeViewMenu.Size);
                    ///
                    /// Buttons Config
                    ///
                    SASFMBP.ButtonConfigGeralSIze = new Rectangle(ButtonConfigGeral.Location, ButtonConfigGeral.Size);
                    ///
                    /// List View
                    ///
                    SASFMBP.ListViewScreen1Size = new Rectangle(ListViewScreen1.Location,ListViewScreen1.Size);
                }
                private void File_Manager_Resize(object sender, EventArgs e)
                {
                    Resize_ComponentConfirm();
                }
                private void Resize_ComponentConfirm()
                {
                Resize_Component(ListCompontsPanel, SASFMBP.ListCompontsPanelSize);
                    ///
                    /// Limitadores de espaçamento do ListCompontsPanel
                    ///
                    Resize_Component(Limiter1, SASFMBP.Limiter1Size);
                    Resize_Component(Limiter2, SASFMBP.Limiter2Size);
                    Resize_Component(Limiter3, SASFMBP.Limiter3Size);
                    ///
                    /// Gamer Componentes
                    ///
                    Resize_Component(PanelGames, SASFMBP.PanelGameSize);
                    Resize_Component(GamesTxtBox, SASFMBP.GameTxtBoxSize);
                    Resize_Component(SeparatorButtonGames, SASFMBP.SeparatorButtonGameSize);
                    Resize_Component(OpenButtonGames, SASFMBP.GameTxtBoxSize);
              
                    ///
                    /// Menu Componentes
                    ///
                    Resize_Component(PanelMenu, SASFMBP.PanelMenuSIze);
                    Resize_Component(MenuTxtBox, SASFMBP.MenuTxtBoxSize);
                    Resize_Component(SeparatorButtonMenu, SASFMBP.SeparatorButtonMenuSize);
                    Resize_Component(OpenButtonMenu, SASFMBP.OpenButtonGameSize);
                        ///
                        /// Treeview Menu
                        /// 
                        Resize_Component(TreeViewMenu, SASFMBP.TreeViewMenuSize);
                    ///
                    /// Buttons Config
                    ///
                    Resize_Component(ButtonConfigGeral, SASFMBP.ButtonConfigGeralSIze);
            
                    ///
                    /// List View
                    ///
                    Resize_Component(ListViewScreen1,SASFMBP.ListViewScreen1Size);

                }
                /// <summary>
                ///     Metodo para redimencionar cada componente
                /// </summary>
                    private void Resize_Component(Control c, Rectangle r)
                    {
                        float Xr = (float)(this.Width) / (float)(SASFMBP.FileManagerOriginal.Width);
                        float Yr = (float)(this.Height) / (float)(SASFMBP.FileManagerOriginal.Height);
                        int newX = (int)(r.X * Xr);
                        int newY = (int)(r.Y * Yr);

                        int newWidth = (int)(r.Width * Xr);
                        int newHeight = (int)(r.Height * Yr);

                        c.Location = new Point(newX, newY);
                        c.Size = new Size(newWidth, newHeight);

                    }
            
        /// <summary>
        /// 
        /// Redicionamento dos componentes
        /// 
        /// </summary>
















































        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        private void nightControlBox1_Click(object sender, EventArgs e)
        {

        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void panel2_Paint_1(object sender, PaintEventArgs e)
        {
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox1_Enter(object sender, EventArgs e)
        {
        }
        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void pictureBox8_Click(object sender, EventArgs e)
        {

        }
        private void treeView2_AfterSelect_1(object sender, TreeViewEventArgs e)
        {

        }
        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void GameTxtBox_Click(object sender, EventArgs e)
        {

        }

        private void Limiter2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ListCompontsPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void MenuTxtBox_Click(object sender, EventArgs e)
        {

        }

        private void Limiter3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitter4_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }

        private void splitter4_SplitterMoving(object sender, SplitterEventArgs e)
        {
   
        }

        private void splitter4_DragDrop(object sender, DragEventArgs e)
        {
        }

        private void splitter4_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void novoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Create_Config config = new Create_Config(selectedpath);
            config.Show();
        }

        private void button1_Click_2(object sender, EventArgs e)
        {

        }
    }
}
