using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using System.Json;

namespace TaskyDrop {

	[Preserve]	
	public class Task {
		public Task () 
		{
		}

		public string Title { get; set; }
		public string Description { get; set; }
		public bool IsDone { get; set; }

		public string id { get; set; }

		public override string ToString ()
		{
			return string.Format ("[Task: Title={0}, Description={1}, IsDone={2}]", Title, Description, IsDone);
		}
	}
}