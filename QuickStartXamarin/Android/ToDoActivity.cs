using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using Microsoft.WindowsAzure.MobileServices;

namespace Sample
{
	[Activity (Label = "todolist", MainLauncher = true, Icon = "@drawable/ic_launcher", Theme="@style/AppTheme")]
	public class MainActivity : Activity
	{
		private static readonly MobileServiceClient MobileService =
			new MobileServiceClient ("MOBILE SERVICE URL", "APPLICATION KEY");

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.ToDoActivity);

			
			progressBar = FindViewById<ProgressBar> (Resource.Id.loadingProgressBar);
			progressBar.Visibility = ViewStates.Gone;

			newItem = FindViewById<EditText> (Resource.Id.NewItem);

			addItem = FindViewById<Button> (Resource.Id.AddItem);
			addItem.Click += OnClickAddItem;

			adapter = new TodoAdapter (MobileService.GetTable<TodoItem>(), this);
			adapter.IsUpdatingChanged += (s, e) =>
			{
				newItem.Enabled =
				addItem.Enabled =
//				refresh.Enabled =
					!adapter.IsUpdating;
			};

			FindViewById<ListView> (Resource.Id.Items).Adapter = adapter;
		}

		private void OnClickAddItem (object sender, EventArgs eventArgs)
		{
			string text = newItem.Text;
			newItem.Text = null;

			adapter.Insert (new TodoItem { Text = text });
		}


		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.activity_main, menu);
			return true;
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == Resource.Id.menu_refresh)
				adapter.RefreshAsync();
			return true;
		}

		TodoAdapter adapter;
		EditText newItem;
		Button addItem; //, refresh;
		ProgressBar progressBar;
	}


}