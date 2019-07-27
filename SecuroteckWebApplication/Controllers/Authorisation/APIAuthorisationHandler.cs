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
        protected override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
        {
            #region Task5
            // TODO:  Find if a header ‘ApiKey’ exists, and if it does, check the database to determine if the given API Key is valid
            //        Then authorise the principle on the current thread using a claim, claimidentity and claimsprinciple
            UserDatabaseAccess udba = new UserDatabaseAccess();
            IEnumerable<string> key;
            request.Headers.TryGetValues("APIKey", out key);
            string APIkey = key.First();
            if (!string.IsNullOrEmpty(key.ToString()))
            {
                if (udba.FindKey(APIkey))
                {
                    User user = udba.ReturnUser(APIkey);
                    Claim name = new Claim("Name", user.UserName);
                    Claim role = new Claim("Role", user.Role);
                    ClaimsIdentity identity = new ClaimsIdentity();
                    identity.AddClaim(name);
                    identity.AddClaim(role);
                    Thread.CurrentPrincipal = new ClaimsPrincipal(identity);
                    //Look at lab 6
                }
            }
            #endregion
            return base.SendAsync(request, cancellationToken);
        }
    }
}