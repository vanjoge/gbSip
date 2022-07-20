using GB28181.Enums;
using SQ.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace GB28181.PTZ
{
    /// <summary>
    /// PTZ指令
    /// </summary>
    public class PTZCmd
    {
        byte[] data = new byte[8] { 0xA5, 0x0F, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
        /// <summary>
        /// 地址
        /// </summary>
        public ushort Address
        {
            get
            {
                return (ushort)(((data[6] & 0xF) << 8) | data[2]);
            }
            set
            {
                data[2] = (byte)(value & 0xFF);
                data[6] = (byte)((data[6] & 0xF0) | (value >> 8 & 0xF));
            }
        }
        /// <summary>
        /// 放大倍数
        /// </summary>
        public byte? ZoomIn
        {
            get
            {
                if ((data[3] & 0x10) > 0)
                {
                    return (byte)(data[6] >> 4);
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    //bit4 => 1 bit5 => 0
                    data[3] = (byte)((data[3] & 0xF) | 0x10);
                    data[6] = (byte)((data[6] & 0xF) | ((value & 0xF) << 4));
                }
                else
                {
                    //bit4 => 0
                    data[3] = (byte)(data[3] & 0x2F);
                    if (!ZoomOut.HasValue)
                    {
                        data[6] = (byte)(data[6] & 0xF);
                    }
                }
            }
        }
        /// <summary>
        /// 缩小倍数
        /// </summary>
        public byte? ZoomOut
        {
            get
            {
                if ((data[3] & 0x20) > 0)
                {
                    return (byte)(data[6] >> 4);
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    //bit4 => 0 bit5 => 1
                    data[3] = (byte)((data[3] & 0xF) | 0x20);
                    data[6] = (byte)((data[6] & 0xF) | ((value & 0xF) << 4));
                }
                else
                {
                    //bit5 => 0
                    data[3] = (byte)(data[3] & 0x1F);
                    if (!ZoomIn.HasValue)
                    {
                        data[6] = (byte)(data[6] & 0xF);
                    }
                }
            }
        }
        /// <summary>
        /// 上
        /// </summary>
        public byte? Up
        {
            get
            {
                if ((data[3] & 0x8) > 0)
                {
                    return data[5];
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    //bit2 => 0 bit3 => 1
                    data[3] = (byte)((data[3] & 0x33) | 0x8);
                    data[5] = value.Value;
                }
                else
                {
                    //bit3 => 0
                    data[3] = (byte)(data[3] & 0x37);
                    if (!Down.HasValue)
                    {
                        data[5] = 0;
                    }
                }
            }
        }
        /// <summary>
        /// 下
        /// </summary>
        public byte? Down
        {
            get
            {
                if ((data[3] & 0x4) > 0)
                {
                    return data[5];
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    //bit2 => 1 bit3 => 0
                    data[3] = (byte)((data[3] & 0x33) | 0x4);
                    data[5] = value.Value;
                }
                else
                {
                    //bit2 => 0
                    data[3] = (byte)(data[3] & 0x3B);
                    if (!Up.HasValue)
                    {
                        data[5] = 0;
                    }
                }
            }
        }
        /// <summary>
        /// 左
        /// </summary>
        public byte? Left
        {
            get
            {
                if ((data[3] & 0x2) > 0)
                {
                    return data[4];
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    //bit0 => 0 bit1 => 1
                    data[3] = (byte)((data[3] & 0x3C) | 0x2);
                    data[4] = value.Value;
                }
                else
                {
                    //bit1 => 0
                    data[3] = (byte)(data[3] & 0x3D);
                    if (!Right.HasValue)
                    {
                        data[4] = 0;
                    }
                }
            }
        }
        /// <summary>
        /// 右
        /// </summary>
        public byte? Right
        {
            get
            {
                if ((data[3] & 0x1) > 0)
                {
                    return data[4];
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    //bit0 => 1 bit1 => 0
                    data[3] = (byte)((data[3] & 0x3C) | 0x1);
                    data[4] = value.Value;
                }
                else
                {
                    //bit0 => 0
                    data[3] = (byte)(data[3] & 0x3E);
                    if (!Left.HasValue)
                    {
                        data[4] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 转PTZCMD
        /// </summary>
        /// <returns></returns>
        public string ToPTZStr()
        {
            data[7] = 0;
            for (int i = 0; i < 7; i++)
            {
                data[7] += data[i];
            }
            return data.BytesToHexString();
        }
    }
}