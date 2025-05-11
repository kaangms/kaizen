using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGenerator
{
    public class CustomBaseConverter
    {
        private readonly string _charset; 
        private readonly int _base;
        public CustomBaseConverter(string charset)
        {
            if (string.IsNullOrWhiteSpace(charset))
                throw new ArgumentNullException(nameof(charset), "Charset cannot be null or empty.");
            _charset = charset;
            _base = charset.Length;
        }
        public ulong DecodeBase(string key)
        {
            ulong value = 0;
            foreach (char c in key)
            {
                value = value * (ulong)_base + (ulong)_charset.IndexOf(c);
            }
            return value;
        }
        public string EncodeBase(ulong value,int codeSize)
        {
            if (codeSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(codeSize), "Code size must be greater than zero.");
            if (value >= Math.Pow(_base, codeSize))
                throw new ArgumentOutOfRangeException(nameof(value), "Value exceeds the maximum representable value.");
            char[] result = new char[codeSize];
            for (int i = codeSize - 1; i >= 0; i--)
            {
                result[i] = _charset[(int)(value %(ulong) _base)];
                value /= (ulong)_base;
            }
            return new string(result);
        }

    }
}