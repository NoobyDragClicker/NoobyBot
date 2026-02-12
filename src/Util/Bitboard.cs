using System.Numerics;
public struct Bitboard : IEquatable<Bitboard>
{
    private ulong _value;

    public Bitboard(ulong value)
    {
        _value = value;
    }

    public static Bitboard FromSquare(int sq) => new Bitboard(1UL << sq);
    public void Clear() => _value = 0;
    public bool Empty() => _value == 0;

    public int Count()
    {
        ulong temp = _value;
        int count = 0;
        while (temp != 0)
        {
            temp &= temp - 1;
            count++;
        }
        return count;
    }

    public int PopCount()
    {
        int count = 0;
        while (_value != 0)
        {
            _value &= _value - 1;
            count++;
        }
        return count;
    }

    public int PopLSB()
    {
        int i = BitOperations.TrailingZeroCount(_value);
        _value &= _value - 1;
        return i;
    }
    public int GetLSB()
    {
        return BitOperations.TrailingZeroCount(_value);
    }

    public void SetSquare(int sq)
    {
        _value |= 1ul << sq;
    }

    public void ClearSquare(int sq)
    {
        _value &= ~(1ul << sq);
    }
    

    public static implicit operator ulong(Bitboard b) => b._value;
    public static implicit operator Bitboard(ulong v) => new Bitboard(v);
    public static bool operator ==(Bitboard a, Bitboard b) => a._value == b._value;
    public static bool operator !=(Bitboard a, Bitboard b) => a._value != b._value;
    public static Bitboard operator <<(Bitboard bb, int other) => new Bitboard(bb._value << other);
    public static Bitboard operator >>(Bitboard bb, int other) => new Bitboard(bb._value >> other);
    public static Bitboard operator &(Bitboard a, Bitboard b) => new Bitboard(a._value & b._value);
    public static Bitboard operator |(Bitboard a, Bitboard b) => new Bitboard(a._value | b._value);
    public static Bitboard operator ^(Bitboard a, Bitboard b) => new Bitboard(a._value ^ b._value);
    public static Bitboard operator ~(Bitboard bb) => new Bitboard(~bb._value);
    public bool Equals(Bitboard other) => _value == other._value;
    public override bool Equals(object? obj)=> obj is Bitboard other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public int CompareTo(Bitboard other) => _value.CompareTo(other._value);
}