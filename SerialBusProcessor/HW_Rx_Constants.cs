using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialBusProcessor
{
  public partial  class HW_Rx_Operation
    {
        const UInt16 RX_MODE = 0X0005;
        const UInt16 CS_CONFIG = 0X0006;
        const UInt16 CD_CONFIG = 0x0007;
        const UInt16 COMM_CONFIG = 0x0008;
        const UInt16 INLOAD_CONFIG = 0x0009;
        const UInt16 TARGET_OUTPUT_VOLT_H = 0X000A;
        const UInt16 TARGET_OUTPUT_VOLT_L = 0X000B;
        const UInt16 CEP_FAST_INTERVAL = 0x000C;
        const UInt16 CEP_SLOW_INTERVAL = 0X000E;
        const UInt16 RPP_INTERVAL = 0x0010;
        const UInt16 ASK_TYPE = 0X0100;
        const UInt16 ASK_HEADER = 0X0101;
        const UInt16 ASK_MESSAGE_START = 0x0102;
        const UInt16 FSK_TYPE = 0X0140;
        const UInt16 FSK_HEADER = 0X0141;
        const UInt16 FSK_MESSAGE_START = 0X0142;
        const UInt16 SIGNAL_STRENGTH_HEADER = 0X0180;
        const UInt16 EXPECTED_UMAX = 0x0181;
        const UInt16 SIGNAL_STRENGTH_VALUE = 0x0182;
        const UInt16 IDENTIFICATION_HEADER = 0x0200;
        const UInt16 QI_VERSION = 0x0201;
        const UInt16 MANUFACTURER_CODE = 0x0202;
        const UInt16 BASIC_DEVICE_CODE = 0x0204;
        const UInt16 EXT_ID_HEADER = 0X0208;
        const UInt16 EXT_DEVICE_ID = 0X0209;
        const UInt16 HOLD_OFF_HEADER = 0X0211;
        const UInt16 HOLD_OFF_TIME = 0X0212;

        const UInt16 CONFIG_HEADER = 0X0300;
        const UInt16 CONFIG_MESSAGE = 0X0301;
        const UInt16 CONFIG_FSK_RESP = 0x0306;

        const UInt16 RECT_VOLTAGE = 0x0607;
        const UInt16 RECT_CURRENT = 0X0609;
        const UInt16 VOUT_VOLTAGE = 0x060b;
        const UInt16 VOUT_CURRENT = 0X060D;
        const UInt16 LC_FREQUENCY = 0X060F;
        public enum FSKResponse
        {
            NotNeg = 0x00,
            ACK ,
            NAK ,
            ND ,
            DemodeFailed,
            ReadFailed = 0xff
        }

        public enum RxMode
        {
            /// <summary>
            /// 读取失败
            /// </summary>
            ReadFailed = -1,
            /// <summary>
            /// Qi Bpp 
            /// </summary>
            Bpp,
            /// <summary>
            /// Samsung PPDE
            /// </summary>
            SFC,
            /// <summary>
            /// Apple Fast Charge
            /// </summary>
            AFC,
            /// <summary>
            /// Qi Epp
            /// </summary>
            EPP
        }
        public enum Cs_Config
        {
            All_Disable = 0,
            Channel_0 = 0x01,
            Channel_1 = 0x02,
            Channel_2 = 0x04,
            Channel_3 = 0x08,
            Channel_4 = 0x10
        }

        public enum Cd_Config
        {
            All_Disable = 0,
            Channel_0 = 0x01,
            Channel_1 = 0x02,
            Channel_2 = 0x04,
            Channel_3 = 0x08
        }

        public enum Inload_Config
        {
            All_Disable = 0,
            Channel_0 = 0x01,
            Channel_1 = 0x02,
            Channel_2 = 0x04,
            Channel_3 = 0x08,
            Channel_4 = 0x10,
            Channel_5 = 0x20
        }
    }
}
