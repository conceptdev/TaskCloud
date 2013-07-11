using System;
using System.Collections.Generic;

namespace MonkeyBox
{
	public class Monkey
	{
		public Monkey ()
		{
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
					Name = "George"
				},
				new Monkey {
					Name = "Hootie"
				},
				new Monkey {
					Name = "Julian"
				},
				new Monkey {
					Name = "Nim"
				},
				new Monkey {
					Name = "Pepe"
				}
			};
		}
	}
}

