using System.Numerics;

namespace CodeGenerator;

public class CustomByteConverter
{
    private readonly byte[] _bytes;

    public CustomByteConverter(int size)
    {
        if (size <= 0 || size > 8)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be between 1 and 8.");

        _bytes = new byte[size];
    }

    public CustomByteConverter(ulong value, int size)
        : this(size)
    {
        FromULong(value);
    }

    public byte this[int index]
    {
        get => _bytes[index];
        set => _bytes[index] = value;
    }

    public byte[] GetBytes() => _bytes;

    public ulong ToULong()
    {
        ulong value = 0;
        for (int i = 0; i < _bytes.Length && i < 8; i++)
        {
            value |= (ulong)_bytes[i] << (8 * i); // Little-endian
        }
        return value;
    }
    public void FromULong(ulong value)
    {
        for (int i = 0; i < _bytes.Length && i < 8; i++)
        {
            _bytes[i] = (byte)(value >> (8 * i)); // Little-endian olarak yaz
        }
    }
}
