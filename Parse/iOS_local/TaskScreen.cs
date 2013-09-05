using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using System;

namespace Parse {
	public class TaskScreen : UIViewController {
		UIButton saveButton, deleteButton;
		UILabel doneLabel;
		UISwitch doneSwitch;
		UITextView descriptionText, titleText;

		Task task;
		TaskListScreen screen;

		public TaskScreen (Task t, TaskListScreen caller) {
			task = t;
			screen = caller;
		}

		public async override void ViewDidLoad ()
		{	
			base.ViewDidLoad ();
			
			#region UI Controls (you could do this in Storyboard if you want)
			saveButton = UIButton.FromType(UIButtonType.Custom);
			saveButton.BackgroundColor = UIColor.FromRGB(81, 189, 63);
			saveButton.SetTitleColor (UIColor.White, UIControlState.Normal);
			saveButton.SetTitleColor (UIColor.DarkGray, UIControlState.Selected);
			saveButton.SetTitleColor (UIColor.DarkGray, UIControlState.Highlighted);
			saveButton.Frame = new RectangleF(10,150,145,40);
			saveButton.SetTitle("Save", UIControlState.Normal);
			saveButton.SetTitle("waiting...", UIControlState.Disabled);
			saveButton.Enabled = false;
			//saveButton.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;

			deleteButton = UIButton.FromType(UIButtonType.Custom);
			deleteButton.BackgroundColor = UIColor.FromRGB(249, 29, 88); //UIColor.Red;
			deleteButton.SetTitleColor (UIColor.White, UIControlState.Normal);
			deleteButton.SetTitleColor (UIColor.DarkGray, UIControlState.Selected);
			deleteButton.SetTitleColor (UIColor.DarkGray, UIControlState.Highlighted);
			deleteButton.Frame = new RectangleF(165,150,145,40);
			deleteButton.SetTitle("Delete", UIControlState.Normal);
			deleteButton.Enabled = false;
			//deleteButton.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;

			doneSwitch = new UISwitch();
			doneSwitch.Frame = new RectangleF(70, 115, 145, 50);
			doneSwitch.Enabled = false;
			doneLabel = new UILabel();
			doneLabel.Frame = new RectangleF(10, 120, 145, 15);
			doneLabel.Text = "Done";
			//doneSwitch.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;
			//doneLabel.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;

			titleText = new UITextView(new RectangleF(10, 10, 300, 40));
			titleText.BackgroundColor = UIColor.FromRGB(240,240,240);
			titleText.Editable = true;
			titleText.Font = UIFont.SystemFontOfSize (18f);
			titleText.BackgroundColor = UIColor.FromRGB(246,246,246);

			descriptionText = new UITextView(new RectangleF(10, 60, 300, 90));
			descriptionText.BackgroundColor = UIColor.FromRGB(240,240,240);
			descriptionText.Editable = true;
			descriptionText.ScrollEnabled = true;
			descriptionText.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			// Add the controls to the view
			this.Add(saveButton);
			this.Add(deleteButton);
			this.Add(doneLabel);
			this.Add(doneSwitch);
			this.Add(descriptionText);   // disabled for  demo (for now...)
			this.Add(titleText);
			#endregion

			saveButton.TouchUpInside += Save;

			deleteButton.TouchUpInside += Delete;

			await Populate ();
		}

		async System.Threading.Tasks.Task Populate () 
		{
			Title = "loading...";
			Task ta = new Task();

			try {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

				ta = AppDelegate.Database.GetItem(task.Id); //Task.FromParseObject (t);
				//TODO: get the latest copy of this item from the cloud
//				var query = ParseObject.GetQuery("Task").WhereEqualTo("objectId", task.Id);
//				var t = await query.FirstAsync();

				if (ta == null) ta = new Task();
			} catch (Exception pe) {
				Console.WriteLine ("Exception:{0}", pe.Message);
			} finally {
				Title = ""; // clear "loading..." message
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			}

			// Populate UI controls
			titleText.Text = ta.Title??"";
			descriptionText.Text = ta.Description??"";
			doneSwitch.On = ta.IsDone;			

			saveButton.Enabled = true;
			doneSwitch.Enabled = true;
			deleteButton.Enabled = true;
		}

		protected async void Save (object sender, EventArgs e)
		{
			task.Title = titleText.Text;
			task.Description = descriptionText.Text;
			task.IsDone = doneSwitch.On;

			descriptionText.ResignFirstResponder ();			// hide keyboard
			titleText.ResignFirstResponder ();

			NavigationController.PopToRootViewController (true);

			// save 
			try {
				AppDelegate.Database.SaveItem(task);
				// TODO: save to cloud
				//await task.ToParseObject().SaveAsync();

				await screen.ReloadAsync();
			} catch (Exception pe) {
				Console.WriteLine ("Exception:{0}", pe.Message);
			} 
		}

		protected async void Delete (object sender, EventArgs e)
		{
			descriptionText.ResignFirstResponder ();			// hide keyboard
			titleText.ResignFirstResponder ();

			NavigationController.PopToRootViewController (true); 

			// delete 
			try {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

				AppDelegate.Database.DeleteItem(task.Id);
				// TODO: delete from the cloud
				//await task.ToParseObject().DeleteAsync();

				await screen.ReloadAsync();
			} catch (Exception pe) {
				Console.WriteLine ("Exception:{0}", pe.Message);
			} finally {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			}
		}
	}
}