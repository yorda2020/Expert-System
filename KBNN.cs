using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
namespace Sample_Ex
{
    public partial class KBNN : Form
    {
        public KBNN()
        {
            InitializeComponent();
        }
        Graph kb, kb1, kb2, kb3;
         //Global variables
        double[,] WeightMatrix = new double[4, 2]; //holds the value of matrix 
        int[] y = new int[14]; //holds the value of each neuron
        int[] b = new int[14]; // holds value of each bias
        delegate double Del(string str);
        delegate double[] Dele(string str);
        ScriptEngine engine = Python.CreateEngine();
        ScriptScope scope;
        private void Form2_Load(object sender, EventArgs e)
        {
            kb = new Graph();
            kb1 = new Graph();
            kb2 = new Graph();
            kb3 = new Graph();
            Notation3Parser np = new Notation3Parser();
             np.Load(kb, @"Yordanos_KB.ttl");//KBANN
            np.Load(kb1, @"Formulae.ttl");
            np.Load(kb2, @"Rules.ttl");
            np.Load(kb3, @"Inconsistency.ttl");//Incon
       

            //Tree view implementation


            treeView1.Nodes.Add("Medical Diagonsis Expert System for Imacted tooth");//root of the structure
            string targetNode = "DiagonsisProcess";

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
                while (stack != null || i == 0)
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
                                treeView1.Nodes[0].Nodes.Add(node);
                            }
                            else if (level == 1)
                            {
                                treeView1.Nodes[0].Nodes[index[0]].Nodes.Add(node);
                            }
                            else if (level == 2)
                            {
                                treeView1.Nodes[0].Nodes[index[0]].Nodes[index[1]].Nodes.Add(node);
                            }
                            else if (level == 3)
                            {
                                treeView1.Nodes[0].Nodes[index[0]].Nodes[index[1]].Nodes[index[2]].Nodes.Add(node);
                            }
                        }
                    }
                    if (y == 0)
                    {
                        x--; index[level - 1]--;
                    }
                    else
                    {
                        index[level] = y;
                        index[level]--; x = index[level]; level += 1;
                    }
                    while (x == -1) { level--; index[level - 1]--; x = index[level - 1]; }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            textBox2.Text = "Click the treeview to show the KB information... " + Environment.NewLine;


            ///////////////////////////
            ///KBNN
            //textBox3.Text = " ";
            //calling of methods and generate the weight matrix based on the KB
            double[] w1 = getWeightWithInInterval("ind:patientAgeNormalPredict"); // calling method
            WeightMatrix[0, 0] = w1[0];
            WeightMatrix[1, 0] = w1[1];
            double[] w2 = getWeightOutOfInInterval("ind:patientAgeAlarmPredict");
            WeightMatrix[2, 0] = w2[0];
            WeightMatrix[3, 0] = w2[1];
            double[] w3 = getWeightWithInInterval("ind:sizeNormalPredict");
            WeightMatrix[0, 1] = w3[0];
            WeightMatrix[1, 1] = w3[1];
            double[] w4 = getWeightOutOfInInterval("ind:sizeAlarmPredict");
            WeightMatrix[2, 1] = w4[0];
            WeightMatrix[3, 1] = w4[1];
            //Displaying the result 
            //textBox3.Text += "[" + WeightMatrix[0, 0]+"]" + Environment.NewLine + "["+ WeightMatrix[1, 0] + "]" + Environment.NewLine + "["+ WeightMatrix[2, 0] + "]" + Environment.NewLine + "["+ WeightMatrix[3, 0] + "]"  + Environment.NewLine;
            // textBox6.Text += "[" + WeightMatrix[0, 1] + "]" + Environment.NewLine + "["+ WeightMatrix[1, 1] + "]" + Environment.NewLine + "["+ WeightMatrix[2, 1] + "]" + Environment.NewLine + "["+ WeightMatrix[3, 1] + "]"  + Environment.NewLine;
            //textBox8.Text += "["+ -1 + "]" + Environment.NewLine + "["+ 1 + " ]" + Environment.NewLine + "["+1 + " ]" + Environment.NewLine +"["+ -1 + "]";
            //textBox7.Text += "["+ -1 + "]" + Environment.NewLine + "["+ 1 + " ]" + Environment.NewLine + "["+ 1 + " ]" + Environment.NewLine + "["+ - 1 + "]";
            //For activateing Nodes of hiden layer2 based on the above node of hidden layer1
            //w21=1.5, w22=1.5, w23=2,w24=3
            //  textBox10.Text += "[" + 1.5 + " ]"+ Environment.NewLine + "[" + 1.5 + " ]";
            // textBox24.Text += "[  " + 2 + " ]" + Environment.NewLine + "[  " + 3 + " ]";
            b[8] = 2; b[9] = -1;
            // textBox2.Text += "[" + -2 + "]" ;
            // textBox23.Text += "[" + -1 + "]";
            // textBox11.Text += "[" + 1.5 + " ]" + Environment.NewLine + "[" + 1.5 + " ]";
            // textBox27.Text += "[ " + 2 + " ]" + Environment.NewLine + "[ " + 3 + " ]";
            b[10] = -2; b[11] = -1;
            //w25=1.5, w26=1.5, w27=2,w28=3
            //textBox9.Text += "[" + -1 + "]"; textBox26.Text += "[" + -1 + "]";
            //For activateing Nodes of output layer based on the above node of hidden layer2
            int[,] weight3 = { { 1, 1 }, { 2, 2 } };
            int[] baisout = { -1, -1 };
            b[12] = -1; b[13] = -1;
            // displaying weight 
            // textBox12.Text += "[" + weight3[0,0] +  "]" + Environment.NewLine + "[" + weight3[1,0] +  "]";
            // textBox21.Text += "[" + weight3[0, 1] +  "]" + Environment.NewLine + "["  + weight3[1, 1] + "]";
            //textBox5.Text += "[" + baisout[0] +  "]" ;
            // textBox20.Text += "[" + baisout[1] + "]";
        }


        ///////////////////////TreeView///////////////////////////////////////

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView1.Refresh();
            textBox3.Text = " ";
            String item = treeView1.SelectedNode.Text.ToString();
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
                        textBox2.Text += Environment.NewLine + "Label: " + r["label"].ToString() + Environment.NewLine;
                        textBox2.Text += Environment.NewLine + "KPI: " + r["hasKPI"].ToString().Substring(9) + Environment.NewLine;
                        textBox2.Text += Environment.NewLine + "Comment: " + r["comment"].ToString() + Environment.NewLine;
                        textBox2.Text += Environment.NewLine + "Sub Processe of : " + r["subProcessOf"].ToString().Substring(9) + Environment.NewLine;
                    }
                }
                if (textBox2.Text == " ")
                {
                    textBox2.Text = "Title = Medical Diagonosis Expert System for Impacted toot" + Environment.NewLine; ;
                    textBox2.Text += Environment.NewLine + "Description = KB of for the process and sub process of the model";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        ///////////////////////KBANN///////////////////////////////////////
        ///
        public double [] getWeightWithInInterval(string state)
            {
                double[] wt = new double[2];
                double min1, max1;
                String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix xsd: <http://www.w3.org/2001/XMLSchema#> 
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:>

                       SELECT  ?KPI ?min ?max
                             WHERE
                                 {
                                  " + state + @" prop:isResultOf ?interval1.
                                  ?interval1 prop:belongTo ?KPI;
                                             prop:hasMin ?min;
                                             prop:hasMax ?max.
                                   }";
            try
                {
                    SparqlResultSet rs = (SparqlResultSet)kb.ExecuteQuery(q1);
                    foreach (SparqlResult r in rs)
                    {
                        min1 = double.Parse(r["min"].ToString().Substring(0, r["min"].ToString().IndexOf('^')));
                        max1 = double.Parse(r["max"].ToString().Substring(0, r["min"].ToString().IndexOf('^')));
                        if(state== "ind:patientAgeNormalPredict")
                        {
                        //y1=1, if w1*x1+b1>=0, w1 is wt[0]     for input x1
                         b[0] = -1;    
                        wt[0] = Math.Round((1 / min1), 3);
                        //y2=1, if w2*x1+b2>=0, w2 is wt[1]
                         b[1] = 1;
                        wt[1] = Math.Round((-1 / max1), 3);
                     }
                    else if (state == "ind:sizeNormalPredict")
                    {
                        //For the second input x2
                        //y5=1, if w5*x2+b5>=0, w5 is wt[0]
                         b[4] = -1;
                        wt[0] = Math.Round((1 / min1), 3);
                        //y6=1, if w6*x2+b6>=0, w5 is wt[1]
                        b[5] = 1;
                        wt[1] = Math.Round((-1 / max1), 3);
                    }
                }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return wt;
            }
        public double[] getWeightOutOfInInterval(string state)
        {
            double[] wt = new double[2];
            double min1, max1;
            String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix xsd: <http://www.w3.org/2001/XMLSchema#> 
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:>

                       SELECT  ?KPI ?min ?max
                             WHERE
                                 {
                                  " + state + @" prop:isResultOf ?interval1.
                                  ?interval1 prop:belongTo ?KPI;
                                             prop:hasValueLess ?min;
                                             prop:hasValueGreater ?max.
                                   }";
            try
            {
                SparqlResultSet rs = (SparqlResultSet)kb.ExecuteQuery(q1);
                foreach (SparqlResult r in rs)
                {
                    min1 = double.Parse(r["min"].ToString().Substring(0, r["min"].ToString().IndexOf('^')));
                    max1 = double.Parse(r["max"].ToString().Substring(0, r["min"].ToString().IndexOf('^')));
                    if (state == "ind:patientAgeAlarmPredict")
                    { 
                        //y3=1, if w3*x1+b3>=0, w3 is wt[0]
                         b[2] = 1;
                        wt[0] = Math.Round((-1 / min1), 3);
                        //y1=1, if w1x1+b>=0, w1 is wt[1]
                         b[3] = -1;
                        wt[1] = Math.Round((1 / max1), 3);
                        //For activateing Node y9 based on the above node y1 and y2
                        //y9=1 y1*w9+y2*w10+b9>0
                    }
                    else if (state == "ind:sizeAlarmPredict")
                    {
                        //For the second input x2
                        //y7=1, if w7*x2+b7>=0, w7 is wt[2]
                         b[6] = 1;
                        wt[0] = Math.Round((-1 / min1), 3);
                        //y8=1, if w8*x2+b8>=0, w8 is wt[3]
                         b[7] = -1;
                        wt[1] = Math.Round((1 / max1), 3);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return wt;
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox4.Text != "")
            {
                //textBox13.Text = ""; textBox14.Text = ""; textBox15.Text = ""; textBox16.Text = ""; textBox17.Text = ""; textBox18.Text = "";
                double x1 = double.Parse(textBox1.Text);
                double x2 = double.Parse(textBox4.Text);
                                //hidden layer1
                if ((WeightMatrix[0, 0] * x1 + b[0]) >= 0) { y[0] = 1; } else { y[0] = 0; }
                if ((WeightMatrix[1, 0] * x1 + b[1]) >= 0) { y[1] = 1; } else { y[1] = 0; }
                if ((WeightMatrix[2, 0] * x1 + b[2]) >= 0) { y[2] = 1; } else { y[2] = 0; }
                if ((WeightMatrix[3, 0] * x1 + b[3]) >= 0) { y[3] = 1; } else { y[3] = 0; }
                if ((WeightMatrix[0, 1] * x2 + b[4]) >= 0) { y[4] = 1; } else { y[4] = 0; }
                if ((WeightMatrix[1, 1] * x2 + b[5]) >= 0) { y[5] = 1; } else { y[5] = 0; }
                if ((WeightMatrix[2, 1] * x2 + b[6]) >= 0) { y[6] = 1; } else { y[6] = 0; }
                if ((WeightMatrix[3, 1] * x2 + b[7]) >= 0) { y[7] = 1; } else { y[7] = 0; }
                //hidden layer 2 w21=1.5,w22=1.5,w23=2, w24=3 w25=1.5, w26=1.5, w27=2 w28=3
                if ((y[0] * 1.5 + y[1] * 1.5 + b[8]) > 0) { y[8] = 1; } else { y[8] = 0; } //and operation
                if ((y[2] * 2 + y[3] * 3 + b[9]) > 0) { y[9] = 1; } else { y[9] = 0; }     //or operation
                if ((y[4] * 1.5 + y[5] * 1.5 + b[10]) > 0) { y[10] = 1; } else { y[10] = 0; }//and operation
                if ((y[6] * 2 + y[7] * 3 + b[11]) > 0) { y[11] = 1; } else { y[11] = 0; }    //or operation
                                                                                            //output layer 
                if ((y[8] * 1 + y[10] * 1 + b[12]) > 0) { y[12] = 1; } else { y[12] = 0; }
                if ((y[9] * 2 + y[11] * 2 + b[13]) > 0) { y[13] = 1; } else { y[13] = 0; }

                // Display output
               // textBox14.Text = "[" + y[0] + "]" + Environment.NewLine + "[" + y[1] + "]" + Environment.NewLine + "[" + y[2] + "]" + Environment.NewLine + "[" + y[3] + "]" + Environment.NewLine;
                //textBox15.Text = "[" + y[4] + "]" + Environment.NewLine + "[" + y[5] + "]" + Environment.NewLine + "[" + y[6] + "]" + Environment.NewLine + "[" + y[7] + "]" + Environment.NewLine;
                //textBox13.Text = "[" + y[8] + "]";
                //textBox22.Text = "[" + y[9] + "]";
                //textBox16.Text = "[" + y[10] + "]" ;
                //textBox25.Text = "[" + y[11] + "]";
               // textBox17.Text = "[" + y[12] + "]" ;
                //textBox19.Text = "[" + y[13] + "]";
                if (y[12] == 1)
                {
                    textBox18.Text = "Normal state of Defect";
                   // textBox18.BackColor = System.Drawing.Color.LightGreen;
                }
                else
                {
                    textBox18.Text = "Alarm state of Defect";
                    //textBox18.BackColor = System.Drawing.Color.Red;
                }
            }
            else
            {
                MessageBox.Show("Please insert input value", "Alert Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        ///////////////////////Formulae///////////////////////////////////////
        

       
         

        private void Formulae_Click(object sender, EventArgs e)
        {
            scope = engine.CreateScope();
            engine.ExecuteFile(@"G:\Yordi\AlemuYordanos_KBANN\Formulae1.py", scope);
            //extBox3.Text = ""; textBox6.Text = "";
            //calling of methods and generate the weight matrix based on the KB
            //For activateing Nodes of hiden layer2 based on the above node of hidden layer1
            Dele del1 = getWeight;
            double[] w1 = del1.Invoke("ind:ageNormalPredict"); // calling method
            WeightMatrix[0, 0] = w1[0];
            WeightMatrix[1, 0] = w1[1];
            double[] w2 = del1.Invoke("ind:sizeAlarmPredict");
            WeightMatrix[2, 0] = w2[0];
            WeightMatrix[3, 0] = w2[1];
            double[] w3 = del1.Invoke("ind:ageNormalPredict");
            WeightMatrix[0, 1] = w3[0];
            WeightMatrix[1, 1] = w3[1];
            double[] w4 = del1.Invoke("ind:sizeAlarmPredict");
            WeightMatrix[2, 1] = w4[0];
            WeightMatrix[3, 1] = w4[1];
            //Displaying the result 
            //textBox3.Text += "[" + WeightMatrix[0, 0] + "]" + Environment.NewLine + "[" + WeightMatrix[1, 0] + "]" + Environment.NewLine + "[" + WeightMatrix[2, 0] + "]" + Environment.NewLine + "[" + WeightMatrix[3, 0] + "]" + Environment.NewLine;
            //textBox6.Text += "[" + WeightMatrix[0, 1] + "]" + Environment.NewLine + "[" + WeightMatrix[1, 1] + "]" + Environment.NewLine + "[" + WeightMatrix[2, 1] + "]" + Environment.NewLine + "[" + WeightMatrix[3, 1] + "]" + Environment.NewLine;

            //Call the output function based on the input 
            dynamic function = scope.GetVariable("getInput");
            dynamic result = function();
            String a = result.ToString();
            double[] res = { double.Parse(a.Substring(1, a.IndexOf(",") - 1)), double.Parse(a.Substring(a.IndexOf(",") + 1, a.Length - (a.IndexOf(",") + 2))) };
            calculateOutput(res);
        }
        public double[] getWeight(string state)
        {
            double[] wt = new double[2];
            double min1, max1;
            String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix xsd: <http://www.w3.org/2001/XMLSchema#> 
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:>

                       SELECT  ?KPI ?minId ?maxId
                             WHERE
                                 {
                                  " + state + @" prop:isResultOf ?interval1.
                                  ?interval1 prop:belongTo ?KPI;
                                             prop:minformulaeId ?minId;
                                             prop:maxformulaeId ?maxId.
                                   }";
            try
            {
                SparqlResultSet rs = (SparqlResultSet)kb1.ExecuteQuery(q1);
                foreach (SparqlResult r in rs)
                {
                    Del del2 = calculateFormulae;
                    min1 = del2.Invoke(r["minId"].ToString());  //delegate to get min value from file by caling the function
                    max1 = del2.Invoke(r["maxId"].ToString());
                    //Calculation of the whieght matrix
                    if (state == "ind:ageNormalPredict")
                    {
                        //y1=1, if w1*x1+b1>=0, w1 is wt[0]     for input x1
                        b[0] = -1;
                        wt[0] = Math.Round((1 / min1), 3);
                        //y2=1, if w2*x1+b2>=0, w2 is wt[1]
                        b[1] = 1;
                        wt[1] = Math.Round((-1 / max1), 3);
                    }
                    else if (state == "ind:sizeNormalPredict")
                    {
                        //For the second input x2
                        //y5=1, if w5*x2+b5>=0, w5 is wt[0]
                        b[4] = -1;
                        wt[0] = Math.Round((1 / min1), 3);
                        //y6=1, if w6*x2+b6>=0, w5 is wt[1]
                        b[5] = 1;
                        wt[1] = Math.Round((-1 / max1), 3);
                    }
                    if (state == "ind:ageAlarmPredict")
                    {
                        //y3=1, if w3*x1+b3>=0, w3 is wt[0]
                        b[2] = 1;
                        wt[0] = Math.Round((-1 / min1), 3);
                        //y1=1, if w1x1+b>=0, w1 is wt[1]
                        b[3] = -1;
                        wt[1] = Math.Round((1 / max1), 3);
                        //For activateing Node y9 based on the above node y1 and y2
                        //y9=1 y1*w9+y2*w10+b9>0
                    }
                    else if (state == "ind:sizeAlarmPredict")
                    {
                        //For the second input x2
                        //y7=1, if w7*x2+b7>=0, w7 is wt[2]
                        b[6] = 1;
                        wt[0] = Math.Round((-1 / min1), 3);
                        //y8=1, if w8*x2+b8>=0, w8 is wt[3]
                        b[7] = -1;
                        wt[1] = Math.Round((1 / max1), 3);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return wt;
        }
        public double calculateFormulae(string formulaeId)
        {
            dynamic function = scope.GetVariable("getInterval");
            dynamic result = function(formulaeId);
            return result;
        }

        

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        public void calculateOutput(double[] input)
        {
            b[8] = -2; b[9] = -1;
            b[10] = -2; b[11] = -1;
            b[12] = -1; b[13] = -1;
            if (input[0] != 0 && input[1] != 0)
            {
                //textBox13.Text = ""; textBox14.Text = ""; textBox15.Text = ""; textBox16.Text = ""; textBox17.Text = ""; textBox18.Text = "";
                double x1 = input[0];
                double x2 = input[1];
                //hidden layer1
                if ((WeightMatrix[0, 0] * x1 + b[0]) >= 0) { y[0] = 1; } else { y[0] = 0; }
                if ((WeightMatrix[1, 0] * x1 + b[1]) >= 0) { y[1] = 1; } else { y[1] = 0; }
                if ((WeightMatrix[2, 0] * x1 + b[2]) >= 0) { y[2] = 1; } else { y[2] = 0; }
                if ((WeightMatrix[3, 0] * x1 + b[3]) >= 0) { y[3] = 1; } else { y[3] = 0; }
                if ((WeightMatrix[0, 1] * x2 + b[4]) >= 0) { y[4] = 1; } else { y[4] = 0; }
                if ((WeightMatrix[1, 1] * x2 + b[5]) >= 0) { y[5] = 1; } else { y[5] = 0; }
                if ((WeightMatrix[2, 1] * x2 + b[6]) >= 0) { y[6] = 1; } else { y[6] = 0; }
                if ((WeightMatrix[3, 1] * x2 + b[7]) >= 0) { y[7] = 1; } else { y[7] = 0; }
                //hidden layer 2 w21=1.5,w22=1.5,w23=2, w24=3 w25=1.5, w26=1.5, w27=2 w28=3
                if ((y[0] * 1.5 + y[1] * 1.5 + b[8]) > 0) { y[8] = 1; } else { y[8] = 0; } //and operation
                if ((y[2] * 2 + y[3] * 3 + b[9]) > 0) { y[9] = 1; } else { y[9] = 0; }     //or operation
                if ((y[4] * 1.5 + y[5] * 1.5 + b[10]) > 0) { y[10] = 1; } else { y[10] = 0; }//and operation
                if ((y[6] * 2 + y[7] * 3 + b[11]) > 0) { y[11] = 1; } else { y[11] = 0; }    //or operation
                                                                                             //output layer 
                if ((y[8] * 1 + y[10] * 1 + b[12]) > 0) { y[12] = 1; } else { y[12] = 0; }
                if ((y[9] * 2 + y[11] * 2 + b[13]) > 0) { y[13] = 1; } else { y[13] = 0; }

                // Display output
                // textBox14.Text = "[" + y[0] + "]" + Environment.NewLine + "[" + y[1] + "]" + Environment.NewLine + "[" + y[2] + "]" + Environment.NewLine + "[" + y[3] + "]" + Environment.NewLine;
                //textBox15.Text = "[" + y[4] + "]" + Environment.NewLine + "[" + y[5] + "]" + Environment.NewLine + "[" + y[6] + "]" + Environment.NewLine + "[" + y[7] + "]" + Environment.NewLine;
                //textBox13.Text = "[" + y[8] + "]";
                //textBox22.Text = "[" + y[9] + "]";
                // textBox16.Text = "[" + y[10] + "]";
                //textBox25.Text = "[" + y[11] + "]";
                //textBox17.Text = "[" + y[12] + "]";
                //textBox19.Text = "[" + y[13] + "]";
                if (y[12] == 1)
                {
                    textBox18.Text = "Normal state of Defect";
                   
                }
                else
                {
                    textBox18.Text = "Alarm state of Defect";
                   
                }
            }
            else
            {
                MessageBox.Show("input value is incorrect modify your file", "Alert Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Relaod_Click(object sender, EventArgs e)
        {
            this.Hide();
            KBNN main = new KBNN();
            main.Show();

        }

              

       
        ///////////////////////Inconsistency checking/////////////////////////////////////

        private void CheckIncon_Click(object sender, EventArgs e)
        {
                           textBox1.Text = " "; //
                int counter = 0;
                // inconsistency checking
                if (checkBox1.Checked == true)
                {
                    String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix owl: <http://www.w3.org/2002/07/owl#>
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:> 
                       SELECT  ?hasKPI ?process
                             WHERE
                                 {
                                  ?hasKPI rdfs:domain ?process.
                                  }";

                    SparqlResultSet rs = (SparqlResultSet)kb2.ExecuteQuery(q1);
                    String q2 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix owl: <http://www.w3.org/2002/07/owl#>
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:> 
                       SELECT  ?ind1 ?ind2
                             WHERE
                                 {
                                 ?ind1 <" + rs[0]["hasKPI"].ToString() + @"> ?ind2.
                                 FILTER EXISTS{?ind1 a <" + rs[0]["process"].ToString() + @">}
                                   }";
                    SparqlResultSet r = (SparqlResultSet)kb3.ExecuteQuery(q2);
                    foreach (SparqlResult rs1 in r)
                    {
                        if (counter == 0)
                        {
                            textBox3.Text += Environment.NewLine + "Domain Inconsistency are:" + Environment.NewLine;
                        }
                        textBox3.Text += rs1["ind1"].ToString() + "  ,  " + rs[0]["process"].ToString() + Environment.NewLine;
                        counter++;
                    }
                }
                if (checkBox3.Checked == true)
                {
                    counter = 0;
                    String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix owl: <http://www.w3.org/2002/07/owl#>
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:> 
                       SELECT  ?ind1 ?ind2 ?ind3 ?ind4
                             WHERE
                                 {
                                     ?ind1 rdf:type ?ind4.
                                     ?ind2 rdf:type ?ind4.
                                     ?ind3 a owl:Class;
                                               owl:oneOf(?ind1 ?ind2).
                                  }";

                    SparqlResultSet rs = (SparqlResultSet)kb2.ExecuteQuery(q1);
                    String q2 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                              prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                              prefix owl: <http://www.w3.org/2002/07/owl#>
                              prefix ind: <urn:inds:>
                              prefix prop: <urn:prop:>
                              prefix class: <urn:class:>
                              prefix process: <urn:process:> 
                       SELECT  ?ind1 
                             WHERE
                                 {
                                  ?ind1 rdf:type <" + rs[0]["ind3"].ToString() + @">.
                                   FILTER(?ind1!=<" + rs[0]["ind1"].ToString() + @">)
                                   FILTER(?ind1!=<" + rs[0]["ind2"].ToString() + @">)
                                  }";
                    SparqlResultSet r = (SparqlResultSet)kb3.ExecuteQuery(q2);
                    foreach (SparqlResult rs1 in r)
                    {
                        if (counter == 0)
                        {
                            textBox3.Text += Environment.NewLine + "oneOf Inconsistency are:" + Environment.NewLine;
                        }
                        textBox3.Text += rs1["ind1"].ToString() + " rdf:type -> " + rs[0]["ind4"].ToString() + Environment.NewLine;
                        counter++;
                    }
                }
                if (checkBox4.Checked == true)
                {
                    counter = 0;
                    //DisjointWith
                    String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix owl: <http://www.w3.org/2002/07/owl#>
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:> 
                       SELECT  ?process1 ?process2 
                             WHERE
                                 {
                                    ?process1 a owl:Class.
                                    ?process2 a owl:Class.
                                    ?process1 owl:disjointWith ?process2.
                                  }";

                    SparqlResultSet rs = (SparqlResultSet)kb2.ExecuteQuery(q1);
                    String q2 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                              prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                              prefix owl: <http://www.w3.org/2002/07/owl#>
                              prefix ind: <urn:inds:>
                              prefix prop: <urn:prop:>
                              prefix class: <urn:class:>
                              prefix process: <urn:process:> 
                       SELECT  ?ind1 
                             WHERE
                                 {
                                   ?ind1 a <" + rs[0]["process1"].ToString() + @"> .
                                   ?ind1 a <" + rs[0]["process2"].ToString() + @"> .
                                  }";
                    SparqlResultSet r = (SparqlResultSet)kb3.ExecuteQuery(q2);
                    foreach (SparqlResult rs1 in r)
                    {
                        if (counter == 0)
                        {
                            textBox3.Text += Environment.NewLine + "disjointWith Inconsistency are:" + Environment.NewLine;
                        }
                        textBox3.Text += " disjointWith at ->" + rs1["ind1"].ToString() + Environment.NewLine;
                        counter++;
                    }
                }

                if (checkBox6.Checked == true)//propertyDisjointWith
                {
                    counter = 0;
                    String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix owl: <http://www.w3.org/2002/07/owl#>
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:> 
                       SELECT  ?prop1 ?prop2     
                             WHERE
                                 {
                                  ?prop1 a owl:ObjectProperty.
                                  ?prop2 a owl:ObjectProperty.
                                  ?prop1 owl:propertyDisjointWith ?prop2.
                                  }";
                    SparqlResultSet rs = (SparqlResultSet)kb2.ExecuteQuery(q1);
                    String q2 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                              prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                              prefix owl: <http://www.w3.org/2002/07/owl#>
                              prefix ind: <urn:inds:>
                              prefix prop: <urn:prop:>
                              prefix class: <urn:class:>
                              prefix process: <urn:process:> 
                       SELECT  ?ind1 ?ind2 ?ind3
                             WHERE
                                 {
                                  ?ind1 <" + rs[0]["prop1"].ToString() + @"> ?ind2.
                                  ?ind1 <" + rs[0]["prop2"].ToString() + @"> ?ind2.
                                  }";
                    SparqlResultSet r = (SparqlResultSet)kb3.ExecuteQuery(q2);
                    foreach (SparqlResult rs1 in r)
                    {
                        if (counter == 0)
                        {
                            textBox3.Text += Environment.NewLine + "propertDisjointWith Inconsistency are:" + Environment.NewLine;

                        }
                        textBox3.Text += "propertyDisjointWith->" + rs1["ind1"].ToString() + Environment.NewLine;
                        counter++;
                    }
                }
                if (checkBox7.Checked == true)//IrreflexiveProperty
                {
                    counter = 0;
                    String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix owl: <http://www.w3.org/2002/07/owl#>
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:> 
                       SELECT  ?prop     
                             WHERE
                                 {
                                  ?prop a owl:ObjectProperty,
                                          owl:IrreflexiveProperty.
                                  }";
                    SparqlResultSet rs = (SparqlResultSet)kb2.ExecuteQuery(q1);
                    String q2 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                              prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                              prefix owl: <http://www.w3.org/2002/07/owl#>
                              prefix ind: <urn:inds:>
                              prefix prop: <urn:prop:>
                              prefix class: <urn:class:>
                              prefix process: <urn:process:> 
                       SELECT  ?ind1 ?ind2
                             WHERE
                                 {
                                  ?ind1 <" + rs[0]["prop"].ToString() + @"> ?ind1.
                                  }";//FILTER NOT EXISTS(?ind1 <" + rs[0]["prop"].ToString() + @"> ?ind1)
                    SparqlResultSet r = (SparqlResultSet)kb3.ExecuteQuery(q2);
                    foreach (SparqlResult rs1 in r)
                    {
                        if (counter == 0)
                        {
                            textBox3.Text += Environment.NewLine + "IrreflexiveProperty Inconsistency are:" + Environment.NewLine;
                        }
                        textBox3.Text += "IrreflexiveProperty at->" + rs1["ind1"].ToString() + Environment.NewLine;
                        counter++;
                    }
                }
                if (checkBox8.Checked == true)//AsymmetricProperty
                {
                    counter = 0;
                    String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix owl: <http://www.w3.org/2002/07/owl#>
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:> 
                       SELECT  ?prop     
                             WHERE
                                 {
                                  ?prop a owl:ObjectProperty,
                                          owl:AsymmetricProperty.
                                  }";
                    SparqlResultSet rs = (SparqlResultSet)kb2.ExecuteQuery(q1);
                    String q2 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                              prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                              prefix owl: <http://www.w3.org/2002/07/owl#>
                              prefix ind: <urn:inds:>
                              prefix prop: <urn:prop:>
                              prefix class: <urn:class:>
                              prefix process: <urn:process:> 
                          SELECT  ?ind1 ?ind2 
                             WHERE
                                 {
                                  ?ind1 <" + rs[0]["prop"].ToString() + @"> ?ind2.
                                  FILTER EXISTS{?ind2 <" + rs[0]["prop"].ToString() + @"> ?ind1.}
                                  }";
                    SparqlResultSet r = (SparqlResultSet)kb3.ExecuteQuery(q2);
                    foreach (SparqlResult rs1 in r)
                    {
                        if (counter == 0)
                        {
                            textBox3.Text += Environment.NewLine + "AsymmetricProperty Inconsistency are:" + Environment.NewLine;
                        }
                        textBox3.Text += "AsymmetricProperty->" + rs1["ind1"].ToString() + Environment.NewLine;
                        counter++;
                    }
                }
                if (checkBox9.Checked == true)//NegativePropertyAssertion
                {
                    counter = 0;
                    String q1 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                          prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                          prefix owl: <http://www.w3.org/2002/07/owl#>
                          prefix ind: <urn:inds:>
                          prefix prop: <urn:prop:>
                          prefix class: <urn:class:>
                          prefix process: <urn:process:> 
                          SELECT  ?ind1 ?ind2 ?prop     
                             WHERE
                                 {
                                  ind:np1 a owl:NegativePropertyAssertion;
                                                   owl:sourceIndividual ?ind1;
                                                   owl:assertionProperty ?prop;
                                                   owl:targetIndividual ?ind2.
                                  }";
                    SparqlResultSet rs = (SparqlResultSet)kb2.ExecuteQuery(q1);
                    String q2 = @"prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
                              prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> 
                              prefix owl: <http://www.w3.org/2002/07/owl#>
                              prefix ind: <urn:inds:>
                              prefix prop: <urn:prop:>
                              prefix class: <urn:class:>
                              prefix process: <urn:process:> 
                       SELECT   ?target
                             WHERE
                                 {
                                    <" + rs[0]["ind1"].ToString() + @"> <" + rs[0]["prop"].ToString() + @"> ?target.
                                    FILTER(?target = <" + rs[0]["ind2"].ToString() + @">)
                                    }";
                    SparqlResultSet r = (SparqlResultSet)kb3.ExecuteQuery(q2);
                    foreach (SparqlResult rs1 in r)
                    {
                        if (counter == 0)
                        {
                            textBox3.Text += Environment.NewLine + "NegativePropertyAssertion Inconsistency are:" + Environment.NewLine;
                        }
                        textBox3.Text += "NegativePropertyAssertion>" + rs1["target"].ToString() + Environment.NewLine;
                        counter++;
                    }
                }
                if (textBox3.Text == " ")
                {
                    MessageBox.Show(" There is no any inconsistency in the KB.", " Inconsistency checking", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox3.Text = "No inconsistency is found";
                }
            }

        private void textBox18_TextChanged(object sender, EventArgs e)
        {

        }



    } 
}
