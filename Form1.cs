using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Sample_Ex
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Graph kb;

        private void Form1_Load_1(object sender, EventArgs e)
        {
            kb = new Graph();
            Notation3Parser np = new Notation3Parser();
            np.Load(kb, @"E:\Second Semester\Software Development Technologies\Application and Kb\Hagos_KB1.ttl");
            TreeView1.Nodes.Add("Power Prediction Model for Hydro Turbine");//root of the structure
            string targetNode = "predictionModel";
            try
            {
                String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix xsd: <http://www.w3.org/2001/XMLSchema#> 
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          SELECT  ?type ?label ?subProcessOf 
                             WHERE
                                 {
                                   ?data  rdf:type ?type;
                                          rdfs:label ?label;
                                          prop:subProcessOf ?subProcessOf.
                                    }";
                //Local variables used for controlling the possition for the treeview
                // stack data structure 
                Stack stack = new Stack(); int[] index = { 0, 0, 0, 0, 0 }; int i = 0;
                int x = 0; String snode = ""; int level = 0;
                while (stack!=null || i==0) 
                {
                    if (i != 0)
                    {
                        targetNode = stack.Pop().ToString();
                    }
                    int y = 0; i++;
                    SparqlResultSet rs = (SparqlResultSet)kb.ExecuteQuery(q1);
                    foreach (SparqlResult r in rs)
                    {
                        String node = r["label"].ToString();
                        snode = r["subProcessOf"].ToString().Substring(9);
                        if (snode.CompareTo(targetNode) == 0)
                        {
                            stack.Push(r["type"].ToString().Substring(12));
                            y++;
                            if (level == 0)
                            {
                                TreeView1.Nodes[0].Nodes.Add(node);
                            }
                            else if (level == 1)
                            {
                                TreeView1.Nodes[0].Nodes[index[0]].Nodes.Add(node);
                            }
                            else if (level == 2 )
                            {
                                TreeView1.Nodes[0].Nodes[index[0]].Nodes[index[1]].Nodes.Add(node);
                            }
                            else if (level == 3)
                            {
                                TreeView1.Nodes[0].Nodes[index[0]].Nodes[index[1]].Nodes[index[2]].Nodes.Add(node);
                            }
                        }
                    }
                    if (y == 0 ) {
                        x--; index[level-1]--;
                    }
                    else
                    {
                        index[level]=y;
                        index[level]--;  x = index[level]; level += 1; }
                    while(x == -1) { level--; index[level-1]--; x = index[level-1]; }
                } 
            }
            catch (Exception ex)
            {              
                Console.WriteLine(ex.Message);
            }
            textBox2.Text = "Click the treeview to show the KB information... " + Environment.NewLine; 
        }
        private void Button1_Click_1(object sender, EventArgs e)
        {
            textBox2.Text = " ";
            TreeView1.Refresh();
        }
        private void Button5_Click_1(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeView1.Refresh();
            textBox2.Text = " ";
            String item = TreeView1.SelectedNode.Text.ToString();
            String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix xsd: <http://www.w3.org/2001/XMLSchema#> 
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:>

                       SELECT  ?type ?label ?hasKPI  ?comment ?subProcessOf
                             WHERE
                                 {
                                  ?data  rdf:type ?type;
                                         rdfs:label ?label;
                                         rdfs:comment ?comment.
                                   OPTIONAL { ?data  prop:subProcessOf ?subProcessOf }                   
                                   OPTIONAL { ?data  prop:hasKPI ?hasKPI}
                                     }";
            try
            {
                    SparqlResultSet rs = (SparqlResultSet)kb.ExecuteQuery(q1);
                    foreach (SparqlResult r in rs)
                    {
                        String node = r["label"].ToString();
                        if (node.CompareTo(item) == 0)
                        {
                            textBox2.Text += "RDF Type: " + r["type"].ToString().Substring(12) + Environment.NewLine;
                            textBox2.Text += Environment.NewLine + "Label: " + r["label"].ToString()+ Environment.NewLine;
                            textBox2.Text += Environment.NewLine + "KPI: " + r["hasKPI"].ToString().Substring(9) + Environment.NewLine;
                            textBox2.Text += Environment.NewLine + "Comment: " + r["comment"].ToString() + Environment.NewLine;
                            textBox2.Text += Environment.NewLine + "Sub Processe of : " + r["subProcessOf"].ToString().Substring(9) + Environment.NewLine;
                        }
                    }
                if (textBox2.Text == " ") {
                    textBox2.Text = "Title = Power Prediction Model for Hydro Turbine" + Environment.NewLine; ;
                    textBox2.Text +=Environment.NewLine+ "Description = KB of for the process and sub process of the model";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
        }
    }
}

       
     

