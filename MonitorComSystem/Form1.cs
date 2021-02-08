using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Globalization;
using System.Data.Common;
using System.Data.SQLite;

namespace MonitorComSystem
{
    public partial class Form1 : Form
    {
        static SerialPort _serialPort;
        private DataBase BD = new DataBase("../db.sqlite3");
        private delegate void SetTextDeleg(string text);
        private DHTDATA DATA = new DHTDATA();
        private int NODEMAC;
        public enum commands {MAC, END, BEGIN, TEMP, HUM, NONE, ERROR, MACNODE};
        public commands COM = commands.NONE;
        private void si_DataReceived(string data)
        {
            
            if (data.Trim() == "MAC")
            {
                COM = commands.MAC;                
            }
            else if (data.Trim() == "BEGIN_PACKAG")
            {
                COM = commands.BEGIN;
                DATA.Clear();
            }
            else if (data.Trim() == "END_PACKAG")
            {
                COM = commands.END;
                listBox1.Items.Add(DateTime.Now.ToString() + "----" + DATA);
                listBox1.TopIndex = listBox1.Items.Count - (int)(listBox1.Height / listBox1.ItemHeight);
                BD.InsertData(DATA.ToString(), 100, NODEMAC);
                FillSelect();
            }
            else if (data.Trim() == "TEMP")
            {
                COM = commands.TEMP;               
            }
            else if (data.Trim() == "HUM")
            {
                COM = commands.HUM;                
            }
            else if (data.Trim() == "ERROR")
            {
                COM = commands.ERROR;
            }
            else if (data.Trim() == "MACNODE")
            {
                COM = commands.MACNODE;
            }
            else
            {
                switch (COM)
                {
                    case commands.MAC:
                        label3.Text = data.Trim();
                        break;
                    case commands.HUM:
                        DATA.hum = float.Parse(data.Trim(), CultureInfo.InvariantCulture.NumberFormat);

                        break;
                    case commands.TEMP:
                        DATA.temp = float.Parse(data.Trim(), CultureInfo.InvariantCulture.NumberFormat);

                        break;
                    case commands.ERROR:
                        listBox1.Items.Add(DateTime.Now.ToString() + "-- ERROR --" + data.Trim());
                        listBox1.TopIndex = listBox1.Items.Count - (int)(listBox1.Height / listBox1.ItemHeight);

                        break;
                    case commands.MACNODE:
                        NODEMAC = Convert.ToInt32(data.Trim());
                        break;
                }
                COM = commands.NONE;
            }
                        
            
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                
                string data = _serialPort.ReadLine();
                this.BeginInvoke(new SetTextDeleg(si_DataReceived),
                                    new object[] { data });
            }
            catch (Exception)
            {
               
            }         
        }

        private void ConnectSerial()
        {
            try
            {
                
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Handshake = Handshake.None;
                    
                    _serialPort.ReadTimeout = 500;
                    _serialPort.WriteTimeout = 500;

                    _serialPort.BaudRate = 9600;
                    _serialPort.Parity = Parity.None;
                    _serialPort.DataBits = 8;
                    _serialPort.StopBits = StopBits.One;
                    _serialPort.PortName = comboBox1.Text;
                    _serialPort.Open();
                    progressBar1.Value = 100;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(ports);
            _serialPort = new SerialPort();
            try
            {
               comboBox1.SelectedIndex = 0;
            }
            catch (Exception)
            {
                MessageBox.Show("Пожалуйста убедитесь что оборудование подключено", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            ConnectSerial();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ConnectSerial();
        }
        

        private void button2_Click(object sender, EventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                progressBar1.Value = 0;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                progressBar1.Value = 0;
                ConnectSerial();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void refreshcom_Click(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(ports);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FillSelect();
        }

        private void FillSelect()
        {
            dataGridView1.Rows.Clear();
            List<Node> list = BD.SelectGateway(label3.Text);
            for (int i = 0; i < list.Count; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = list[i].Discription;
                dataGridView1.Rows[i].Cells[1].Value = list[i].Mac;
                dataGridView1.Rows[i].Cells[2].Value = list[i].Type;
            }
        }
    }
}
