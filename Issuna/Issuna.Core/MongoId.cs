using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;

namespace Issuna.Core
{
    /// <summary>
    /// MongoId is a 12-byte Id consists of:
    /// a 4-byte value representing the seconds since the Unix epoch,
    /// a 3-byte machine identifier,
    /// a 2-byte process id, and
    /// a 3-byte counter, starting with a random value.
    /// </summary>
    [Serializable]
    public struct MongoId : IComparable<MongoId>, IEquatable<MongoId>
    {
        private static MongoId __emptyInstance = default(MongoId);
        private static int __staticMachine = (GetMachineHash() + AppDomain.CurrentDomain.Id) & 0x00ffffff;
        private static short __staticPid = GetPid();
        private static int __staticIncrement = (new Random()).Next();

        private int _a;
        private int _b;
        private int _c;

        public MongoId(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (bytes.Length != 12)
            {
                throw new ArgumentException("Byte array must be 12 bytes long", "bytes");
            }

            FromByteArray(bytes, 0, out _a, out _b, out _c);
        }

        public MongoId(byte[] bytes, int offset)
        {
            FromByteArray(bytes, offset, out _a, out _b, out _c);
        }

        public MongoId(DateTime timestamp, int machine, short pid, int increment)
            : this(ObjectIdTimer.GetTimestampFromDateTime(timestamp), machine, pid, increment)
        {
        }

        public MongoId(int timestamp, int machine, short pid, int increment)
        {
            if ((machine & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("machine", "The machine value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }
            if ((increment & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("increment", "The increment value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }

            _a = timestamp;
            _b = (machine << 8) | (((int)pid >> 8) & 0xff);
            _c = ((int)pid << 24) | increment;
        }

        public MongoId(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var bytes = ObjectIdHexer.ParseHexString(value);
            FromByteArray(bytes, 0, out _a, out _b, out _c);
        }

        public static MongoId Empty
        {
            get { return __emptyInstance; }
        }

        public int Timestamp
        {
            get { return _a; }
        }

        public int Machine
        {
            get { return (_b >> 8) & 0xffffff; }
        }

        public short Pid
        {
            get { return (short)(((_b << 8) & 0xff00) | ((_c >> 24) & 0x00ff)); }
        }

        public int Increment
        {
            get { return _c & 0xffffff; }
        }

        public DateTime CreationTime
        {
            get { return ObjectIdTimer.UnixEpoch.AddSeconds(Timestamp); }
        }

        public static bool operator <(MongoId left, MongoId right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(MongoId left, MongoId right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator ==(MongoId left, MongoId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MongoId left, MongoId right)
        {
            return !(left == right);
        }

        public static bool operator >=(MongoId left, MongoId right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator >(MongoId left, MongoId right)
        {
            return left.CompareTo(right) > 0;
        }

        public static MongoId GenerateNewId()
        {
            return GenerateNewId(ObjectIdTimer.GetTimestampFromDateTime(DateTime.UtcNow));
        }

        public static MongoId GenerateNewId(DateTime timestamp)
        {
            return GenerateNewId(ObjectIdTimer.GetTimestampFromDateTime(timestamp));
        }

        public static MongoId GenerateNewId(int timestamp)
        {
            int increment = Interlocked.Increment(ref __staticIncrement) & 0x00ffffff; // only use low order 3 bytes
            return new MongoId(timestamp, __staticMachine, __staticPid, increment);
        }

        public static byte[] Pack(int timestamp, int machine, short pid, int increment)
        {
            if ((machine & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("machine", "The machine value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }
            if ((increment & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("increment", "The increment value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }

            byte[] bytes = new byte[12];
            bytes[0] = (byte)(timestamp >> 24);
            bytes[1] = (byte)(timestamp >> 16);
            bytes[2] = (byte)(timestamp >> 8);
            bytes[3] = (byte)(timestamp);
            bytes[4] = (byte)(machine >> 16);
            bytes[5] = (byte)(machine >> 8);
            bytes[6] = (byte)(machine);
            bytes[7] = (byte)(pid >> 8);
            bytes[8] = (byte)(pid);
            bytes[9] = (byte)(increment >> 16);
            bytes[10] = (byte)(increment >> 8);
            bytes[11] = (byte)(increment);
            return bytes;
        }

        public static void Unpack(byte[] bytes, out int timestamp, out int machine, out short pid, out int increment)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (bytes.Length != 12)
            {
                throw new ArgumentOutOfRangeException("bytes", "Byte array must be 12 bytes long.");
            }

            timestamp = (bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3];
            machine = (bytes[4] << 16) + (bytes[5] << 8) + bytes[6];
            pid = (short)((bytes[7] << 8) + bytes[8]);
            increment = (bytes[9] << 16) + (bytes[10] << 8) + bytes[11];
        }

        public static MongoId Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            MongoId objectId;
            if (TryParse(s, out objectId))
            {
                return objectId;
            }
            else
            {
                var message = string.Format("'{0}' is not a valid 24 digit hex string.", s);
                throw new FormatException(message);
            }
        }

        public static bool TryParse(string s, out MongoId objectId)
        {
            // don't throw ArgumentNullException if s is null
            if (s != null && s.Length == 24)
            {
                byte[] bytes;
                if (ObjectIdHexer.TryParseHexString(s, out bytes))
                {
                    objectId = new MongoId(bytes);
                    return true;
                }
            }

            objectId = default(MongoId);
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        private static int GetMachineHash()
        {
            var hostName = Environment.MachineName; // use instead of Dns.HostName so it will work offline
            return 0x00ffffff & hostName.GetHashCode(); // use first 3 bytes of hash
        }

        private static short GetPid()
        {
            try
            {
                return (short)GetCurrentProcessId(); // use low order two bytes only
            }
            catch (SecurityException)
            {
                return 0;
            }
        }

        private static void FromByteArray(byte[] bytes, int offset, out int a, out int b, out int c)
        {
            a = (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
            b = (bytes[offset + 4] << 24) | (bytes[offset + 5] << 16) | (bytes[offset + 6] << 8) | bytes[offset + 7];
            c = (bytes[offset + 8] << 24) | (bytes[offset + 9] << 16) | (bytes[offset + 10] << 8) | bytes[offset + 11];
        }

        public int CompareTo(MongoId other)
        {
            int result = ((uint)_a).CompareTo((uint)other._a);
            if (result != 0) { return result; }
            result = ((uint)_b).CompareTo((uint)other._b);
            if (result != 0) { return result; }
            return ((uint)_c).CompareTo((uint)other._c);
        }

        public bool Equals(MongoId other)
        {
            return
                _a == other._a &&
                _b == other._b &&
                _c == other._c;
        }

        public override bool Equals(object obj)
        {
            if (obj is MongoId)
            {
                return Equals((MongoId)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = 37 * hash + _a.GetHashCode();
            hash = 37 * hash + _b.GetHashCode();
            hash = 37 * hash + _c.GetHashCode();
            return hash;
        }

        public byte[] ToByteArray()
        {
            var bytes = new byte[12];
            ToByteArray(bytes, 0);
            return bytes;
        }

        public void ToByteArray(byte[] destination, int offset)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (offset + 12 > destination.Length)
            {
                throw new ArgumentException("Not enough room in destination buffer.", "offset");
            }

            destination[offset + 0] = (byte)(_a >> 24);
            destination[offset + 1] = (byte)(_a >> 16);
            destination[offset + 2] = (byte)(_a >> 8);
            destination[offset + 3] = (byte)(_a);
            destination[offset + 4] = (byte)(_b >> 24);
            destination[offset + 5] = (byte)(_b >> 16);
            destination[offset + 6] = (byte)(_b >> 8);
            destination[offset + 7] = (byte)(_b);
            destination[offset + 8] = (byte)(_c >> 24);
            destination[offset + 9] = (byte)(_c >> 16);
            destination[offset + 10] = (byte)(_c >> 8);
            destination[offset + 11] = (byte)(_c);
        }

        public override string ToString()
        {
            return ObjectIdHexer.ToHexString(ToByteArray());
        }

        internal static class ObjectIdTimer
        {
            private static readonly DateTime __unixEpoch;
            private static readonly long __dateTimeMaxValueMillisecondsSinceEpoch;
            private static readonly long __dateTimeMinValueMillisecondsSinceEpoch;

            static ObjectIdTimer()
            {
                __unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                __dateTimeMaxValueMillisecondsSinceEpoch = (DateTime.MaxValue - __unixEpoch).Ticks / 10000;
                __dateTimeMinValueMillisecondsSinceEpoch = (DateTime.MinValue - __unixEpoch).Ticks / 10000;
            }

            public static long DateTimeMaxValueMillisecondsSinceEpoch
            {
                get { return __dateTimeMaxValueMillisecondsSinceEpoch; }
            }

            public static long DateTimeMinValueMillisecondsSinceEpoch
            {
                get { return __dateTimeMinValueMillisecondsSinceEpoch; }
            }

            public static DateTime UnixEpoch { get { return __unixEpoch; } }

            public static DateTime ToUniversalTime(DateTime dateTime)
            {
                if (dateTime == DateTime.MinValue)
                {
                    return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
                }
                else if (dateTime == DateTime.MaxValue)
                {
                    return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
                }
                else
                {
                    return dateTime.ToUniversalTime();
                }
            }

            public static int GetTimestampFromDateTime(DateTime timestamp)
            {
                var secondsSinceEpoch = (long)Math.Floor((ToUniversalTime(timestamp) - UnixEpoch).TotalSeconds);
                if (secondsSinceEpoch < int.MinValue || secondsSinceEpoch > int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("timestamp");
                }
                return (int)secondsSinceEpoch;
            }
        }

        internal static class ObjectIdHexer
        {
            public static string ToHexString(byte[] bytes)
            {
                if (bytes == null)
                {
                    throw new ArgumentNullException("bytes");
                }
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    sb.AppendFormat("{0:x2}", b);
                }
                return sb.ToString();
            }

            public static bool TryParseHexString(string s, out byte[] bytes)
            {
                try
                {
                    bytes = ParseHexString(s);
                }
                catch
                {
                    bytes = null;
                    return false;
                }

                return true;
            }

            public static byte[] ParseHexString(string s)
            {
                if (s == null)
                {
                    throw new ArgumentNullException("s");
                }

                byte[] bytes;
                if ((s.Length & 1) != 0)
                {
                    s = "0" + s; // make length of s even
                }
                bytes = new byte[s.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    string hex = s.Substring(2 * i, 2);
                    try
                    {
                        byte b = Convert.ToByte(hex, 16);
                        bytes[i] = b;
                    }
                    catch (FormatException e)
                    {
                        throw new FormatException(
                            string.Format("Invalid hex string {0}. Problem with substring {1} starting at position {2}",
                            s,
                            hex,
                            2 * i),
                            e);
                    }
                }

                return bytes;
            }
        }
    }
}
