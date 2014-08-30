/* 
 * Android Gesture Detectors for C#
 *
 * Converted from https://github.com/almeros/android-gesture-detectors
 * by Zack Gramana <zack@xamarin.com>
 *
 * */

using Android.Content;

namespace Android.Views
{
	/// <author>Almer Thie (code.almeros.com)</author>
	public abstract class BaseGestureDetector
	{
		protected internal readonly Context mContext;

		protected internal bool mGestureInProgress;

		protected internal MotionEvent mPrevEvent;

		protected internal MotionEvent mCurrEvent;

		protected internal float mCurrPressure;

		protected internal float mPrevPressure;

		protected internal long mTimeDelta;

		/// <summary>
		/// This value is the threshold ratio between the previous combined pressure
		/// and the current combined pressure.
		/// </summary>
		/// <remarks>
		/// This value is the threshold ratio between the previous combined pressure
		/// and the current combined pressure. When pressure decreases rapidly
		/// between events the position values can often be imprecise, as it usually
		/// indicates that the user is in the process of lifting a pointer off of the
		/// device. This value was tuned experimentally.
		/// </remarks>
		protected internal const float PressureThreshold = 0.67f;

		public BaseGestureDetector(Context context)
		{
			mContext = context;
		}

		/// <summary>
		/// All gesture detectors need to be called through this method to be able to
		/// detect gestures.
		/// </summary>
		/// <remarks>
		/// All gesture detectors need to be called through this method to be able to
		/// detect gestures. This method delegates work to handler methods
		/// (handleStartProgressEvent, handleInProgressEvent) implemented in
		/// extending classes.
		/// </remarks>
		/// <param name="event"></param>
		/// <returns></returns>
		public virtual bool OnTouchEvent(MotionEvent evt)
		{
            MotionEventActions actionCode = (evt.Action & MotionEventActions.Mask);
			if (!mGestureInProgress)
			{
				HandleStartProgressEvent(actionCode, evt);
			}
			else
			{
				HandleInProgressEvent(actionCode, evt);
			}
			return true;
		}

		/// <summary>
		/// Called when the current event occurred when NO gesture is in progress
		/// yet.
		/// </summary>
		/// <remarks>
		/// Called when the current event occurred when NO gesture is in progress
		/// yet. The handling in this implementation may set the gesture in progress
		/// (via mGestureInProgress) or out of progress
		/// </remarks>
		/// <param name="actionCode"></param>
		/// <param name="event"></param>
		protected internal abstract void HandleStartProgressEvent(MotionEventActions actionCode, MotionEvent
			 evt);

		/// <summary>Called when the current event occurred when a gesture IS in progress.</summary>
		/// <remarks>
		/// Called when the current event occurred when a gesture IS in progress. The
		/// handling in this implementation may set the gesture out of progress (via
		/// mGestureInProgress).
		/// </remarks>
		/// <param name="action"></param>
		/// <param name="event"></param>
		protected internal abstract void HandleInProgressEvent(MotionEventActions actionCode, MotionEvent
			 evt);

		protected internal virtual void UpdateStateByEvent(MotionEvent curr)
		{
			MotionEvent prev = mPrevEvent;
			// Reset mCurrEvent
			if (mCurrEvent != null)
			{
				mCurrEvent.Recycle();
				mCurrEvent = null;
			}
			mCurrEvent = MotionEvent.Obtain(curr);
			// Delta time
			mTimeDelta = curr.EventTime - prev.EventTime;
			// Pressure
			mCurrPressure = curr.GetPressure(curr.ActionIndex);
			mPrevPressure = prev.GetPressure(prev.ActionIndex);
		}

		protected internal virtual void ResetState()
		{
			if (mPrevEvent != null)
			{
				mPrevEvent.Recycle();
				mPrevEvent = null;
			}
			if (mCurrEvent != null)
			{
				mCurrEvent.Recycle();
				mCurrEvent = null;
			}
			mGestureInProgress = false;
		}

		/// <summary>
		/// Returns
		/// <code>true</code>
		/// if a gesture is currently in progress.
		/// </summary>
		/// <returns>
		/// 
		/// <code>true</code>
		/// if a gesture is currently in progress,
		/// <code>false</code>
		/// otherwise.
		/// </returns>
		public bool IsInProgress
		{
            get { return mGestureInProgress; }
		}

		/// <summary>
		/// Return the time difference in milliseconds between the previous accepted
		/// GestureDetector event and the current GestureDetector event.
		/// </summary>
		/// <remarks>
		/// Return the time difference in milliseconds between the previous accepted
		/// GestureDetector event and the current GestureDetector event.
		/// </remarks>
		/// <returns>Time difference since the last move event in milliseconds.</returns>
		public virtual long GetTimeDelta()
		{
			return mTimeDelta;
		}

		/// <summary>
		/// Return the event time of the current GestureDetector event being
		/// processed.
		/// </summary>
		/// <remarks>
		/// Return the event time of the current GestureDetector event being
		/// processed.
		/// </remarks>
		/// <returns>Current GestureDetector event time in milliseconds.</returns>
		public virtual long EventTime
		{
            get {
                return mCurrEvent.EventTime;
            }
		}
	}
}
