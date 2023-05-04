using System;

namespace eReaderConverter.Redundant
{
    // 標数2の有限体の実装.
    sealed partial class GaloisField
    {
        // 原始多項式X^8+X^7+X^2+X+1をビット列に変換したもの. 0x187 = 110000111(2).
        private const uint primitivePolynomial = 0x187u;

        // 2元体上の8次元ベクトルで元を指定.
        public static GaloisField GetElementByVector(byte v) => vector[v];

        // 原始元の指数で元を指定. 零元は255で指定する.
        public static GaloisField GetElementByExponent(byte i) => exponent[i];

        // 零元. 直接取得出来たらなんか嬉しそうな気がしたので.
        public static GaloisField Zero { get; }

        private readonly static GaloisField[] vector = new GaloisField[256];
        private readonly static GaloisField[] exponent = new GaloisField[256];

        static GaloisField()
        {
            for ((byte i, uint v) = (0, 1); i < 255; i++, v <<= 1)
            {
                // mod (pp).
                if (v > 0xFF) v ^= primitivePolynomial;

                vector[v] = exponent[i] = new GaloisField((byte)v, i);
            }
            // ガロア体の零元. 指数255は指数0に等しいが, 便宜的に255を割り当てる.
            Zero = vector[0] = exponent[255] = new GaloisField(255, 0);
        }

        // イコールはオブジェクトとしての同一性で判定していいのでオーバーロードしていない.

        // 2元体においては足し算も引き算も同じ.
        // ベクトルの加法 = bitwise xor.
        public static GaloisField operator +(GaloisField a, GaloisField b) => vector[a.Vector ^ b.Vector];
        public static GaloisField operator -(GaloisField a, GaloisField b) => vector[a.Vector ^ b.Vector];

        public static GaloisField operator *(GaloisField a, GaloisField b) => (a.IsZero || b.IsZero) ? Zero : exponent[(a.Exponent + b.Exponent) % 0xFF];
        public static GaloisField operator /(GaloisField a, GaloisField b) => a * b.Inverse();
    }

    partial class GaloisField
    {
        // 原始元による基底でベクトル空間と見なしたときの係数.
        public byte Vector { get; }

        // 原始元の指数で表した場合.
        public byte Exponent { get; }

        public bool IsZero { get => Exponent == 255; }

        // 乗法逆元を取得する. 零元でこれを呼ぶと怒る.
        public GaloisField Inverse()
            => IsZero ? 
                throw new DivideByZeroException() : 
                GetElementByExponent((byte)(Exponent == 0 ? 0 : (255 - Exponent)));

        // n乗は指数をn倍すればよい.
        public GaloisField Pow(byte n) => IsZero ? Zero : exponent[Exponent * n % 0xFF];

        private GaloisField(byte vec, byte exp) => (Vector, Exponent) = (vec, exp);
    }
}
