using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Json;
using Android.Runtime;
using System.Collections.Generic;

namespace Parse {
	[Preserve]	
	public class Task {
		public Task () 
		{
			Id = "";
		}

		public string Id {get;set;}
		public string Title { get; set; }
		public string Description { get; set; }
		public bool IsDone { get; set; }

		public override string ToString ()
		{
			return string.Format ("[Task: Title={0}, Description={1}, IsDone={2}]", Title, Description, IsDone);
		}

		public ParseObject ToParseObject ()
		{
			var po = new ParseObject("Task");
			if (Id != string.Empty)
				po.ObjectId = Id;
			po["Title"] = Title;
			po["Description"] = Description;
			po["IsDone"] = IsDone;

			return po;
		}

		public static Task FromParseObject (ParseObject po)
		{
			var t = new Task();
			t.Id = po.ObjectId;
			t.Title = Convert.ToString(po["Title"]);
			t.Description = Convert.ToString (po["Description"]);
			t.IsDone = Convert.ToBoolean (po["IsDone"]);
			return t;
		}

		public static async System.Threading.Tasks.Task<List<Task>> GetAll () 
		{
			var query = ParseObject.GetQuery ("Task").OrderBy ("Title");
			var ie = await query.FindAsync ();

			var tl = new List<Task> ();
			foreach (var t in ie) {
				tl.Add (Task.FromParseObject (t));
			}

			return tl;
		}
	}
}