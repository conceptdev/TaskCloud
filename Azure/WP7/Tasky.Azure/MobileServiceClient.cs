//    Copyright 2012 Ken Egozi
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

// https://github.com/kenegozi/azure-mobile-csharp-sdk

namespace MobileServices.Sdk {
	public class MobileServiceTable<TItem> : MobileServiceTable {
		public MobileServiceTable(MobileServiceClient client, string tableName)
			: base(client, tableName) {
		}

		public void GetAll(Action<TItem[], Exception> continuation) {
			Get(null, continuation);
		}

		public void Get(MobileServiceQuery query, Action<TItem[], Exception> continuation) {
			Get(query, (arr, ex) => {
				if (ex != null) {
					continuation(null, ex);
					return;
				}

				continuation(arr.ToObject<TItem[]>(MobileServiceClient.Serializer), null);
			});
		}

		public void Insert(TItem item, Action<TItem, Exception> continuation) {
			var jobject = JObject.FromObject(item, MobileServiceClient.Serializer);
			Insert(jobject, (ans, err) => {
				if (err != null) {
					continuation(default(TItem), err);
					return;
				}

				var results = JsonConvert.DeserializeObject<TItem>(ans);

				continuation(results, null);
			});
		}

	}

	public class MobileServiceTable {
		private readonly MobileServiceClient client;
		private readonly string tableName;

		public MobileServiceTable(MobileServiceClient client, string tableName) {
			this.client = client;
			this.tableName = tableName;
		}

		void JArrayHandler(string ans, Exception err, Action<JArray, Exception> continuation) {
			if (err != null) {
				continuation(null, err);
				return;
			}

			var results = JArray.Parse(ans);

			continuation(results, null);
		}

		public void GetAll(Action<JArray, Exception> continuation) {
			Get(null, continuation);
		}

		public void Get(MobileServiceQuery query, Action<JArray, Exception> continuation) {
			var tableUrl = "tables/" + tableName;
			if (query != null) {
				var queryString = query.ToString();
				if (queryString.Length > 0) {
					tableUrl += "?" + queryString;
				}
			}
			client.Get(tableUrl, (res, exception) => JArrayHandler(res, exception, continuation));
		}

		public void Update(JObject updates, Action<Exception> continuation) {
			JToken idToken;

			if (updates.TryGetValue("id", out idToken) == false) {
				throw new Exception("missing [id] field");
			}

			var id = idToken.Value<object>().ToString();
			var tableUrl = "tables/" + tableName + "/" + id;

			client.Patch(tableUrl, updates, (s, exception) => continuation(exception));
		}

		public void Delete(object id, Action<Exception> continuation) {
			var tableUrl = "tables/" + tableName + "/" + id;
			client.Delete(tableUrl, continuation);
		}

		public void Insert(JObject item, Action<string, Exception> continuation) {
			var tableUrl = "tables/" + tableName;

			item.Remove("id");

			var nullProperties = item.Properties().Where(p => p.Value.Type == JTokenType.Null).ToArray();
			foreach (var nullProperty in nullProperties) {
				item.Remove(nullProperty.Name);
			}

			client.Post(tableUrl, item, continuation);
		}

	}

	public class MobileServiceClient {
		internal static readonly JsonSerializer Serializer;

		readonly string applicationKey;
		readonly string serviceUrl;

		public string CurrentAuthToken { get; private set; }
		public string CurrentUserId { get; private set; }

		public MobileServiceClient(string serviceUrl, string applicationKey) {
			this.serviceUrl = serviceUrl;
			this.applicationKey = applicationKey;
		}

		static MobileServiceClient() {
			Serializer = new JsonSerializer();
			Serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
			Serializer.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			Serializer.DateFormatHandling = DateFormatHandling.IsoDateFormat;//.DateTimeZoneHandling = DateTimeZoneHandling.Utc
		}

		public void Logout() {
			CurrentUserId = null;
			CurrentAuthToken = null;
		}

		public void Login(string liveAuthenticationToken, Action<string, Exception> continuation) {
			var client = new WebClient();
			var url = serviceUrl + "login?mode=authenticationToken";
			client.UploadStringCompleted += (x, args) => {
				if (args.Error != null) {
					var ex = args.Error;
					if (args.Error.InnerException != null)
						ex = args.Error.InnerException;
					continuation(null, ex);
					return;
				}
				var result = JObject.Parse(args.Result);
				CurrentAuthToken = result["authenticationToken"].Value<string>();
				CurrentUserId = result["user"]["userId"].Value<string>();
				continuation(CurrentUserId, null);
			};
			client.Headers[HttpRequestHeader.ContentType] = "application/json";
			var payload = new JObject();
			payload["authenticationToken"] = liveAuthenticationToken;
			client.UploadStringAsync(new Uri(url), payload.ToString());
		}

		public void Get(string relativeUrl, Action<string, Exception> continuation) {
			Execute("GET", relativeUrl, string.Empty, continuation);
		}

		public void Post(string relativeUrl, object payload, Action<string, Exception> continuation) {
			Execute("POST", relativeUrl, payload, continuation);
		}

		public void Delete(string relativeUrl, Action<Exception> continuation) {
			Execute("DELETE", relativeUrl, string.Empty, (s, err) => continuation(err));
		}

		public void Patch(string relativeUrl, object payload, Action<string, Exception> continuation) {
			Execute("PATCH", relativeUrl, payload, continuation);
		}

		void Execute(string method, string relativeUrl, object payload, Action<string, Exception> continuation) {
			var endpointUrl = serviceUrl + relativeUrl;
			var client = new WebClient();
			client.UploadStringCompleted += (x, args) =>
				OperationCompleted(args, continuation);
			client.DownloadStringCompleted += (x, args) =>
				OperationCompleted(args, continuation);
			SetMobileServiceHeaders(client);
			if (method == "GET") {
				client.DownloadStringAsync(new Uri(endpointUrl));
				return;
			}

			var payloadString = payload as string;
			if (payloadString == null && payload != null) {
				var buffer = new StringBuilder();
				using (var writer = new StringWriter(buffer))
					Serializer.Serialize(writer, payload);
				payloadString = buffer.ToString();
			}
			client.UploadStringAsync(new Uri(endpointUrl), method, payloadString);
		}

		void OperationCompleted(AsyncCompletedEventArgs args, Action<string, Exception> continuation) {
			if (args.Error != null) {
				var ex = args.Error;
				var webException = ex as WebException;
				if (webException != null) {
					var response = webException.Response as HttpWebResponse;
					if (response != null) {
						var code = response.StatusCode;
						var msg = response.StatusDescription;
						try {
							using (var reader = new StreamReader(response.GetResponseStream())) {
								msg += "\r\n" + reader.ReadToEnd();
							}
						}
						catch (Exception) {
							msg += "\r\nResponse body could not be extracted";
						}
						ex = new Exception(string.Format("Http error [{0}] - {1}", (int)code, msg), ex);
					}
				}
				continuation(null, ex);
				return;
			}
			string result = null;
			var uploadStringCompletedEventArgs = args as UploadStringCompletedEventArgs;
			if (uploadStringCompletedEventArgs != null)
				result = uploadStringCompletedEventArgs.Result;
			var downloadStringCompletedEventArgs = args as DownloadStringCompletedEventArgs;
			if (downloadStringCompletedEventArgs != null)
				result = downloadStringCompletedEventArgs.Result;
			if (result == null) {
				throw new InvalidOperationException("args should be either UploadStringCompletedEventArgs or DownloadStringCompletedEventArgs");
			}
			continuation(result, null);
		}

		void SetMobileServiceHeaders(WebClient client) {
			if (CurrentAuthToken != null) {
				client.Headers["X-ZUMO-AUTH"] = CurrentAuthToken;
			}
			if (applicationKey != null) {
				client.Headers["X-ZUMO-APPLICATION"] = applicationKey;
			}
		}

		public MobileServiceTable GetTable(string tableName) {
			return new MobileServiceTable(this, tableName);
		}

		public MobileServiceTable<TItem> GetTable<TItem>(string tableName) {
			return new MobileServiceTable<TItem>(this, tableName);
		}

		public MobileServiceTable<TItem> GetTable<TItem>() {
			var tableName = typeof(TItem).Name;
			return GetTable<TItem>(tableName);
		}
	}

	public class MobileServiceQuery {
		private int top;
		private int skip;
		private string orderby;
		private string filter;
		private string select;

		public MobileServiceQuery Top(int top) {
			this.top = top;
			return this;
		}

		public MobileServiceQuery Skip(int skip) {
			this.skip = skip;
			return this;
		}

		public MobileServiceQuery OrderBy(string orderby) {
			this.orderby = orderby;
			return this;
		}

		public MobileServiceQuery Filter(string filter) {
			this.filter = filter;
			return this;
		}

		public MobileServiceQuery Select(string select) {
			this.select = select;
			return this;
		}

		public override string ToString() {
			var query = new List<string>();
			if (top != 0) {
				query.Add("$top=" + top);
			}
			if (skip != 0) {
				query.Add("$skip=" + skip);
			}
			if (!string.IsNullOrEmpty(filter)) {
				query.Add("$filter=" + filter);
			}
			if (!string.IsNullOrEmpty(select)) {
				query.Add("$select=" + select);
			}
			if (!string.IsNullOrEmpty(orderby)) {
				query.Add("$orderby=" + orderby);
			}

			return string.Join("&", query);
		}
	}
}
