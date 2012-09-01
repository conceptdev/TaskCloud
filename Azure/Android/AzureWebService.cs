using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Json;

namespace Azure
{
	/// <summary>
	/// Loosely based on
	/// http://chrisrisner.com/Windows-Azure-Mobile-Services-and-iOS
	/// </summary>
	public static class AzureWebService
	{

        static string subdomain = "xxxxxxx"; // your subdomain
        static string MobileServiceAppId = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"; // your application key

        static string GetAllUrl = "https://" + subdomain + ".azure-mobile.net/tables/TodoItem"; //?$filter=(complete%20eq%20false)
        static string GetUrl = "https://" + subdomain + ".azure-mobile.net/tables/TodoItem?$filter=(id%20eq%20{0})";
        static string AddUrl = "https://" + subdomain + ".azure-mobile.net/tables/TodoItem";
        static string UpdateUrl = "https://" + subdomain + ".azure-mobile.net/tables/TodoItem/{0}";
        static string DeleteUrl = "https://" + subdomain + ".azure-mobile.net/tables/TodoItem/{0}";
		/// <summary>
		/// GET /tables/TodoItem
		/// </summary>
		public static List<Task> LoadTodos(Action<List<Task>> whenDone ) 
		{
			var tasks = new List<Task>();
			WebClient client = new WebClient();
			try {
                ServicePointManager.CertificatePolicy = new NoCheck(); // HACK: sorry, quick hack around SSL cert issue
				// make it synchronous
				client.Headers.Add (HttpRequestHeader.Accept, "application/json");
				client.Headers.Add ("X-ZUMO-APPLICATION", MobileServiceAppId);
				var response = client.DownloadData (GetAllUrl); // GET
				// ...and wait...
				var responseString = System.Text.Encoding.UTF8.GetString(response);
				// RETURNS [{"id":1,"text":"Port to iOS and Android","complete":false}]
				
				var responseJson = JsonValue.Parse (responseString); //HACK:
				
				if (responseJson != null)
				{
					tasks = new List<Task>();
					for (var j = 0;j <responseJson.Count; j++) {
						var t = responseJson[j];// as JsonValue;
						var task = new Task(t);
						
						tasks.Add (task);
					}
					whenDone(tasks); // hacky to keep doing this...?
				}
				
				Console.WriteLine ("Json response => " + responseString);
				
			} catch (System.Net.WebException e) {
				Console.WriteLine ("X-ZUMO-APPLICATION failed" + e.Message);
			}
			return tasks;
		}

        /// <summary>
        /// GET /tables/TodoItem/{id}
        /// </summary>
        public static Task GetTodo(int id)
        {
            Task task = null;
            WebClient client = new WebClient();
            try {
                ServicePointManager.CertificatePolicy = new NoCheck(); // HACK: sorry, quick hack around SSL cert issue
                // make it synchronous
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                client.Headers.Add("X-ZUMO-APPLICATION", MobileServiceAppId);
                var response = client.DownloadData(String.Format(GetUrl, id)); // GET
                // ...and wait...
                var responseString = System.Text.Encoding.UTF8.GetString(response);
                // RETURNS [{"id":1,"text":"Port to iOS and Android","complete":false}]

                var responseJson = JsonValue.Parse(responseString);

                if (responseJson != null) {
                    for (var j = 0; j < responseJson.Count; j++) {
                        var t = responseJson[j];// as JsonValue;
                        task = new Task(t);
                        break; // just one required :)
                    }
                }

                Console.WriteLine("Json get response => " + responseString);

            } catch (System.Net.WebException e) {
                Console.WriteLine("X-ZUMO-APPLICATION failed" + e.Message);
            }
            return task;
        }

		/// <summary>
		/// PATCH /tables/TodoItem
		/// 
		/// </summary>
		public static void UpdateTodo(Task t) 
		{
			WebClient client = new WebClient();
			try {
                ServicePointManager.CertificatePolicy = new NoCheck(); // HACK: sorry, quick hack around SSL cert issue
				// make it synchronous
				client.Headers.Add (HttpRequestHeader.Accept, "application/json");
				client.Headers.Add (HttpRequestHeader.ContentType, "application/json");
				client.Headers.Add ("X-ZUMO-APPLICATION", MobileServiceAppId);
				
				var payload = t.ToJson ();
				var response = client.UploadString (String.Format (UpdateUrl,t.Id), "PATCH", payload); 
				// ...and wait...
				var responseString = response;
				
				//var responseJson = JsonValue.Parse (responseString);
				Console.WriteLine ("Update Json response => " + responseString);
				
			} catch (System.Net.WebException e) {
				Console.WriteLine ("X-ZUMO-APPLICATION update failed" + e.Message);
			}
		}

		/// <summary>
		/// POST /tables/TodoItem
		/// 
		/// </summary>
		public static void AddTodo(Task t) 
		{
			WebClient client = new WebClient();
			try {
                ServicePointManager.CertificatePolicy = new NoCheck(); // HACK: sorry, quick hack around SSL cert issue
				// make it synchronous
				client.Headers.Add (HttpRequestHeader.Accept, "application/json");
				client.Headers.Add (HttpRequestHeader.ContentType, "application/json");
				client.Headers.Add ("X-ZUMO-APPLICATION", MobileServiceAppId);
				
				var payload = t.ToJson ();
				var response = client.UploadString (AddUrl, "POST", payload); // PATCH
				// ...and wait...
				var responseString = response;
				
				//var responseJson = JsonValue.Parse (responseString); //HACK:
				Console.WriteLine ("Add Json response => " + responseString);
				
			} catch (System.Net.WebException e) {
				Console.WriteLine ("X-ZUMO-APPLICATION add failed" + e.Message);
			}
		}

        /// <summary>
        /// DELETE /tables/TodoItem/{id}
        /// </summary>
        public static void DeleteTodo(Task t)
        {
            WebClient client = new WebClient();
            try {
                ServicePointManager.CertificatePolicy = new NoCheck(); // HACK: sorry, quick hack around SSL cert issue
                // make it synchronous
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add("X-ZUMO-APPLICATION", MobileServiceAppId);

                var payload = t.ToJson();
                var response = client.UploadString(String.Format(DeleteUrl, t.Id), "DELETE", payload); // DELETE
                // ...and wait...
                var responseString = response;

                //var responseJson = JsonValue.Parse (responseString); //HACK:
                Console.WriteLine("Delete Json response => " + responseString);

            } catch (System.Net.WebException e) {
                Console.WriteLine("X-ZUMO-APPLICATION add failed" + e.Message);
            }
        }

        /// <summary>
        ///  HACK: sorry, quick hack around SSL cert issue
        /// </summary>
        class NoCheck : ICertificatePolicy {

            public bool CheckValidationResult(ServicePoint srvPoint, System.Security.Cryptography.X509Certificates.X509Certificate certificate, WebRequest request, int certificateProblem)
            {
                return true;
            }
        }
	}
}

