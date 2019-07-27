using Newtonsoft.Json.Linq;
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

            IEnumerable<string> key;
            Request.Headers.TryGetValues("ApiKey", out key);
            if (key != null)
            {
                udba.CreateLog(key.First(), "user/get");
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
                    udba.CreateLog(response, "user/post");
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
            this.Request.Headers.TryGetValues("ApiKey", out key);
            string APIKey = key.First();
            if (udba.FindKey(APIKey))
            {
                udba.CreateLog(APIKey, "user/deleted");
                udba.DeleteUser(username);
                deleted = true;
            }
            return Request.CreateResponse(HttpStatusCode.OK, deleted);
        }

        [AdminRole]
        [APIAuthorise]
        [ActionName("changerole")]
        public HttpResponseMessage Post(JObject body)
        {
            bool changed;
            if (body != null)
            {
                string username = body.GetValue("username").ToString();
                string role = body.GetValue("role").ToString();
                if (role != "Admin" && role != "User")
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "NOT DONE: Role does not exist");
                }
                else
                {
                    try
                    {
                        changed = udba.ChangeRole(username, role);
                    }
                    catch
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "NOT DONE: An error occured");
                    }
                    if (changed)
                    {
                        IEnumerable<string> key;
                        Request.Headers.TryGetValues("ApiKey", out key);
                        udba.CreateLog(key.First(), "user/changerole");
                        return Request.CreateResponse(HttpStatusCode.OK, "DONE");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "NOT DONE: User does not exist");
                    }
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "NOT DONE: No body sent");
            }
        }
    }
}