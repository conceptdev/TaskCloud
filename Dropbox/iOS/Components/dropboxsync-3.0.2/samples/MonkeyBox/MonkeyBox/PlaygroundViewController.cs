using System;
using MonoTouch.UIKit;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

namespace MonkeyBox
{
	public class PlaygroundViewController : UIViewController
	{
		PlayGroundView PlayGroundView;

		public PlaygroundViewController ()
		{
			this.Title = "MonkeyBox";
			this.NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Refresh, (sender,e) => {
				DropboxDatabase.Shared.Update();
			});
		}
		public override void ViewWillAppear (bool animated)
		{
			PlayGroundView.Parent = this;
			DropboxDatabase.Shared.MonkeysUpdated += HandleMonkeysUpdated;
			PlayGroundView.UpdateMonkeys (DropboxDatabase.Shared.Monkeys);
			base.ViewWillAppear (animated);
			this.BecomeFirstResponder ();
		}
		public override void ViewDidDisappear (bool animated)
		{
			PlayGroundView.Parent = null;
			DropboxDatabase.Shared.MonkeysUpdated -= HandleMonkeysUpdated;
			base.ViewDidDisappear (animated);
		}

		void HandleMonkeysUpdated (object sender, EventArgs e)
		{
			PlayGroundView.UpdateMonkeys (DropboxDatabase.Shared.Monkeys);
		}

		public override void LoadView ()
		{
            View = PlayGroundView = new PlayGroundView ();
            if (new Version(MonoTouch.Constants.Version) < new Version(7,0,0)) return;
            NavigationController.NavigationBar.TintColor = UIColor.FromRGB (0.564f, 0.0f, 0.015f);
            EdgesForExtendedLayout = UIRectEdge.None;
            NavigationController.NavigationBar.Translucent = false;
            NavigationController.NavigationBar.Opaque = true;
            NavigationController.NavigationBarHidden = true;
		}

		public void UpdateMonkey(Monkey monkey)
		{
			DropboxDatabase.Shared.Update (monkey);
		}

		public override void MotionEnded (UIEventSubtype motion, UIEvent evt)
		{
			base.MotionEnded (motion, evt);
			DropboxDatabase.Shared.Reset ();
		}
		public override bool CanBecomeFirstResponder {
			get {
				return true;
			}
		}

	}

	public class PlayGroundView : UIView
	{
		UIPinchGestureRecognizer pinchGesture;
		public PlaygroundViewController Parent;
		public PlayGroundView ()
		{
			pinchGesture = new UIPinchGestureRecognizer (Scale);
			this.AddGestureRecognizer (pinchGesture);

			var rotationGesture = new UIRotationGestureRecognizer (Rotate);
			this.AddGestureRecognizer (rotationGesture);

			var panGesture = new UIPanGestureRecognizer (Move);
			this.AddGestureRecognizer (panGesture);

			this.BackgroundColor = UIColor.DarkGray;
		}
		Dictionary<Monkey, MonkeyView> MonkeyDictionary = new Dictionary<Monkey, MonkeyView> ();
		public void UpdateMonkeys(Monkey[] monkeys)
		{
			UIView.BeginAnimations ("monkeys");
			for(int i = 0; i < monkeys.Length; i ++){
				Monkey monkey = monkeys[i];
				MonkeyView view;
				MonkeyDictionary.TryGetValue(monkey,out view);
				if (view == null){
					view = new MonkeyView (monkey);
					MonkeyDictionary.Add(monkey,view);
				}
				view.Update (monkey, this.Bounds);
				this.InsertSubview(view,i);
			}
			UIView.CommitAnimations ();
		}

		MonkeyView currentMonkey;

		public MonkeyView CurrentMonkey {
			get {
				if (currentMonkey == null)
					currentMonkey = Subviews.FirstOrDefault () as MonkeyView;
				return currentMonkey;
			}
			set {
				if (currentMonkey == value)
					return;
				currentMonkey = value;
				this.BringSubviewToFront (currentMonkey);
				for (int i = 0; i < Subviews.Length; i ++) {
					var view = Subviews [i] as MonkeyView;
					view.Monkey.Z = i;
					Parent.UpdateMonkey (view.Monkey);
				}



			}
		}

		float lastScale = 1f;

		void Scale (UIPinchGestureRecognizer gesture)
		{
			if (CurrentMonkey == null)
				return;
			if (gesture.State == UIGestureRecognizerState.Began)
				lastScale = 1f;
			var scale = 1f - (lastScale - gesture.Scale);

			var transform = CurrentMonkey.Transform;
			transform.Scale (scale, scale);
			CurrentMonkey.Transform = transform;

			lastScale = gesture.Scale;
			if(gesture.State == UIGestureRecognizerState.Ended)
			{
				CurrentMonkey.UpdateMonkey(Bounds);
				Parent.UpdateMonkey(CurrentMonkey.Monkey);
			}
		}

		float lastRotation = 0f;

		void Rotate (UIRotationGestureRecognizer gesture)
		{
			if (CurrentMonkey == null)
				return;
			if (gesture.State == UIGestureRecognizerState.Ended) {
				lastRotation = 0;
				CurrentMonkey.UpdateMonkey(Bounds);
				Parent.UpdateMonkey(CurrentMonkey.Monkey);
				return;
			}
			var rotation = 0 - (lastRotation - gesture.Rotation);
			var transform = CurrentMonkey.Transform;
			transform.Rotate (rotation);
			CurrentMonkey.Transform = transform;

			lastRotation = gesture.Rotation;
		}

		PointF initialPoint;

		void Move (UIPanGestureRecognizer gesture)
		{
			if (CurrentMonkey == null)
				return;
			var point = gesture.TranslationInView (this);

			if (gesture.State == UIGestureRecognizerState.Began)
				initialPoint = CurrentMonkey.Center;

			point.X += initialPoint.X;
			point.Y += initialPoint.Y;

			CurrentMonkey.Center = point;
			if(gesture.State == UIGestureRecognizerState.Ended)
			{
				CurrentMonkey.UpdateMonkey(Bounds);
				Parent.UpdateMonkey(CurrentMonkey.Monkey);
			}
		}
	}
}

