using System;
using System.Threading;

namespace Issuna.Core
{
    [Serializable]
    public struct CatkinId : IComparable<CatkinId>, IEquatable<CatkinId>
    {
        public const int ReservedBits = 1;   // 1 bit reserved, 0 by default
        public const int TimestampBits = 41; // 41 bits timestamp with milliseconds, max ‭2199023255551‬, 2199023255551/31536000000 = 69.73 years
        public const int RegionBits = 4;     // 4 bits covers 16 regions
        public const int MachineBits = 5;    // 5 bits covers 32 servers
        public const int SequenceBits = 13;  // 13 bits covers 8192/millisecond

        public const int SequenceShift = 0;
        public const int MachineShift = SequenceShift + SequenceBits;
        public const int RegionShift = MachineShift + MachineBits;
        public const int TimestampShift = RegionShift + RegionBits;
        public const int ReservedShift = TimestampShift + TimestampBits;

        public const int ReservedMask = -1 ^ (-1 << ReservedBits);
        public const long TimestampMask = -1L ^ (-1L << TimestampBits);
        public const int RegionMask = -1 ^ (-1 << RegionBits);
        public const int MachineMask = -1 ^ (-1 << MachineBits);
        public const int SequenceMask = -1 ^ (-1 << SequenceBits);

        public const int MaxReserved = -1 ^ (-1 << ReservedBits);
        public const long MaxTimestamp = -1L ^ (-1L << TimestampBits);
        public const int MaxRegion = -1 ^ (-1 << RegionBits);
        public const int MaxMachine = -1 ^ (-1 << MachineBits);
        public const int MaxSequence = -1 ^ (-1 << SequenceBits);

        private static int __staticSequence = (new Random()).Next();

        private byte _reserved;
        private byte _region;
        private ushort _machine;
        private long _timestamp;
        private int _sequence;

        public CatkinId(long id)
        {
            Unpack(id, out _reserved, out _timestamp, out _region, out _machine, out _sequence);
        }

        public CatkinId(byte reserved, long timestamp, byte region, ushort machine, int sequence)
        {
            SanityCheck(reserved, timestamp, region, machine, sequence);

            _reserved = reserved;
            _timestamp = timestamp;
            _region = region;
            _machine = machine;
            _sequence = sequence;
        }

        public byte Reserved { get { return _reserved; } }
        public long Timestamp { get { return _timestamp; } }
        public byte Region { get { return _region; } }
        public ushort Machine { get { return _machine; } }
        public int Sequence { get { return _sequence; } }
        public DateTime CreationTime { get { return CatkinIdTimer.ToDateTimeFromMillisecondsSinceCatkinIdEpoch(Timestamp); } }

        public long ToLong()
        {
            return Pack(Reserved, Timestamp, Region, Machine, Sequence);
        }

        public static long Pack(byte reserved, long timestamp, byte region, ushort machine, int sequence)
        {
            SanityCheck(reserved, timestamp, region, machine, sequence);

            long id = 0;

            id |= ((long)(reserved & ReservedMask)) << ReservedShift;
            id |= ((long)(timestamp & TimestampMask)) << TimestampShift;
            id |= ((long)(region & RegionMask)) << RegionShift;
            id |= ((long)(machine & MachineMask)) << MachineShift;
            id |= ((long)(sequence & SequenceMask)) << SequenceShift;

            return id;
        }

        public static void Unpack(long catkinId, out byte reserved, out long timestamp, out byte region, out ushort machine, out int sequence)
        {
            reserved = (byte)((catkinId >> ReservedShift) & ReservedMask);
            timestamp = (long)((catkinId >> TimestampShift) & TimestampMask);
            region = (byte)((catkinId >> RegionShift) & RegionMask);
            machine = (ushort)((catkinId >> MachineShift) & MachineMask);
            sequence = (int)((catkinId >> SequenceShift) & SequenceMask);
        }

        public static CatkinId Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            CatkinId catkinId;
            if (TryParse(value, out catkinId))
            {
                return catkinId;
            }
            else
            {
                var message = string.Format("'{0}' is not a valid CatkinId string.", value);
                throw new FormatException(message);
            }
        }

        public static CatkinId Parse(long value)
        {
            return new CatkinId(value);
        }

        public static bool TryParse(string value, out CatkinId catkinId)
        {
            if (!string.IsNullOrEmpty(value))
            {
                long id;
                if (long.TryParse(value, out id))
                {
                    catkinId = new CatkinId(id);
                    return true;
                }
            }

            catkinId = default(CatkinId);
            return false;
        }

        public static bool TryParse(long value, out CatkinId catkinId)
        {
            try
            {
                catkinId = new CatkinId(value);
                return true;
            }
            catch
            {
                catkinId = default(CatkinId);
                return false;
            }
        }

        private static void SanityCheck(byte reserved, long timestamp, byte region, ushort machine, int sequence)
        {
            if (reserved < 0 || MaxReserved < reserved)
            {
                throw new ArgumentOutOfRangeException("reserved",
                    string.Format("The 'reserved' value must be between 0 and {0} (it must fit in {1} bits).",
                        MaxReserved, ReservedBits));
            }
            if (timestamp < 0 || MaxTimestamp < timestamp)
            {
                throw new ArgumentOutOfRangeException("timestamp",
                    string.Format("The 'timestamp' value must be between 0 and {0} (it must fit in {1} bits).",
                        MaxTimestamp, TimestampBits));
            }
            if (region < 0 || MaxRegion < region)
            {
                throw new ArgumentOutOfRangeException("region",
                    string.Format("The 'region' value must be between 0 and {0} (it must fit in {1} bits).",
                        MaxRegion, RegionBits));
            }
            if (machine < 0 || MaxMachine < machine)
            {
                throw new ArgumentOutOfRangeException("machine",
                    string.Format("The 'machine' value must be between 0 and {0} (it must fit in {1} bits).",
                        MaxMachine, MachineBits));
            }
            if (sequence < 0 || MaxSequence < sequence)
            {
                throw new ArgumentOutOfRangeException("sequence",
                    string.Format("The 'sequence' value must be between 0 and {0} (it must fit in {1} bits).",
                        MaxSequence, SequenceBits));
            }
        }

        public static CatkinId GenerateNewId()
        {
            return GenerateNewId(timestamp: (DateTime?)null);
        }

        public static CatkinId GenerateNewId(byte reserved = 0, DateTime? timestamp = null, byte region = 0, ushort machine = 0, int? sequence = null)
        {
            long timestampLong = CatkinIdTimer.GetMillisecondsSinceCatkinIdEpochFromDateTime(timestamp.HasValue ? timestamp.Value : DateTime.UtcNow);

            return GenerateNewId(reserved: reserved, timestamp: timestampLong, region: region, machine: machine, sequence: sequence);
        }

        public static CatkinId GenerateNewId(byte reserved = 0, long? timestamp = null, byte region = 0, ushort machine = 0, int? sequence = null)
        {
            long timestampLong = timestamp.HasValue ? timestamp.Value : CatkinIdTimer.GetMillisecondsSinceCatkinIdEpochFromDateTime(DateTime.UtcNow);

            int sequenceInt = -1;
            if (sequence.HasValue)
            {
                sequenceInt = sequence.Value;
            }
            else
            {
                int increment = Interlocked.Increment(ref __staticSequence);
                sequenceInt = increment & SequenceMask;
            }

            return new CatkinId(reserved, timestampLong, region, machine, sequenceInt);
        }

        public int CompareTo(CatkinId other)
        {
            return ((long)this).CompareTo((long)other);
        }

        public bool Equals(CatkinId other)
        {
            return ((long)this).Equals((long)other);
        }

        public override bool Equals(object obj)
        {
            if (obj is CatkinId)
            {
                return Equals((CatkinId)obj);
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
            return string.Format("Reserved[{0}], Timestamp[{1}], Region[{2}], Machine[{3}], Sequence[{4}]",
                Reserved, Timestamp, Region, Machine, Sequence);
        }

        public static explicit operator long(CatkinId catkinId)
        {
            return catkinId.ToLong();
        }

        public static explicit operator string(CatkinId catkinId)
        {
            return catkinId.ToString();
        }

        public static bool operator <(CatkinId left, CatkinId right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(CatkinId left, CatkinId right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator ==(CatkinId left, CatkinId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CatkinId left, CatkinId right)
        {
            return !(left == right);
        }

        public static bool operator >=(CatkinId left, CatkinId right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator >(CatkinId left, CatkinId right)
        {
            return left.CompareTo(right) > 0;
        }

        internal static class CatkinIdTimer
        {
            private static readonly DateTime __unixEpoch;
            private static readonly long __dateTimeMaxValueSecondsSinceEpoch;
            private static readonly long __dateTimeMinValueSecondsSinceEpoch;
            private static readonly long __dateTimeMaxValueMillisecondsSinceEpoch;
            private static readonly long __dateTimeMinValueMillisecondsSinceEpoch;

            private static readonly DateTime __catkinIdEpoch;
            private static readonly long __catkinIdEpochOffsetBySeconds; // 1483228800

            static CatkinIdTimer()
            {
                __unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                __dateTimeMaxValueSecondsSinceEpoch = (DateTime.MaxValue - __unixEpoch).Ticks / 10000 / 1000;
                __dateTimeMinValueSecondsSinceEpoch = (DateTime.MinValue - __unixEpoch).Ticks / 10000 / 1000;
                __dateTimeMaxValueMillisecondsSinceEpoch = (DateTime.MaxValue - __unixEpoch).Ticks / 10000;
                __dateTimeMinValueMillisecondsSinceEpoch = (DateTime.MinValue - __unixEpoch).Ticks / 10000;

                __catkinIdEpoch = new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                __catkinIdEpochOffsetBySeconds = (__catkinIdEpoch - __unixEpoch).Ticks / 10000 / 1000;
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

            public static DateTime CatkinIdEpoch { get { return __catkinIdEpoch; } }

            public static long CatkinIdEpochOffsetBySeconds { get { return __catkinIdEpochOffsetBySeconds; } }

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
                    throw new ArgumentOutOfRangeException("dateTime");
                }
                return secondsSinceEpoch;
            }

            public static long GetMillisecondsSinceEpochFromDateTime(DateTime dateTime)
            {
                var millisecondsSinceEpoch = (long)Math.Floor((ToUniversalTime(dateTime) - UnixEpoch).TotalMilliseconds);
                if (millisecondsSinceEpoch < long.MinValue || millisecondsSinceEpoch > long.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("dateTime");
                }
                return millisecondsSinceEpoch;
            }

            public static DateTime ToDateTimeFromSecondsSinceCatkinIdEpoch(long secondsSinceCatkinIdEpoch)
            {
                return ToDateTimeFromSecondsSinceEpoch(CatkinIdEpochOffsetBySeconds + secondsSinceCatkinIdEpoch);
            }

            public static DateTime ToDateTimeFromMillisecondsSinceCatkinIdEpoch(long millisecondsSinceCatkinIdEpoch)
            {
                return ToDateTimeFromMillisecondsSinceEpoch((CatkinIdEpochOffsetBySeconds * 1000) + millisecondsSinceCatkinIdEpoch);
            }

            public static long GetSecondsSinceCatkinIdEpochFromDateTime(DateTime dateTime)
            {
                return GetSecondsSinceEpochFromDateTime(dateTime) - CatkinIdEpochOffsetBySeconds;
            }

            public static long GetMillisecondsSinceCatkinIdEpochFromDateTime(DateTime dateTime)
            {
                return GetMillisecondsSinceEpochFromDateTime(dateTime) - (CatkinIdEpochOffsetBySeconds * 1000);
            }
        }
    }
}
