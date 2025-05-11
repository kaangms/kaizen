using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator;
public class CodeGeneratorHelper
{
    private const int _codeSize = 8; 
    private const int _seedSize = 3;
    private readonly byte[] _securityKey;
    private readonly byte[] _securityKeySalt;
    private static readonly Random _random = new();
    private static readonly SHA256 _sha256 = SHA256.Create();
    private readonly CustomBaseConverter _baseConverter;
    private readonly string _charset;

    public CodeGeneratorHelper(string charset)
    {
        /// Generates a security key using a bash script (`generate-key.sh`) in the project directory.
        /// Default key length is 64 characters but can be adjusted (e.g., `./generate-key.sh 128`).
        /// Use a company-specific key for better security and store it securely.
        var securityKey = "/;Gy$deMv0b=U7|#SKFFC4NCdRQc217&;LPfA;)l&uf-gYJlE=)/ER#A&8nJv34W";
        _securityKey = Encoding.ASCII.GetBytes(securityKey);
        _securityKeySalt = new byte[securityKey.Length + _seedSize];
        _charset=charset;
        Array.Copy(_securityKey!, 0, _securityKeySalt, 0, _securityKey!.Length);
        if (string.IsNullOrWhiteSpace(securityKey))
            throw new ArgumentNullException(nameof(securityKey), "Security key cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(charset))
            throw new ArgumentNullException(nameof(charset), "Charset cannot be null or empty.");
        _baseConverter = new CustomBaseConverter(charset);

    }
    public List<string> GenerateKeys(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
        List<string> keys = [];
        var uniqueSeeds = GenerateUniqueSeeds(count);
        for (int i = 0; i < uniqueSeeds.Length; i++)
        {
            // Convert the seed to bytes
            var seedBytes = new CustomByteConverter(uniqueSeeds[i], _seedSize).GetBytes();
            // Add salt to the seed bytes and hash the seed bytes with the security key
            byte derivedKeyByte = DeriveKeyByte(seedBytes);
            byte checksum = DeriveChecksum(seedBytes, derivedKeyByte);
            var raw =
            ((ulong)((uint)(uniqueSeeds[i] & 0xFFFFFF)) << 12) |
            ((ulong)((byte)(derivedKeyByte & 0xFF)) << 4) |
            ((ulong)((byte)(checksum & 0x0F)));
            keys.Add(_baseConverter.EncodeBase(raw, _codeSize));
        }
        return keys;
    }

    private byte DeriveKeyByte(byte[] seedBytes)
    {
        var hash = _sha256.ComputeHash([.. seedBytes, .. _securityKeySalt]);
        var deriveKeyByte = (byte)(hash[0] ^ hash[1] ^ hash[2] ^ hash[3]);
        return deriveKeyByte;
    }
    private static byte DeriveChecksum(byte[] seedBytes, byte derivedKeyByte) => (byte)((seedBytes.Sum(x => x) + derivedKeyByte) % 16);


    public ulong[] GenerateUniqueSeeds(int numberOfSeeds)
    {
        var max = (int)Math.Pow(255, _seedSize);
        if (numberOfSeeds > max)
            throw new ArgumentOutOfRangeException(nameof(numberOfSeeds), "Number of seeds exceeds the maximum unique values.");
        var indices = new ulong[max];
        for (var i = 0; i < max; i++)
        {
            indices[i] = (ulong)i;
        }
        // Fisher-Yates Shuffle Algorithm
        // Shuffle the array to get random unique indices
        for (var i = max - 1; i > 0; i--)
        {
            var j = _random.Next(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }
        return [.. indices.Take(numberOfSeeds)];
    }
    public bool ValidateKey(string code)
    {
        if (code.Length != _codeSize || code.Any(c => !_charset.Contains(c)))
            return false;

        ulong decoded = _baseConverter.DecodeBase(code);
        uint seed = (uint)(decoded >> 12);
        byte keyByte = (byte)((decoded >> 4) & 0xFF);
        byte checksum = (byte)(decoded & 0x0F);

        var seedBytes = new CustomByteConverter(seed, _seedSize).GetBytes();
        byte expectedKeyByte = DeriveKeyByte(seedBytes);
        byte expectedChecksum = DeriveChecksum(seedBytes, expectedKeyByte);

        return keyByte == expectedKeyByte && checksum == expectedChecksum;
    }
    // private string EncodeBase23(ulong value,int baseNValue)
    // {
    //     char[] result = new char[_codeSize];
    //     for (int i = _codeSize - 1; i >= 0; i--)
    //     {
    //         result[i] = _charset[(int)(value % 23)];
    //         value /= 23;
    //     }
    //     return new string(result);
    // }
    // private ulong DecodeBase23(string key)
    // {
    //     ulong value = 0;
    //     foreach (char c in key)
    //     {
    //         value = value * 23 + (ulong)_charset.IndexOf(c);
    //     }
    //     return value;
    // }
}