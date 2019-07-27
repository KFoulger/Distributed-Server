using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SecuroteckWebApplication.Models;

namespace SecuroteckWebApplication.Controllers
{
    public class APIAuthorisationHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            #region Task5
            UserDatabaseAccess udba = new UserDatabaseAccess();
            IEnumerable<string> key;
            request.Headers.TryGetValues("ApiKey", out key);
            if (key != null)
            {
                string APIkey = key.First();
                if (!string.IsNullOrEmpty(APIkey))
                {
                    if (udba.FindKey(APIkey))
                    {
                        User user = udba.ReturnUser(APIkey);
                        Claim name = new Claim(ClaimTypes.Name, user.UserName);
                        Claim role = new Claim(ClaimTypes.Role, user.Role);
                        ClaimsIdentity id = new ClaimsIdentity(new Claim[]
                        {
                        name,
                        role
                        }, APIkey);
                        Thread.CurrentPrincipal = new ClaimsPrincipal(id);
                        return await base.SendAsync(request, cancellationToken);
                    }
                }
            }
            #endregion
            return await base.SendAsync(request, cancellationToken);
        }
    }
}