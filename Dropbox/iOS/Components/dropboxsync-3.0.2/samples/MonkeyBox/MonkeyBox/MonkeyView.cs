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
			Transform = transform;

            // Convert location from top/left to center coords.
            var widthOffset = Frame.Width * 0.5f;
            var heightOffset = Frame.Width * 0.5f;

            var x = bounds.Width * monkey.X;
            var y = bounds.Height * monkey.Y;

            Center = new PointF (x + widthOffset, y + heightOffset);
		}

        public void UpdateMonkey(RectangleF bounds)
		{
            // Save location in top/left, not center,
            // in order to make it easier to draw the
            // monkeys on Android and other platforms.

            var widthOffset = Frame.Width * 0.5f;
            var heightOffset = Frame.Width * 0.5f;

            Monkey.X = (Center.X - widthOffset) / bounds.Width;
            Monkey.Y = (Center.Y - heightOffset) / bounds.Height;

			Monkey.Scale = Transform.GetScale ();
			Monkey.Rotation = Transform.GetRotation ();
		}
	}
}

