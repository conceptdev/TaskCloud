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
	//TODO: implement proper lifecycle methods
	[Activity (Label = "Task Details")]			
	public class TaskScreen : Activity {
		protected Task task = new Task();
		protected Button cancelDeleteButton = null;
		protected EditText notesTextEdit = null;
		protected EditText nameTextEdit = null;
		protected Button saveButton = null;
		CheckBox doneCheckbox;
		
		protected override void OnCreate (Bundle bundle)
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

            int taskID = Intent.GetIntExtra("TaskID", -1);
			if (taskID >= 0) 
				;//task = AzureWebService.GetTodo(taskID); // from Azure
            else
                task = new Task();
			
			nameTextEdit = FindViewById<EditText>(Resource.Id.txtName);
			//notesTextEdit = FindViewById<EditText>(Resource.Id.txtNotes);
			saveButton = FindViewById<Button>(Resource.Id.btnSave);
			doneCheckbox = FindViewById<CheckBox>(Resource.Id.chkDone);
			
			// find all our controls
			cancelDeleteButton = FindViewById<Button>(Resource.Id.btnCancelDelete);
			
			// set the cancel delete based on whether or not it's an existing task
			cancelDeleteButton.Text = (task.Id == "" ? "Cancel" : "Delete");
			
			// name
			nameTextEdit.Text = task.Title;
			
			// notes
			//notesTextEdit.Text = task.Notes;
			
			doneCheckbox.Checked = task.IsDone;

			// button clicks 
			cancelDeleteButton.Click += (sender, e) => { CancelDelete(); };
			saveButton.Click += (sender, e) => { Save(); };
		}

		protected void Save()
		{
            task.Title = nameTextEdit.Text;
			//task.Notes = notesTextEdit.Text;
			task.IsDone = doneCheckbox.Checked;
			if (task.Id != "") 
				; //AzureWebService.UpdateTodo (task);
            else
				; //AzureWebService.AddTodo(task);

			Finish();
		}
		
		protected void CancelDelete()
		{
			if (task.Id != "")
				; //AzureWebService.DeleteTodo(task);
			
			Finish();
		}
	}
}