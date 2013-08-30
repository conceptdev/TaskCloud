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
		UIButton addButton2;
		
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

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, async (s,e) => {
				var task = new Task() {Title="<new task>"};
				// Save to Parse
				await task.ToParseObject().SaveAsync();
				tasks.Add (task);
				Reload (); // show the new task
			});
			NavigationItem.RightBarButtonItem = addButton;

			addButton2 = UIButton.FromType (UIButtonType.Custom);
			addButton2.Frame = new System.Drawing.RectangleF ();
			addButton2.TouchUpInside += async (s,e) => {
				var task = new Task() {Title="<new task>"};
				// Save to Parse
				await task.ToParseObject().SaveAsync();
				tasks.Add (task);

				var ts = new TaskScreen (task);
				NavigationController.PushViewController (ts, true);
			};
			NavigationItem.RightBarButtonItem = addButton;
		}
	
		public override async void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			tasks = await Task.GetAll();
			Reload ();
		}

		public void Reload() {
			Console.WriteLine ("Reload MonoTouch.Dialog");
		
			var tasklist = from task in tasks
				orderby task.Title
					select (Element)new TickElement (task.Title, task.IsDone, () => {
				var ts = new TaskScreen (task);
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