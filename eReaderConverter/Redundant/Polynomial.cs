using System;
using System.Linq;

namespace eReaderConverter.Redundant
{
    // GaloisField上の多項式環の実装.
    class Polynomial
    {
        /// <summary>
        /// 多項式の次元を返します.
        /// </summary>
        public int Degree { get; }

        /// <summary>
        /// i次の項の係数を返します.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public GaloisField this[uint i] { get => i > Degree ? GaloisField.Zero : elements[i]; }

        public GaloisField Top { get => this[(uint)Degree]; }

        public GaloisField Substitute(GaloisField a)
        {
            var vec = 0;
            foreach (var b in Enumerable.Range(0, Degree + 1).Select(_ => elements[_] * a.Pow((byte)_)))
                vec ^= b.Vector;
            return GaloisField.GetElementByVector((byte)vec);
        }

        private readonly GaloisField[] elements;
        public static Polynomial Zero { get; } = new Polynomial();
        private Polynomial()
        {
            Degree = 0;
            elements = new GaloisField[1] { GaloisField.Zero };
        }
        public Polynomial(params GaloisField[] coeff)
        {
            // 上位の項が0で埋まっていたら乗算でバグるのでコピーしない.
            var r = coeff.Length - 1;
            while (r >= 0 && coeff[r].IsZero) r--;
            if (r < 0) throw new ArgumentException("係数をすべて0にすることはできません");

            Degree = r;

            elements = new GaloisField[r + 1];
            for (int i = 0; i <= r; i++) elements[i] = coeff[i];
        }
        public Polynomial(params byte[] coeffIndexes)
        {
            // 上位の項が0で埋まっていたら乗算でバグるのでコピーしない.
            var coeff = coeffIndexes.Select(_ => GaloisField.GetElementByExponent(_)).ToArray();
            var r = coeff.Length - 1;
            while (r >= 0 && coeff[r].IsZero) r--;
            if (r < 0) throw new ArgumentException("係数をすべて0にすることはできません");

            Degree = r;

            elements = new GaloisField[r + 1];
            for (int i = 0; i <= r; i++) elements[i] = coeff[i];
        }

        public static Polynomial operator +(Polynomial p, Polynomial q)
        {
            if (q.Degree > p.Degree) (p, q) = (q, p);

            var f = p.elements.ToArray();
            for (int i = 0; i < q.elements.Length; i++)
                f[i] += q.elements[i];

            return new Polynomial(f);
        }
        public static Polynomial operator -(Polynomial p, Polynomial q)
        {
            if (q.Degree > p.Degree) (p, q) = (q, p);

            var f = p.elements.ToArray();
            for (int i = 0; i < q.elements.Length; i++) f[i] -= q.elements[i];

            return new Polynomial(f);
        }
        public static Polynomial operator *(Polynomial p, Polynomial q)
        {
            if (q.Degree > p.Degree) (p, q) = (q, p);

            var f = p.elements;
            var g = q.elements;

            var h = Enumerable.Repeat(GaloisField.Zero, p.Degree + q.Degree + 1).ToArray();
            for (int i = 0; i < f.Length; i++)
                for (int j = 0; j < g.Length; j++)
                    h[i + j] += f[i] * g[j];

            return new Polynomial(h.ToArray());
        }
        public static Polynomial operator /(Polynomial p, Polynomial q)
        {
            if (q.Top.IsZero) throw new DivideByZeroException();

            if (q.Degree > p.Degree) return Zero;

            var f = p.elements.ToArray(); // 書き換えを行うのでコピーする.
            var g = q.elements.Reverse().ToArray();
            var div = g[0].Inverse();

            var h = new GaloisField[p.Degree - q.Degree + 1];
            for (int i = f.Length - 1; i >= g.Length - 1; i--)
            {
                var c = h[i - g.Length + 1] = f[i] * div;
                for (int j = g.Length - 1; j >= 0; j--)
                    f[i - j] -= c * g[j];
            }

            return new Polynomial(h);
        }
        public static Polynomial operator %(Polynomial p, Polynomial q)
        {
            var f = p.elements.ToArray(); // 書き換えを行うのでコピーする.
            var g = q.elements.Reverse().ToArray();
            var div = g[0].Inverse();

            for (int i = f.Length - 1; i >= g.Length - 1; i--)
                for (int j = g.Length - 1; j >= 0; j--)
                    f[i - j] -= f[i] * g[j] * div;

            if (f.All(_ => _.IsZero)) return Zero;

            return new Polynomial(f);
        }

        public override string ToString()
            => this == Zero ? "0" : string.Join("+", elements.Select((e, i) => (e, i)).Where(_ => !_.e.IsZero).Select(_ => $"α^{_.e.Exponent}" + (_.i > 0 ? $"X^{_.i}" : "")).Reverse());
    }
}
