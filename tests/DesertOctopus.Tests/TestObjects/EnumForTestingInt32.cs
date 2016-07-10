using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializerTests.TestObjects
{
    [Serializable]
    public enum EnumForTestingInt32 : int
    {
        Zero = 0,
        One = 1,
        Two = 2
    }
    [Serializable]
    public enum EnumForTestingUint32 : uint
    {
        Zero = 0,
        One = 1,
        Two = 2
    }

    [Serializable]
    public enum EnumForTestingInt16 : short
    {
        Zero = 0,
        One = 1,
        Two = 2
    }

    [Serializable]
    public enum EnumForTestingInt64 : long
    {
        Zero = 0,
        One = 1,
        Two = 2
    }

    [Serializable]
    public enum EnumForTestingUint64 : long
    {
        Zero = 0,
        One = 1,
        Two = 2
    }

    [Serializable]
    public enum EnumForTestingByte : Byte
    {
        Zero = 0,
        One = 1,
        Two = 2
    }

    [Serializable]
    public enum EnumForTestingSbyte : SByte
    {
        Zero = 0,
        One = 1,
        Two = 2
    }

    [Serializable]
    public enum EnumForTestingUint16 : ushort
    {
        Zero = 0,
        One = 1,
        Two = 2
    }
}
