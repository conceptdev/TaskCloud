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
				var added = AzureWebService.AddTodo (task);
				tasks.Add (added);
				Reload (); // show the new task
			});
			NavigationItem.RightBarButtonItem = addButton;

			// UIBarButtonSystemItem.Refresh or http://barrow.io/posts/iphone-emoji/
			refreshButton = new UIBarButtonItem('\uE049'.ToString ()
			, UIBarButtonItemStyle.Plain
			, (s,e) => {
				tasks = AzureWebService.LoadTodos(Reload);
			});
			NavigationItem.LeftBarButtonItem = refreshButton;

			tasks = AzureWebService.LoadTodos(Reload);
		}
	
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			Reload ();
		}

		public void Reload() {
			Console.WriteLine ("Reload MonoTouch.Dialog");
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
	}
}