using Bottlecap.Net.Bots.Alexa.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Bottlecap.Net.Bots.Alexa.Web
{
    public class DirectiveRequest : Bottlecap.Web.WebRequestItem
    {
        public const string URI = "v1/directives";

        private readonly string _token;
        private readonly Directive _directive;

        public DirectiveRequest(string token, string baseUri, Directive directive)
            : base(null, Bottlecap.Web.WebRequestType.Post)
        {
            _token = token;
            _directive = directive;

            TargetUri = new Uri(new Uri(baseUri), URI);
            ContentType = "application/json";
        }

        protected override void ConstructRequestContent(ref Stream stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            {
                writer.Write(Json.JsonFactory.Current.ToJson(_directive));
            }
        }

        public override async Task<IEnumerable<Pair<string, string>>> ConstructHeaders()
        {
            var headers = new List<Pair<string, string>>(await base.ConstructHeaders());

            headers.Add(new Pair<string, string>("Authorization", String.Format("Bearer {0}", _token)));

            return headers;
        }
    }
}
