using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Graphics;
using Android.Views;

namespace Parse {
	[Activity (Label = "Task Details")]			
	public class TaskScreen : Activity {
		protected Task task = new Task();
		protected Button deleteButton = null;
		protected EditText notesTextEdit = null;
		protected EditText nameTextEdit = null;
		protected Button saveButton = null;
		CheckBox doneCheckbox;
		string taskID;

		protected async override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

            #region color the titlebar
            View titleView = Window.FindViewById(Android.Resource.Id.Title);
			if (titleView != null) {
			  IViewParent parent = titleView.Parent;
			  if (parent != null && (parent is View)) {
			    View parentView = (View)parent;
			    parentView.SetBackgroundColor(Color.Rgb(0x26, 0x75 ,0xFF)); //38, 117 ,255
			  }
            }
            #endregion

            // set our layout to be the details screen
            SetContentView(Resource.Layout.TaskDetails);

            taskID = Intent.GetStringExtra ("TaskID");

			// find all our controls
			nameTextEdit = FindViewById<EditText>(Resource.Id.txtName);
			//notesTextEdit = FindViewById<EditText>(Resource.Id.txtNotes);
			saveButton = FindViewById<Button>(Resource.Id.btnSave);
			doneCheckbox = FindViewById<CheckBox>(Resource.Id.chkDone);
			deleteButton = FindViewById<Button>(Resource.Id.btnCancelDelete);
			
			// set the cancel delete based on whether or not it's an existing task
			deleteButton.Enabled = false;
			saveButton.Text = "waiting...";
			saveButton.Enabled = false;

			// button clicks 
			deleteButton.Click += Delete;
			saveButton.Click += Save;

			if (!String.IsNullOrEmpty(taskID)) 
				task = await Populate();
			else
				task = new Task();

			// name
			nameTextEdit.Text = task.Title;
			
			// notes
			//notesTextEdit.Text = task.Notes;

			doneCheckbox.Checked = task.IsDone;

			saveButton.Text = "Save";
			saveButton.Enabled = true;
			deleteButton.Enabled = true;
		}

		async System.Threading.Tasks.Task<Parse.Task> Populate () 
		{
			Task ta = new Task();

			try {
				var query = ParseObject.GetQuery("Task").WhereEqualTo("objectId", taskID);
				var t = await query.FirstAsync();
				ta = Task.FromParseObject (t);
			} catch (ParseException pe) {
				Console.WriteLine ("Parse Exception:{0}", pe.Message);
			}
			return ta;
		}

		protected async void Save(object sender, EventArgs e)
		{
            task.Title = nameTextEdit.Text;
			//task.Notes = notesTextEdit.Text;
			task.IsDone = doneCheckbox.Checked;

			try {
				await task.ToParseObject().SaveAsync();
				Finish();
			} catch (ParseException pe) {
				Console.WriteLine ("Parse Exception:{0}", pe.Message);
				Finish();
			} 
		}
		
		protected async void Delete(object sender, EventArgs e)
		{
			try {
				await task.ToParseObject().DeleteAsync();
				Finish();
			} catch (ParseException pe) {
				Console.WriteLine ("Parse Exception:{0}", pe.Message);
				Finish();
			} 
		}
	}
}