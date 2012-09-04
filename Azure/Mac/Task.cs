using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
//using System.Json;
using MonoMac.Foundation;

namespace Tasky {
	[Preserve]	
	public class Task {
		public Task () 
		{
			Id = -1;
		}
		public Task (object json)
		{
			var o = json as System.Collections.Generic.Dictionary<string,object>;
			Id = (int)o["id"];
			Title = (string)o["text"];
			IsDone = (bool)o["complete"];
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

