using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SecuroteckWebApplication.Controllers
{
    public class TalkBackController : ApiController
    {
        [ActionName("Hello")]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello World");
        }

        [ActionName("Sort")]
        public HttpResponseMessage Get([FromUri]int[] integers)
        {
            //TASK1
            // TODO: 
            // sort the integers into ascending order
            if (!ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
            {
                Array.Sort(integers);
                return Request.CreateResponse(HttpStatusCode.OK, integers);
            }
            // send the integers back as the api/talkback/sort response
        }

    }
}
