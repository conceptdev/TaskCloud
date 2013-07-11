using System;
using DropBoxSync.iOS;
using MonoTouch.Foundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace TaskyDrop
{
	/// <summary>
	/// Check out the Dropbox Datastore API 
	/// https://www.dropbox.com/developers/datastore
	/// </summary>
	public class DropboxDatabase
	{
		public event EventHandler TasksUpdated;

		static string tableName = "tasks";

		static DropboxDatabase shared;
		public static DropboxDatabase Shared {
			get {
				if (shared == null)
					shared = new DropboxDatabase ();
				return shared;
			}
		}

		public Task[] Tasks { get; set;}

		public DropboxDatabase ()
		{
			Tasks = new Task[0];
		}

		DBDatastore store;
		NSTimer timer;
		public bool AutoUpdating { get; set; }

		public void Init ()
		{
			if (store != null)
				return;
			DBError error;
			store = DBDatastore.OpenDefaultStoreForAccount (DBAccountManager.SharedManager.LinkedAccount, out error);
			var sync = store.Sync (null);

			store.AddObserver (store, () => {
				Console.Write("store observer ");

				store.Sync(null); // needed?

				var table = store.GetTable (tableName);
				var results = table.Query (null, out error);

				Console.WriteLine(results.Length);

				ProccessResults (results);
			});


			// TIMER TO AUTOUPDATE
			AutoUpdating = true;
			store.BeginInvokeOnMainThread(()=>{
				timer = NSTimer.CreateRepeatingScheduledTimer(2,()=>{
					if(!AutoUpdating)
						return;
					//Console.WriteLine("AutoUpdating"); // SPAM
					store.Sync(null);
				});
			});
		}

		public Dictionary<string,DBRecord> records = new Dictionary<string, DBRecord> ();
		public Dictionary<string,Task> taskDictionary = new Dictionary<string, Task> ();

		static object locker = new object();

		//TODO: tidy up this method!!!
		void ProccessResults (DBRecord[] results)
		{
			taskDictionary.Clear ();

			records = results.ToDictionary (x => x.RecordId.ToString (), x => x);
			lock (locker) {
				Console.WriteLine("ProcessResults" + results.Length.ToString());

				//foreach (var result in results) {
				for (var i = 0; i < results.Length; i++) { var result = results [i];
					var id = result.RecordId.ToString ();
					Console.WriteLine ("id " + id + " " + i);
					TaskyDrop.Task t;
					taskDictionary.TryGetValue (id, out t);
					if (t == null) {
						t = result.ToTask ();
						taskDictionary.Add (id, t);
					} else {
						t.Update (result);
					}
				}
				Tasks = taskDictionary.Select (x => x.Value).OrderBy (x => x.Title).ToArray ();
				Console.WriteLine("Updated " + Tasks.Length);
				store.BeginInvokeOnMainThread (() => {
					if (TasksUpdated != null) {
						Console.WriteLine("TasksUpdated handler called " + Tasks.Length);
						TasksUpdated (this, EventArgs.Empty);
					}
				});

				Console.WriteLine ("DONE");
			}
		}

		public void Update (Task t)
		{
			DBError error;

			var table = store.GetTable (tableName);
			var r = table.GetRecord (t.id, out error);
			if (r == null)
				table.Insert (t.ToDictionary ());
			else 
				r.Update (t); 

			store.SyncAsync (null);
		}
		public void Update()
		{
			store.SyncAsync (null);
		}
		public void Delete (Task t)
		{
			DBError error;

			var table = store.GetTable (tableName);
			var r = table.GetRecord (t.id, out error);
			r.DeleteRecord();

			store.SyncAsync (null);
		}
	}
}

