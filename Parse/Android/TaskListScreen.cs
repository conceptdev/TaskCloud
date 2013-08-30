using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Graphics;
using Android.Views;

namespace Parse {
	[Activity (Label = "TaskyAzure", MainLauncher = true, Icon="@drawable/launcher")]			
	public class HomeScreen : Activity {
		protected TaskListAdapter taskList;
		protected IList<Task> tasks;
		protected Button addTaskButton = null;
		protected ListView taskListView = null;
		
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

			// set our layout to be the home screen
			SetContentView(Resource.Layout.TaskListScreen);

			//Find our controls
			taskListView = FindViewById<ListView> (Resource.Id.lstTasks);
			addTaskButton = FindViewById<Button> (Resource.Id.btnAddTask);

			// wire up add task button handler
			if(addTaskButton != null) {
				addTaskButton.Click += (sender, e) => {
					StartActivity(typeof(TaskScreen));
				};
			}
			
			// wire up task click handler
			if(taskListView != null) {
				taskListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
					var taskDetails = new Intent (this, typeof (TaskScreen));
					taskDetails.PutExtra ("TaskID", tasks[e.Position].Id);
					StartActivity (taskDetails);
				};
			}

			//tasks = AzureWebService.LoadTodos (Reload);
		}
		
		protected override void OnResume ()
		{
			base.OnResume ();

			// yep, we go to the network on every resume... this is just a demo after all
			tasks = AzureWebService.LoadTodos (Reload);
		}

		void Reload(List<Task> tasks) 
		{
            this.tasks = tasks;
			// create our adapter
			taskList = new TaskListAdapter(this, tasks);
			
			//Hook up our adapter to our ListView
			taskListView.Adapter = taskList;
		}
	}
}