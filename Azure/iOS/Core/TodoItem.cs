using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using System.Json;

namespace Azure {
	[Preserve]	
	public class TodoItem {
		public TodoItem () 
		{
			Id = -1;
		}
		public TodoItem (JsonValue json)
		{
			Id = json["id"];
			Title = json["text"];
			IsDone = json["complete"];
		}
		public int Id {get;set;}
		public string Title { get; set; }
		public string Description { get; set; }
		public bool IsDone { get; set; }

		public override string ToString ()
		{
			return string.Format ("[Task: Title={0}, Description={1}, IsDone={2}]", Title, Description, IsDone);
		}
		public string ToJson()
		{
			var json ="";
			if (Id < 0) // for inserting, do not specify primary-key field
				json = @"{""text"":"""+Title+@""",""complete"":"+IsDone.ToString().ToLower()+@"}";
			else       // for updating, must provide primary-key field
				json = @"{""id"":"+Id+@",""text"":"""+Title+@""",""complete"":"+IsDone.ToString().ToLower()+@"}";
			return json;
		}
	}
}

