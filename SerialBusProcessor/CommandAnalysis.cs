
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SerialBusProcessor
{
    public enum LoadChannel : byte { Channel1 = 1, Channel2, Channel3, Channel4, Channel5 };
    public enum PowersupplyChannel : byte { Channel1 = 1, Channel2, Channel3, Channel4, Channel5 };
    public enum RxMode : byte { BPP = 0, SFC, AFC, EPP };
    public enum PowerMode : byte { None = 0, Adapter_PD, Onboard_PD, Adapter_QC, Onboard_QC, Adpter_PQQC, Onboard_PQQC, Fixed };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CmdHeader
    {
        public byte header0;
        public byte header1;
        public byte length_h;
        public byte length_l;
        public byte version;
        public byte localtype;
        public byte targettype;
        public byte targetld_h;
        public byte targetId_l;
        public byte ctrlcode;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PowersupplyConfigCmd : CmdHeader
    {
        public byte channel;
        public byte mode;
        public byte Vset_h;
        public byte Vset_l;
        public byte action;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PowersupplyConfigResp : PowersupplyConfigCmd
    { }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SingleQuiryPowersupplyCmd : CmdHeader
    {
        public byte channel;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SingleQuiryPowersupplyResp : CmdHeader
    {
        public byte channel;
        public byte curr_h;
        public byte curr_l;
        public byte volt_h;
        public byte volt_l;
        public byte temp_h;
        public byte temp_l;
        public byte crc16_h;
        public byte crc16_l;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ConfigLoadCurrentCmd : CmdHeader
    {
        public byte channel;
        public byte curr_h;
        public byte curr_l;
        public byte action;
        public byte resvred;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ConfigLoadCurrentResp : ConfigLoadCurrentCmd
    {

    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SingleQuiryLoadCurrentCmd : CmdHeader
    {
        public byte channel;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SingleQuiryLoadResp : CmdHeader
    {
        public byte channel;
        public byte curr_h;
        public byte curr_l;
        public byte volt_h;
        public byte volt_l;
        public byte temp_h;
        public byte temp_l;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RxInitConfigCmd : CmdHeader
    {
        public byte mode;
        public byte testfunc;
        public byte reserved;
        public byte action;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RxEPTConifgCmd : CmdHeader
    {
        public byte eptcode;
        public UInt16 reserved;
        public byte action;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RxInitConfigResp : RxInitConfigCmd
    {

    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RxEPTConfigResp : RxEPTConifgCmd
    { }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ASMHeader : CmdHeader
    {
        public byte localmode;
        public byte ASMcount_h;
        public byte ASMcount_l;
        public byte QiState;
        public byte QiHeader;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SignalStrengthPacket : ASMHeader
    {
        public byte QiPowerStrength;
        public byte Qichecksum;
        public byte reserved;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class IdentificationPacket : ASMHeader
    {
        public byte QiVersion;
        public byte QiManufactureId_h;
        public byte QiManufactureId_l;
        public byte QiDeviceId1;
        public byte QiDeviceId2;
        public byte QiDeviceId3;
        public byte QiDeviceId4;
        public byte Qichecksum;
        public byte crc16_h;
        public byte crc16_l;

    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ConfigurationPacket : ASMHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] QiMessage;
        public byte Qichecksum;
        public UInt16 reserved;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FODStatusPacket : ASMHeader
    {
        public byte QiFODmode;
        public byte QiQref;
        public byte Qichecksum;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GeneralRequestPacket : ASMHeader
    {
        public byte QiRequest;
        public byte Qichecksum;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SpecificRequstPacket : ASMHeader
    {
        public byte QiRequest;
        public byte QiParam;
        public byte Qichecksum;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ControlErrorPacket : ASMHeader
    {
        public byte QiCE;
        public byte Qichecksum;
        public byte reserved1;
        public byte reserved2;
        public byte reserved3;
        public byte Vrect_h;
        public byte Vrect_l;
        public byte Iout_h;
        public byte Iout_l;
        public byte Vout_h;
        public byte Vout_l;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ReceivedPower8Packet : ASMHeader
    {
        public byte QiRP;
        public byte Qichecksum;
        public byte reserved1;
        public byte reserved2;
        public byte reserved3;
        public byte Vrect_h;
        public byte Vrect_l;
        public byte Iout_h;
        public byte Iout_l;
        public byte Vout_h;
        public byte Vout_l;
        public byte Freq_h;
        public byte Freq_l;
        public byte RPeqv_h;
        public byte RPeqv_l;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ReceivedPower24Packet : ASMHeader
    {
        public byte QiRPmode;
        public byte QiRP_h;
        public byte QiRP_l;
        public byte Qichecksum;
        public byte reserved;
        public byte Vrect_h;
        public byte Vrect_l;
        public byte Iout_h;
        public byte Iout_l;
        public byte Vout_h;
        public byte Vout_l;
        public byte Freq_h;
        public byte Freq_l;
        public byte RPeqv_h;
        public byte RPeqv_l;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class HWPropPacket : ASMHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] QiMessage;
        public byte Qichecksum;
        public byte Vrect_h;
        public byte Vrect_l;
        public byte Iout_h;
        public byte Iout_l;
        public byte Vout_h;
        public byte Vout_l;
        public byte Freq_h;
        public byte Freq_l;
        public byte RPeqv_h;
        public byte RPeqv_l;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class EndPowerTransferPacket : ASMHeader
    {
        public byte QiEPTcode;
        public byte Qichecksum;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SSHandshakePacket : ASMHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public byte[] QiMessage;
        public byte Qichecksum;
        public byte reserved;
        public byte crc16_h;
        public byte crc16_l;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SSProprietaryPacket : ASMHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] QiMessage;
        public byte Qichecksum;
        public byte crc16_h;
        public byte crc16_l;
    }

    public class HWCommandAnalyzer
    {
        private const int VERSION = 0x10;
        private const byte TYPE_LOAD = (byte)'L';
        private const byte TYPE_CTRL = (byte)'C';
        private const byte TYPE_RX = (byte)'R';
        private const byte TYPE_POWER = (byte)'P';
        public HWCommandAnalyzer()
        {
        }
        public static UInt16 CalcCRC16(UInt16 crc, byte[] data, int length)
        {
            UInt16 i;
            byte k;
            UInt16 ACC, TOPBIT;
            UInt16 remainder = crc;
            TOPBIT = 0x8000;
            for (i = 0; i < length; ++i)
            {
                ACC = data[i];
                remainder ^= (UInt16)(ACC << 8);
                for (k = 8; k > 0; --k)
                {
                    if ((remainder & TOPBIT) != 0)
                    {
                        remainder = (UInt16)((remainder << 1) ^ 0x8005);
                    }
                    else
                    {
                        remainder = (UInt16)(remainder << 1);
                    }
                }
            }
            remainder = (UInt16)(remainder ^ 0x0000);
            return remainder;
        }
        private static byte[] Struct2Array<T>(T obj)
        {
            IntPtr pbuf;
            int size = Marshal.SizeOf(obj);
            pbuf = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, pbuf, false);
            byte[] ar = new byte[size];
            Marshal.Copy(pbuf, ar, 0, size);
            Marshal.FreeHGlobal(pbuf);
            return ar;
        }

        private static T Array2Struct<T>(byte[] ar)
        {
            IntPtr pbuf;
            int size = Marshal.SizeOf(typeof(T));
            pbuf = Marshal.AllocHGlobal(size);
            Marshal.Copy(ar, 0, pbuf, size);
            object OBO = Marshal.PtrToStructure(pbuf, typeof(T));
            Marshal.FreeHGlobal(pbuf);
            return (T)OBO;
        }
        private static void InitHeader(Type type, CmdHeader hdr, byte ctrlCode, byte targetType, UInt16 targetID)
        {
            int size = Marshal.SizeOf(type);
            hdr.header0 = 0x5a;
            hdr.header1 = 0xa5;
            hdr.length_h = (byte)(size >> 8);
            hdr.length_l = (byte)(size & 0xff);
            hdr.ctrlcode = ctrlCode;
            hdr.version = VERSION;
            hdr.targettype = targetType;
            hdr.localtype = TYPE_CTRL;
            hdr.targetld_h = (byte)(targetID >> 8);
            hdr.targetId_l = (byte)(targetID & 0xff);
        }
        /// <summary>
        /// 配置负载对应通道的电流值
        /// </summary>
        /// <param name="ch">配置的通道</param>
        /// <param name="curr">配置的电流值，单位mA</param>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public byte[] ConfigLoadCurrent(LoadChannel ch, int curr, int targetId = 0)
        {
            byte[] ar = null;
            UInt16 crc;
            ConfigLoadCurrentCmd clcc = new ConfigLoadCurrentCmd();
            InitHeader(typeof(ConfigLoadCurrentCmd), (CmdHeader)clcc, 0x01, TYPE_LOAD, (UInt16)targetId);
            clcc.channel = (byte)ch;
            clcc.curr_h = (byte)(curr >> 8);
            clcc.curr_l = (byte)(curr & 0xff);

            ar = Struct2Array<ConfigLoadCurrentCmd>(clcc);
            crc = CalcCRC16(0x0000, ar, ar.Length - 2);
            ar[ar.Length - 2] = (byte)(crc >> 8);
            ar[ar.Length - 1] = (byte)(crc & 0xff);

            return ar;
        }
        /// <summary>
        /// 查询对应通道的负载状态。
        /// </summary>
        /// <param name="ch">要查询的通道</param>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public byte[] SingleQuiryLoad(LoadChannel ch, int targetId = 0)
        {
            byte[] ar = null;
            UInt16 crc;
            SingleQuiryLoadCurrentCmd sqlc = new SingleQuiryLoadCurrentCmd();
            InitHeader(typeof(SingleQuiryLoadCurrentCmd), (CmdHeader)sqlc, 0x02, TYPE_LOAD, (UInt16)targetId);
            sqlc.channel = (byte)ch;
            ar = Struct2Array<SingleQuiryLoadCurrentCmd>(sqlc);
            crc = CalcCRC16(0, ar, ar.Length - 2);
            ar[ar.Length - 2] = (byte)(crc >> 8);
            ar[ar.Length - 1] = (byte)(crc & 0xff);
            return ar;
        }
        /// <summary>
        /// 配置RX的工作模式
        /// </summary>
        /// <param name="mode">工作模式</param>
        /// <param name="is_fod">是否配置为FOD状态</param>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public byte[] RxConfigMode(RxMode mode, byte is_fod, int targetId = 0)
        {
            byte[] ar = null;
            UInt16 crc;
            RxInitConfigCmd cmd = new RxInitConfigCmd();
            InitHeader(typeof(RxInitConfigCmd), (CmdHeader)cmd, 0xA1, TYPE_RX, (UInt16)targetId);
            cmd.mode = (byte)mode;
            cmd.testfunc = is_fod;
            cmd.action = 0x01;
            ar = Struct2Array<RxInitConfigCmd>(cmd);
            crc = CalcCRC16(0, ar, ar.Length - 2);
            ar[ar.Length - 2] = (byte)(crc >> 8);
            ar[ar.Length - 1] = (byte)(crc & 0xff);
            return ar;
        }
        /// <summary>
        /// 设置RX的EPT退出码
        /// </summary>
        /// <param name="EPTcode"></param>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public byte[] RxSetEPT(byte EPTcode, int targetId = 0)
        {
            byte[] ar = null;
            UInt16 crc;
            RxEPTConifgCmd cmd = new RxEPTConifgCmd();
            InitHeader(typeof(RxEPTConifgCmd), (CmdHeader)cmd, 0xA2, TYPE_RX, (UInt16)targetId);
            cmd.eptcode = EPTcode;
            cmd.action = 0x01;
            ar = Struct2Array<RxEPTConifgCmd>(cmd);
            crc = CalcCRC16(0, ar, ar.Length - 2);
            ar[ar.Length - 2] = (byte)(crc >> 8);
            ar[ar.Length - 1] = (byte)(crc & 0xff);
            return ar;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public byte[] SingleQuiryPowersupply(PowersupplyChannel ch, int targetId = 0)
        {
            byte[] ar = null;
            UInt16 crc;
            SingleQuiryPowersupplyCmd cmd = new SingleQuiryPowersupplyCmd();
            InitHeader(typeof(SingleQuiryPowersupplyCmd), (CmdHeader)cmd, 0x02, TYPE_POWER, (UInt16)targetId);
            cmd.channel = (byte)ch;
            ar = Struct2Array<SingleQuiryPowersupplyCmd>(cmd);
            crc = CalcCRC16(0, ar, ar.Length - 2);
            ar[ar.Length - 2] = (byte)(crc >> 8);
            ar[ar.Length - 1] = (byte)(crc & 0xff);
            return ar;
        }

        /// <summary>
        /// 获取电源配置命令的字节数组
        /// </summary>
        /// <param name="ch">配置电源的通道</param>
        /// <param name="mode">对应通道的工作模式</param>
        /// <param name="vset">DCDC模式下的输出电压，单位‘毫伏’</param>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public byte[] PowersupplyConfig(PowersupplyChannel ch, PowerMode mode, int vset = 5000, int targetId = 0)
        {
            byte[] ar = null;
            UInt16 crc;
            PowersupplyConfigCmd cmd = new PowersupplyConfigCmd();
            InitHeader(typeof(PowersupplyConfigCmd), (CmdHeader)cmd, 0x01, TYPE_POWER, (UInt16)targetId);
            cmd.mode = (byte)mode;
            cmd.channel = (byte)ch;
            cmd.Vset_h = (byte)(vset >> 8);
            cmd.Vset_l = (byte)(vset & 0xff);
            ar = Struct2Array<PowersupplyConfigCmd>(cmd);
            crc = CalcCRC16(0, ar, ar.Length - 2);
            ar[ar.Length - 2] = (byte)(crc >> 8);
            ar[ar.Length - 1] = (byte)(crc & 0xff);
            return ar;
        }
        private SingleQuiryPowersupplyResp _singleQuiryPowersupplyResp;
        private PowersupplyConfigResp _powersupplyConfigResp;
        private ConfigLoadCurrentResp _configLoadCurrentResp;
        private SingleQuiryLoadResp _singleQuiryLoadResp;
        private RxInitConfigResp _rxInitConfigResp;
        private RxEPTConfigResp _rxEPTConfigResp;
        private SignalStrengthPacket _ssPkt;
        private EndPowerTransferPacket _eptPkt;
        private ControlErrorPacket _cePkt;
        private ReceivedPower8Packet _rp8Pkt;
        private ReceivedPower24Packet _rp24Pkt;
        private IdentificationPacket _idPkt;
        private ConfigurationPacket _cfgPkt;
        private GeneralRequestPacket _grPkt;
        private SpecificRequstPacket _srPkt;
        private FODStatusPacket _fsPkt;
        private HWPropPacket _hwpPkt;
        private SSProprietaryPacket _sspPkt;
        private SSHandshakePacket _sshsPkt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dca"></param>
        /// <param name="index"></param>
        public delegate void PacketReceivedHandler(HWCommandAnalyzer dca, int index);
        /// <summary>
        /// 指示已接收到RX上传的SS报文，并封装在<see cref="SSPacket"/>属性中。
        /// </summary>
        public event PacketReceivedHandler SignalStrength_Received;
        /// <summary>
        /// 获取接收到的SS报文。
        /// </summary>
        public SignalStrengthPacket SSPacket { get { return _ssPkt; } }
        /// <summary>
        /// 指示已接收到RX上传的CE报文，并封装在<see cref="CEPacket"/>属性中。
        /// </summary>
        public event PacketReceivedHandler ControlError_Received;
        /// <summary>
        /// 
        /// </summary>
        public ControlErrorPacket CEPacket { get { return _cePkt; } }
        /// <summary>
        /// 指示已接收到RX上传的8bitRP报文，并封装在<see cref="RP8Packet"/>属性中。
        /// </summary>
        public event PacketReceivedHandler ReceivedPower8b_Received;
        /// <summary>
        /// 
        /// </summary>
        public ReceivedPower8Packet RP8Packet { get { return _rp8Pkt; } }
        /// <summary>
        /// 指示已接收到RX上传的24bitRP报文，并封装在<see cref="RP24Packet"/>属性中。
        /// </summary>
        public event PacketReceivedHandler ReceivedPower24b_Received;
        /// <summary>
        /// 
        /// </summary>
        public ReceivedPower24Packet RP24Packet { get { return _rp24Pkt; } }
        /// <summary>
        /// 指示已接收到RX上传的Identification报文，并封装在<see cref="IDPacket"/>属性中。
        /// </summary>
        public event PacketReceivedHandler Identification_Received;
        /// <summary>
        /// 
        /// </summary>
        public IdentificationPacket IDPacket { get { return _idPkt; } }
        /// <summary>
        /// 指示已接收到RX上传的Configuration报文，并封装在<see cref="CFGPacket"/>属性中。
        /// </summary>
        public event PacketReceivedHandler Configuration_received;
        /// <summary>
        /// 
        /// </summary>
        public ConfigurationPacket CFGPacket { get { return _cfgPkt; } }
        /// <summary>
        /// 指示已接收到RX上传的EPT报文，并封装在<see cref="EPTPacket"/>属性中。
        /// </summary>
        public event PacketReceivedHandler EPT_Received;
        /// <summary>
        /// 
        /// </summary>
        public EndPowerTransferPacket EPTPacket { get { return _eptPkt; } }
        /// <summary>
        /// 指示已接收到RX上传的GeneralRequest报文，并封装在<see cref="GRPacket"/>属性中。
        /// </summary>
        public event PacketReceivedHandler GeneralRequest_Received;
        /// <summary>
        /// 
        /// </summary>
        public GeneralRequestPacket GRPacket { get { return _grPkt; } }
        /// <summary>
        /// 指示已接收到RX上传的SpecificRequest报文，并封装在<see cref="RP8Packet"/>属性中。
        /// </summary>
        public event PacketReceivedHandler SpecificRequest_Received;
        /// <summary>
        /// 
        /// </summary>
        public SpecificRequstPacket SRPacket { get { return _srPkt; } }
        /// <summary>
        /// 指示已接收到RX上传的FODStatus报文，并封装在<see cref="FSPacket"/>属性中。
        /// </summary>
        public event PacketReceivedHandler FODStatus_Received;

        /// <summary>
        /// 
        /// </summary>
        public FODStatusPacket FSPacket { get { return _fsPkt; } }
        /// <summary>
        /// 
        /// </summary>
        public event PacketReceivedHandler HWPP_Recevied;
        /// <summary>
        /// 指示已接收到RX上传的HWProprietary报文，并封装在<see cref="HWPPacket"/>属性中。
        /// </summary>
        public HWPropPacket HWPPacket { get { return _hwpPkt; } }
        /// <summary>
        /// 
        /// </summary>
        public event PacketReceivedHandler SSHandshake_Received;
        /// <summary>
        /// 
        /// </summary>
        public SSHandshakePacket SSHSPacket { get { return _sshsPkt; } }
        public event PacketReceivedHandler SSProprietary_Received;
        public SSProprietaryPacket SSPPacket { get { return _sspPkt; } }
        /// <summary>
        /// 指示已接收到E-Load上传的配置反馈消息，并封装在<see cref="ConfigLoadResp"/>属性中。
        /// </summary>
        public event PacketReceivedHandler ConfigLoadResponse_Received;
        /// <summary>
        /// 
        /// </summary>
        public ConfigLoadCurrentResp ConfigLoadResp { get { return _configLoadCurrentResp; } }
        /// <summary>
        /// 指示已接收到E-Load上传的单次查询反馈消息，并封装在<see cref="SingleQuiryResult"/>属性中。
        /// </summary>
        public event PacketReceivedHandler SingleQuiryResponse_Received;
        /// <summary>
        /// 
        /// </summary>
        public SingleQuiryLoadResp SingleQuiryResult { get { return _singleQuiryLoadResp; } }
        /// <summary>
        /// 指示已接收到RX上传的模式设置反馈消息，并封装在<see cref="RxModeResp"/>属性中。
        /// </summary>
        public event PacketReceivedHandler RxModeResponse_Received;
        /// <summary>
        /// 
        /// </summary>
        public RxInitConfigResp RxModeResp { get { return _rxInitConfigResp; } }
        /// <summary>
        /// 
        /// </summary>
        public event PacketReceivedHandler RxEPTResponse_Received;

        /// <summary>
        /// 指示已接收到RX上传的EPT设置反馈消息，并封装在<see cref="RxEPTResp"/>属性中。
        /// </summary>
        public RxEPTConfigResp RxEPTResp { get { return _rxEPTConfigResp; } }

        /// <summary>
        /// 
        /// </summary>
        public event PacketReceivedHandler PowersupplyConfigResponse_Received;
        /// <summary>
        /// 
        /// </summary>
        public PowersupplyConfigResp PowersupplyConfigResp { get { return _powersupplyConfigResp; } }

        public event PacketReceivedHandler SingleQuiryPowersupplyResponse_Received;

        public SingleQuiryPowersupplyResp PowersupplyResp { get { return _singleQuiryPowersupplyResp; } }
        /// <summary>
        /// 分析接收到报文，该函数可以解析RX，E-Load和程控电源的报文，并会触发对应的事件响应。
        /// </summary>
        /// <param name="index">用于指定设备序号</param>
        /// <param name="data">报文数据包</param>
        /// <param name="count">报文长度</param>
        public void Analyzer(int index, byte[] data, int count)
        {

            UInt16 crc = CalcCRC16(0, data, count - 2);
            if (data[5] != TYPE_RX)
            {
                if (crc != data[count - 2] * 256 + data[count - 1])
                {
                    Console.WriteLine("device frame error......................................");
                    return;
                }
            }
            /* byte[5] localtype, byte[9] ctrlcode */
            switch (data[5])
            {
                case TYPE_RX:
                    {
                        if (data[9] == 0xDA)
                        {
                            /* byte[14] Qi packet header*/
                            switch (data[14])
                            {
                                case 0x01:// SignalStrength
                                    _ssPkt = Array2Struct<SignalStrengthPacket>(data);
                                    SignalStrength_Received?.Invoke(this, index);
                                    break;
                                case 0x02:// EPT
                                    _eptPkt = Array2Struct<EndPowerTransferPacket>(data);
                                    EPT_Received?.Invoke(this, index); break;
                                case 0x03:// CE
                                    _cePkt = Array2Struct<ControlErrorPacket>(data);
                                    ControlError_Received?.Invoke(this, index);
                                    break;
                                case 0x04:// RP-8bit
                                    _rp8Pkt = Array2Struct<ReceivedPower8Packet>(data);
                                    ReceivedPower8b_Received?.Invoke(this, index);
                                    break;
                                case 0x31:// RP-24bit
                                    _rp24Pkt = Array2Struct<ReceivedPower24Packet>(data);
                                    ReceivedPower24b_Received?.Invoke(this, index);
                                    break;
                                case 0x51:// Configuration
                                    _cfgPkt = Array2Struct<ConfigurationPacket>(data);
                                    Configuration_received?.Invoke(this, index);
                                    break;
                                case 0x71:// Identification
                                    _idPkt = Array2Struct<IdentificationPacket>(data);
                                    Identification_Received?.Invoke(this, index);
                                    break;
                                case 0x07:// GeneralReqeust
                                    _grPkt = Array2Struct<GeneralRequestPacket>(data);
                                    GeneralRequest_Received?.Invoke(this, index);
                                    break;
                                case 0x20:// SpecificRequest
                                    _srPkt = Array2Struct<SpecificRequstPacket>(data);
                                    SpecificRequest_Received?.Invoke(this, index);
                                    break;
                                case 0x22:// FODStatus
                                    _fsPkt = Array2Struct<FODStatusPacket>(data);
                                    FODStatus_Received?.Invoke(this, index);
                                    break;
                                case 0x48:
                                    _hwpPkt = Array2Struct<HWPropPacket>(data);
                                    HWPP_Recevied?.Invoke(this, index);
                                    break;
                                case 0x18:
                                    _sshsPkt = Array2Struct<SSHandshakePacket>(data);
                                    SSHandshake_Received?.Invoke(this, index);
                                    break;
                                case 0x28:
                                    _sspPkt = Array2Struct<SSProprietaryPacket>(data);
                                    SSProprietary_Received?.Invoke(this, index);
                                    break;
                            }
                        }
                        else if (data[9] == 0xA1)
                        {
                            _rxInitConfigResp = Array2Struct<RxInitConfigResp>(data);
                            RxModeResponse_Received?.Invoke(this, index);
                        }
                        else if (data[9] == 0xA2)
                        {
                            _rxEPTConfigResp = Array2Struct<RxEPTConfigResp>(data);
                            RxEPTResponse_Received?.Invoke(this, index);
                        }
                    }
                    break;
                case TYPE_LOAD:
                    {
                        if (data[9] == 0x01)
                        {
                            _configLoadCurrentResp = Array2Struct<ConfigLoadCurrentResp>(data);
                            ConfigLoadResponse_Received?.Invoke(this, index);
                        }
                        else if (data[9] == 0x02)
                        {
                            _singleQuiryLoadResp = Array2Struct<SingleQuiryLoadResp>(data);
                            SingleQuiryResponse_Received?.Invoke(this, index);
                        }
                    }
                    break;
                case TYPE_POWER:
                    if (data[9] == 0x01)
                    {
                        _powersupplyConfigResp = Array2Struct<PowersupplyConfigResp>(data);
                        PowersupplyConfigResponse_Received?.Invoke(this, index);
                    }
                    else if (data[9] == 0x02)
                    {
                        _singleQuiryPowersupplyResp = Array2Struct<SingleQuiryPowersupplyResp>(data);
                        if (_singleQuiryPowersupplyResp != null)
                            SingleQuiryPowersupplyResponse_Received?.Invoke(this, index);
                    }
                    break;
            }
        }
    }
}
