using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace Parse {
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

			LoadData ();

			saveButton.TouchUpInside += async (sender, e) => {

				task.Title = titleText.Text;
				task.Description = descriptionText.Text;
				task.IsDone = doneSwitch.On;

				descriptionText.ResignFirstResponder ();			// hide keyboard
				titleText.ResignFirstResponder ();

				NavigationController.PopToRootViewController (true);

				// save to Parse
				await task.ToParseObject().SaveAsync();
			};

			deleteButton.TouchUpInside += async (sender, e) => {

				descriptionText.ResignFirstResponder ();			// hide keyboard
				titleText.ResignFirstResponder ();

				NavigationController.PopToRootViewController (true); // doesn't reflect deletion yet

				// delete from Parse
				await task.ToParseObject().DeleteAsync();
			};
		}

		void LoadData () 
		{
			//Title = task.Title??"";
			titleText.Text = task.Title??"";
			descriptionText.Text = task.Description??"";
			doneSwitch.On = task.IsDone;			

			saveButton.Enabled = true;
			doneSwitch.Enabled = true;
			deleteButton.Enabled = true;
		}
	}
}