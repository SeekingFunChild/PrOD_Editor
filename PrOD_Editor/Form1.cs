using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PrOD_Editor
{
    public partial class Form1 : Form
    {
        private bool isYaz0 = false;
        private PrOD m_PrOD;

        TreeView treeView;
        TreeNode root;
        Button Save;
        TextBox SearchText;
        Button Search;
        ListView SearchResults;
        Button NewNode;
        Button NewInstance;
        Button DeleteNode;

        List<TreeNode> searched_nodes;

        string inPath;

        public Form1()
        {
            searched_nodes = new List<TreeNode>();

            InitializeComponent();

            treeView = new TreeView();
            treeView.Width = this.Width - 200;
            treeView.Height = this.Height - 100;

            treeView.LabelEdit = true;
            treeView.NodeMouseClick += treeView_NodeMouseClick;
            treeView.AfterLabelEdit += treeView_AfterLabelEdit;

            root = new TreeNode("root");
            treeView.Nodes.Add(root);

            this.Controls.Add(treeView);

            Save = new Button();
            Save.Text = "Save";
            Save.Top += treeView.Bottom + 10;
            Save.Left = treeView.Right;
            Save.Click += SaveClick;
            this.Controls.Add(Save);

            SearchText = new TextBox();
            SearchText.Top = treeView.Bottom + 10;
            SearchText.Left = treeView.Left;
            Controls.Add(SearchText);

            Search = new Button();
            Search.Text = "Search";
            Search.Top = treeView.Bottom + 10;
            Search.Left = SearchText.Right + 10;
            Search.Click += SearchClick;
            Controls.Add(Search);

            SearchResults = new ListView();
            SearchResults.Top = treeView.Top;
            SearchResults.Left = treeView.Right + 10;
            SearchResults.Height = treeView.Height;
            SearchResults.Width = 160;
            SearchResults.View = View.Details;
            SearchResults.Columns.Add("index", 60, HorizontalAlignment.Left);
            SearchResults.Columns.Add("Value", 200, HorizontalAlignment.Left);
            SearchResults.FullRowSelect = true;
            SearchResults.MouseClick += ListView_Click;
            Controls.Add(SearchResults);

            NewNode = new Button();
            NewNode.Text = "NewNode";
            NewNode.Top = treeView.Bottom + 10;
            NewNode.Left = Search.Right + 10;
            NewNode.Click += NewNodeClick;
            Controls.Add(NewNode);

            NewInstance = new Button();
            NewInstance.Text = "NewInstance";
            NewInstance.Top = treeView.Bottom + 10;
            NewInstance.Left = NewNode.Right + 10;
            NewInstance.Click +=NewInstanceClick;
            Controls.Add(NewInstance);

            DeleteNode = new Button();
            DeleteNode.Text = "DeleteNode";
            DeleteNode.Top = treeView.Bottom + 10;
            DeleteNode.Left = NewInstance.Right + 10;
            DeleteNode.Click += DeleteNodeClick;
            Controls.Add(DeleteNode);
        }


        private void SaveClick(object sender, EventArgs e)
        {
            PrOD prOD = treeViewToPrOD();
            byte[] bytes=prOD.getBytes();
            if(inPath!=null)
            {
                if(isYaz0)
                {
                    File.WriteAllBytes(inPath, Yaz0.encode(bytes));
                }
                else
                {
                    File.WriteAllBytes(inPath, bytes);
                }
            }
            MessageBox.Show("Save finished");
        }

        private void SearchClick(object sender, EventArgs e)
        {
            searched_nodes.Clear();
            SearchTreeNode(root,SearchText.Text);
            SearchResults.Items.Clear();

            if (searched_nodes.Count == 0)
            {
                MessageBox.Show("not found");
                return;
            }

            int index = 0;
            SearchResults.BeginUpdate();
            foreach (TreeNode treeNode in searched_nodes)
            {
                treeView.SelectedNode = treeNode;
                treeView.Focus();

                ListViewItem lvi = new ListViewItem();
                lvi.Text = index.ToString();
                lvi.SubItems.Add(treeNode.Text);
                SearchResults.Items.Add(lvi);

                index++;
            }
            SearchResults.EndUpdate();
        }

        private void SearchTreeNode(TreeNode rtNode,string text)
        {
            if (text != null && text != "")
            {
                if (rtNode.Text.ToLower().IndexOf(text.ToLower()) != -1)
                {
                    searched_nodes.Add(rtNode);
                }

                foreach (TreeNode node in rtNode.Nodes)
                {
                    SearchTreeNode(node, text);
                }
            }
        }

        private void ListView_Click(object sender, MouseEventArgs e)
        {
            //MessageBox.Show(SearchResults.FocusedItem.Text);
            int index = int.Parse(SearchResults.FocusedItem.Text);
            var node = searched_nodes[index];
            treeView.SelectedNode = node;
            treeView.Focus();
        }

        private void NewNodeClick(object sender, EventArgs e)
        {
            TreeNode selNode = treeView.SelectedNode;
            TreeNode newNode = new TreeNode("new node");
            selNode.Nodes.Add(newNode);
        }

        private void NewInstanceClick(object sender, EventArgs e)
        {
            TreeNode selNode = treeView.SelectedNode;

            TreeNode meshInstance = new TreeNode("instance");

            TreeNode position = new TreeNode("positon");
            TreeNode x = new TreeNode("0.0");
            TreeNode y = new TreeNode("0.0");
            TreeNode z = new TreeNode("0.0");
            position.Nodes.Add(x);
            position.Nodes.Add(y);
            position.Nodes.Add(z);
            meshInstance.Nodes.Add(position);

            TreeNode rotation = new TreeNode("rotation");
            x = new TreeNode("0.0");
            y = new TreeNode("0.0");
            z = new TreeNode("0.0");
            rotation.Nodes.Add(x);
            rotation.Nodes.Add(y);
            rotation.Nodes.Add(z);
            meshInstance.Nodes.Add(rotation);

            TreeNode ufScale = new TreeNode("uniformScale");
            ufScale.Nodes.Add(new TreeNode("1.0"));
            meshInstance.Nodes.Add(ufScale);

            selNode.Nodes.Add(meshInstance);
        }

        private void DeleteNodeClick(object sender, EventArgs e)
        {
            TreeNode selNode = treeView.SelectedNode;
            TreeNode parent = selNode.Parent;
            if(parent!=null)
            {
                parent.Nodes.Remove(selNode);
            }
        }

        private PrOD treeViewToPrOD()
        {
            List<string> names=new List<string>();
            List<PrOD.Mesh> meshes=new List<PrOD.Mesh>();

            //names
            foreach(TreeNode node in root.Nodes)
            {
                if(!names.Contains(node.Text))
                {
                    names.Add(node.Text);
                }
            }
            names.Sort();

            //meshes
            foreach (TreeNode node in root.Nodes)
            {
                PrOD.Mesh mesh = new PrOD.Mesh();
                mesh.name = node.Text;
                mesh.nameOffset = PrOD.getNameOffset(names,mesh.name);

                mesh.instancesCount = (uint)node.Nodes.Count;
                treeNodeToMesh(mesh, node);

                meshes.Add(mesh);
            }
            meshes.Sort(customCmp);

            PrOD prod = new PrOD(names,meshes);
            return prod;
        }

        private int customCmp(PrOD.Mesh m1, PrOD.Mesh m2)
        {
            if(m1.nameOffset>m2.nameOffset)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        private void treeNodeToMesh(PrOD.Mesh mesh,TreeNode treeNode)
        {
            foreach(TreeNode node in treeNode.Nodes)
            {
                mesh.instances.Add(treeNodeToMeshInstance(node));
            }
        }

        private PrOD.Mesh.MeshInstance treeNodeToMeshInstance(TreeNode treeNode)
        {
            float[] pos = new float[3];
            float[] rot = new float[3];
            float ufscale = 0;

            pos[0] = float.Parse(treeNode.Nodes[0].Nodes[0].Text);
            pos[1] = float.Parse(treeNode.Nodes[0].Nodes[1].Text);
            pos[2] = float.Parse(treeNode.Nodes[0].Nodes[2].Text);

           rot[0] = float.Parse(treeNode.Nodes[1].Nodes[0].Text);
            rot[1] = float.Parse(treeNode.Nodes[1].Nodes[1].Text);
            rot[2] = float.Parse(treeNode.Nodes[1].Nodes[2].Text);

            ufscale = float.Parse(treeNode.Nodes[2].Nodes[0].Text);

            return new PrOD.Mesh.MeshInstance(pos,rot,ufscale);
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView.SelectedNode = e.Node;
                e.Node.BeginEdit();
            }
        }

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            e.Node.EndEdit(true);
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            inPath = null;
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i].Trim() != "")
                {
                    byte[] bytes = File.ReadAllBytes(s[i]);
                    isYaz0 = Yaz0.IsYaz0(bytes);
                    if(isYaz0)
                    {
                        m_PrOD = new PrOD(Yaz0.decode(bytes));
                    }
                    else
                    {
                        m_PrOD = new PrOD(bytes);
                    }
                    ShowPrOD();
                    this.Text = "PrODEditor" + "(" + s[i] + ")";
                    this.inPath = s[i];
                    break;
                }
            }
         }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void ShowPrOD()
        {
            root.Nodes.Clear();

            for(int i=0;i<m_PrOD.m_meshes.Count;i++)
            {
                TreeNode meshNode = new TreeNode(m_PrOD.m_meshes[i].name);
                ShowMesh(meshNode, m_PrOD.m_meshes[i]);
                root.Nodes.Add(meshNode);
            }
        }

        private void ShowMesh(TreeNode node,PrOD.Mesh mesh)
        {
            for(int i=0;i<mesh.instancesCount;i++)
            {
                TreeNode meshInstance = new TreeNode("instance");

                TreeNode position = new TreeNode("positon");
                TreeNode x = new TreeNode(mesh.instances[i].position[0].ToString());
                TreeNode y = new TreeNode(mesh.instances[i].position[1].ToString());
                TreeNode z = new TreeNode(mesh.instances[i].position[2].ToString());
                position.Nodes.Add(x);
                position.Nodes.Add(y);
                position.Nodes.Add(z);
                meshInstance.Nodes.Add(position);

                TreeNode rotation = new TreeNode("rotation");
                x = new TreeNode(mesh.instances[i].rotation[0].ToString());
                 y = new TreeNode(mesh.instances[i].rotation[1].ToString());
                z = new TreeNode(mesh.instances[i].rotation[2].ToString());
                rotation.Nodes.Add(x);
                rotation.Nodes.Add(y);
                rotation.Nodes.Add(z);
                meshInstance.Nodes.Add(rotation);

                TreeNode ufScale = new TreeNode("uniformScale");
                ufScale.Nodes.Add(new TreeNode(mesh.instances[i].uniformScale.ToString()));
                meshInstance.Nodes.Add(ufScale);

                node.Nodes.Add(meshInstance);
            }
        }
    }
}
