using SerialBusProcessor;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace test
{
    public partial class Form1 : Form
    {
        SerialBusProcessor.SerialBusProcessor sbp;
        Thread[] testthreads;
        int[] volt;
        int[] curr;
        byte[][] recvcmdbuf;
        EventWaitHandle[] cmdwait;
        HWCommandAnalyzer analyzer;
        byte flag = 0;
        public Form1()
        {
            testthreads = new Thread[100];
            volt = new int[5];
            curr = new int[5];
            recvcmdbuf = new byte[100][];
            analyzer = new HWCommandAnalyzer();
            analyzer.SingleQuiryResponse_Received += Analyzer_SingleQuiryResponse_Received;
            analyzer.RxModeResponse_Received += Analyzer_RxInitResponse_Received;
            analyzer.ConfigLoadResponse_Received += Analyzer_ConfigLoadResponse_Received;
            analyzer.ControlError_Received += Analyzer_ControlError_Received;
            analyzer.ReceivedPower8b_Received += Analyzer_ReceivedPower8b_Received;
            analyzer.SingleQuiryPowersupplyResponse_Received += Analyzer_SingleQuiryPowersupplyResponse_Received ;
            analyzer.PowersupplyConfigResponse_Received += Analyzer_PowersupplyConfigResponse_Received;
            cmdwait = new EventWaitHandle[100];
            for(int i = 0;i<100;i++)
            {
                cmdwait[i] = new EventWaitHandle(false, EventResetMode.AutoReset);
                cmdwait[i].Reset();
            }
            SerialPort sp = new SerialPort("COM12", 115200, Parity.None, 8, StopBits.One);

            sbp = new SerialBusProcessor.SerialBusProcessor(this, 256, sp);
            sbp.SerialCommandArrival += Sbp_SerialCommandArrial;
            sbp.SerialDataArrival += Sbp_SerialDataArrival;

            InitializeComponent();
            sbp.OpenSerialBusProcessor();
        }
        int iii = 0;
        private void Analyzer_PowersupplyConfigResponse_Received(HWCommandAnalyzer dca, int index)
        {
            Console.WriteLine( string.Format("power config resp{0}",iii++));
            flag = 0x0e;
        }

        private void Analyzer_SingleQuiryPowersupplyResponse_Received(HWCommandAnalyzer dca, int index)
        {
            int volt = dca.PowersupplyResp.volt_h * 256 + dca.PowersupplyResp.volt_l;
            int curr = dca.PowersupplyResp.curr_h * 256 + dca.PowersupplyResp.curr_l;
            Console.WriteLine (string.Format( "power single quriy resp.Volt:{0},Curr:{1}",volt,curr));
            flag = 0x0e;

        }

        private void Analyzer_ConfigLoadResponse_Received(HWCommandAnalyzer dca, int index)
        {
            //MessageBox.Show((dca.ConfigLoadResp.curr_h * 256 + dca.ConfigLoadResp.curr_l).ToString());
        }

        private void Analyzer_ReceivedPower8b_Received(HWCommandAnalyzer dca,int index)
        {
            richTextBox1.AppendText("RX"+index.ToString()+":"+"Received Power:" + dca.RP8Packet.QiRP.ToString() + "\n");
        }

        private void Analyzer_ControlError_Received(HWCommandAnalyzer dca,int index)
        {
            richTextBox1.AppendText("RX" + index.ToString() + ":" + "Control Error:" + ((sbyte)dca.CEPacket.QiCE).ToString() + "\n");
            richTextBox1.AppendText("Current:" + (dca.CEPacket.Iout_h * 256 + dca.CEPacket.Iout_l).ToString() + "\n");
        }

        private void Analyzer_RxInitResponse_Received(HWCommandAnalyzer dca,int index)
        {
            Console.WriteLine(((RxMode)(dca.RxModeResp.mode)).ToString());
        }

        private void Analyzer_SingleQuiryResponse_Received(HWCommandAnalyzer dca,int index)
        {
            MessageBox.Show("Load" + dca.SingleQuiryResult.channel.ToString() + "-Current:" + (dca.SingleQuiryResult.curr_h * 256 + dca.SingleQuiryResult.curr_l).ToString()+
                            "Load" + dca.SingleQuiryResult.channel.ToString() + "-Voltage:" + (dca.SingleQuiryResult.volt_h * 256 + dca.SingleQuiryResult.volt_l).ToString());
        }

        private T2 Converter<T1, T2>(T1 ch)
        {
            throw new NotImplementedException();
        }

        private void Sbp_SerialDataArrival(int ch, byte[] command, int length)
        {
            string printf = string.Format("channel:{0:d}\n", ch);
            printf += "recv:";
            for (int i = 0; i < length; i++)
            {
                printf += command[i].ToString("X2");
            }
            printf += "\n";
            analyzer.Analyzer(ch,command,length);
            flag |=(byte)(1 << (ch - 5)); ;
            Console.WriteLine(printf);


        }

        private delegate void ControlTextChange(string s);
        void richboxtext_update(string s)
        {
            if(richTextBox1.InvokeRequired == true)
            {
                ControlTextChange ctc = new ControlTextChange(richboxtext_update);
                this.Invoke(ctc,new object[] { s });
            }
            else
            {
                richTextBox1.Text += s;
            }
        }
        private void getpowerinfo(object obj)
        {

        }
        private delegate void SingleParamDelegate(object obj);
        private void test_thread(object ch)
        {
            while(true)
            {
                getpowerinfo(ch);
            }
        }
        private void Sbp_SerialCommandArrial(int ch, byte[] command, int length)
        {
            string printf = string.Format("channel:{0:d}, cmd:", ch);
            for (int i = 0; i < length; i++)
            {
                printf += " 0x"+command[i].ToString("X2");
            }
            printf += "\n";
            richTextBox1.Text += printf;
            int header = command[0];
            cmdwait[ch].Set();
            recvcmdbuf[ch] = command;
            switch (header)
            {
                case 0x90:
                    break;
                default: break;
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //sbp.CloseSerialBusProcessor();
            //foreach (var v in testthreads)
            //{
            //    if (null != v)
            //    {
            //        v.Abort();
            //    }
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //byte[] cmdbuf = new byte[] { 0x00, };
            ////sbp.Send_Command(0x4b,cmdbuf, 6);
            //var task = new Task(() =>
            //{
            //    do
            //    {

            //        while (cmdwait[0x4b].WaitOne(1000) == false)
            //            continue;
            //    } while (recvcmdbuf[0x4b][0] != 0xcc);
            //    Console.Out.WriteLine("ack receive.");
            //});
            //task.Start();
            sbp.CloseSerialBusProcessor();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.SelectionLength = 0;
            richTextBox1.ScrollToCaret();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sbp.OpenSerialBusProcessor();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //byte[] ar = new byte[] { 0x5A, 0xA5, 0x00, 0x13 ,0x10 ,0x50, 0x43 ,0x00 ,0x00 ,0x02, 0x01 ,0x00 ,0x00 ,0x00 ,0x0D ,0x06 ,0x1E ,0x2C ,0x62 };
            byte[] ar = new byte[] { 0x5A, 0xA5, 0x00, 0x11, 0x10, 0x50, 0x43, 0x00, 0x00, 0x01, 0x01, 0x01, 0x13, 0x88, 0x01, 0x26, 0x53 };
            //serialPort1.Write(ar, 0, ar.Length);
            analyzer.Analyzer(60, ar, ar.Length);
            //sbp.Send_Data(5, ar, ar.Length);

        }

        private async void button4_Click(object sender, EventArgs e)
        {
            byte[] ar;
            ar = analyzer.RxConfigMode(RxMode.BPP, 0);
            //sbp.Send_Data(1, ar, ar.Length);
            //Thread.Sleep(2000);
            //ar = analyzer.SingleQuiryPowersupply(PowersupplyChannel.Channel3);
            int i = 0;
            for (; i <100; i++)
            {
                for (int j = 5; j < 10; j++)
                {
                    sbp.Send_Data(j, ar, ar.Length);
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Send:");
                    foreach (var b in ar)
                    {
                        sb.Append(b.ToString("X02"));
                    }
                    richTextBox1.Text += sb.ToString() + '\n';
                }
                await Task.Run(new Action(() => { while (flag != 0x1f) ; flag = 0; }));
                Console.WriteLine(string.Format("count:{0}\n", i));
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //sbp.reboot(1, false);
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BIN File| *.bin";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                sbp.UpdateFirmware(ofd.FileName,0);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BIN File| *.bin";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                sbp.UpdateFirmware(ofd.FileName,1);
            }
        }
        private void task()
        {
            DateTime start;
            while (threadkeep)
            {
                start = DateTime.Now;
                while ((DateTime.Now - start).TotalMilliseconds < 1000) ;
                this.Invoke(onInc);
            }
        }
        delegate  void INC();
        event INC onInc;
        void inc()
        {
            tim++;
        }
        int tim = 0;
        bool threadkeep = true;
        private void button7_Click(object sender, EventArgs e)
        {
            threadkeep = true;
            onInc += inc;
            Thread t = new Thread(new ThreadStart(task));
            t.Start();
            Thread.Sleep(10000);
            threadkeep = false;
            onInc -= inc;
            MessageBox.Show($"Tim:{tim}");

        }
    }
}
