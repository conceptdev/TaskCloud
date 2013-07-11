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

namespace TaskyDrop {
	public class TaskListScreen : DialogViewController {

		UIBarButtonItem addButton, refreshButton;
		
		List<Task> tasks; // local copy of task list

		public TaskListScreen () : base (UITableViewStyle.Plain, new RootElement("Loading..."))
		{
			tasks = new List<Task>();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, (s,e) =>{
				var task = new Task() {Title="<new task>"};
				// Save to Azure
				tasks.Add (task);
				Reload (); // show the new task
			});
			NavigationItem.RightBarButtonItem = addButton;

			// 'cloud' http://barrow.io/posts/iphone-emoji/
			refreshButton = new UIBarButtonItem('\uE049'.ToString ()
			, UIBarButtonItemStyle.Plain
			, (s,e) => {
				new UIAlertView("Dropbox Datastore API", "Save key-value data to the Dropbox 'cloud' using Xamarin and the new Dropbox component", null, "OK", null).Show();
//				DropboxDatabase.Shared.Update();
//				HandleTasksUpdated(null,null);
			});
			NavigationItem.LeftBarButtonItem = refreshButton;


			HandleTasksUpdated(null,null);  // first time thru
		}
	
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			DropboxDatabase.Shared.TasksUpdated += HandleTasksUpdated;

			HandleTasksUpdated (null, null);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			DropboxDatabase.Shared.TasksUpdated -= HandleTasksUpdated;
		}

		void HandleTasksUpdated (object sender, EventArgs e)
		{
			tasks = DropboxDatabase.Shared.Tasks.ToList(); // get the updated tasks
			Console.WriteLine("HandleTasksUpdated triggered " + tasks.Count);
			Reload ();
		}

		public void Reload() {
			Console.WriteLine ("Reload MonoTouch.Dialog " + tasks.Count);
			InvokeOnMainThread(()=>{
				Root = 	new RootElement ("TaskyDrop") {
						new Section () {
						from task in tasks
						orderby task.Title
						select (Element) new TickElement (task.Title, task.IsDone, () =>{
							var ts = new TaskScreen(task);
							NavigationController.PushViewController (ts, true);
						}) 
					}
				};
			});
		}
	}

	/// <summary>
	/// MonoTouch.Dialog tick in a table cell
	/// </summary>
	public class TickElement : CheckboxElement {

		public TickElement(string caption, bool value, NSAction clicked)
			: base(caption, value)
		{
			this.Tapped += clicked;
		}
	}
}