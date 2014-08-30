using System;
using DropBoxSync.iOS;
using MonoTouch.Foundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Net.Mime;

namespace MonkeyBox
{
	public class DropboxDatabase
	{
		public event EventHandler MonkeysUpdated;

		public Monkey[] Monkeys { get; set; }

		static DropboxDatabase shared;
		
		public bool AutoUpdating { get; set; }

		public static DropboxDatabase Shared {
			get {
				if (shared == null)
					shared = new DropboxDatabase ();
				return shared;
			}
		}

		DBDatastore store;

		public DropboxDatabase ()
		{
			Monkeys = new Monkey[0];
		}

		public void Init ()
		{
			if (store != null)
				return;
			DBError error;
			store = DBDatastore.OpenDefaultStore (DBAccountManager.SharedManager.LinkedAccount, out error);
			store.Sync (out error);
            store.AddObserver (store, () => {
				LoadData ();
			});
			AutoUpdating = true;
		}

		public Dictionary<string,DBRecord> records = new Dictionary<string, DBRecord> ();
		public Dictionary<string,Monkey> monkeyDictionary = new Dictionary<string, Monkey> ();

        public void LoadData ()
		{
            new NSObject().BeginInvokeOnMainThread(()=>{
                var table = store.GetTable ("monkeys");
				DBError error;
				var results = table.Query (null, out error);
				
				if (results.Length == 0) {
					populateMonkeys ();
					return;
				}
				ProccessResults (results);
			});
		}

		void ProccessResults (DBRecord[] results)
		{
			records = results.ToDictionary (x => x.Fields ["Name"].ToString (), x => x);
			foreach (var result in results) {
				var name = result.Fields ["Name"].ToString ();
				Monkey monkey;
				monkeyDictionary.TryGetValue (name, out monkey);
				if (monkey == null) {
					monkey = result.ToMonkey ();
					monkeyDictionary.Add (name, monkey);
				} else {
					monkey.Update (result);
				}
			}
			Monkeys = monkeyDictionary.Select (x => x.Value).OrderBy (x => x.Z).ToArray ();
			store.BeginInvokeOnMainThread (() => {
				if (MonkeysUpdated != null)
					MonkeysUpdated (this, EventArgs.Empty);
			});
		}

		public void DeleteAll ()
		{
			populated = false;
			var table = store.GetTable ("monkeys");
			DBError error;
			var results = table.Query (new NSDictionary (), out error);
			foreach (var result in results) {
				result.DeleteRecord ();
			}
			store.Sync (out error);
		}

		bool populated = false;

		void populateMonkeys ()
		{
			if (populated)
				return;
			populated = true;
			Monkeys = Monkey.GetAllMonkeys ();
			DBError error;
			var table = store.GetTable ("monkeys");
			foreach (var monkey in Monkeys) {
				bool inserted = false;
				table.GetOrInsertRecord (monkey.Name, monkey.ToDictionary (), inserted, out error);
			}
			store.Sync (out error);

		}

		public void Update (Monkey monkey)
		{
			DBRecord record;
            var hasRecord = records.TryGetValue (monkey.Name, out record);
            var fields = monkey.ToDictionary ();
            var inserted = false;
            DBError error;
            if (hasRecord)
                record.Update (fields);
            else
                store.GetTable("monkeys").GetOrInsertRecord (monkey.Name, fields, inserted, out error);
			store.SyncAsync ();
		}

		public void Update()
		{
			store.SyncAsync ();
		}

		public void Reset()
		{
			DeleteAll();
		}
	}

	public static class DropboxHelper
	{
		public static NSDictionary ToDictionary (this Monkey monkey)
		{
            var keys = new NSString[] {
                new NSString("Name"),
                new NSString("Rotation"),
                new NSString("Scale"),
                new NSString ("X"),
                new NSString ("Y"),
                new NSString ("Z"),
            };
            var values = new NSObject[] {
                new NSString(monkey.Name),
                new NSNumber(monkey.Rotation),
                new NSNumber(monkey.Scale),
                new NSNumber(monkey.X),
                new NSNumber(monkey.Y),
                new NSNumber(monkey.Z),
            };
            return NSDictionary.FromObjectsAndKeys (values, keys);
		}

		public static Monkey ToMonkey (this DBRecord record)
		{
			return new Monkey ().Update (record);
			
		}

		public static Monkey Update (this Monkey monkey, DBRecord record)
		{
            monkey.Name = record.Fields ["Name"].ToString ();
            monkey.Rotation = ((NSNumber)record.Fields ["Rotation"]).FloatValue;
            monkey.Scale = ((NSNumber)record.Fields ["Scale"]).FloatValue;
            monkey.X = ((NSNumber)record.Fields ["X"]).FloatValue;
            monkey.Y =((NSNumber)record.Fields ["Y"]).FloatValue;
            monkey.Z =((NSNumber)record.Fields ["Z"]).IntValue;
			return monkey;
		}
	}
}

