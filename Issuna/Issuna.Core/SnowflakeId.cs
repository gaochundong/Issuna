using System;

namespace Issuna.Core
{
    /// <summary>
    /// Twitter Snowflake: 1bit no-use + 41bit timestamp + 10bit machine + 12bit sequence.
    /// </summary>
    public class SnowflakeId
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public const long TwitterEpoch = 1288834974657L;

        public const int TimestampBits = 41;
        public const int DataCenterIdBits = 5;
        public const int WorkerIdBits = 5;
        public const int SequenceBits = 12;

        public const long MaxDataCenterId = -1L ^ (-1L << DataCenterIdBits);
        public const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);

        public const int WorkerIdShift = SequenceBits;
        public const int DataCenterIdShift = SequenceBits + WorkerIdBits;
        public const int TimestampShift = SequenceBits + WorkerIdBits + DataCenterIdBits;

        public const long TimestampMask = -1L ^ (-1L << TimestampBits);
        public const long DataCenterIdMask = -1L ^ (-1L << DataCenterIdBits);
        public const long WorkerIdMask = -1L ^ (-1L << WorkerIdBits);
        public const long SequenceMask = -1L ^ (-1L << SequenceBits);

        private long _sequence = 0L;
        private long _lastTimestamp = -1L;
        private readonly object _lock = new object();

        public SnowflakeId(long dataCenterId, long workerId, long sequence = 0L)
        {
            if (dataCenterId > MaxDataCenterId || dataCenterId < 0)
            {
                throw new ArgumentException(string.Format("DataCenterId can't be greater than {0} or less than 0", MaxDataCenterId));
            }
            if (workerId > MaxWorkerId || workerId < 0)
            {
                throw new ArgumentException(string.Format("WorkerId can't be greater than {0} or less than 0", MaxWorkerId));
            }

            this.DataCenterId = dataCenterId;
            this.WorkerId = workerId;
            this.Sequence = sequence;
        }

        public long DataCenterId { get; private set; }
        public long WorkerId { get; private set; }
        public long Sequence { get { return _sequence; } private set { _sequence = value; } }

        public long GenerateNewId()
        {
            lock (_lock)
            {
                var timestamp = NextTimestamp();

                if (timestamp < _lastTimestamp)
                {
                    throw new InvalidOperationException(string.Format(
                        "Clock moved backwards, then refusing to generate id for {0} milliseconds.", _lastTimestamp - timestamp));
                }

                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & SequenceMask;
                    if (_sequence == 0)
                    {
                        timestamp = TilNextMillisecond(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;

                var id = ((timestamp - TwitterEpoch) << TimestampShift) |
                         (DataCenterId << DataCenterIdShift) |
                         (WorkerId << WorkerIdShift) |
                         (_sequence);

                return id;
            }
        }

        private long TilNextMillisecond(long lastTimestamp)
        {
            var timestamp = NextTimestamp();
            while (timestamp <= lastTimestamp)
            {
                timestamp = NextTimestamp();
            }
            return timestamp;
        }

        private long NextTimestamp()
        {
            return (long)((DateTime.UtcNow - UnixEpoch).Ticks / 10000);
        }

        public static void Unpack(long snowflake, out long timestamp, out long dataCenterId, out long workerId, out long sequence)
        {
            timestamp = (long)((snowflake >> TimestampShift) & TimestampMask);
            dataCenterId = (long)((snowflake >> DataCenterIdShift) & DataCenterIdMask);
            workerId = (long)((snowflake >> WorkerIdShift) & WorkerIdMask);
            sequence = (long)(snowflake & SequenceMask);
        }
    }
}
