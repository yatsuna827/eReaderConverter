using System;
using System.Linq;

namespace eReaderConverter.Redundant
{
    static class ReedSolomon
    {
        private readonly static Polynomial generationPolynomial = GenerateGenerationPolynomial();

        /// <summary>
        /// b=120, エラーサイズ=16の生成多項式.
        /// </summary>
        /// <returns></returns>
        private static Polynomial GenerateGenerationPolynomial()
        {
            var p = new Polynomial(120, 0);
            for (byte i = 121; i < 136; i++)
                p *= new Polynomial(i, 0);

            return p;
        }

        /// <summary>
        /// エラーサイズ = 16.
        /// 48byteまでのデータに16byteのエラー訂正符号を付け加える.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] AppendErrorInfo(this byte[] data)
        {
            if (data.Length > 0x30) throw new ArgumentException("入力データ長は0x30以下である必要があります");

            var res = data.ToArray().Concat(new byte[16]).ToArray();
            var I = new Polynomial(res.Reverse().Select(_ => GaloisField.GetElementByVector(_)).ToArray());
            var c = I % generationPolynomial;
            for (uint i = 0; i < 16; i++) res[0x30 + i] = (byte)(c[15 - i].Vector ^ 0xFF);

            return res;
        }
    }
}
