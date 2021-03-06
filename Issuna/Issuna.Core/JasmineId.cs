﻿using System;
using System.Threading;

namespace Issuna.Core
{
    [Serializable]
    public struct JasmineId : IComparable<JasmineId>, IEquatable<JasmineId>
    {
        public const int ReservedBits = 1;   // 1 bit reserved, 0 by default
        public const int TimestampBits = 30; // 30 bits timestamp with seconds, max ‭1073741823‬, 1073741823/31536000 = 34.04 years
        public const int RegionBits = 4;     // 4 bits covers 16 regions
        public const int MachineBits = 9;    // 9 bits covers 512 servers
        public const int SequenceBits = 20;  // 20 bits covers 1048576/second

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

        public JasmineId(long id)
        {
            Unpack(id, out _reserved, out _timestamp, out _region, out _machine, out _sequence);
        }

        public JasmineId(byte reserved, long timestamp, byte region, ushort machine, int sequence)
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
        public DateTime CreationTime { get { return JasmineIdTimer.ToDateTimeFromSecondsSinceJasmineIdEpoch(Timestamp); } }

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

        public static void Unpack(long jasmineId, out byte reserved, out long timestamp, out byte region, out ushort machine, out int sequence)
        {
            reserved = (byte)((jasmineId >> ReservedShift) & ReservedMask);
            timestamp = (long)((jasmineId >> TimestampShift) & TimestampMask);
            region = (byte)((jasmineId >> RegionShift) & RegionMask);
            machine = (ushort)((jasmineId >> MachineShift) & MachineMask);
            sequence = (int)((jasmineId >> SequenceShift) & SequenceMask);
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
                var message = string.Format("'{0}' is not a valid JasmineId string.", value);
                throw new FormatException(message);
            }
        }

        public static JasmineId Parse(long value)
        {
            return new JasmineId(value);
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

        public static bool TryParse(long value, out JasmineId jasmineId)
        {
            try
            {
                jasmineId = new JasmineId(value);
                return true;
            }
            catch
            {
                jasmineId = default(JasmineId);
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

        public static JasmineId GenerateNewId()
        {
            return GenerateNewId(timestamp: (DateTime?)null);
        }

        public static JasmineId GenerateNewId(byte reserved = 0, DateTime? timestamp = null, byte region = 0, ushort machine = 0, int? sequence = null)
        {
            long timestampLong = JasmineIdTimer.GetSecondsSinceJasmineIdEpochFromDateTime(timestamp.HasValue ? timestamp.Value : DateTime.UtcNow);

            return GenerateNewId(reserved: reserved, timestamp: timestampLong, region: region, machine: machine, sequence: sequence);
        }

        public static JasmineId GenerateNewId(byte reserved = 0, long? timestamp = null, byte region = 0, ushort machine = 0, int? sequence = null)
        {
            long timestampLong = timestamp.HasValue ? timestamp.Value : JasmineIdTimer.GetSecondsSinceJasmineIdEpochFromDateTime(DateTime.UtcNow);

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

            return new JasmineId(reserved, timestampLong, region, machine, sequenceInt);
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
            return string.Format("Reserved[{0}], Timestamp[{1}], Region[{2}], Machine[{3}], Sequence[{4}]",
                Reserved, Timestamp, Region, Machine, Sequence);
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

        internal static class JasmineIdTimer
        {
            private static readonly DateTime __unixEpoch;
            private static readonly long __dateTimeMaxValueSecondsSinceEpoch;
            private static readonly long __dateTimeMinValueSecondsSinceEpoch;
            private static readonly long __dateTimeMaxValueMillisecondsSinceEpoch;
            private static readonly long __dateTimeMinValueMillisecondsSinceEpoch;

            private static readonly DateTime __jasmineIdEpoch;
            private static readonly long __jasmineIdEpochOffsetBySeconds; // 1483228800

            static JasmineIdTimer()
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

            public static DateTime JasmineIdEpoch { get { return __jasmineIdEpoch; } }

            public static long JasmineIdEpochOffsetBySeconds { get { return __jasmineIdEpochOffsetBySeconds; } }

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

            public static DateTime ToDateTimeFromSecondsSinceJasmineIdEpoch(long secondsSinceJasmineIdEpoch)
            {
                return ToDateTimeFromSecondsSinceEpoch(JasmineIdEpochOffsetBySeconds + secondsSinceJasmineIdEpoch);
            }

            public static DateTime ToDateTimeFromMillisecondsSinceJasmineIdEpoch(long millisecondsSinceJasmineIdEpoch)
            {
                return ToDateTimeFromMillisecondsSinceEpoch((JasmineIdEpochOffsetBySeconds * 1000) + millisecondsSinceJasmineIdEpoch);
            }

            public static long GetSecondsSinceJasmineIdEpochFromDateTime(DateTime dateTime)
            {
                return GetSecondsSinceEpochFromDateTime(dateTime) - JasmineIdEpochOffsetBySeconds;
            }

            public static long GetMillisecondsSinceJasmineIdEpochFromDateTime(DateTime dateTime)
            {
                return GetMillisecondsSinceEpochFromDateTime(dateTime) - (JasmineIdEpochOffsetBySeconds * 1000);
            }
        }
    }
}
