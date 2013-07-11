using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;

namespace MonkeyBox
{
	public class MonkeyView : UIControl
	{
		UIImageView image;
		public Monkey Monkey { get; private set; }
		public MonkeyView (Monkey monkey)
		{
			Monkey = monkey;
			image = new UIImageView (UIImage.FromBundle (monkey.Name));
			this.AddSubview (image);
			this.Frame = image.Frame;
		}

		public PlayGroundView CurrentPlayground {get;set;}

		public override void MovedToSuperview ()
		{
			base.MovedToSuperview ();
			if (this.Superview is PlayGroundView)
				CurrentPlayground = (PlayGroundView)this.Superview;
		}
		public override void TouchesBegan (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			CurrentPlayground.CurrentMonkey = this;
		}

		public void Update(Monkey monkey, RectangleF bounds)
		{
			var transform = CGAffineTransform.MakeIdentity ();
			transform.Rotate (monkey.Rotation);
			transform.Scale (monkey.Scale, monkey.Scale);
			this.Transform = transform;

			var x = bounds.Width * monkey.X;
			var y = bounds.Height * monkey.Y;
			this.Center = new PointF (x, y);

		}
		public void UpdateMonkey(int Z,RectangleF bounds)
		{
			Monkey.X = Center.X / bounds.Width;
			Monkey.Y = Center.Y / bounds.Height;


			Monkey.Scale = Transform.GetScale ();
			Monkey.Rotation = Transform.GetRotation ();
		}
	}
}

