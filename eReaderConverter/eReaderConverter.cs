﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using eReaderConverter.Redundant;

namespace eReaderConverter
{
    public static class EReaderConverter
    {
        private readonly static byte[] shortDotCodeHeader = new byte[0x30]
        {
            0x00, 0x30, 0x01, 0x01, 0x00, 0x01, 0x05, 0x10,
            0x00, 0x00, 0x10, 0x12, 0x00, 0x00, 0x02, 0x00,
			0x00, 0x00, 0x10, 0x47, 0xEF, 0x19, 0x00, 0x00, 
            0x00, 0x08, 0x4E, 0x49, 0x4E, 0x54, 0x45, 0x4E, 
            0x44, 0x4F, 0x00, 0x22, 0x00, 0x09, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x57,
		};
        private readonly static byte[] longDotCodeHeader = new byte[0x30] {
            0x00, 0x30, 0x01, 0x02, 0x00, 0x01, 0x08, 0x10,
            0x00, 0x00, 0x10, 0x12, 0x00, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x10, 0x9A, 0x99, 0x19, 0x00, 0x00,
            0x00, 0x08, 0x4E, 0x49, 0x4E, 0x54, 0x45, 0x4E,
            0x44, 0x4F, 0x00, 0x22, 0x00, 0x09, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x57,
		};

        private readonly static byte[] shortHeader = new byte[0x18] {
            0x00, 0x02, 0x00, 0x01, 0x40, 0x10, 0x00, 0x1C,
            0x10, 0x6F, 0x40, 0xDA, 0x39, 0x25, 0x8E, 0xE0,
            0x7B, 0xB5, 0x98, 0xB6, 0x5B, 0xCF, 0x7F, 0x72
        };
        private readonly static byte[] longHeader = new byte[0x18] {
            0x00, 0x03, 0x00, 0x19, 0x40, 0x10, 0x00, 0x2C,
            0x0E, 0x88, 0xED, 0x82, 0x50, 0x67, 0xFB, 0xD1,
            0x43, 0xEE, 0x03, 0xC6, 0xC6, 0x2B, 0x2C, 0x93
        };

        private const int SHORT_CODE = 18, LONG_CODE = 28;

        // 1枚で完結するタイプのみ対応.
        // 複数枚必要なタイプは知らん.
        public static byte[] Bin2Raw(this byte[] bin)
        {
            var dotCodeLength = (bin.Length == 0x81c) ? LONG_CODE : SHORT_CODE; // Multiの場合は1セグメントの長さで見ないとおかしくなる.

            // 変換処理.
            var dotCodeTemp = new byte[0xB60];
            var interleave = (dotCodeLength == LONG_CODE) ? 0x2C : 0x1C;
            for (int i = 0; i < interleave; i++)
            {
                var data = (i == 0 ? RawHeader(bin, dotCodeLength) : RawBody(bin, i - 1)).AppendErrorInfo();
                for (int j = 0; j < 0x40; j++) dotCodeTemp[j * interleave + i] = data[j];
            }

            // 書込処理.
            var raw = new byte[0xB60];
            // ヘッダの書込.
            var header = dotCodeLength == LONG_CODE ? longHeader : shortHeader;
            for (int i = 0; i < dotCodeLength; i++)
            {
                raw[i * 0x68] = header[(i * 2) % 0x18];
                raw[(i * 0x68) + 1] = header[(i * 2 + 1) % 0x18];
            }

            var dotcodepointer = 0;
            var temp1 = (dotCodeLength == LONG_CODE) ? 0xB38 : 0x724;
            var temp2 = (dotCodeLength == LONG_CODE) ? 0xB60 : 0x750;
            // ボディの書込.
            for (int i = 2; i < temp1; i++)
            {
                if ((i % 0x68) == 0) i += 2;
                raw[i] = dotCodeTemp[dotcodepointer++];
            }
            // フッターの書込.
            for (int i = temp1; i < temp2; i++) raw[i] = (byte)(i & 0xFF);

            return raw;
        }

        private static byte[] RawHeader(byte[] bindata, int codeLength)
        {
            var data = (codeLength == LONG_CODE ? longDotCodeHeader : shortDotCodeHeader).ToArray();

            data[0x0D] = bindata[0];
            data[0x0C] = bindata[1];
            data[0x11] = bindata[2];
            data[0x10] = bindata[3];

            data[0x26] = bindata[4];
            data[0x27] = bindata[5];
            data[0x28] = bindata[6];
            data[0x29] = bindata[7];
            data[0x2A] = bindata[8];
            data[0x2B] = bindata[9];
            data[0x2C] = bindata[10];
            data[0x2D] = bindata[11];

            // checksumの埋め込み.
            {
                byte temp = 0;
                for (int i = 0; i < 12; i++)
                    temp ^= bindata[i];
                data[0x2E] = temp;
            }

            data[0x12] = 0x10;
            data[0x02] = 1;

            {
                var temp = 0;
                for (int i = 0x0C; i < 0x81C; i++)
                    temp += ((i & 1) == 1) ? bindata[i] : (bindata[i] << 8);
                temp &= 0xFFFF;
                temp ^= 0xFFFF;
                data[0x13] = (byte)((temp >> 8) & 0xFF);
                data[0x14] = (byte)(temp & 0xFF);
            }
            {
                byte temp = 0;
                for (int i = 0; i < 0x2F; i++)
                    temp += data[i];

                for (int i = 1; i < 0x2C; i++)
                {
                    byte temp2 = 0;
                    for (int k = 0; k < 0x30; k++) temp2 ^= bindata[((i - 1) * 0x30) + k + 0x0C];
                    temp += temp2;
                }

                data[0x2F] = (byte)(temp ^ 0xFF);
            }

            return data;
        }
        private static byte[] RawBody(byte[] bindata, int p)
        {
            var data = new byte[0x30];
            for (int i = 0; i < data.Length; i++)
                data[i] = bindata[(p * 0x30) + 0x0C + i];

            return data;
        }

        private readonly static int[] modulation8to10, modulation10to8;
        private readonly static (int X, int Y)[] toDotBlockCoordinate;

        static EReaderConverter()
        {
            modulation8to10 = new int[]
            {
                0x0, 0x1, 0x2, 0x12, 0x4, 0x5, 0x6, 0x16,
                0x8, 0x9, 0xA, 0x14, 0xC, 0xD, 0x11, 0x10
            };
            modulation10to8 = Enumerable.Repeat(-1, 0x17).ToArray();
            for (int i = 0; i < modulation8to10.Length; i++)
                modulation10to8[modulation8to10[i]] = i;

            var z = new List<(int, int)>();
            for (int i = 7; i <= 32; i++) z.Add((i, 4));
            for (int i = 7; i <= 32; i++) z.Add((i, 5));
            for (int i = 7; i <= 32; i++) z.Add((i, 6));

            for (int k = 7; k < 33; k++) for (int i = 3; i <= 36; i++) z.Add((i, k));

            for (int i = 7; i <= 32; i++) z.Add((i, 33));
            for (int i = 7; i <= 32; i++) z.Add((i, 34));
            for (int i = 7; i <= 32; i++) z.Add((i, 35));

            toDotBlockCoordinate = z.ToArray();
        }

        // ドットコードのBitmapからrawに変換する.
        public static byte[] Bmp2Raw(this byte[] bmp)
        {
            IEnumerable<bool> ToDots(byte x)
            {
                for (int i = 0; i < 8; i++)
                    yield return ((x >> (7 - i)) & 1) == 0;
            }

            var bmpDots = Enumerable.Range(0, 36).Select(_ => new List<bool>()).ToArray();
            for (int row = 0; row < 36; row++)
                for (int i = 0; i < 124; i++)
                    bmpDots[row].AddRange(ToDots(bmp[0x3E + (35 - row) * 124 + i]));

            var dotCode = new bool[28][][];
            for (int block = 0; block < 28; block++)
            {
                dotCode[block] = new bool[36][];
                for (int row = 0; row < 36; row++)
                {
                    dotCode[block][row] = new bool[36];
                    for (int column = 0; column < 36; column++)
                    {
                        dotCode[block][row][column] = bmpDots[row][block * 35 + column];
                    }
                }
            }

            // 
            var z = toDotBlockCoordinate.Select(_ => (W: _.X - 2, H: _.Y - 2)).ToArray();
            byte[] ConvertToBytes(bool[] state)
            {
                var arr = new byte[104];
                for (int i = 0; i < 104; i++)
                {
                    int b = 0, c = 0;
                    for (int k = 0; k < 5; k++)
                    {
                        if (state[i * 10 + (4 - k)]) b |= (1 << k);
                        if (state[i * 10 + (9 - k)]) c |= (1 << k);
                    }
                    arr[i] = (byte)((modulation10to8[b] << 4) | modulation10to8[c]);
                }
                return arr;
            }

            var binary = new List<byte>();
            for (int i = 0; i < 28; i++)
                binary.AddRange(ConvertToBytes(z.Select(_ => dotCode[i][_.H][_.W]).ToArray()));

            return binary.ToArray();
        }

        private static bool[][] Raw2DotData(this byte[] raw)
        {
            if (raw.Length != 0xB60)
                throw new Exception("バイナリファイルのサイズが不正です. 0xB60 bytesである必要があります.");

            var dotData = new bool[28][];
            for (int t = 0; t < 28; t++)
            {
                var temp = new bool[1040];
                for (int i = 0; i < 104; i++)
                {
                    // 0xXY -> X Yを10bitに変換して左から順にbool化.
                    var b = modulation8to10[raw[t * 104 + i] >> 4];
                    var c = modulation8to10[raw[t * 104 + i] & 0xF];
                    for (int k = 0; k < 5; k++)
                    {
                        temp[i * 10 + k] = ((b >> (4 - k)) & 1) == 1;
                        temp[i * 10 + k + 5] = ((c >> (4 - k)) & 1) == 1;
                    }
                }
                dotData[t] = temp;
            }

            return dotData;
        }
        public static Bitmap Bin2Bmp(this byte[] bin, int margin = 0, bool x2 = false)
            => bin.Bin2Raw().Raw2Bmp(margin, x2);
        public static Bitmap Raw2Bmp(this byte[] raw, int margin = 0, bool x2 = false)
            => x2 ? raw.Raw2DotData().DrawDot2x(margin) : raw.Raw2DotData().DrawDot(margin);
    }
}
