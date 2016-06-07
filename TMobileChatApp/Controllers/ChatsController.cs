using Microsoft.SharePoint.Client.UserProfiles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using TMobileChatApp.Models;

namespace TMobileChatApp.Controllers
{
    public class ChatsController : ApiController
    {
        private TMobileChatAppContext db = new TMobileChatAppContext();

        // POST: api/Chats
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PostChat(Chat chat)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string userId = await CheckAuthAndGetId();
            Chat chatToSave =
                new Chat
                {
                    SenderId = userId,
                    RecipientId = chat.RecipientId,
                    ChatText = chat.ChatText
                };

            db.Chats.Add(chat);
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.OK);
        }

        [Route(@"chats/sinceDate/{fromDate:regex(^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$)}")]
        [ResponseType(typeof(IEnumerable<Chat>))]
        public async Task<IHttpActionResult> GetChats(DateTime fromDate)
        {
            List<Chat> returnChates = new List<Chat>();
            string userId = await CheckAuthAndGetId();
            fromDate = fromDate.ToUniversalTime();

            returnChates = db.Chats.Where(c => (c.SenderId == userId || c.RecipientId == userId) && c.PostDate >= fromDate).ToList();

            return Ok(returnChates);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ChatExists(int id)
        {
            return db.Chats.Count(e => e.ChatId == id) > 0;
        }

        private async Task<string> CheckAuthAndGetId()
        {
            CookieHeaderValue authCookie = Request.Headers.GetCookies("FedAuth").FirstOrDefault();
            CookieHeaderValue rtFaCookie = Request.Headers.GetCookies("rtFa").FirstOrDefault();
            if (authCookie != null && rtFaCookie != null)
            {
                //string authCookieStr = "77u/PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz48U1A+RmFsc2UsMGguZnxtZW1iZXJzaGlwfDEwMDMwMDAwOGJmOTNkNzRAbGl2ZS5jb20sMCMuZnxtZW1iZXJzaGlwfHNhaXNAc2xhbG9tY29uc3VsdGluZy5vbm1pY3Jvc29mdC5jb20sMTMxMTAyMTg2NTcyMzQzMTczOzEzMTA2OTQ2MjczMDAwMDAwMCxGYWxzZSxXZnlhSHNPYVpVOUdpSElVR3JySGpTbTk0Y3hPOWdnbnZGYzhWdHhZbExJaVhxZGJQRmo0WitvU1l3TC9vUmwxSDlpTkNhelVTdFBRVmV0NmJtK3FpVVJrUlVDQWYrdVl6dVVDa0w2VTUzeDZMS0tscTYvc2h0NW13NWkyaC9XbGlzZlpBdDJsWFEwSG84bHFNV2dQN21FTG9aTE01Q1VDemNnelg5ZmNjcGNmODljV1VCUDBuYytnSk5NalBXQjJIRC95Tk1yMGZMZEVERXJ2b3hzS1R2V3p1SzZ4YnFZTldyM084THZMdHliRDFNamlvTjRHYjFKVHoyZWM2Yy83eTB2S1N3aFVEeTFvdVZta1N3NU1jNkFaYkNxMVk2cGdkTm9ZUEdFNzZJUHg4RktqYzNBY20raVhqRU5KelBuZ2FzWWJGYXl6QzNadjdXQlRFbFpLd2c9PSxodHRwczovL3NsYWxvbWNvbnN1bHRpbmcuc2hhcmVwb2ludC5jb20vc2l0ZXMvdG1vaW50cmFuZXQ8L1NQPg==";
                //string rtFaCookieStr = "KKw6mBRxRfW/p2SkTdteFtC1VW4K4t/zvxzqrLMMdQMPNQKayqjBN4uG+bynInS/lOLDBD7zgNjJkvWqomcb3R6iI9W5pTYuxOZzbJTff5f2M9QoHgTUPzkdu9990HdhPJobXwncGA7I4d04Jw5yDUG+FdRKbKPlf39z6YQoI+0gIW/wPctgJSpsb0NJwhEsKGV7OEFlCy5JCZhw1ebcfguImdXhNz4SZOed7JOIlIBz6U9mc0clV3XqO3CgRmA04tEuRiwuOCbec2cWDbTgCACoJj0E50mc4Vej/mTKZelfGUqlFgyIdLncOQbAjQtd2Sl7Zr5HfZWwrP4eWQA/+QT142Lpc9v26aAPfZYVdS6qE3ON5CB14G/N8h/bkCAYIAAAAA==";
                string authCookieStr = authCookie["FedAuth"].Value;
                string rtFaCookieStr = rtFaCookie["rtFa"].Value;
                string userId = "";

                var baseAddress = new Uri("https://slalomconsulting.sharepoint.com/");
                var cookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    cookieContainer.Add(baseAddress, new Cookie("FedAuth", authCookieStr));
                    cookieContainer.Add(baseAddress, new Cookie("rtFa", rtFaCookieStr));

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // New code:
                    HttpResponseMessage response = await client.GetAsync("sites/tmointranet/_api/SP.UserProfiles.PeopleManager/GetMyProperties/UserProfileProperties");
                    if (response.IsSuccessStatusCode)
                    {
                        object personProperties = await response.Content.ReadAsAsync<object>();
                        userId = ((personProperties as Newtonsoft.Json.Linq.JObject)["value"] as Newtonsoft.Json.Linq.JArray).Children().FirstOrDefault(c => c.Value<string>("Key") == "EmployeeID").Value<string>("Value").ToString();
                    }
                    else
                    {
                        throw new InvalidDataException(string.Format("User not authenticated, invalid authentication cookies!/r/n{0}/r/n{1}", authCookieStr, rtFaCookieStr));
                    }
                }

                return userId;
            }
            else
            {
                throw new InvalidDataException("User not authenticated, no authentication cookies passed!");
            }
        }
    }
}