#define COM_PORT
#define INVALID_THREAD

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO.Ports;

namespace SerialPort_BT
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            serialPort1.Encoding = Encoding.GetEncoding("gb2312");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int comboBoxcnt;
            //SearchAndAddSerialToComboBox

#if (COM_PORT)
            string comboBoxPortStr;
            for (comboBoxcnt = 0x00; comboBoxcnt <= 20; comboBoxcnt++)
            {
                comboBoxPortStr = "COM" + comboBoxcnt.ToString();
                comboBox1.Items.Add(comboBoxPortStr);
            }
            comboBox1.Text = "COM1";
            comboBox2.Text = "9600";

            textBox1.Text = "";
#else
            string comboBoxStr;
            for (comboBoxcnt = 0x00; comboBoxcnt <= 0xFF; comboBoxcnt++)
            {
                comboBoxStr = comboBoxcnt.ToString("x").ToUpper();
                if (comboBoxStr.Length == 1)
                    comboBoxStr = "0" + comboBoxStr;
                comboBoxStr = "0x" + comboBoxStr;
                comboBox1.Items.Add(comboBoxStr);
            }
#endif
        }

#if (NONE_USED)
        private void SearchAndAddSerialToComboBox(SerialPort MyPort, ComboBox MyBox)
        {
            string[] mystring = new string[20];
            string[] buff = new string[1];//扫描可用串口程序
            string buffer;
            int count = 0;
            MyBox.Items.Clear();
            for (int i = 1; i < 20; i++)
            {
                try
                {
                    buffer = "COM" + i.ToString();
                    MyPort.PortName = buffer;
                    MyPort.Open();
                    mystring[i - 1] = buffer;
                    count++;
                    if (count == 1)//寻找第一个找的串口
                    {
                        buff[0] = buffer;
                    }
                    MyBox.Items.Add(buffer);//把可用的串口添加到列表中
                    MyPort.Close();
                }
                catch
                {
                }
            }
            MyBox.Text = buff[0];//把寻找到的第一个串口名称添加到列表当前显示
        }
#endif

        /*
         * * 若需在不同執行緒下控制元件，會遇到「跨執行緒控制無效…」的問題
         * * https://dotblogs.com.tw/shinli/2015/04/16/151076
         * * 目前參考網路上的解法有二：
         * * 1. 直接對 Form 的屬性作改變，此法較不安全，若程式很單純不複雜，以此解即可：
         * *    Form.CheckForIllegalCrossThreadCalls = False
         * * 2. 採委派的方式，此法較為正統，但撰寫上較為複雜：
         * *    as below
         */

#if (INVALID_THREAD)
        private delegate void UpdateUICallBack(string value);
        private void UpdateUI(string value)
        {
            if (this.InvokeRequired)
            {
                UpdateUICallBack uu = new UpdateUICallBack(UpdateUI);
                this.Invoke(uu, value);
            }
            else
            {
                //ctl.Text = value;
                //textBox1.AppendText(value);
                value = (value.Length == 1) ? "0" + value : value;
                if (value.CompareTo("\r\n") != 0)
                    value += " ";
                textBox1.AppendText(value);
            }
        }
#else
        private delegate void UpdateUICallBack(string value, Control ctl);
        private void UpdateUI(string value, Control ctl)
        {
            if (this.InvokeRequired)
            {
                UpdateUICallBack uu = new UpdateUICallBack(UpdateUI);
                this.Invoke(uu, value, ctl);
            }
            else
            {
                //ctl.Text = value;
                textBox1.AppendText(value);
            }
        }
#endif

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (radioButton1.Checked) /*String*/
            {
                //textBox1.AppendText (serialPort1.ReadExisting());
#if (INVALID_THREAD)
                UpdateUI(serialPort1.ReadExisting());
#else
                UpdateUI(serialPort1.ReadExisting(), textBox1);
#endif
            }
            else  /*Hex*/
            {
                byte[] buff = new byte[serialPort1.BytesToRead];

                serialPort1.Read(buff, 0, serialPort1.BytesToRead);
                foreach (byte item in buff)    //读取Buff中存的数据
                {
                    String recvData = Convert.ToString(item).ToUpper();
#if (INVALID_THREAD)
                    UpdateUI(recvData);
#else
                    UpdateUI(serialPort1.ReadExisting(), textBox1);
#endif
                }
            }

            UpdateUI("\r\n");
        }

        private void BtnClean_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void BtnOpenPort_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);

                serialPort1.Open();
                btnOpenPort.Enabled = false;
                btnClosePort.Enabled = true;
            }

            catch
            {
                MessageBox.Show("串口設置錯誤", "Serial Error");
            }
        }

        private void BtnClosePort_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();
                btnOpenPort.Enabled = true;
                btnClosePort.Enabled = false;
            }
            catch
            {

            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.ScrollToCaret();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.WriteLine(textBox2.Text);
            }
            catch (Exception err)
            {
                if (!serialPort1.IsOpen)
                {
                    MessageBox.Show("請開啟串口", "Serial Error");
                }
                else
                {
                    serialPort1.Close();
                    btnOpenPort.Enabled = true;
                    btnClosePort.Enabled = false;

                    MessageBox.Show("串口寫入錯誤", "Serial Error");
                }
            }
        }
    }
}
