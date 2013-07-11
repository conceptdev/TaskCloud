using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace TaskyDrop {
	public class TaskScreen : UIViewController {
		UIButton saveButton, deleteButton;
		UILabel doneLabel;
		UISwitch doneSwitch;
		UITextView descriptionText, titleText;

		Task task;
		
		public TaskScreen (Task t) {
			task = t;
		}

		public override void ViewDidLoad ()
		{	
			base.ViewDidLoad ();
			
			#region UI Controls (you could do this in XIB if you want)
			saveButton = UIButton.FromType(UIButtonType.RoundedRect);
			saveButton.Frame = new RectangleF(10,240,145,40);
			saveButton.SetTitle("Save", UIControlState.Normal);
			saveButton.SetTitle("waiting...", UIControlState.Disabled);
			saveButton.Enabled = false;

			deleteButton = UIButton.FromType(UIButtonType.RoundedRect);
			deleteButton.Frame = new RectangleF(160,240,145,40);
			deleteButton.SetTitle("Delete", UIControlState.Normal);
			deleteButton.Enabled = false;
			
			doneSwitch = new UISwitch();
			doneSwitch.Frame = new RectangleF(10, 205, 145, 50);
			doneSwitch.Enabled = false;
			doneLabel = new UILabel();
			doneLabel.Frame = new RectangleF(100, 210, 100, 15);
			doneLabel.Text = "Done";

			titleText = new UITextView(new RectangleF(10, 10, 300, 40));
			titleText.BackgroundColor = UIColor.FromRGB(240,240,240);
			titleText.Editable = true;
			titleText.Font = UIFont.SystemFontOfSize(20f);
			titleText.BackgroundColor = UIColor.FromRGB(240,240,240);
			titleText.ReturnKeyType = UIReturnKeyType.Next;
			titleText.ShouldChangeText = (t, c, r) => {
				if (r == "\n") {
					descriptionText.BecomeFirstResponder();
					return false;
				}
				return true;
			};

			descriptionText = new UITextView(new RectangleF(10, 60, 300, 180));
			descriptionText.BackgroundColor = UIColor.FromRGB(240,240,240);
			descriptionText.Editable = true;
			descriptionText.ScrollEnabled = true;
			descriptionText.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			descriptionText.ReturnKeyType = UIReturnKeyType.Done;
			descriptionText.ShouldChangeText = (t, c, r) => {
				if (r == "\n") {
					descriptionText.ResignFirstResponder();
					return false;
				}
				return true;
			};

			// Add the controls to the view
			this.Add(saveButton);
			this.Add(deleteButton);
			this.Add(doneLabel);
			this.Add(doneSwitch);
			this.Add(descriptionText);   // disabled for Azure demo (for now...)
			this.Add(titleText);
			#endregion

			LoadData ();

			saveButton.TouchUpInside += (sender, e) => {

				task.Title = titleText.Text;
				task.Description = descriptionText.Text;
				task.IsDone = doneSwitch.On;

				// save to Dropbox
				DropboxDatabase.Shared.Update(task);

				descriptionText.ResignFirstResponder ();			// hide keyboard
				titleText.ResignFirstResponder ();

				NavigationController.PopToRootViewController (true);
			};

			deleteButton.TouchUpInside += (sender, e) => {
				
				// delete to Dropbox 
				DropboxDatabase.Shared.Delete(task);
				
				descriptionText.ResignFirstResponder ();			// hide keyboard
				titleText.ResignFirstResponder ();

				NavigationController.PopToRootViewController (true); // doesn't reflect deletion yet
			};
		}

		void LoadData () {
			Title = task.Title??"";
			titleText.Text = task.Title??"";
			if (titleText.Text == "<new task>")
				titleText.Text = "";
			descriptionText.Text = task.Description??"";
			doneSwitch.On = task.IsDone;			

			saveButton.Enabled = true;
			doneSwitch.Enabled = true;
			deleteButton.Enabled = true;
		}
	}
}