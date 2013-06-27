using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sample
{
	public partial class SampleViewController : UIViewController
	{
		private static readonly MobileServiceClient MobileService =
			new MobileServiceClient ("https://xamarin-todo-cd1.azure-mobile.net/", "OAUpCEPbHLIZQiOWinrUSCCBgdXHiA25");

		private readonly IMobileServiceTable<TodoItem> todoTable = MobileService.GetTable<TodoItem>();

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public SampleViewController (IntPtr handle) : base (handle)
		{
		}

		private TodoSource itemController;

		#region View lifecycle

		private UIAlertView prompt;
		partial void Add (NSObject sender)
		{
			if (prompt != null)
				prompt.Dispose ();

			prompt = new UIAlertView ("Add Item", String.Empty, null, "Cancel", "Add");
			prompt.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
			prompt.Clicked += (s, e) => 
			{
				if (e.ButtonIndex == 1)
				{
					UITextField text = prompt.GetTextField (0);
					todoTable.InsertAsync (new TodoItem { Text = text.Text })
						.ContinueWith (t => itemController.RefreshAsync(), scheduler);
				}
			};

			prompt.Show();
		}

		partial void Refresh (NSObject sender)
		{
			itemController.RefreshAsync();
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			itemController = new TodoSource (ItemsTable, todoTable);
			itemController.IsUpdatingChanged += (sender, e) =>
			{
				RefreshButton.Enabled =
				AddButton.Enabled =
					!itemController.IsUpdating;
			};

			ItemsTable.Source = itemController;
		}

		[Obsolete("This gets rid of a warning, this method deprecated in iOS6")]
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}

		#endregion
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			if (UserInterfaceIdiomIsPhone) {
				return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
			} else {
				return true;
			}
		}

		private TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
	}

	public class TodoSource
		: UITableViewSource
	{
		public TodoSource (UITableView view, IMobileServiceTable<TodoItem> table)
		{
			tableView = view;
			this.table = table;
			RefreshAsync();
		}

		public event EventHandler IsUpdatingChanged;

		public bool IsUpdating
		{
			get { return isUpdating; }
			private set
			{
				isUpdating = value;

				var changed = IsUpdatingChanged;
				if (changed != null)
					changed (this, EventArgs.Empty);
			}
		}

		public void RefreshAsync()
		{
			IsUpdating = true;
			//HACK: change which tasks are listed...
			//table.Where (ti => !ti.Complete).ToListAsync()    //HACK: only show if not complete
			//table.Where (ti => ti.Complete).ToListAsync()     //HACK: only show if complete
			table.ToListAsync()     							//HACK: show ALL
				.ContinueWith (t =>
				{
					items = t.Result;
					tableView.ReloadData();
					IsUpdating = false;
				}, scheduler);
		}

		public void Insert (TodoItem item)
		{
			IsUpdating = true;

			NSIndexPath path = NSIndexPath.Create (items.Count);
			items.Add (item);

			tableView.InsertRows (new[] { path }, UITableViewRowAnimation.Automatic);

			table.InsertAsync (item).ContinueWith (t =>
			{
				if (t.IsFaulted)
				{
					items.Remove (item);
					tableView.DeleteRows (new[] { path }, UITableViewRowAnimation.Automatic);
				}

				IsUpdating = false;
			}, scheduler);
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			if (items == null)
				return 0;

			return items.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell ("todo");
			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Default, "todo");

			var item = items[indexPath.Row];
			cell.Accessory = item.Complete ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
			cell.TextLabel.Text = item.Text;

			return cell;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			IsUpdating = true;

			TodoItem item = items[indexPath.Row];
//			item.Complete = true;
			item.Complete = !item.Complete; //HACK: better to make it a switch

			tableView.ReloadRows (new[] { indexPath }, UITableViewRowAnimation.Automatic);
			table.UpdateAsync (item)
				.ContinueWith (t => 
				{
					//HACK: don't remove if we don't want to
					//items.RemoveAt (indexPath.Row);	
					//tableView.ReloadData();

					IsUpdating = false;
				}, scheduler);
		}

		private bool isUpdating;
		private UITableView tableView;
		private IMobileServiceTable<TodoItem> table;
		private List<TodoItem> items;
		private readonly TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
	}
}

