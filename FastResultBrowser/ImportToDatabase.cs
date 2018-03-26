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
    public partial class ImportToDatabase : Form
    {
        string m_filePath = string.Empty;
        int m_recordCount = 0;

        public ImportToDatabase(string filePath, int recordCount)
        {
            InitializeComponent();
            m_filePath = filePath; m_recordCount = recordCount;
            groupBox1.Text = $"File Path: { m_filePath}";
            progressBar1.Maximum = m_recordCount;
            resultLabel.Text = $"Record Count: 0 / {m_recordCount}";

            if (backgroundWorker1.IsBusy != true)
            {
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
            }

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy != true)
            {
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                backgroundWorker1.CancelAsync();
            }

            this.Close();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            string line;
            int counter = 0;
            var sr = new System.IO.StreamReader(m_filePath);
            bool insertMode = false;
            while ((line = sr.ReadLine()) != null)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    counter++;
                    if (line.Length > 4)
                    {
                        if ("Time" == line.Substring(0, 4))
                        {
                            try
                            {
                                DeleteCreateTable(line);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                            
                            insertMode = true;
                        }
                        if (insertMode)
                        {
                            try
                            {
                                InsertIntoTable(line);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }

                            worker.ReportProgress(counter);
                        }
                    }

                    //if (100 == counter)
                    //    break;
                }
            }

            sr.Close();
        }


        private void DeleteCreateTable(string headerLine)
        {
            var queryPart = string.Empty;

            var aColumns = headerLine.Split('\t').ToArray();

            foreach (var column in aColumns)
            {
                queryPart += column + " REAL,";
            }

            queryPart = queryPart.Substring(0, queryPart.Length - 1);

            var queryString = $"CREATE TABLE t1({queryPart});";

            try
            {
                SQLiteClient.DropTable("t1");
            }
            catch (Exception)
            {
                // supress the error
                // throw;
            }

            try
            {
                SQLiteClient.CreateTable(queryString);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void InsertIntoTable(string dataLine)
        {
            var check = dataLine.Substring(0, 4);

            if (("Time" == check) || ("(s)\t" == check))
                return;

            var aValues = dataLine.Split('\t').ToArray();
            var queryPart = string.Empty;
            decimal d = 0.0M;

            foreach (var value in aValues)
            {
                d = Decimal.Parse(value, System.Globalization.NumberStyles.Float);
                queryPart += d + ",";
            }

            queryPart = queryPart.Substring(0, queryPart.Length - 1);

            var queryString = $"INSERT INTO t1 VALUES({queryPart});";

            try
            {
                SQLiteClient.InsertIntoTable(queryString);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            double progressValue = ((double)e.ProgressPercentage / (double)m_recordCount) * 100;



            resultLabel.Text = $"Record Count: {e.ProgressPercentage} / {m_recordCount} - {  progressValue.ToString("F4")  }%";
            //resultLabel.Text = (e.ProgressPercentage.ToString() + "%");
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnCancel.Text = "Close";

            if (e.Cancelled == true)
            {
                resultLabel.Text = "Canceled!";
                
            }
            else if (e.Error != null)
            {
                resultLabel.Text = "Error: " + e.Error.Message;
            }
            else
            {
                resultLabel.Text = "Done!";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
