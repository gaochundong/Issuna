using System.Net;
using Happer.Http;
using Issuna.Core;

namespace Issuna.RestService
{
    public class MongoIdModule : Module
    {
        public MongoIdModule()
            : base(@"/mongo")
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
            int timestamp = 0;

            if (this.Request.Query.timestamp.HasValue)
            {
                if (!int.TryParse(this.Request.Query.timestamp.Value.ToString(), out timestamp)
                    || timestamp < 0)
                {
                    return HttpStatusCode.BadRequest;
                }
            }

            return timestamp > 0 ?
                MongoId.GenerateNewId(timestamp).ToString() : MongoId.GenerateNewId().ToString();
        }

        private dynamic InverseId(dynamic parameters)
        {
            if (!this.Request.Query.id.HasValue)
            {
                return HttpStatusCode.BadRequest;
            }

            MongoId id;
            if (!MongoId.TryParse(this.Request.Query.id.ToString(), out id))
            {
                return HttpStatusCode.BadRequest;
            }

            return this.Response.AsJson(id);
        }
    }
}
