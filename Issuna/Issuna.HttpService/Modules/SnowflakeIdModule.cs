using System;
using System.Net;
using Happer.Http;
using Issuna.Core;

namespace Issuna.HttpService
{
    public class SnowflakeIdModule : Module
    {
        private SnowflakeId _snowflakeId = new SnowflakeId();

        public SnowflakeIdModule()
            : base(@"/snowflake")
        {
            Get("/generate", parameters =>
            {
                return GenerateId(parameters);
            });
            Get("/decode", parameters =>
            {
                return DecodeId(parameters);
            });
        }

        private dynamic GenerateId(dynamic parameters)
        {
            return _snowflakeId.GenerateNewId().ToString();
        }

        private dynamic DecodeId(dynamic parameters)
        {
            if (!this.Request.Query.id.HasValue)
            {
                return HttpStatusCode.BadRequest;
            }

            long timestamp = 0;
            long dataCenterId = 0;
            long workerId = 0;
            long sequence = 0;
            SnowflakeId.Unpack(long.Parse(this.Request.Query.id),
                out timestamp, out dataCenterId, out workerId, out sequence);

            return this.Response.AsJson(new Snowflake(timestamp, dataCenterId, workerId, sequence));
        }

        public class Snowflake
        {
            public Snowflake(long timestamp = 0, long dataCenterId = 0, long workerId = 0, long sequence = 0)
            {
                this.Timestamp = timestamp;
                this.DataCenterId = dataCenterId;
                this.WorkerId = workerId;
                this.Sequence = sequence;
            }

            public long Timestamp { get; set; }
            public long DataCenterId { get; set; }
            public long WorkerId { get; set; }
            public long Sequence { get; set; }

            public DateTime CreationTime
            {
                get
                {
                    return SnowflakeId.UnixEpoch.AddMilliseconds(SnowflakeId.TwitterEpoch + Timestamp);
                }
            }
        }
    }
}
