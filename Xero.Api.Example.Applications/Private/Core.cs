using Xero.Api.Core;
using Xero.Api.Infrastructure.OAuth;
using Xero.Api.Infrastructure.RateLimiter;
using Xero.Api.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace Xero.Api.Example.Applications.Private
{
    public class Core : XeroCoreApi
    {
        private static readonly DefaultMapper Mapper = new DefaultMapper();
        private static readonly Settings ApplicationSettings = new Settings();

        public Core(X509Certificate2 cetificate, bool includeRateLimiter = false) :
            base(ApplicationSettings.Uri,
                new PrivateAuthenticator(cetificate),
                new Consumer(ApplicationSettings.Key, ApplicationSettings.Secret),
                null,
                Mapper,
                Mapper,
                includeRateLimiter ? new RateLimiter() : null)
        {
        }
    }
}
