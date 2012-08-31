using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

using System.Net;
using System.Json;

namespace Azure {
	public class TaskListScreen : DialogViewController {

		UIBarButtonItem addButton, refreshButton;
		
		List<Task> tasks; // local copy of task list

		public TaskListScreen () : base (UITableViewStyle.Plain, new RootElement("Loading..."))
		{
			Title = "TaskyAzure";
			tasks = new List<Task>();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, (s,e) =>{
				var task = new Task() {Title="<new task>"};
				// Save to Azure
				AddTodo (task);
				Reload (); // to reflect the added task
			});
			NavigationItem.RightBarButtonItem = addButton;
			
			// UIBarButtonSystemItem.Refresh or http://barrow.io/posts/iphone-emoji/
			refreshButton = new UIBarButtonItem('\uE049'.ToString ()
			, UIBarButtonItemStyle.Plain
			, (s,e) => {
				LoadTodos();
			});
			NavigationItem.LeftBarButtonItem = refreshButton;

			LoadTodos();
		}
	
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			Reload ();
		}

		void Reload() {
			InvokeOnMainThread(()=>{
				Root = 	new RootElement ("TaskyAzure") {
						new Section () {
						from task in tasks
						orderby task.Title
						select (Element) new StringElement (task.Title, () =>{
							var ts = new TaskScreen(task);
							NavigationController.PushViewController (ts, true);
						})
					}
				};
			});
		}

		/// <summary>
		/// Loosely based on
		/// http://chrisrisner.com/Windows-Azure-Mobile-Services-and-iOS
		/// </summary>
		void LoadTodos() 
		{
			WebClient client = new WebClient();
			try {
				// make it synchronous
				client.Headers.Add (HttpRequestHeader.Accept, "application/json");
				client.Headers.Add ("X-ZUMO-APPLICATION", Constants.MobileServiceAppId);
				var response = client.DownloadData (Constants.GetAllUrl); // GET
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
					Reload (); // hacky to keep doing this...?
				}

				Console.WriteLine ("Json response => " + responseString);

			} catch (System.Net.WebException e) {
				Console.WriteLine ("X-ZUMO-APPLICATION failed" + e.Message);
			}
		}
		internal static void UpdateTodo(Task t) 
		{
			WebClient client = new WebClient();
			try {
				// make it synchronous
				client.Headers.Add (HttpRequestHeader.Accept, "application/json");
				client.Headers.Add (HttpRequestHeader.ContentType, "application/json");
				client.Headers.Add ("X-ZUMO-APPLICATION", Constants.MobileServiceAppId);

				var payload = t.ToJson ();
				var response = client.UploadString (Constants.UpdateUrl + t.Id, "PATCH", payload); // PATCH
				// ...and wait...
				var responseString = response;
				
				var responseJson = JsonValue.Parse (responseString); //HACK:

				Console.WriteLine ("Update Json response => " + responseString);
				
			} catch (System.Net.WebException e) {
				Console.WriteLine ("X-ZUMO-APPLICATION update failed" + e.Message);
			}
		}
		void AddTodo(Task t) 
		{
			WebClient client = new WebClient();
			try {
				// make it synchronous
				client.Headers.Add (HttpRequestHeader.Accept, "application/json");
				client.Headers.Add (HttpRequestHeader.ContentType, "application/json");
				client.Headers.Add ("X-ZUMO-APPLICATION", Constants.MobileServiceAppId);

				var payload = t.ToJson ();
				var response = client.UploadString (Constants.AddUrl, "POST", payload); // PATCH
				// ...and wait...
				var responseString = response;

				var responseJson = JsonValue.Parse (responseString); //HACK:
				
				Console.WriteLine ("Add Json response => " + responseString);
				
			} catch (System.Net.WebException e) {
				Console.WriteLine ("X-ZUMO-APPLICATION add failed" + e.Message);
			}
		}
	}
}