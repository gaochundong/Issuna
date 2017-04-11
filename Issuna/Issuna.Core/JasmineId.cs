using System;
using System.Threading;

namespace Issuna.Core
{
    /// <summary>
    /// JasmineId is a 64-bit(long/int64) ID consists of:
    /// 63-bit as V-bit, reserved 1 bit, 0 by default.
    /// 62-61-bit as R-bit, represents deployment region, 2 bits covers 4 regions.
    /// 60-51-bit as M-bits, represents machine identifier, 10 bits covers 1024 servers.
    /// 50-bit as P-bit, represents time precision for using second(0) or millisecond(1), 0 by default.
    /// 49-20-bit as T-bits, timestamp if the P-bit is second(0), 30 bits covers more than 30 years.
    /// 49-10-bit as T-bits, timestamp if the P-bit is millisecond(1), 40 bits covers more than 30 years.
    /// 19-0-bit as Q-bits, sequence if the P-bit is second(0), 20 bits covers 1048576/second.
    /// 9-0-bit as Q-bits, sequence if the P-bit is millisecond(1), 10 bits covers 1024/millisecond.
    /// </summary>
    [Serializable]
    public struct JasmineId : IComparable<JasmineId>, IEquatable<JasmineId>
    {
        private static int __staticSequence = (new Random()).Next();

        private byte _reserved;
        private byte _region;
        private ushort _machine;
        private byte _precision;
        private long _timestamp;
        private int _sequence;

        public JasmineId(long id)
        {
            Unpack(id, out _reserved, out _region, out _machine, out _precision, out _timestamp, out _sequence);
        }

        public JasmineId(byte reserved, byte region, ushort machine, byte precision, long timestamp, int sequence)
        {
            SanityCheck(reserved, region, machine, precision, timestamp, sequence);

            _reserved = reserved;
            _region = region;
            _machine = machine;
            _precision = precision;
            _timestamp = timestamp;
            _sequence = sequence;
        }

        public byte Reserved { get { return _reserved; } }
        public byte Region { get { return _region; } }
        public ushort Machine { get { return _machine; } }
        public byte Precision { get { return _precision; } }
        public long Timestamp { get { return _timestamp; } }
        public int Sequence { get { return _sequence; } }

        public DateTime CreationTime
        {
            get
            {
                return Precision == 0 ?
                  LongIdTimer.ToDateTimeFromSecondsSinceLongIdEpoch(Timestamp)
                  : LongIdTimer.ToDateTimeFromMillisecondsSinceLongIdEpoch(Timestamp);
            }
        }

        public long ToLong()
        {
            return Pack(Reserved, Region, Machine, Precision, Timestamp, Sequence);
        }

        public static long Pack(byte reserved, byte region, ushort machine, byte precision, long timestamp, int sequence)
        {
            SanityCheck(reserved, region, machine, precision, timestamp, sequence);

            long id = 0;

            id |= (long)(reserved & 0x01) << 63;
            id |= (long)(region & 0x03) << 61;
            id |= (long)(machine & 0x03ff) << 51;
            id |= (long)(precision & 0x01) << 50;
            id |= (long)((timestamp & (precision == 0 ? 0x3fffffff : 0xffffffffff)) << (precision == 0 ? 20 : 10));
            id |= (long)(sequence & (precision == 0 ? 0x0fffff : 0x03ff));

            return id;
        }

        public static void Unpack(long jasmineId, out byte reserved, out byte region, out ushort machine, out byte precision, out long timestamp, out int sequence)
        {
            reserved = (byte)((jasmineId >> 63) & 0x01);
            region = (byte)((jasmineId >> 61) & 0x03);
            machine = (ushort)((jasmineId >> 51) & 0x03ff);
            precision = (byte)((jasmineId >> 50) & 0x01);
            timestamp = (long)((jasmineId >> (precision == 0 ? 20 : 10)) & (precision == 0 ? 0x3fffffff : 0xffffffffff));
            sequence = (int)(jasmineId & (precision == 0 ? 0x0fffff : 0x03ff));
        }

        public static JasmineId Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            JasmineId jasmineId;
            if (TryParse(value, out jasmineId))
            {
                return jasmineId;
            }
            else
            {
                var message = string.Format("'{0}' is not a valid LongId string.", value);
                throw new FormatException(message);
            }
        }

        public static bool TryParse(string value, out JasmineId jasmineId)
        {
            if (!string.IsNullOrEmpty(value))
            {
                long id;
                if (long.TryParse(value, out id))
                {
                    jasmineId = new JasmineId(id);
                    return true;
                }
            }

            jasmineId = default(JasmineId);
            return false;
        }

        private static void SanityCheck(byte reserved, byte region, ushort machine, byte precision, long timestamp, int sequence)
        {
            if ((reserved & 0xfe) != 0)
            {
                throw new ArgumentOutOfRangeException("reserved", "The 'reserved' value must be between 0 and 1 (it must fit in 1 bit).");
            }
            if ((region & 0xfc) != 0)
            {
                throw new ArgumentOutOfRangeException("region", "The 'region' value must be between 0 and 3 (it must fit in 2 bits).");
            }
            if ((machine & 0xfc00) != 0)
            {
                throw new ArgumentOutOfRangeException("machine", "The 'machine' value must be between 0 and 1023 (it must fit in 10 bits).");
            }
            if ((precision & 0xfe) != 0)
            {
                throw new ArgumentOutOfRangeException("precision", "The 'precision' value must be between 0 and 1 (it must fit in 1 bit).");
            }
            if (precision == 0 && (timestamp & 0x7fffffffc0000000) != 0)
            {
                throw new ArgumentOutOfRangeException("timestamp", "The 'timestamp' value must be between 0 and 1,073,741,823 (it must fit in 30 bits).");
            }
            if (precision == 1 && (timestamp & 0x7fffff0000000000) != 0)
            {
                throw new ArgumentOutOfRangeException("timestamp", "The 'timestamp' value must be between 0 and ‭1,099,511,627,775‬ (it must fit in 40 bits).");
            }
            if (precision == 0 && (sequence & 0x7ff00000) != 0)
            {
                throw new ArgumentOutOfRangeException("sequence", "The 'sequence' value must be between 0 and ‭1,048,575‬ (it must fit in 20 bits).");
            }
            if (precision == 1 && (sequence & 0x7ffffc00) != 0)
            {
                throw new ArgumentOutOfRangeException("sequence", "The 'sequence' value must be between 0 and 1023 (it must fit in 10 bits).");
            }
        }

        public static JasmineId GenerateNewId(byte reserved = 0, byte region = 0, ushort machine = 0, byte precision = 0, DateTime? timestamp = null)
        {
            long timestampLong = precision == 0 ?
                LongIdTimer.GetSecondsSinceLongIdEpochFromDateTime(timestamp.HasValue ? timestamp.Value : DateTime.UtcNow)
                : LongIdTimer.GetMillisecondsSinceLongIdEpochFromDateTime(timestamp.HasValue ? timestamp.Value : DateTime.UtcNow);

            return GenerateNewId(reserved, region, machine, precision, timestamp: timestampLong);
        }

        public static JasmineId GenerateNewId(byte reserved = 0, byte region = 0, ushort machine = 0, byte precision = 0, long? timestamp = null)
        {
            long timestampLong = precision == 0 ?
                timestamp.HasValue ? timestamp.Value : LongIdTimer.GetSecondsSinceLongIdEpochFromDateTime(DateTime.UtcNow)
                : timestamp.HasValue ? timestamp.Value : LongIdTimer.GetMillisecondsSinceLongIdEpochFromDateTime(DateTime.UtcNow);

            int increment = Interlocked.Increment(ref __staticSequence);
            int sequence = precision == 0 ? (increment & 0x0fffff) : (increment & 0x03ff);

            return new JasmineId(reserved, region, machine, precision, timestampLong, sequence);
        }

        public int CompareTo(JasmineId other)
        {
            return ((long)this).CompareTo((long)other);
        }

        public bool Equals(JasmineId other)
        {
            return ((long)this).Equals((long)other);
        }

        public override bool Equals(object obj)
        {
            if (obj is JasmineId)
            {
                return Equals((JasmineId)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return ((long)this).GetHashCode();
        }

        public override string ToString()
        {
            return ((long)this).ToString();
        }

        public string ToText()
        {
            return string.Format("Reserved[{0}], Region[{1}], Machine[{2}], Precision[{3}], Timestamp[{4}], Sequence[{5}]",
                Reserved, Region, Machine, Precision, Timestamp, Sequence);
        }

        public static explicit operator long(JasmineId jasmineId)
        {
            return jasmineId.ToLong();
        }

        public static explicit operator string(JasmineId jasmineId)
        {
            return jasmineId.ToString();
        }

        public static bool operator <(JasmineId left, JasmineId right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(JasmineId left, JasmineId right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator ==(JasmineId left, JasmineId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(JasmineId left, JasmineId right)
        {
            return !(left == right);
        }

        public static bool operator >=(JasmineId left, JasmineId right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator >(JasmineId left, JasmineId right)
        {
            return left.CompareTo(right) > 0;
        }

        internal static class LongIdTimer
        {
            private static readonly DateTime __unixEpoch;
            private static readonly long __dateTimeMaxValueSecondsSinceEpoch;
            private static readonly long __dateTimeMinValueSecondsSinceEpoch;
            private static readonly long __dateTimeMaxValueMillisecondsSinceEpoch;
            private static readonly long __dateTimeMinValueMillisecondsSinceEpoch;

            private static readonly DateTime __jasmineIdEpoch;
            private static readonly long __jasmineIdEpochOffsetBySeconds; // 1483228800

            static LongIdTimer()
            {
                __unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                __dateTimeMaxValueSecondsSinceEpoch = (DateTime.MaxValue - __unixEpoch).Ticks / 10000 / 1000;
                __dateTimeMinValueSecondsSinceEpoch = (DateTime.MinValue - __unixEpoch).Ticks / 10000 / 1000;
                __dateTimeMaxValueMillisecondsSinceEpoch = (DateTime.MaxValue - __unixEpoch).Ticks / 10000;
                __dateTimeMinValueMillisecondsSinceEpoch = (DateTime.MinValue - __unixEpoch).Ticks / 10000;

                __jasmineIdEpoch = new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                __jasmineIdEpochOffsetBySeconds = (__jasmineIdEpoch - __unixEpoch).Ticks / 10000 / 1000;
            }

            public static long DateTimeMaxValueSecondsSinceEpoch
            {
                get { return __dateTimeMaxValueSecondsSinceEpoch; }
            }

            public static long DateTimeMinValueSecondsSinceEpoch
            {
                get { return __dateTimeMinValueSecondsSinceEpoch; }
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

            public static DateTime LongIdEpoch { get { return __jasmineIdEpoch; } }

            public static long LongIdEpochOffsetBySeconds { get { return __jasmineIdEpochOffsetBySeconds; } }

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

            public static DateTime ToDateTimeFromSecondsSinceEpoch(long secondsSinceEpoch)
            {
                if (secondsSinceEpoch < DateTimeMinValueSecondsSinceEpoch ||
                    secondsSinceEpoch > DateTimeMaxValueSecondsSinceEpoch)
                {
                    throw new ArgumentOutOfRangeException("secondsSinceEpoch");
                }

                if (secondsSinceEpoch == DateTimeMaxValueSecondsSinceEpoch)
                {
                    return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
                }
                else
                {
                    return UnixEpoch.AddTicks(secondsSinceEpoch * 1000 * 10000);
                }
            }

            public static DateTime ToDateTimeFromMillisecondsSinceEpoch(long millisecondsSinceEpoch)
            {
                if (millisecondsSinceEpoch < DateTimeMinValueMillisecondsSinceEpoch ||
                    millisecondsSinceEpoch > DateTimeMaxValueMillisecondsSinceEpoch)
                {
                    throw new ArgumentOutOfRangeException("millisecondsSinceEpoch");
                }

                if (millisecondsSinceEpoch == DateTimeMaxValueMillisecondsSinceEpoch)
                {
                    return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
                }
                else
                {
                    return UnixEpoch.AddTicks(millisecondsSinceEpoch * 10000);
                }
            }

            public static long GetSecondsSinceEpochFromDateTime(DateTime dateTime)
            {
                var secondsSinceEpoch = (long)Math.Floor((ToUniversalTime(dateTime) - UnixEpoch).TotalSeconds);
                if (secondsSinceEpoch < long.MinValue || secondsSinceEpoch > long.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("timestamp");
                }
                return secondsSinceEpoch;
            }

            public static long GetMillisecondsSinceEpochFromDateTime(DateTime dateTime)
            {
                var millisecondsSinceEpoch = (long)Math.Floor((ToUniversalTime(dateTime) - UnixEpoch).TotalMilliseconds);
                if (millisecondsSinceEpoch < long.MinValue || millisecondsSinceEpoch > long.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("timestamp");
                }
                return millisecondsSinceEpoch;
            }

            public static DateTime ToDateTimeFromSecondsSinceLongIdEpoch(long secondsSinceLongIdEpoch)
            {
                return ToDateTimeFromSecondsSinceEpoch(LongIdEpochOffsetBySeconds + secondsSinceLongIdEpoch);
            }

            public static DateTime ToDateTimeFromMillisecondsSinceLongIdEpoch(long millisecondsSinceLongIdEpoch)
            {
                return ToDateTimeFromMillisecondsSinceEpoch((LongIdEpochOffsetBySeconds * 1000) + millisecondsSinceLongIdEpoch);
            }

            public static long GetSecondsSinceLongIdEpochFromDateTime(DateTime dateTime)
            {
                return GetSecondsSinceEpochFromDateTime(dateTime) - LongIdEpochOffsetBySeconds;
            }

            public static long GetMillisecondsSinceLongIdEpochFromDateTime(DateTime dateTime)
            {
                return GetMillisecondsSinceEpochFromDateTime(dateTime) - (LongIdEpochOffsetBySeconds * 1000);
            }
        }
    }
}
