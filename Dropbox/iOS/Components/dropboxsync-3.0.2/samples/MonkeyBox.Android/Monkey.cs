using System;
using System.Collections.Generic;
using DropboxSync.Android;

namespace MonkeyBox
{
	public class Monkey
	{
        public DBFields ToFields ()
        {
            var fields = new DBFields();
            fields.Set("X", (double)X);
            fields.Set("Y", (double)Y);
            fields.Set("Z", (long)Z);
            fields.Set("Name", Name);
            fields.Set ("Rotation", Math.PI * Rotation / 180f); // In degrees radian.
            fields.Set("Scale", (double)Scale);
            return fields;
        }

		static Random random = new Random();
		public Monkey ()
		{
			Scale = (float)Math.Max(random.NextDouble(),.33);
			X = (float)random.NextDouble();
			Y = (float)random.NextDouble();
		}
		public string Name {get;set;}
		public float Rotation {get;set;}
		public float Scale {get;set;}
		public float X {get;set;}
		public float Y {get;set;}
		public int Z {get;set;}

		public static Monkey[] GetAllMonkeys()
		{
			return new Monkey[] {
				new Monkey{
					Name = "Fred",
				},
				new Monkey{
					Name = "George",
				},
				new Monkey {
					Name = "Hootie",
				},
				new Monkey {
					Name = "Julian",
				},
				new Monkey {
					Name = "Nim",
				},
				new Monkey {
					Name = "Pepe",
				}
			};
		}
	}
}

