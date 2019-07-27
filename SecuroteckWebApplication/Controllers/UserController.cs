using SecuroteckWebApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SecuroteckWebApplication.Controllers
{
    public class UserController : ApiController
    {
        UserDatabaseAccess udba = new UserDatabaseAccess();

        [ActionName("new")]
        public HttpResponseMessage Get([FromUri]string username)
        {
            string response = "False - User Does Not Exist! Did you mean to do a POST to create a new user?";
            bool found = udba.FindUser(username);
            if (found)
            {
                response = "True - User Does Exist!";
            }
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [ActionName("new")]
        public HttpResponseMessage Post([FromBody]string username)
        {
            string response = "Oops. This username is already in use. Please try again with a new username.";
            bool found = udba.FindUser(username);
            if (!found)
            {
                if (string.IsNullOrEmpty(username))
                {
                    response = "Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json";
                    return Request.CreateResponse(HttpStatusCode.BadRequest, response);
                }
                else
                {
                    response = udba.CreateUser(username);
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, response);
            }
        }

        [APIAuthorise]
        [ActionName("removeuser")]
        public HttpResponseMessage Delete([FromUri]string username)
        {
            bool deleted = false;
            IEnumerable<string> key;
            this.Request.Headers.TryGetValues("APIKey", out key);
            if (udba.FindUser(key.ToString()))
            {
                udba.DeleteUser(username);
                deleted = true;
            }
            return Request.CreateResponse(HttpStatusCode.OK, deleted);
        }
    }
}
