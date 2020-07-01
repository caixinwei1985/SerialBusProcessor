using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Windows.Forms;
using STM32ISP;
namespace SerialBusProcessor
{
    public delegate void SerialDataHandler(int ch, byte[] command, int length);
    public delegate void DeviceRepowerupDelegate();
    enum SerialBus_DeviceType : byte
    {
        PowerSupply,
        ElectricLoad,
        Sniffer,
        Transmitter,
        Receiver
    }
    public delegate void PortDataArrivalHandler(int portnum, byte[] data, int count);
    public enum ComStopBit : byte { ONE_BIT = 0, HALF_BIT = 1, TWO_BIT = 2, ONEHALF_BIT = 3 };
    public enum ComParity : byte { NONE = 0, EVEN = 1, ODD = 2 };
    public class SerialBusProcessor : Object
    {
        private Control ctrl;                       //Get UI thread condition
        /// <summary>
        /// 串口总线端口
        /// </summary>
        /// 
        private const byte DH0 = 0xaa;
        private const byte DH1 = 0x55;
        private SerialPort scomm;
        private bool finddevice;
        private Mutex transferbufferlock;
        private byte[] deviceorder;
        private EventWaitHandle receivethreadwait;
        private byte[] transferbuffer;
        private byte[][] receivebuffs;
        private Thread receivethread;
        private bool abortreceivethread;
        private bool abortscannthread;
        private bool is_traversaled = false;
        public event SerialDataHandler SerialCommandArrival;
        public event SerialDataHandler SerialDataArrival;
        public event EventHandler DeviceFind;
        public DeviceRepowerupDelegate RepowerupCallback { get; set; }
        public int ComBaudRate { set { scomm.BaudRate = value; } }
        public StopBits ComStopBits { set { scomm.StopBits = value; } }
        public Parity ComParity { set { scomm.Parity = value; } }
        public int ComDataBits { set { scomm.DataBits = value; } }
        public bool Is_Open { get; set; }
        /// <summary>
        /// 获取总线上设备的连入顺序
        /// </summary>
        public byte[] DeviceOrder { get { return deviceorder; } }
        /// <summary>
        /// 获取是否已经找到总线设备
        /// </summary>
        public bool FindDevice { get { return finddevice; } }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="datastream">传输数据的字节流</param>
        /// <param name="length">字节流长度</param>
        private void Write_Transferbuffer(byte[] datastream, int length)
        {
            if (scomm == null || scomm.IsOpen == false)
            {
                return;
            }
            transferbufferlock.WaitOne();
            scomm.Write(datastream, 0, length);
            transferbufferlock.ReleaseMutex();
        }
        private void OnSerialDataArrival(int ch, byte[] data, int length)
        {
            //Console.Out.WriteLine("Data arrival");
        }
        private void OnSerialCommandArrival(int ch, byte[] cmd, int length)
        {
            Console.Out.WriteLine("Command arrival");
            Console.Write(ch.ToString() + " ");
            for (int i = 0; i < length; i++)
                Console.Write(cmd[i].ToString("X") + " ");
            Console.WriteLine("");
            switch (cmd[0])
            {
                case 0xc8:
                    int dev_num = cmd[1];
                    deviceorder = new byte[dev_num];
                    /* 将设备地址按由前到后的顺序重新排列 */
                    Console.WriteLine("Node order: ");
                    for (int i = 0; i < dev_num; i++)
                    {
                        deviceorder[i] = cmd[dev_num + 1 - i];
                        Console.Write(deviceorder[i]);
                        Console.WriteLine(" ");
                    }
                    Console.WriteLine("");
                    break;
                case 0x08:
                    ispwaithandle.Set();
                    ispwait = true;
                    break;
                default: break;
            }
        }

        private byte bytesXOR(byte[] bytes, int count)
        {
            byte xor = 0;
            for (int i = 0; i < count; i++)
            {
                xor ^= bytes[i];
            }
            return xor;
        }
        public void traversal_deices()
        {
            byte[] cmd = new byte[] { 0x01, 0x00 };
            Send_Command(0xff, cmd, 2);
        }
        /// <summary>
        /// 配置指定模组的设备端串口模式
        /// </summary>
        /// <param name="ch">模组地址</param>
        /// <param name="baudrate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="stopBit">停止位</param>
        public void Config_DeviceCom(int ch, UInt32 baudrate, ComParity parity, ComStopBit stopBit)
        {
            byte[] cmd = new byte[6];
            cmd[0] = 0x51;
            cmd[1] = (byte)((baudrate >> 16) & 0xff);
            cmd[2] = (byte)((baudrate >> 08) & 0xff);
            cmd[3] = (byte)((baudrate >> 00) & 0xff);
            cmd[4] = (byte)parity;
            cmd[5] = (byte)stopBit;
            Send_Command(ch, cmd, 6);
        }
        /// <summary>
        /// 向指定设备地址发送数据信息
        /// </summary>
        /// <param name="ch">接收数据的设备地址</param>
        /// <param name="data"> 数据信息</param>
        /// <param name="length">数据长度</param>
        public void Send_Data(int ch, byte[] data, int length)
        {
            byte[] buffer = new byte[length + 6];
            buffer[0] = DH0;
            buffer[1] = DH1;
            buffer[2] = 0x00;
            buffer[3] = (byte)(buffer.Length);
            buffer[4] = (byte)ch;
            for (int i = 0; i < length; i++)
            {
                buffer[5 + i] = data[i];
            }

            buffer[buffer.Length - 1] = bytesXOR(buffer, buffer.Length - 1);
            Write_Transferbuffer(buffer, buffer.Length);
        }

        /// <summary>
        /// 向指定设备地址发送命令信息
        /// </summary>
        /// <param name="ch"> 接收命令的模组地址(5的倍数) </param>
        /// <param name="cmd">命令信息 </param>
        /// <param name="length">命令长度</param>
        public void Send_Command(int ch, byte[] cmd, int length)
        {
            byte[] buffer = new byte[length + 6];
            buffer[0] = 0xff;
            buffer[1] = 0xff;
            buffer[2] = 0x00;
            buffer[3] = (byte)(length + 6);
            buffer[4] = (byte)ch;
            for (int i = 0; i < length; i++)
            {
                buffer[5 + i] = cmd[i];
            }

            buffer[buffer.Length - 1] = bytesXOR(buffer, buffer.Length - 1);
            Write_Transferbuffer(buffer, buffer.Length);
        }
        ~SerialBusProcessor()
        {
            // abort receive thread
            abortreceivethread = true;
            abortscannthread = true;
            Thread.Sleep(10);
        }
        private void common_construct(int channels, SerialPort sp)
        {
            scomm = sp;
            scomm.DataReceived += Scomm_DataReceived;

            transferbufferlock = new Mutex(false);
            transferbuffer = new byte[1024];

            /* if there is no data arrival,suspend the receive thread, decresase CPU load */
            receivethreadwait = new EventWaitHandle(true, EventResetMode.AutoReset);
            receivethreadwait.Reset();

            receivebuffs = new byte[256][];
            SerialDataArrival += OnSerialDataArrival;
            SerialCommandArrival += OnSerialCommandArrival;

            Is_Open = false;
        }
        public SerialBusProcessor(Control ui_ctrl, int channels)
        {
            ctrl = ui_ctrl;
            finddevice = false;
            scomm = new SerialPort();
            common_construct(channels, scomm);
            //scomm.DataReceived += Scomm_DataReceived;

            //transferbufferlock = new Mutex(false);
            //transferbuffer = new byte[1024];

            ///* if there is no data arrival,suspend the receive thread, decresase CPU load */
            //receivethreadwait = new EventWaitHandle(true, EventResetMode.AutoReset);
            //receivethreadwait.Reset();
            //receivethread = new Thread(new ThreadStart(receive_process));

            //receivebuffs = new byte[channels][];
            //SerialDataArrival += OnSerialDataArrival;
            //SerialCommandArrival += OnSerialCommandArrival;

            /*these codes for automatic device scanning */
            abortscannthread = false;
            // create and start device scanning thread
            new Thread(new ThreadStart(scann_device)).Start();
        }
        /* construction function */
        /// <summary>
        /// VB.net调用该重载的构造函数
        /// </summary>
        /// <param name="channels">总线上设备通道的数量</param>
        /// <param name="sp">总线上设备通道的数量</param>
        public SerialBusProcessor(int channels, SerialPort sp)
        {
            finddevice = true;
            abortscannthread = true;
            common_construct(channels, sp);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ui_ctrl">创建当前实例的父控件</param>
        /// <param name="channels">总线上设备通道的数量</param>
        /// <param name="sp">与总线连接的串口对象</param>
        public SerialBusProcessor(Control ui_ctrl, int channels, SerialPort sp)
        {
            ctrl = ui_ctrl;
            finddevice = true;
            abortscannthread = true;
            common_construct(channels, sp);
        }
        private void Scomm_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // 接收线程的等待信号置位，解除接收线程的挂起状态
            receivethreadwait.Set();
        }
        /// <summary>
        /// 对指定地址的设备实施软复位
        /// </summary>
        /// <param name="subAddr">接口设备地址，5的倍数</param>
        public void ResetDevice(int subAddr)
        {
            byte[] cmd = new byte[] { 0x00, 0x00 };
            Send_Command(subAddr, cmd, 2);
        }
        /// <summary>
        /// 启动总线
        /// </summary>
        public void OpenSerialBusProcessor()
        {
            if (!Is_Open)
            {
                abortreceivethread = false;
                scomm.Parity = Parity.None;
                scomm.BaudRate = 115200;
                scomm.Open();
                scomm.DiscardInBuffer();
                scomm.ReadTimeout = 1000;
                receivethread = new Thread(new ThreadStart(receive_process));
                receivethread.IsBackground = true;
                receivethread.Start();
                Is_Open = true;
                if (!is_traversaled)
                {
                    is_traversaled = true;
                    traversal_deices();
                }

            }
        }
        /// <summary>
        /// 关闭总线
        /// </summary>
        public void CloseSerialBusProcessor()
        {
            abortreceivethread = true;
            abortscannthread = true;
            scomm.Close();
            receivethreadwait.Set();
            Is_Open = false;
        }
        int calc_msglength(int header)
        {
            int len;
            if (header < 0x20)
            {
                len = 1;
            }
            else if (header < 0x80)
            {
                len = 2 + ((header - 0x20) >> 4);
            }
            else if (header < 0xE0)
            {
                len = 8 + ((header - 0x80) >> 3);
            }
            else
            {
                len = 20 + ((header - 0xE0) >> 2);
            }
            return len;
        }
        private byte calc_checksum(byte chk, byte data)
        {
            return (byte)(chk ^ data);
        }
        /// <summary>
        /// 侦听总线上接收到的数据
        /// </summary>
        private void receive_process()
        {
            byte[] cmd;
            int pkt_length;
            int channel;
            byte checksum;
            while (!abortreceivethread)
            {
                try
                {
                    //如果接收缓存中待读取的数据长度为零，将当前线程挂起
                    if (scomm.BytesToRead == 0)
                        receivethreadwait.WaitOne();
                    if (scomm.IsOpen == false)
                    {
                        continue;
                    }
                    while (this.scomm.BytesToRead > 0)
                    {
                        int b;
                        checksum = 0;
                        scomm.ReadTimeout = 100;

                        /* preamble byte detection  */
                        b = scomm.ReadByte();
                        if (0xff != b && DH0 != b)
                        {
                            continue;
                        }
                        checksum = calc_checksum(checksum, (byte)b);
                        if (DH0 == b)
                        {
                            /* byte[1]*/
                            b = scomm.ReadByte();
                            if (b < 0 || DH1 != b)
                                continue;
                            checksum = calc_checksum(checksum, (byte)b);
                            /* packet length_h byte[2]  */
                            b = scomm.ReadByte();
                            if (b != 0)
                                continue;
                            /* packet length byte[3]  */
                            b = scomm.ReadByte();
                            if (b < 0)
                                continue;
                            checksum = calc_checksum(checksum, (byte)b);
                            pkt_length = (byte)b;

                            /* packet channel byte[4] */
                            b = scomm.ReadByte();
                            if (b < 0)
                                continue;
                            channel = b;
                            checksum = calc_checksum(checksum, (byte)b);

                            /* if not alloc buffer to specific channel, alloc for it */
                            if (null == receivebuffs[channel])
                                receivebuffs[channel] = new byte[256];

                            cmd = receivebuffs[channel];
                            /* get the rest data */
                            scomm.ReadTimeout = -1;
                            DateTime start = DateTime.Now;
                            bool timeout = false;
                            while (scomm.BytesToRead < pkt_length - 5)
                            {
                                if ((DateTime.Now - start).TotalMilliseconds > 1000)
                                {
                                    timeout = true;
                                    break;
                                }
                            }
                            if (timeout)
                                continue;
                           int rrs = scomm.Read(cmd, 0, pkt_length - 5);

                            for (int i = 0; i < pkt_length - 6; i++)
                                checksum = calc_checksum(checksum, cmd[i]);

                            /* verify checksum */
                            if (checksum != cmd[pkt_length - 6])
                            {
                                Console.WriteLine("校验错误");
                                continue;
                            }

                            if (SerialDataArrival != null)
                            {
                                if (null != ctrl)
                                {
                                    ctrl.Invoke(SerialDataArrival, new object[] { channel, cmd, pkt_length - 6 });
                                }
                                else
                                {
                                    SerialDataArrival(channel, cmd, pkt_length - 6);
                                }
                            }
                        }
                        else
                        {
                            b = scomm.ReadByte();
                            if (0xff != b)
                                continue;
                            checksum += calc_checksum(checksum, (byte)b);
                            /* packet length_h byte[2]  */
                            b = scomm.ReadByte();
                            if (b != 0)
                                continue;
                            /* packet length byte[3]  */
                            b = scomm.ReadByte();
                            if (b < 0)
                                continue;
                            checksum += calc_checksum(checksum, (byte)b);
                            pkt_length = (byte)b;
                            /* packet channel byte[4] */
                            b = scomm.ReadByte();
                            if (b < 0)
                                continue;
                            channel = b;
                            checksum += calc_checksum(checksum, (byte)b);

                            /* if not alloc buffer to specific channel, alloc for it */
                            if (null == receivebuffs[channel])
                            {
                                receivebuffs[channel] = new byte[256];
                            }
                            cmd = receivebuffs[channel];
                            /* get the rest data */
                            DateTime start = DateTime.Now;
                            bool timeout = false;
                            while(scomm.BytesToRead < pkt_length)
                            {
                                if((DateTime.Now-start).TotalMilliseconds > 1000)
                                {
                                    timeout = true;
                                    break;
                                }
                            }
                            if (timeout)
                                continue;
                            scomm.Read(cmd, 0, pkt_length - 5);
                            for (int i = 0; i < pkt_length - 6; i++)
                            {
                                checksum += calc_checksum(checksum, cmd[i]);
                            }

                            /* verify checksum */
                            if (checksum != cmd[pkt_length - 6])
                                continue;

                            if (channel < 0xff)
                            {
                                if (SerialCommandArrival != null)
                                {
                                    /* 剥除总线数据包的外壳，传递设备实际报文 */
                                    if (null != ctrl)
                                    {
                                        ctrl.Invoke(SerialCommandArrival, new object[] { channel, cmd, pkt_length - 6 });
                                    }
                                    else
                                    {
                                        SerialCommandArrival(channel, cmd, pkt_length - 6);
                                    }
                                }
                            }
                            else
                            {
                                /* 系统命令通道直接调用内部响应函数 */
                                OnSerialCommandArrival(channel, cmd, pkt_length - 6);
                            }
                            cmd.Initialize();
                        }
                    }
                }
                catch (TimeoutException toexp)
                {
                    Console.WriteLine(toexp.Message, "连接错误");
                }
                catch (InvalidOperationException ivdopexp)
                {
                    Console.WriteLine(ivdopexp.Message, "操作错误");
                }
                catch
                {
                    Console.WriteLine("发生不可知异常");
                    //finddevice = false;
                    ///* abortscannthread false means user didn't call auto-scanning serialport construct function */
                    //if(abortscannthread == false)
                    //    new Thread(new ThreadStart(scann_device)).Start();
                    //break;
                }
            }
            Console.WriteLine("recv proc abort");
        }

        public static string Find_Portname()
        {
            SerialPort com = new SerialPort();
            byte[] cmd = new byte[] { 0xff, 0xff, 0x00, 0x09, 0xff, 0x00, 0x00, 0x00, 0xf6 };
            byte[] buf = new byte[8];
            com.BaudRate = 115200;
            com.StopBits = StopBits.One;
            com.Parity = Parity.None;
            com.DataBits = 8;
            com.ReadTimeout = 1000;
            string portname = "";
            string[] names = SerialPort.GetPortNames();
            foreach (var n in names)
            {
                bool success = true;
                DateTime dt = DateTime.Now;
                try
                {
                    com.PortName = n;
                    com.Open();
                    // 发送0x00命令，等待设备响应
                    com.Write(cmd, 0, 8);
                    while (true)
                    {
                        // timeout condition
                        if ((DateTime.Now - dt).TotalMilliseconds > 1000)
                        {
                            throw new TimeoutException();
                        }
                        // to find the command preamble "0xff 0xff"
                        if (com.ReadByte() != 0xff)
                            continue;
                        if (com.ReadByte() != 0xff)
                        {
                            throw new TimeoutException();
                        }
                        int len = com.ReadByte();
                        com.Read(buf, 0, len - 3);
                        // command header "0x08" means ACK
                        if (buf[1] == 0x08)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(n + ex.Message);
                    success = false;
                }
                finally
                {
                    com.Close();
                }
                if (success)
                {
                    portname = n;
                    Console.WriteLine("Device port name " + n + "\n");
                    break;
                }

            }
            return portname;
        }
        /// <summary>
        /// 扫描所有已有的串口，找到总线端口
        /// </summary>
        void scann_device()
        {

            SerialPort com = new SerialPort();
            byte[] cmd = new byte[] { 0xff, 0xff, 0x00, 0x09, 0xff, 0x00, 0x00, 0x00, 0xf6 };
            byte[] buf = new byte[8];
            com.BaudRate = 115200;
            com.StopBits = StopBits.One;
            com.Parity = Parity.None;
            com.DataBits = 8;
            com.ReadTimeout = 1000;

            while (finddevice == false && abortscannthread == false)
            {
                string[] names = SerialPort.GetPortNames();
                foreach (var n in names)
                {
                    bool success = true;
                    DateTime dt = DateTime.Now;
                    try
                    {
                        com.PortName = n;
                        com.Open();
                        // 发送0x00命令，等待设备响应
                        com.Write(cmd, 0, 8);
                        while (true)
                        {
                            // timeout condition
                            if ((DateTime.Now - dt).TotalMilliseconds > 1000)
                            {
                                throw new TimeoutException();
                            }
                            // to find the command preamble "0xff 0xff"
                            if (com.ReadByte() != 0xff)
                                continue;
                            if (com.ReadByte() != 0xff)
                            {
                                throw new TimeoutException();
                            }
                            int len = com.ReadByte();
                            com.Read(buf, 0, len - 3);
                            // command header "0x08" means ACK
                            if (buf[1] == 0x08)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(n + ex.Message);
                        success = false;
                    }
                    finally
                    {
                        com.Close();
                    }
                    if (success)
                    {
                        this.scomm.Close();
                        // find the target com port
                        this.scomm.PortName = n;
                        // set flag to exit thread
                        finddevice = true;
                        // call event callback
                        if (DeviceFind != null)
                            ctrl.Invoke(DeviceFind, new object[] { this, new EventArgs() });
                        Console.WriteLine("Device port name " + n + "\n");
                        break;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private EventWaitHandle ispwaithandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private bool ispwait = false;
        /// <summary>
        /// 软件重启<see cref="deviceIdx"/>对应的设备进入到systemboot模式下做ISP升级
        /// </summary>
        /// <param name="deviceIdx">设备在级联总线中的顺序数，从'0'开始计数</param>
        /// <param name="is_reboot">该设备是否之前调用过reboot函数且成功执行</param>
        /// <returns>0:成功，1:重启reset超时，负数:其对应正数所对应的设备直连命令响应失败</returns>
        public int reboot(int deviceIdx, bool is_reboot)
        {
            int k;
            // directly link command
            byte[] cmd = new byte[] { 0x03, 0x00 };
            byte[] rst = new byte[] { 0xff, 0x00 };
            if (!is_reboot)
            {
                ispwaithandle.Reset();
                ispwait = false;
                // send isp_reboot command
                Send_Command(deviceorder[deviceIdx], rst, 2);
                Thread.Sleep(500);
                Console.WriteLine("Target module reboot");
            }
            return 0;
        }
        private bool isp_write(byte[] buff, int count)
        {
            scomm.DiscardOutBuffer();
            scomm.Write(buff, 0, count);
            return true;
        }

        private bool isp_read(byte[] buff, int count)
        {
            int cnt;
            try
            {
                cnt = scomm.Read(buff, 0, count);
            }
            catch
            {
                cnt = 0;
            }
            if (cnt > 0)
            {
                return true;
            }
            else
            {
                MessageBox.Show("串口连接故障");
            }
            return false;
        }
        public  int UpdateFirmware(string fw_path, int deviceidx)
        {
            string portname = scomm.PortName;
            STMisp stmisp = new STMisp();

            stmisp.Write = isp_write;
            stmisp.Read = isp_read;

            // 成功重启标志
            bool is_reboot = false;
            // 成功下载标志
            bool is_download = false;
            int repeat_cnt = 0;
            while (repeat_cnt < 2 && !is_download)
            {
                this.CloseSerialBusProcessor();
                this.OpenSerialBusProcessor();

                bool proc_fail = false;

                //总线重新上电
                RepowerupCallback?.Invoke();
                Thread.Sleep(1000);
                repeat_cnt++;
                int rbrs = reboot(deviceidx, is_reboot);
                if (rbrs != 1)
                {
                    is_reboot = true;
                }
                if (rbrs != 0)
                    continue;
                Console.WriteLine(string.Format("module {0:d} reboot succ", deviceorder[deviceidx]));
                scomm.DiscardInBuffer();
                this.CloseSerialBusProcessor();
                Thread.Sleep(500);
                scomm.PortName = portname;
                scomm.Parity = Parity.Even;
                scomm.BaudRate = 115200;
                scomm.StopBits = StopBits.One;
                scomm.DataBits = 8;
                scomm.ReadTimeout = 2000;
                scomm.Open();
                ISPACK ack;
                ack = stmisp.InitBL();
                if (ack == ISPACK.ISP_TO)
                    continue;
                while (ack == ISPACK.ISP_NACK)
                    ack = stmisp.InitBL();
                // 当前设备尚未下载成功，进入的ISP下载流程
                if (!is_download)
                {
                    ack = stmisp.Erase();
                    if (ack == ISPACK.ISP_TO)
                        continue;
                    if (ack == ISPACK.ISP_NACK)
                    {
                        ack = stmisp.Read_unprotect();
                        if (ack != ISPACK.ISP_ACK)
                            continue;
                        Thread.Sleep(50);
                        if (stmisp.InitBL() == ISPACK.ISP_TO)
                            continue;
                    }

                    System.IO.FileStream fs = new System.IO.FileStream(fw_path, System.IO.FileMode.Open);
                    long len_rem = fs.Length;
                    uint start_addr = 0x08000000;
                    byte[] wm = new byte[256];
                    if (proc_fail == false)
                    {
                        while (len_rem >= 256)
                        {
                            fs.Read(wm, 0, 256);
                            if (WriteMemoryResult.OK != stmisp.WriteMemory(start_addr, 256, wm))
                            {
                                proc_fail = true;
                                break;
                            }
                            len_rem -= 256;
                            start_addr += 256;
                            Console.WriteLine("remain bytes:{0}", len_rem);
                            Thread.Sleep(100);
                        }
                        if (len_rem > 0 && proc_fail == false)
                        {
                            fs.Read(wm, 0, (int)len_rem);
                            if (WriteMemoryResult.OK != stmisp.WriteMemory(start_addr, 256, wm))
                            {
                                proc_fail = true;
                            }
                            Thread.Sleep(100);
                        }
                        fs.Close();
                    }
                }
                if (proc_fail)
                    continue;
                Console.WriteLine("write memory succ");
                is_download = true;
                // 下载成功后一定要执行GoApp命令，确保重新上电后切换到userboot模式
                if (stmisp.Go(0x08000000) != ISPACK.ISP_ACK)
                {
                    Console.WriteLine("go fail");
                    continue;
                }
                Console.WriteLine("go succ");
                repeat_cnt = 0;
                scomm.Close();
                break;
            }
            Thread.Sleep(1000);
            this.CloseSerialBusProcessor();
            this.OpenSerialBusProcessor();
            if (is_download)
                return 0;
            return -1;
        }
    }
}
