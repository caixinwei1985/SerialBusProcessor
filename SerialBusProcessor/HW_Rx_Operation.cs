using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace SerialBusProcessor
{
    /// <summary>
    /// Rx通知指令处理方法
    /// </summary>
    /// <param name="type">通知事件类型,0x00:comm event,0x01protection event</param>
    /// <param name="evt">事件寄存器内容</param>
    public delegate void RxNotifyHandler(byte type, byte evt);
    public partial class HW_Rx_Operation
    {
        private byte _cmd;
        private int _channel;
        private byte _target;
        private byte[] _readbuf;
        private SerialBusProcessor _sbp;
        private AutoResetEvent _wr_event;
        private AutoResetEvent _rd_event;

        public event RxNotifyHandler RxNotifyArrival;
        public HW_Rx_Operation(SerialBusProcessor sbp, int channel, byte target)
        {
            _sbp = sbp;
            _channel = channel;
            _target = target;
            _sbp.SerialDataArrival += _sbp_SerialDataArrival;
            _readbuf = new byte[128];
            _wr_event = new AutoResetEvent(false);
            _rd_event = new AutoResetEvent(false);
        }

        public async Task<bool> SetRXModeAsync(RxMode mode)
        {
            return await WriteRegsAsync(RX_MODE, new byte[] { (byte)mode });
        }


        public async Task<RxMode> GetRXModeAsync()
        {
            byte[] buf = new byte[1];
            RxMode rxMode;
            bool b = await ReadRegsAsync(RX_MODE, buf);
            if (b)
                rxMode = (RxMode)buf[0];
            else
                rxMode = RxMode.ReadFailed;
            return rxMode;
        }
        private void _sbp_SerialDataArrival(int ch, byte[] command, int length)
        {
            if (ch == _channel)
                OnResponseArrival(command, length);
        }
        /// <summary>
        /// 设置输出电压的目标值，单位mV
        /// </summary>
        /// <param name="volt">设定电压值</param>
        /// <returns>true:OK,false:Fail</returns>
        public async Task<bool> SetTargetOutputVoltAsync(UInt16 volt)
        {
            return await WriteRegsAsync(TARGET_OUTPUT_VOLT_H, (byte)(volt >> 8), (byte)(volt & 0xff));
        }
        /// <summary>
        /// 异步获取目标电压的输出设置
        /// </summary>
        /// <returns>fetch failed returns 0xffff</returns>
        public async Task<UInt16> GetTargetOutputVoltAsync()
        {
            byte[] buf = new byte[2];
            UInt16 volt;
            bool b = await ReadRegsAsync(TARGET_OUTPUT_VOLT_H, buf);
            if (b)
            {
                volt = (UInt16)(buf[0] * 256 + buf[1]);
            }
            else
            {
                volt = 0xffff;
            }
            return volt;
        }
        /// <summary>
        /// 发送专属自定义命令 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<bool> SendPPP(byte header, params byte[] msg)
        {
            bool rs = true;
            bool b = await WriteRegsAsync(ASK_HEADER, header);
            if (b)
            {
                b = await WriteRegsAsync(ASK_MESSAGE_START, msg);
                if (!b)
                    rs = false;
            }
            else
            {
                rs = false;
            }
            return rs;
        }

        public async Task<bool> Enable_SignalStrength(bool isfixed)
        {
            bool b;
            if (isfixed)
                b = await WriteRegsAsync(SIGNAL_STRENGTH_HEADER, 2);
            else
                b = await WriteRegsAsync(SIGNAL_STRENGTH_HEADER, 1);
            return b;
        }
        public async Task<bool> Disable_SignalStrength()
        {
            bool b = await WriteRegsAsync(SIGNAL_STRENGTH_HEADER, 0);
            return b;
        }
        public async Task<bool> SetExpectedUmax(UInt16 umax)
        {
            bool b;
            b = await WriteRegsAsync(EXPECTED_UMAX, (byte)(umax >> 8), (Byte)(umax & 0xff));
            return b;
        }

        public async Task<int> GetSignalStrength()
        {
            byte[] buf = new byte[1];
            bool b = await ReadRegsAsync(SIGNAL_STRENGTH_VALUE, buf);
            if (b)
                return buf[0];
            else
                return -1;
        }

        public async Task<bool> SetSignalStrength(byte ss)
        {
            return await WriteRegsAsync(SIGNAL_STRENGTH_VALUE, ss);
        }

        public async Task<bool> EnableIdentification()
        {
            return await WriteRegsAsync(IDENTIFICATION_HEADER, 0x71);
        }
        public async Task<bool> DisableIdentification()
        {
            return await WriteRegsAsync(IDENTIFICATION_HEADER, 0x00);
        }
        public async Task<bool> SetQiVersion(byte version)
        {
            return await WriteRegsAsync(QI_VERSION, version);
        }
        public async Task<bool> SetManufacturerCode(UInt16 manufacturercode)
        {
            return await WriteRegsAsync(MANUFACTURER_CODE, (byte)(manufacturercode >> 8), (byte)(manufacturercode & 0xff));
        }
        public async Task<FSKResponse> GetConfigurationFskResp()
        {
            byte[] buf = new byte[1];
            bool b = await ReadRegsAsync(CONFIG_FSK_RESP, buf);
            if (b)
            {
                return (FSKResponse)buf[0];
            }
            return FSKResponse.ReadFailed;
        }
        public async Task<bool> SetDeviceCode(UInt32 devicecode)
        {
            byte[] buf = new byte[4];
            buf[0] = (byte)(devicecode >> 24);
            buf[1] = (byte)((devicecode >> 16) & 0xff);
            buf[2] = (byte)((devicecode >> 8) & 0xff);
            buf[3] = (byte)(devicecode & 0xff);
            return await WriteRegsAsync(BASIC_DEVICE_CODE, buf);
        }
        public async Task<bool> EnableExtID()
        {
            return await WriteRegsAsync(EXT_DEVICE_ID, 0x81);
        }
        public async Task<bool> DisableExtID()
        {
            return await WriteRegsAsync(EXT_DEVICE_ID, 0x00);
        }
        public async Task<bool> EnableHoldOff()
        {
            return await WriteRegsAsync(HOLD_OFF_HEADER, 0x06);
        }
        public async Task<bool> DisableHoldOff()
        {
            return await WriteRegsAsync(HOLD_OFF_HEADER, 0x00);
        }
        public async Task<bool> SetHoldOffTime(byte holdoff)
        {
            return await WriteRegsAsync(HOLD_OFF_TIME, holdoff);
        }
        public async Task<bool> EnableConfigration()
        {
            return await WriteRegsAsync(CONFIG_HEADER, 0x51);
        }
        public async Task<bool> DisableConfiguration()
        {
            return await WriteRegsAsync(CONFIG_HEADER, 0x00);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="power">设定最大功率，单位0.5W</param>
        /// <returns></returns>
        public async Task<bool> SetMaximumPower(byte power)
        {
            byte[] buf = new byte[1];
            bool b = await ReadRegsAsync(CONFIG_MESSAGE + 0, buf);
            if (b)
            {
                buf[0] &= 0xc0;
                buf[0] |= (byte)(power & 0x3f);
                b = await WriteRegsAsync(CONFIG_MESSAGE + 0, buf);
                return b;
            }
            return false;
        }
        public async Task<bool> SetCofingPacktCount(byte count)
        {
            byte[] buf = new byte[1];
            bool b = await ReadRegsAsync(CONFIG_MESSAGE + 2, buf);
            if (b)
            {
                buf[0] &= 0xf8;
                buf[0] |= (byte)(count & 0x07);
                b = await WriteRegsAsync(CONFIG_MESSAGE + 2, buf);
                return b;
            }
            return false;
        }
        public async Task<bool> SetProfile(bool isEPP)
        {
            byte[] buf = new byte[1];
            bool b = await ReadRegsAsync(CONFIG_MESSAGE + 4, buf);
            if (b)
            {
                if (isEPP)
                    buf[0] |= 0x80;
                else
                    buf[0] &= 0x7f;
                b = await WriteRegsAsync(CONFIG_MESSAGE + 4, buf);
                return b;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="polarity">true:Negative,fasle:Positive</param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public async Task<bool> SetFSKParams(bool polarity, byte depth)
        {
            byte[] buf = new byte[1];
            bool b = await ReadRegsAsync(CONFIG_MESSAGE + 4, buf);
            if (b)
            {
                if (polarity)
                    buf[0] |= 0x40;
                else
                    buf[0] &= 0xbf;
                buf[0] &= 0xcf;
                buf[0] |= depth;
                b = await WriteRegsAsync(CONFIG_MESSAGE + 4, buf);
                return b;
            }
            return false;
        }

        public async Task<int> GetRectVolt()
        {
            byte[] buf = new byte[2];
            bool b = await ReadRegsAsync(RECT_VOLTAGE, buf);
            if (b)
            {
                return buf[0] * 256 + buf[1];
            }
            return -1;
        }
        public async Task<int> GetLCFrequency()
        {
            byte[] buf = new byte[4];
            bool b = await ReadRegsAsync(LC_FREQUENCY, buf);
            if (b)
            {
                return (buf[0] << 24) + (buf[1] << 16) + (buf[2] << 8) + buf[3];
            }
            return -1;
        }
        public async Task<int> GetRectCurr()
        {
            byte[] buf = new byte[2];
            bool b = await ReadRegsAsync(RECT_CURRENT, buf);
            if (b)
            {
                return buf[0] * 256 + buf[1];
            }
            return -1;
        }
        public async Task<int> GetVoutVolt()
        {
            byte[] buf = new byte[2];
            bool b = await ReadRegsAsync(VOUT_VOLTAGE, buf);
            if (b)
            {
                return buf[0] * 256 + buf[1];
            }
            return -1;
        }

        public async Task<int> GetVoutCurr()
        {
            byte[] buf = new byte[2];
            bool b = await ReadRegsAsync(VOUT_CURRENT, buf);
            if (b)
            {
                return buf[0] * 256 + buf[1];
            }
            return -1;
        }
        public void OnResponseArrival(byte[] dataBuffer, int length)
        {
            UInt16 crc = HWCommandAnalyzer.CalcCRC16(0, dataBuffer, dataBuffer.Length - 2);
            if (dataBuffer[dataBuffer.Length - 2] * 256 + dataBuffer[dataBuffer.Length - 1] != crc)
                return;
            if (_cmd == (byte)~dataBuffer[4])
            {
                if (_cmd == 0x0b)
                {
                    _wr_event.Set();
                }
                if (_cmd == 0x0c)
                {
                    int size = dataBuffer[8] * 256 + dataBuffer[9];
                    _readbuf = new byte[size];
                    for (int i = 0; i < size; i++)
                        _readbuf[i] = dataBuffer[10 + i];
                    _rd_event.Set();
                }
            }
            else if (~dataBuffer[4] == 0x15)
            {
                // 通知信息到
                RxNotifyArrival?.Invoke(dataBuffer[6], dataBuffer[8]);
            }
        }
        private bool waitwriteresponse()
        {
            bool rs = true;
            if (!_wr_event.WaitOne(1000))
            {
                rs = false;
            }
            return rs;
        }
        private bool waitreadresponse()
        {
            bool rs = true;
            if (!_rd_event.WaitOne(1000))
            {
                rs = false;
            }
            return rs;
        }
        public async Task<bool> WriteRegsAsync(UInt16 reg_addr, params byte[] data)
        {
            byte[] buf = new byte[12 + data.Length];
            _cmd = 0x0b;
            buf[0] = 0x5a;
            buf[1] = 0xa5;
            buf[2] = (byte)(buf.Length / 256);
            buf[3] = (byte)(buf.Length & 0xff);
            buf[4] = _cmd;
            buf[5] = _target;
            buf[6] = (byte)(reg_addr >> 8);
            buf[7] = (byte)(reg_addr & 0xff);
            buf[8] = (byte)(data.Length >> 8);
            buf[9] = (byte)(data.Length & 0xff);
            for (int i = 0; i < data.Length; i++)
            {
                buf[10 + i] = data[i];
            }
            bool b;
            UInt16 CRC = HWCommandAnalyzer.CalcCRC16(0X0000, buf, buf.Length - 2);
            buf[buf.Length - 2] = (byte)(CRC >> 8);
            buf[buf.Length - 1] = (byte)(CRC & 0xff);
            _sbp.Send_Data(_channel, buf, buf.Length);

            b = await Task.Run<bool>(new Func<bool>(waitwriteresponse)); ;
            if (b)
            {
                return true;
            }
            else
                return false;
        }
        public async Task<bool> ReadRegsAsync(UInt16 reg_addr, byte[] data)
        {
            byte[] buf = new byte[12 + data.Length];
            _cmd = 0x0c;
            /* 生成读数据帧 */
            buf[0] = 0x5a;
            buf[1] = 0xa5;
            buf[2] = (byte)(buf.Length / 256);
            buf[3] = (byte)(buf.Length & 0xff);
            buf[4] = _cmd;
            buf[5] = _target;
            buf[6] = (byte)(reg_addr >> 8);
            buf[7] = (byte)(reg_addr & 0xff);
            buf[8] = (byte)(data.Length >> 8);
            buf[9] = (byte)(data.Length & 0xff);
            UInt16 CRC = HWCommandAnalyzer.CalcCRC16(0X0000, buf, buf.Length - 2);
            buf[buf.Length - 2] = (byte)(CRC >> 8);
            buf[buf.Length - 1] = (byte)(CRC & 0xff);
            /*发送数据帧*/
            _sbp.Send_Data(_channel, buf, buf.Length);
            /*启动应答等待任务*/
            Task<bool> task = new Task<bool>(new Func<bool>(waitreadresponse));
            task.Start();
            /*等待任务完成返回*/
            bool b = await task;
            if (b)  //结果位True,收到应答帧
            {
                if (_readbuf.Length != data.Length)
                    b = false;
                else
                {
                    _readbuf.CopyTo(data, 0);
                }
            }
            else //结果为False,没有收到应答帧
            {

            }
            return b;
        }
    }

}
