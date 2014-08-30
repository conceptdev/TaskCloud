/* 
 * Android Gesture Detectors for C#
 *
 * Converted from https://github.com/almeros/android-gesture-detectors
 * by Zack Gramana <zack@xamarin.com>
 *
 * */

using Android.Content;
using Android.Util;
using Android.Views;

namespace Android.Views
{
	/// <author>Almer Thie (code.almeros.com)</author>
	public abstract class TwoFingerGestureDetector : BaseGestureDetector
	{
		private readonly float mEdgeSlop;

		private float mRightSlopEdge;

		private float mBottomSlopEdge;

		protected internal float mPrevFingerDiffX;

		protected internal float mPrevFingerDiffY;

		protected internal float mCurrFingerDiffX;

		protected internal float mCurrFingerDiffY;

		private float mCurrLen;

		private float mPrevLen;

		public TwoFingerGestureDetector(Context context) : base(context)
		{
			ViewConfiguration config = ViewConfiguration.Get(context);
			mEdgeSlop = config.ScaledEdgeSlop;
		}

		protected internal abstract override void HandleStartProgressEvent(MotionEventActions actionCode
			, MotionEvent evt);

		protected internal abstract override void HandleInProgressEvent(MotionEventActions actionCode, MotionEvent
			 evt);

		protected internal override void UpdateStateByEvent(MotionEvent curr)
		{
			base.UpdateStateByEvent(curr);
			MotionEvent prev = mPrevEvent;
			mCurrLen = -1;
			mPrevLen = -1;
			// Previous
			float px0 = prev.GetX(0);
			float py0 = prev.GetY(0);
			float px1 = prev.GetX(1);
			float py1 = prev.GetY(1);
			float pvx = px1 - px0;
			float pvy = py1 - py0;
			mPrevFingerDiffX = pvx;
			mPrevFingerDiffY = pvy;
			// Current
			float cx0 = curr.GetX(0);
			float cy0 = curr.GetY(0);
			float cx1 = curr.GetX(1);
			float cy1 = curr.GetY(1);
			float cvx = cx1 - cx0;
			float cvy = cy1 - cy0;
			mCurrFingerDiffX = cvx;
			mCurrFingerDiffY = cvy;
		}

		/// <summary>
		/// Return the current distance between the two pointers forming the
		/// gesture in progress.
		/// </summary>
		/// <remarks>
		/// Return the current distance between the two pointers forming the
		/// gesture in progress.
		/// </remarks>
		/// <returns>Distance between pointers in pixels.</returns>
		public virtual float GetCurrentSpan()
		{
			if (mCurrLen == -1)
			{
				float cvx = mCurrFingerDiffX;
				float cvy = mCurrFingerDiffY;
				mCurrLen = FloatMath.Sqrt(cvx * cvx + cvy * cvy);
			}
			return mCurrLen;
		}

		/// <summary>
		/// Return the previous distance between the two pointers forming the
		/// gesture in progress.
		/// </summary>
		/// <remarks>
		/// Return the previous distance between the two pointers forming the
		/// gesture in progress.
		/// </remarks>
		/// <returns>Previous distance between pointers in pixels.</returns>
		public virtual float GetPreviousSpan()
		{
			if (mPrevLen == -1)
			{
				float pvx = mPrevFingerDiffX;
				float pvy = mPrevFingerDiffY;
				mPrevLen = FloatMath.Sqrt(pvx * pvx + pvy * pvy);
			}
			return mPrevLen;
		}

		/// <summary>MotionEvent has no getRawX(int) method; simulate it pending future API approval.
		/// 	</summary>
		/// <remarks>MotionEvent has no getRawX(int) method; simulate it pending future API approval.
		/// 	</remarks>
		/// <param name="event"></param>
		/// <param name="pointerIndex"></param>
		/// <returns></returns>
		protected internal static float GetRawX(MotionEvent evt, int pointerIndex)
		{
			float offset = evt.GetX() - evt.RawX;
			if (pointerIndex < evt.PointerCount)
			{
				return evt.GetX(pointerIndex) + offset;
			}
			return 0f;
		}

		/// <summary>MotionEvent has no getRawY(int) method; simulate it pending future API approval.
		/// 	</summary>
		/// <remarks>MotionEvent has no getRawY(int) method; simulate it pending future API approval.
		/// 	</remarks>
		/// <param name="event"></param>
		/// <param name="pointerIndex"></param>
		/// <returns></returns>
		protected internal static float GetRawY(MotionEvent evt, int pointerIndex)
		{
			float offset = evt.GetY() - evt.RawY;
			if (pointerIndex < evt.PointerCount)
			{
				return evt.GetY(pointerIndex) + offset;
			}
			return 0f;
		}

		/// <summary>Check if we have a sloppy gesture.</summary>
		/// <remarks>
		/// Check if we have a sloppy gesture. Sloppy gestures can happen if the edge
		/// of the user's hand is touching the screen, for example.
		/// </remarks>
		/// <param name="event"></param>
		/// <returns></returns>
		protected internal virtual bool IsSloppyGesture(MotionEvent evt)
		{
			// As orientation can change, query the metrics in touch down
			DisplayMetrics metrics = mContext.Resources.DisplayMetrics;
            mRightSlopEdge = metrics.WidthPixels - mEdgeSlop;
            mBottomSlopEdge = metrics.HeightPixels - mEdgeSlop;
			float edgeSlop = mEdgeSlop;
			float rightSlop = mRightSlopEdge;
			float bottomSlop = mBottomSlopEdge;
			float x0 = evt.RawX;
			float y0 = evt.RawY;
			float x1 = GetRawX(evt, 1);
			float y1 = GetRawY(evt, 1);
			bool p0sloppy = x0 < edgeSlop || y0 < edgeSlop || x0 > rightSlop || y0 > bottomSlop;
			bool p1sloppy = x1 < edgeSlop || y1 < edgeSlop || x1 > rightSlop || y1 > bottomSlop;
			if (p0sloppy && p1sloppy)
			{
				return true;
			}
			else
			{
				if (p0sloppy)
				{
					return true;
				}
				else
				{
					if (p1sloppy)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
