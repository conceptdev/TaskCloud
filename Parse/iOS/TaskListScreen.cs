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

namespace Parse {
	public class TaskListScreen : DialogViewController {

		UIBarButtonItem addButton;

		List<Task> tasks; // local copy of task list

		public TaskListScreen () : base (UITableViewStyle.Plain, new RootElement("Loading..."))
		{
			Title = "TaskyParse";

			UITextAttributes ta = new UITextAttributes ();
			ta.Font = UIFont.SystemFontOfSize (20f);
			UINavigationBar.Appearance.SetTitleTextAttributes (ta);
			UILabel.Appearance.Font = UIFont.SystemFontOfSize (20f);
			ta.Font = UIFont.SystemFontOfSize (12f);
			UIBarButtonItem.Appearance.SetTitleTextAttributes (ta, UIControlState.Normal);
			tasks = new List<Task>();
		}

		public override async void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, (s,e) => {
				var task = new Task() {Title="<new task>"};

				var ts = new TaskScreen (task, this);
				NavigationController.PushViewController (ts, true);
			});
			NavigationItem.RightBarButtonItem = addButton;

			await ReloadAsync();
		}
	
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			Reload ();
		}

		public async System.Threading.Tasks.Task ReloadAsync()
		{
			try {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
				tasks = await Task.GetAll();
			} catch (ParseException pe) {

			} finally {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			}
			Reload ();
		}

		public void Reload() {
			Console.WriteLine ("Reload MonoTouch.Dialog");
		
			var tasklist = from task in tasks
				orderby task.Title
					select (Element)new TickElement (task.Title, task.IsDone, () => {
				var ts = new TaskScreen (task, this);
				NavigationController.PushViewController (ts, true);
			});

			var s = new Section ();
			s.AddAll (tasklist);

			Root = new RootElement ("TaskyParse") { s };
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