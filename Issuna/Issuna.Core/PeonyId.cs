using System;
using System.Threading;

namespace Issuna.Core
{
    /// <summary>
    /// PeonyId is a 64-bit(long/int64) ID consists of:
    /// 1-bit as reserved, 0 by default.
    /// 40-bit represents timestamp with milliseconds, 40 bits with milliseconds covers more than 30 years.
    /// 3-bit represents deployment region, 3 bits covers 7 regions.
    /// 10-bit represents machine identifier, 10 bits covers 1024 servers.
    /// 10-bit represents sequence, 10 bits covers 1024/millisecond.
    /// </summary>
    [Serializable]
    public struct PeonyId : IComparable<PeonyId>, IEquatable<PeonyId>
    {
        public const int ReservedBits = 1;   // 1 bit reserved, 0 by default
        public const int TimestampBits = 40; // 40 bits with milliseconds covers more than 30 years
        public const int RegionBits = 3;     // 3 bits covers 7 regions
        public const int MachineBits = 10;   // 10 bits covers 1024 servers
        public const int SequenceBits = 10;  // 10 bits covers 1024/millisecond

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

        public PeonyId(long id)
        {
            Unpack(id, out _reserved, out _timestamp, out _region, out _machine, out _sequence);
        }

        public PeonyId(byte reserved, long timestamp, byte region, ushort machine, int sequence)
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
        public DateTime CreationTime { get { return PeonyIdTimer.ToDateTimeFromMillisecondsSincePeonyIdEpoch(Timestamp); } }

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

        public static void Unpack(long peonyId, out byte reserved, out long timestamp, out byte region, out ushort machine, out int sequence)
        {
            reserved = (byte)((peonyId >> ReservedShift) & ReservedMask);
            timestamp = (long)((peonyId >> TimestampShift) & TimestampMask);
            region = (byte)((peonyId >> RegionShift) & RegionMask);
            machine = (ushort)((peonyId >> MachineShift) & MachineMask);
            sequence = (int)((peonyId >> SequenceShift) & SequenceMask);
        }

        public static PeonyId Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            PeonyId peonyId;
            if (TryParse(value, out peonyId))
            {
                return peonyId;
            }
            else
            {
                var message = string.Format("'{0}' is not a valid PeonyId string.", value);
                throw new FormatException(message);
            }
        }

        public static PeonyId Parse(long value)
        {
            return new PeonyId(value);
        }

        public static bool TryParse(string value, out PeonyId peonyId)
        {
            if (!string.IsNullOrEmpty(value))
            {
                long id;
                if (long.TryParse(value, out id))
                {
                    peonyId = new PeonyId(id);
                    return true;
                }
            }

            peonyId = default(PeonyId);
            return false;
        }

        public static bool TryParse(long value, out PeonyId peonyId)
        {
            try
            {
                peonyId = new PeonyId(value);
                return true;
            }
            catch
            {
                peonyId = default(PeonyId);
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

        public static PeonyId GenerateNewId(byte reserved = 0, DateTime? timestamp = null, byte region = 0, ushort machine = 0, int? sequence = null)
        {
            long timestampLong = PeonyIdTimer.GetMillisecondsSincePeonyIdEpochFromDateTime(timestamp.HasValue ? timestamp.Value : DateTime.UtcNow);

            return GenerateNewId(reserved: reserved, timestamp: timestampLong, region: region, machine: machine, sequence: sequence);
        }

        public static PeonyId GenerateNewId(byte reserved = 0, long? timestamp = null, byte region = 0, ushort machine = 0, int? sequence = null)
        {
            long timestampLong = timestamp.HasValue ? timestamp.Value : PeonyIdTimer.GetMillisecondsSincePeonyIdEpochFromDateTime(DateTime.UtcNow);

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

            return new PeonyId(reserved, timestampLong, region, machine, sequenceInt);
        }

        public int CompareTo(PeonyId other)
        {
            return ((long)this).CompareTo((long)other);
        }

        public bool Equals(PeonyId other)
        {
            return ((long)this).Equals((long)other);
        }

        public override bool Equals(object obj)
        {
            if (obj is PeonyId)
            {
                return Equals((PeonyId)obj);
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

        public static explicit operator long(PeonyId peonyId)
        {
            return peonyId.ToLong();
        }

        public static explicit operator string(PeonyId peonyId)
        {
            return peonyId.ToString();
        }

        public static bool operator <(PeonyId left, PeonyId right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(PeonyId left, PeonyId right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator ==(PeonyId left, PeonyId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PeonyId left, PeonyId right)
        {
            return !(left == right);
        }

        public static bool operator >=(PeonyId left, PeonyId right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator >(PeonyId left, PeonyId right)
        {
            return left.CompareTo(right) > 0;
        }

        internal static class PeonyIdTimer
        {
            private static readonly DateTime __unixEpoch;
            private static readonly long __dateTimeMaxValueSecondsSinceEpoch;
            private static readonly long __dateTimeMinValueSecondsSinceEpoch;
            private static readonly long __dateTimeMaxValueMillisecondsSinceEpoch;
            private static readonly long __dateTimeMinValueMillisecondsSinceEpoch;

            private static readonly DateTime __peonyIdEpoch;
            private static readonly long __peonyIdEpochOffsetBySeconds; // 1483228800

            static PeonyIdTimer()
            {
                __unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                __dateTimeMaxValueSecondsSinceEpoch = (DateTime.MaxValue - __unixEpoch).Ticks / 10000 / 1000;
                __dateTimeMinValueSecondsSinceEpoch = (DateTime.MinValue - __unixEpoch).Ticks / 10000 / 1000;
                __dateTimeMaxValueMillisecondsSinceEpoch = (DateTime.MaxValue - __unixEpoch).Ticks / 10000;
                __dateTimeMinValueMillisecondsSinceEpoch = (DateTime.MinValue - __unixEpoch).Ticks / 10000;

                __peonyIdEpoch = new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                __peonyIdEpochOffsetBySeconds = (__peonyIdEpoch - __unixEpoch).Ticks / 10000 / 1000;
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

            public static DateTime PeonyIdEpoch { get { return __peonyIdEpoch; } }

            public static long PeonyIdEpochOffsetBySeconds { get { return __peonyIdEpochOffsetBySeconds; } }

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

            public static DateTime ToDateTimeFromSecondsSincePeonyIdEpoch(long secondsSincePeonyIdEpoch)
            {
                return ToDateTimeFromSecondsSinceEpoch(PeonyIdEpochOffsetBySeconds + secondsSincePeonyIdEpoch);
            }

            public static DateTime ToDateTimeFromMillisecondsSincePeonyIdEpoch(long millisecondsSincePeonyIdEpoch)
            {
                return ToDateTimeFromMillisecondsSinceEpoch((PeonyIdEpochOffsetBySeconds * 1000) + millisecondsSincePeonyIdEpoch);
            }

            public static long GetSecondsSincePeonyIdEpochFromDateTime(DateTime dateTime)
            {
                return GetSecondsSinceEpochFromDateTime(dateTime) - PeonyIdEpochOffsetBySeconds;
            }

            public static long GetMillisecondsSincePeonyIdEpochFromDateTime(DateTime dateTime)
            {
                return GetMillisecondsSinceEpochFromDateTime(dateTime) - (PeonyIdEpochOffsetBySeconds * 1000);
            }
        }
    }
}
