using System;
using MonoTouch.CoreGraphics;

namespace MonkeyBox
{
	public static class Extensions
	{

		public static float GetScale (this CGAffineTransform t)
		{
			return (float)Math.Sqrt((double)(t.xx * t.xx + t.xy * t.xy));
		}

		public static float GetRotation (this CGAffineTransform t)
		{
			return (float)Math.Atan2((double)t.yx,(double)t.xx);
		}
	}
}

