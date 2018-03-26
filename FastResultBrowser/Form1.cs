using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FastResultBrowser
{
    public partial class frmMain : Form
    {
        DataTable dt;

        /*
         * Time TotWindV    WindVxi WindVyi WindVzi HorWindV HorWndDir VerWndDir
         * RotPwr RotTorq RotThrust RotCp   RotCq RotCt
         * LSSTipPxa LSSTipVxa   LSSTipAxa
         * TipDxc1 TipDyc1 TipDzc1 TipDxb1 TipDyb1 TipALxb1 TipALyb1 TipALzb1 TipRDxb1 TipRDyb1 TipClrnc1
         * Spn1ALxb1 Spn2ALxb1   Spn3ALxb1 Spn1ALyb1   Spn2ALyb1 Spn3ALyb1   Spn1ALzb1 Spn2ALzb1   Spn3ALzb1
         * PtchPMzc1
         * RootFxc1 RootFyc1    RootFzc1 RootFxb1    RootFyb1 RootMxc1    RootMyc1 RootMzc1    RootMxb1 RootMyb1
         * Spn1MLxb1 Spn2MLxb1   Spn3MLxb1 Spn1MLyb1   Spn2MLyb1 Spn3MLyb1   Spn1MLzb1 Spn2MLzb1   Spn3MLzb1
         */

        public frmMain()
        {
            InitializeComponent();
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFSTFileToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            FeedTreeView();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OpenFSTFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var lineCount = System.IO.File.ReadLines(openFileDialog1.FileName).Count()-8;

                if (DialogResult.Yes == MessageBox.Show(this, $"Really import {lineCount} lines?","Sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    new ImportToDatabase(openFileDialog1.FileName, lineCount).ShowDialog(this);
                    FeedTreeView();
                }
            }
        }

        private void FeedTreeView()
        {
            treeView1.Nodes.Clear();

            foreach (DataColumn item in SQLiteClient.GetTableColumns())
            {
                if(item.ColumnName != "Time")
                    treeView1.Nodes.Add(new TreeNode(item.ColumnName));
            }

            treeView1.SelectedNode = treeView1.Nodes[0];

        }

        private void ChartDataBind(string columnName)
        {
            chart1.Series.Clear();
            chart1.DataBindTable(SQLiteClient.QueryTable(columnName).DefaultView, "Time");
            chart1.Series[columnName].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ChartDataBind(treeView1.SelectedNode.Text);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            if(null != dt)
                dt.Clear();

            string line = string.Empty; string[] aColumns = null; string[] aUnits = null;
            int lineNo = 1;
            using (var sr = new System.IO.StreamReader(@"D:\works\wind\2500kW\DUdesignR\FAST_simulation\fast.out"))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (lineNo == 7)
                    {
                        aColumns = line.Split('\t').ToArray();
                    }
                    if (lineNo == 8)
                    {
                        aUnits = line.Split('\t').ToArray();

                        dt = new DataTable("t1");

                        int j = 0;
                        foreach (var item in aColumns)
                        {
                            dt.Columns.Add($"{item} - {aUnits[j]}", typeof(Double));
                            treeView1.Nodes.Add(new TreeNode($"{item} - {aUnits[j]}"));

                            j++;
                        }

                        DataColumn[] PrimaryKeyColumns = new DataColumn[1];
                        PrimaryKeyColumns[0] = dt.Columns["Time"];
                        dt.PrimaryKey = PrimaryKeyColumns;


                    }
                    if (lineNo >= 9)
                    {
                        var aValues = line.Split('\t').ToArray();

                        var dr = dt.NewRow();

                        int k = 0;

                        foreach (var value in aValues)
                        {
                            dr[k] = Decimal.Parse(value, System.Globalization.NumberStyles.Float);
                            k++;
                        }
                        dt.Rows.Add(dr);
                    }
                    lineNo++;
                }


            }


            chart1.Series.Clear();
            chart1.DataBindTable(dt.DefaultView);
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

        }
    }
}
