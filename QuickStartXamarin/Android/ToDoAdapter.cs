using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace Sample
{
	public class TodoAdapter
		: BaseAdapter<TodoItem>
	{
		public TodoAdapter (IMobileServiceTable<TodoItem> table, Context context)
		{
			inflater = (LayoutInflater)context.GetSystemService (Context.LayoutInflaterService);
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

		public override bool HasStableIds
		{
			get { return true; }
		}

		public override int Count
		{
			get { return items.Count; }
		}

		public override TodoItem this [int position]
		{
			get { return items[position]; }
		}

		public override long GetItemId (int position)
		{
			return items[position].Id;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			TodoItem item = items[position];

			View view = inflater.Inflate (Resource.Layout.ToDoItem, null);

			CheckBox checkbox = view.FindViewById<CheckBox> (Resource.Id.checkToDoItem);
			checkbox.Text = item.Text;

			EventHandler<CompoundButton.CheckedChangeEventArgs> ev = null;
			ev = (o, e) =>
			{
				checkbox.CheckedChange -= ev;

				IsUpdating = true;

				item.Complete = true;
				table.UpdateAsync (item).ContinueWith (t =>
				                                       {
					items.RemoveAt (position);
					NotifyDataSetChanged();
					IsUpdating = false;
				}, scheduler);
			};

			checkbox.CheckedChange += ev;

			return view;
		}

		public void RefreshAsync()
		{
			IsUpdating = true;
			table.Where (ti => !ti.Complete).ToListAsync()
				.ContinueWith (t =>
				               {
					items = t.Result;
					NotifyDataSetChanged();
					IsUpdating = false;
				}, scheduler);
		}

		public void Insert (TodoItem item)
		{
			IsUpdating = true;
			items.Add (item);
			NotifyDataSetChanged();

			table.InsertAsync (item).ContinueWith (t =>
			                                            {
				if (t.IsFaulted)
				{
					items.Remove (item);
					NotifyDataSetChanged();
				}

				IsUpdating = false;
			}, scheduler);
		}

		private List<TodoItem> items = new List<TodoItem>();

		private readonly LayoutInflater inflater;
		private readonly IMobileServiceTable<TodoItem> table;
		private readonly TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
		private bool isUpdating;
	}
}

