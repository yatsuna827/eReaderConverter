using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;

namespace eReaderConverter
{
    internal static class BitmapExtension
    {
        private static readonly bool[][] addressBar = new bool[29][]
        {
            new bool[26] { true, false, false, false, false, false, false, false, false, false, true, true, false, false, true, true, false, false, false, false, true, true, false, false, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, false, true, true, false, true, false, false, false, true, true, false, false, true, false, true, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, false, true, true, false, true, true, true, true, true, false, true, false, false, false, true, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, false, true, true, true, false, false, true, false, false, false, false, false, false, true, true, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, false, true, true, true, false, true, false, true, false, true, true, false, true, true, true, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, false, true, true, true, true, false, true, true, true, false, true, true, true, true, false, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, false, true, true, true, true, true, false, false, true, true, false, true, false, true, false, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, false, false, false, false, true, false, true, false, false, false, false, false, true, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, false, false, false, true, false, true, true, true, true, false, true, false, true, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, false, false, true, false, true, true, false, false, true, true, true, false, false, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, false, false, true, true, false, false, false, true, false, true, false, false, false, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, false, true, false, false, false, true, true, true, true, true, false, true, false, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, false, true, false, true, true, false, true, false, false, true, true, true, false, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, false, true, true, false, false, false, false, true, false, false, true, true, true, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, false, true, true, true, true, true, false, false, true, false, false, true, true, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, true, false, false, false, true, true, false, false, false, false, false, true, true, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, true, false, false, true, false, false, false, true, true, false, true, true, true, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, true, false, true, false, true, false, true, false, true, true, true, true, false, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, true, false, true, true, false, true, true, true, false, true, false, true, false, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, true, true, false, false, false, false, false, true, true, true, false, false, false, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, true, true, false, true, true, true, false, false, false, true, true, false, false, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, true, true, true, false, false, true, true, true, false, false, true, false, true, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, false, true, true, true, true, true, false, true, false, true, false, false, false, true, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, true, false, false, false, false, false, true, true, false, false, false, true, false, false, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, true, false, false, false, true, true, false, true, true, true, false, false, false, false, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, true, false, false, true, false, false, false, false, false, true, true, false, false, true, true, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, true, false, false, true, true, true, true, false, true, false, true, true, false, true, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, true, false, true, false, false, true, false, true, true, true, true, true, true, true, false, false },
            new bool[] { true, false, false, false, false, false, false, false, false, true, true, false, true, false, true, false, true, true, false, false, true, false, true, true, true, false }
        };
        private readonly static (int X, int Y)[] toDotBlockCoordinate;

        static BitmapExtension()
        {
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

        public static Bitmap DrawDot(this bool[][] dotData, int margin = 0)
        {
            if (margin < 0) throw new ArgumentException("margin cannot be negative.");

            const int w = 35;
            const int h = 40;

            var imgWidth = w * 28 + 5 + margin * 2;
            var imgHeight = h + margin * 2;

            var bmp = new Bitmap(imgWidth, imgHeight, PixelFormat.Format1bppIndexed);
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly, bmp.PixelFormat);

            var stride = bmpData.Stride;
            var pixels = new byte[stride * bmp.Height];
            // 白で塗りつぶす
            for (var i = 0; i < pixels.Length; i++) pixels[i] = 0xFF;

            // 29個の大ドットと、大ドットを縦に結ぶアドレスバーを描画する
            for (int i = 0; i < 29; i++)
            {
                var offsetX = i * 35 + margin;
                var offsetY = margin;

                pixels.DotBigDot(stride, offsetX, offsetY);
                pixels.DotBigDot(stride, offsetX, offsetY + 35);

                for (int j = 0; j < addressBar[i].Length; j++)
                    if (addressBar[i][j]) pixels.SetPixel(stride, offsetX + 2, offsetY + 7 + j);
            }

            // 大ドットを横に結ぶ点線を描画する
            for (int i = 0; i < 28; i++)
            {
                var offsetX = i * 35 + margin;
                var offsetY = margin;

                for (int k = 0; k < 6; k++)
                {
                    pixels.SetPixel(stride, offsetX + 8 + k * 2, offsetY + 2);
                    pixels.SetPixel(stride, offsetX + 21 + k * 2, offsetY + 2);
                    pixels.SetPixel(stride, offsetX + 8 + k * 2, offsetY + 37);
                    pixels.SetPixel(stride, offsetX + 21 + k * 2, offsetY + 37);
                }
            }

            for (int i = 0; i < dotData.Length; i++)
            {
                var blockData = dotData[i];
                var offsetX = i * 35 + margin;
                var offsetY = margin;
                for (int k = 0; k < blockData.Length; k++)
                {
                    if (blockData[k])
                        pixels.SetPixel(stride, offsetX + toDotBlockCoordinate[k].X, offsetY + toDotBlockCoordinate[k].Y);
                }
            }

            return bmp;
        }
        public static Bitmap DrawDot2x(this bool[][] dotData, int margin = 0)
        {
            const int w = 35;
            const int h = 40;

            var imgWidth = (w * 28 + 5) * 2 + margin * 2;
            var imgHeight = h * 2 + margin * 2;

            var bmp = new Bitmap(imgWidth, imgHeight, PixelFormat.Format1bppIndexed);
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly, bmp.PixelFormat);

            var stride = bmpData.Stride;
            var pixels = new byte[stride * bmp.Height];
            // 白で塗りつぶす
            for (var i = 0; i < pixels.Length; i++) pixels[i] = 0xFF;

            // 29個の大ドットと、大ドットを縦に結ぶアドレスバーを描画する
            for (int i = 0; i < 29; i++)
            {
                var offsetX = (i * 35 * 2) + margin;
                var offsetY = margin;

                pixels.DotBigDot2x(stride, offsetX, offsetY);
                pixels.DotBigDot2x(stride, offsetX, offsetY + 35 * 2);

                for (int j = 0; j < addressBar[i].Length; j++)
                {
                    if (addressBar[i][j])
                        pixels.Dot2x(stride, offsetX, offsetY, 2, j + 7);
                }
            }

            // 大ドットを横に結ぶ点線を描画する
            for (int i = 0; i < 28; i++)
            {
                var offsetX = (i * 35 * 2 + margin);
                var offsetY = margin;

                for (int k = 0; k < 6; k++)
                {
                    pixels.Dot2x(stride, offsetX, offsetY, 8 + k * 2, 2);
                    pixels.Dot2x(stride, offsetX, offsetY, 21 + k * 2, 2);
                    pixels.Dot2x(stride, offsetX, offsetY, 8 + k * 2, 37);
                    pixels.Dot2x(stride, offsetX, offsetY, 21 + k * 2, 37);
                }
            }

            for (int i = 0; i < dotData.Length; i++)
            {
                var blockData = dotData[i];
                var offsetX = margin + i * 35 * 2;
                var offsetY = margin;
                for (int k = 0; k < blockData.Length; k++)
                {
                    if (blockData[k])
                        pixels.Dot2x(stride, offsetX, offsetY, toDotBlockCoordinate[k].X, toDotBlockCoordinate[k].Y);
                }
            }

            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private static void DotBigDot(this byte[] bmp, int stride, int offsetX, int offsetY)
        {
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    if (x == 0 && y == 0) continue;
                    if (x == 4 && y == 0) continue;
                    if (x == 0 && y == 4) continue;
                    if (x == 4 && y == 4) continue;
                    bmp.SetPixel(stride, x + offsetX, y + offsetY);
                }
            }
        }
        private static void DotBigDot2x(this byte[] bmp, int stride, int offsetX, int offsetY)
        {
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    if (x == 0 && y == 0) continue;
                    if (x == 4 && y == 0) continue;
                    if (x == 0 && y == 4) continue;
                    if (x == 4 && y == 4) continue;
                    bmp.SetPixel(stride, 2 * x + offsetX, 2 * y + offsetY);
                    bmp.SetPixel(stride, 2 * x + 1 + offsetX, 2 * y + offsetY);
                    bmp.SetPixel(stride, 2 * x + offsetX, 2 * y + 1 + offsetY);
                    bmp.SetPixel(stride, 2 * x + 1 + offsetX, 2 * y + 1 + offsetY);
                }
            }
        }
        private static void Dot2x(this byte[] bmp, int stride, int offsetX, int offsetY, int x, int y)
        {
            var _x = offsetX + x * 2;
            var _y = offsetY + y * 2;

            bmp.SetPixel(stride, _x + 1, _y + 1);
        }

        private static void SetPixel(this byte[] imageData, int stride, int x, int y)
        {
            var byteIndex = x / 8;
            var bitIndex = 7 - (x % 8);
            var pixelIndex = y * stride + byteIndex;
            var mask = (byte)(1 << bitIndex);

            // 指定された座標に対応するbitを0にする
            imageData[pixelIndex] &= (byte)~mask;
        }
    }
}
