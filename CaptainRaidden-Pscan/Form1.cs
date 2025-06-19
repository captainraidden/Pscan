using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace Pscan
{
    public partial class Form1 : Form
    {
        private readonly List<int> commonPorts = new List<int>()
        {
            21, 22, 23, 25, 53, 80, 110, 139, 143, 443,
            445, 3306, 3389, 8080, 8443, 25565, 27015
            // add more ports if u want :)
        };

        private int loadingProgress = 0;
        private string targetIP;
       

        public Form1()
        {
            InitializeComponent();

            
            guna2ProgressBar1.Minimum = 0;
            guna2ProgressBar1.Maximum = 100;
            guna2ProgressBar1.Value = 0;
           
            loadingTimer.Interval = 100;
            loadingTimer.Tick += LoadingTimer_Tick;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();

            targetIP = txtIP.Text.Trim();
            if (string.IsNullOrEmpty(targetIP))
            {
                MessageBox.Show("Please enter a valid IP address.");
                return;
            }

            guna2Button1.Enabled = false;
            guna2ProgressBar1.Value = 0;
            guna2ProgressBar1.Visible = true;
            loadingProgress = 0;

            loadingTimer.Stop(); 
            loadingTimer.Start(); 
        }

        private void LoadingTimer_Tick(object sender, EventArgs e)
        {
            loadingProgress += 3;

            if (loadingProgress >= 100)
            {
                loadingTimer.Stop();

              
                guna2ProgressBar1.Value = 0;
                guna2Button1.Enabled = true;

                foreach (int port in commonPorts)
                {
                    int currentPort = port;
                    ThreadPool.QueueUserWorkItem(_ => ScanPort(targetIP, currentPort));
                }
            }
            else
            {
                guna2ProgressBar1.Value = loadingProgress;
            }
        }

        private void ScanPort(string ip, int port)
        {
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    client.ReceiveTimeout = 1000;
                    client.SendTimeout = 1000;
                    client.Connect(ip, port);

                    AddResult(port, "Open", GetServiceName(port));
                }
                catch
                {
                    // Port closed - ignore
                }
            }
        }

        private void AddResult(int port, string status, string service)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddResult(port, status, service)));
                return;
            }

            // Prevent duplicate row entries for the same port
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value.ToString() == port.ToString())
                    return;
            }

            dataGridView1.Rows.Add(port.ToString(), status, service);
        }

        private string GetServiceName(int port)
        {
            Dictionary<int, string> services = new Dictionary<int, string>()
            {
                { 21, "FTP" }, { 22, "SSH" }, { 23, "Telnet" }, { 25, "SMTP" },
                { 53, "DNS" }, { 80, "HTTP" }, { 110, "POP3" }, { 139, "NetBIOS" },
                { 143, "IMAP" }, { 443, "HTTPS" }, { 445, "SMB" },
                { 3306, "MySQL" }, { 3389, "RDP" }, { 8080, "HTTP-Proxy" },
                { 8443, "HTTPS Alt" }, { 25565, "Minecraft" }, { 27015, "Source Engine" }
            };

            return services.ContainsKey(port) ? services[port] : "Unknown";
        }

        private void copyPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var row = dataGridView1.SelectedRows[0];
                string port = row.Cells[0].Value?.ToString();

                Clipboard.SetText(port);
                MessageBox.Show($"Port {port} copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please select a port row to copy.");
            }
        }

        private void copyAllPortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("No ports to copy.");
                return;
            }

            List<string> ports = new List<string>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                string port = row.Cells[0].Value?.ToString();
                if (!string.IsNullOrEmpty(port))
                    ports.Add(port);
            }

            if (ports.Count > 0)
            {
                Clipboard.SetText(string.Join(Environment.NewLine, ports));
                MessageBox.Show("All ports copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No valid ports found.");
            }
        }


        private void clearOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
        }
    }
}
