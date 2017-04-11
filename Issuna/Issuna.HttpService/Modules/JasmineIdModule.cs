using System.Net;
using Happer.Http;
using Issuna.Core;

namespace Issuna.HttpService
{
    public class JasmineIdModule : Module
    {
        public JasmineIdModule()
            : base(@"/jasmine")
        {
            Get("/generate", parameters =>
            {
                return GenerateId(parameters);
            });
            Get("/inverse", parameters =>
            {
                return InverseId(parameters);
            });
        }

        private dynamic GenerateId(dynamic parameters)
        {
            byte reserved = 0;
            long timestamp = -1;
            byte region = 0;
            ushort machine = 0;
            int sequence = -1;

            if (this.Request.Query.reserved.HasValue)
            {
                if (!byte.TryParse(this.Request.Query.reserved.Value.ToString(), out reserved))
                {
                    return HttpStatusCode.BadRequest;
                }
            }
            if (this.Request.Query.timestamp.HasValue)
            {
                if (!long.TryParse(this.Request.Query.timestamp.Value.ToString(), out timestamp)
                    || timestamp < 0)
                {
                    return HttpStatusCode.BadRequest;
                }
            }
            if (this.Request.Query.region.HasValue)
            {
                if (!byte.TryParse(this.Request.Query.region.Value.ToString(), out region))
                {
                    return HttpStatusCode.BadRequest;
                }
            }
            if (this.Request.Query.machine.HasValue)
            {
                if (!ushort.TryParse(this.Request.Query.machine.Value.ToString(), out machine))
                {
                    return HttpStatusCode.BadRequest;
                }
            }
            if (this.Request.Query.sequence.HasValue)
            {
                if (!int.TryParse(this.Request.Query.sequence.Value.ToString(), out sequence)
                    || sequence < 0)
                {
                    return HttpStatusCode.BadRequest;
                }
            }

            return JasmineId.GenerateNewId(
                reserved: reserved,
                timestamp: (timestamp > 0 ? (long?)timestamp : null),
                region: region,
                machine: machine,
                sequence: (sequence > 0 ? (int?)sequence : null)).ToLong().ToString();
        }

        private dynamic InverseId(dynamic parameters)
        {
            if (!this.Request.Query.id.HasValue)
            {
                return HttpStatusCode.BadRequest;
            }

            JasmineId id;
            if (!JasmineId.TryParse(this.Request.Query.id.ToString(), out id))
            {
                return HttpStatusCode.BadRequest;
            }

            return this.Response.AsJson(id);
        }
    }
}
