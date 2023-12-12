using IPTLab2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTLab2.Algorithms
{
    public static class RandGen
    {
        private static long nextXn(long a, long xn, long c, long m)
        {
            return (a * xn + c) % m;
        }

        public static KeyValuePair<List<long>, long> Gener(GeneratorParams pars, long numCount)
        {
            long period = -1;
            long x = pars.x0;

            List<long> res = new List<long> { x };
            for (int i = 0; i < numCount - 1; i++)
            {
                x = nextXn(pars.a, x, pars.c, pars.m);
                if (period < 0 && x == pars.x0)
                {
                    period = i + 1;
                }
                res.Add(x);
            }

            if (period < 0)
            {
                long i = res.Count;
                while (period < 0)
                {
                    x = nextXn(pars.a, x, pars.c, pars.m);
                    if (x == pars.x0)
                    {
                        period = i;
                    }
                    i += 1;
                }
            }

            return new KeyValuePair<List<long>, long>(res, period);
        }

        public static byte[] GenerBytes(GeneratorParams pars, long numCount)
        {
            long x = pars.x0;

            List<long> res = new List<long> { x };
            byte[] bytes = new byte[numCount];
            for (int i = 0; i < numCount - 1; i++)
            {
                x = nextXn(pars.a, x, pars.c, pars.m);
                res.Add(x);
                bytes[i] = (byte)(res[i] % 256);
            }

            return bytes;
        }
    }
}
