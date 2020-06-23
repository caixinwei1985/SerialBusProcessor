using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace STM32ISP
{
    public enum ISPACK { ISP_ACK, ISP_NACK, ISP_TO };
    public enum WriteMemoryResult { OK, CommandTO, CommandNACK, AddressTO, AddressNACK, DataTO, DataNACK, LengthError };
    public delegate bool DelegateIOCallback(byte[] buffer, int count);
    public class STMisp
    {
        public DelegateIOCallback Write;
        public DelegateIOCallback Read;

        public STMisp()
        {
        }
        private ISPACK get_ack()
        {
            byte[] ack = new byte[1];
            if (Read(ack, 1))
            {
                if (ack[0] == 0x79)
                {
                    return ISPACK.ISP_ACK;
                }
                else if (ack[0] == 0x1F)
                {
                    return ISPACK.ISP_NACK;
                }
                else
                    return ISPACK.ISP_TO;
            }
            return ISPACK.ISP_TO;
        }
        public string GetVersion()
        {
            return "";
        }
        public ISPACK Go(UInt32 target)
        {
            byte[] cmd_go = new byte[] { 0x21, 0xDE };
            byte[] cmd_app = new byte[] { (byte)(target >> 24), (byte)(target >> 16), (byte)(target >> 8), (byte)(target >> 0), 0x08 };
            Write(cmd_go, 2);
            ISPACK ack = get_ack();
            if (ack == ISPACK.ISP_ACK)
            {
                Write(cmd_app, 5);
                ack = get_ack();
                
                return ack;
            }
            return ack;
        }

        public ISPACK InitBL()
        {
            byte[] cmd_init = new byte[] { 0x7f };
            Write(cmd_init, 1);
            ISPACK ack = get_ack();
            return ack;

        }
        public ISPACK Erase()
        {
            byte[] cmd_erase = new byte[] { 0x44, 0xbb };
            byte[] cmd_spec = new byte[] { 0xff, 0xff, 0x00 };
            ISPACK ack;
            Write(cmd_erase, 2);
            ack = get_ack();
            if (ack == ISPACK.ISP_ACK)
            {
                Write(cmd_spec, 3);
                ack = get_ack();
                return ack;
            }
            return ack;
        }
        public bool Align()
        {
            byte[] a = new byte[1] { 0x00 };
            do
            {
                Write(a, 1);
            } while (get_ack() == ISPACK.ISP_TO);
            return true;
        }
        public ISPACK Read_protect()
        {
            byte[] cmd_rp = new byte[] { 0x82, 0x7d };
            Write(cmd_rp, 2);
            ISPACK ack = get_ack();
            if (ack == ISPACK.ISP_ACK)
            {
                ack = get_ack();
                return ack;

            }
            return ack;
        }

        public ISPACK Read_unprotect()
        {
            byte[] cmd_rup = new byte[] { 0x92, 0x6d };
            ISPACK ack;
            Write(cmd_rup, 2);
            // get first ACK
            ack = get_ack();
            if (ack == ISPACK.ISP_ACK)
            {
                // get second ACK
                ack = get_ack();
                return ack;
            }
            return ack;
        }
        public WriteMemoryResult WriteMemory(UInt32 addr, int count, byte[] data)
        {
            /*如果数据长度不是4的倍数*/
            if ((data.Length & 0x03) != 0)
                return WriteMemoryResult.LengthError;
            byte[] cmd_wr = new byte[] { 0x31, 0xce };
            byte[] cmd_addr = new byte[] {(byte)((addr>>24)&0xff),
                                          (byte)((addr>>16)&0xff),
                                          (byte)((addr>>08)&0xff),
                                          (byte)((addr>>00)&0xff),
                                           0x00};
            cmd_addr[4] = (byte)(cmd_addr[0] ^ cmd_addr[1] ^ cmd_addr[2] ^ cmd_addr[3]);

            byte[] cmd_data = new byte[count + 2];
            ISPACK ack;
            cmd_data[0] = (byte)(count - 1);

            cmd_data[cmd_data.Length - 1] = cmd_data[0];
            for (int i = 0; i < data.Length; i++)
            {
                cmd_data[cmd_data.Length - 1] ^= data[i];
                cmd_data[1 + i] = data[i];
            }
            Write(cmd_wr, 2);
            ack = get_ack();
            if (ack == ISPACK.ISP_ACK)
            {
                Write(cmd_addr, 5);
                ack = get_ack();
                if (ack == ISPACK.ISP_ACK)
                {
                    Write(cmd_data, cmd_data.Length);
                    ack = get_ack();
                    cmd_data = null;
                    if (ack == ISPACK.ISP_ACK)
                    {
                        return WriteMemoryResult.OK;
                    }
                    if (ack == ISPACK.ISP_TO)
                        return WriteMemoryResult.DataTO;
                    return WriteMemoryResult.DataNACK;

                }
                if (ISPACK.ISP_TO == ack)
                {
                    return WriteMemoryResult.AddressTO;
                }
                else
                {
                    return WriteMemoryResult.AddressNACK;
                }
            }
            cmd_data = null;
            if (ISPACK.ISP_TO == ack)
            {
                return WriteMemoryResult.CommandTO;
            }
            else
            {
                return WriteMemoryResult.CommandNACK;
            }
        }
    }
}
