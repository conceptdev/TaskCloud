using Android.Content;
using Android.Util;
using Android.Views;
using System.Collections.Generic;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Graphics;
using System;
using Android.Media;
using Android.Animation;
using Android.Views.Animations;
using Java.Util;
using Android.Support.V4.View;
using Java.Lang;

namespace MonkeyBox.Android
{
    public class MonkeyView : ImageView
    {
        protected ScaleGestureDetector PinchDetector;
        protected GestureDetector Detector;
        protected VelocityTracker VelocityTracker;

        protected Monkey monkey;

        DisplayMetrics Metrics { get; set; }

        public Monkey Monkey {
            get {
                return monkey;
            }
            set {
                var needsRedraw = monkey != null;
                monkey = value;
                if (needsRedraw)
                    Invalidate();
            }
        }

        public Rect CurrentBounds {
            get {
                var rect = new RectF(Drawable.Bounds);
                Matrix.MapRect(rect);
                return new Rect((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
            }
        }

        public MonkeyView (Context context, IAttributeSet attrs) :
            base (context, attrs)
        {
            Initialize ();
        }

        public MonkeyView (Context context, Monkey monkey) :
            base (context)
        {
            Monkey = monkey;
            Initialize ();
        }

        public MonkeyView (Context context, IAttributeSet attrs, Monkey monkey) :
            base (context, attrs)
        {
            Monkey = monkey;
            Initialize ();
        }

        public MonkeyView (Context context, IAttributeSet attrs, int defStyle, Monkey monkey) :
            base (context, attrs, defStyle)
        {
            Monkey = monkey;
            Initialize ();
        }

        void Initialize ()
        {
        }
    }
}

