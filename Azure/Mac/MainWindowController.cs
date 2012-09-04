
using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Tasky
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public MainWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		// Call to load from the XIB/NIB file
		public MainWindowController () : base ("MainWindow")
		{
			Initialize ();
		}
		
		// Shared initialization code
		void Initialize ()
		{
		}
		
		#endregion

		List<Task> tasks;
		public override void WindowDidLoad ()
		{
			base.WindowDidLoad ();
			tasks = AzureWebService.LoadTodos(Reload);
		}

		//strongly typed window accessor
		public new MainWindow Window {
			get {
				return (MainWindow)base.Window;
			}
		}
		public void Reload (List<Task> tasks)
		{
			TaskTable.DataSource = new TableDataSource(tasks);
		}
		public class TableDataSource : NSTableViewDataSource {
			List<Task> tasks;
			public TableDataSource (List<Task> tasks) {
				this.tasks = tasks;
			}
			public override int GetRowCount (NSTableView tableView)
			{
					return tasks.Count;
			}
			public override NSObject GetObjectValue (NSTableView tableView, NSTableColumn tableColumn, int row)
			{
				var task = tasks[row];
				if (tableColumn.Identifier.ToString () == "text")
					return new NSString(task.Title);
				else if (tableColumn.Identifier.ToString () == "complete")
					return new NSString(task.IsDone?"Done":"Incomplete");
				return new NSString("");
			}
		}
	}
}

