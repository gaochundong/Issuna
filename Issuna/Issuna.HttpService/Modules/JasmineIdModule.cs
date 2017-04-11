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
            byte region = 0;
            ushort machine = 0;
            byte precision = 0;
            long timestamp = 0;

            if (this.Request.Query.region.HasValue)
            {
                if (!byte.TryParse(this.Request.Query.region.Value.ToString(), out region)
                    || region < 0)
                {
                    return HttpStatusCode.BadRequest;
                }
            }
            if (this.Request.Query.machine.HasValue)
            {
                if (!ushort.TryParse(this.Request.Query.machine.Value.ToString(), out machine)
                    || machine < 0)
                {
                    return HttpStatusCode.BadRequest;
                }
            }
            if (this.Request.Query.precision.HasValue)
            {
                if (!byte.TryParse(this.Request.Query.precision.Value.ToString(), out precision)
                    || precision < 0)
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

            return JasmineId.GenerateNewId(reserved, region, machine, precision,
                timestamp: (timestamp > 0 ? (long?)timestamp : null)).ToLong().ToString();
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
