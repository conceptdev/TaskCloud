using System;
using MonoTouch.Foundation;
using DropBoxSync.iOS;

namespace TaskyDrop
{
	public static class TaskiOSExtensions
	{
		public static NSDictionary ToDictionary (this Task t)
		{
			var keys = new NSString[] {
				new NSString("Title"),
				new NSString("Description"),
				new NSString("IsDone")
			};
			var values = new NSObject[] {
				new NSString(t.Title),
				new NSString(t.Description),
				new NSString(t.IsDone.ToString())
			};
			return NSDictionary.FromObjectsAndKeys (values, keys);
		}

		public static Task ToTask (this DBRecord record)
		{
			return new Task ().Update (record);
		}

		public static Task Update (this Task t, DBRecord record)
		{
			t.id = record.RecordId;

			t.Title = record.Fields [new NSString ("Title")].ToString ();
			t.Description = record.Fields [new NSString ("Description")].ToString ();
			t.IsDone = Convert.ToBoolean (record.Fields [new NSString ("IsDone")].ToString ());
			return t;
		}

		public static DBRecord Update (this DBRecord record, Task t)
		{
			record.SetObject (new NSString(t.Title), "Title");
			record.SetObject (new NSString(t.Description), "Description");
			record.SetObject (new NSString(t.IsDone.ToString()), "IsDone");
			return record;
		}
	}
}