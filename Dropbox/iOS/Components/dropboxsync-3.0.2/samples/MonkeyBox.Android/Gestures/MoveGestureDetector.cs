/* 
 * Android Gesture Detectors for C#
 *
 * Converted from https://github.com/almeros/android-gesture-detectors
 * by Zack Gramana <zack@xamarin.com>
 *
 * */

using Android.Content;
using Android.Graphics;
using Android.Views;

namespace Android.Views
{
	/// <author>Almer Thie (code.almeros.com)</author>
	public class MoveGestureDetector : BaseGestureDetector
	{
		/// <summary>
		/// Listener which must be implemented which is used by MoveGestureDetector
		/// to perform callbacks to any implementing class which is registered to a
		/// MoveGestureDetector via the constructor.
		/// </summary>
		/// <remarks>
		/// Listener which must be implemented which is used by MoveGestureDetector
		/// to perform callbacks to any implementing class which is registered to a
		/// MoveGestureDetector via the constructor.
		/// </remarks>
		/// <seealso cref="SimpleOnMoveGestureListener">SimpleOnMoveGestureListener</seealso>
		public interface IOnMoveGestureListener
		{
			bool OnMove(MoveGestureDetector detector);

			bool OnMoveBegin(MoveGestureDetector detector);

			void OnMoveEnd(MoveGestureDetector detector);
		}

		/// <summary>
		/// Helper class which may be extended and where the methods may be
		/// implemented.
		/// </summary>
		/// <remarks>
		/// Helper class which may be extended and where the methods may be
		/// implemented. This way it is not necessary to implement all methods
		/// of OnMoveGestureListener.
		/// </remarks>
		public class SimpleOnMoveGestureListener : MoveGestureDetector.IOnMoveGestureListener
		{
			public virtual bool OnMove(MoveGestureDetector detector)
			{
				return false;
			}

			public virtual bool OnMoveBegin(MoveGestureDetector detector)
			{
				return true;
			}

			public virtual void OnMoveEnd(MoveGestureDetector detector)
			{
			}
			// Do nothing, overridden implementation may be used
		}

		private static readonly PointF FocusDeltaZero = new PointF();

		private readonly MoveGestureDetector.IOnMoveGestureListener mListener;

		private PointF mCurrFocusInternal;

		private PointF mPrevFocusInternal;

		private PointF mFocusExternal = new PointF();

		private PointF mFocusDeltaExternal = new PointF();

		public MoveGestureDetector(Context context, MoveGestureDetector.IOnMoveGestureListener
			 listener) : base(context)
		{
			mListener = listener;
		}

		protected internal override void HandleStartProgressEvent(MotionEventActions actionCode, MotionEvent
			 evt)
		{
			switch (actionCode)
			{
				case MotionEventActions.Down:
				{
					ResetState();
					// In case we missed an UP/CANCEL event
					mPrevEvent = MotionEvent.Obtain(evt);
					mTimeDelta = 0;
					UpdateStateByEvent(evt);
					break;
				}

				case MotionEventActions.Move:
				{
					mGestureInProgress = mListener.OnMoveBegin(this);
					break;
				}
			}
		}

		protected internal override void HandleInProgressEvent(MotionEventActions actionCode, MotionEvent
			 evt)
		{
			switch (actionCode)
			{
				case MotionEventActions.Up:
				case MotionEventActions.Cancel:
				{
					mListener.OnMoveEnd(this);
					ResetState();
					break;
				}

				case MotionEventActions.Move:
				{
					UpdateStateByEvent(evt);
					// Only accept the event if our relative pressure is within
					// a certain limit. This can help filter shaky data as a
					// finger is lifted.
					if (mCurrPressure / mPrevPressure > PressureThreshold)
					{
						bool updatePrevious = mListener.OnMove(this);
						if (updatePrevious)
						{
							mPrevEvent.Recycle();
							mPrevEvent = MotionEvent.Obtain(evt);
						}
					}
					break;
				}
			}
		}

		protected internal override void UpdateStateByEvent(MotionEvent curr)
		{
			base.UpdateStateByEvent(curr);
			MotionEvent prev = mPrevEvent;
			// Focus intenal
			mCurrFocusInternal = DetermineFocalPoint(curr);
			mPrevFocusInternal = DetermineFocalPoint(prev);
			// Focus external
			// - Prevent skipping of focus delta when a finger is added or removed
			bool mSkipNextMoveEvent = prev.PointerCount != curr.PointerCount;
			mFocusDeltaExternal = mSkipNextMoveEvent ? FocusDeltaZero : new PointF(mCurrFocusInternal
				.X - mPrevFocusInternal.X, mCurrFocusInternal.Y - mPrevFocusInternal.Y);
			// - Don't directly use mFocusInternal (or skipping will occur). Add 
			// 	 unskipped delta values to mFocusExternal instead.
			mFocusExternal.X += mFocusDeltaExternal.X;
			mFocusExternal.Y += mFocusDeltaExternal.Y;
		}

		/// <summary>Determine (multi)finger focal point (a.k.a.</summary>
		/// <remarks>
		/// Determine (multi)finger focal point (a.k.a. center point between all
		/// fingers)
		/// </remarks>
		/// <param name="MotionEvent">e</param>
		/// <returns>PointF focal point</returns>
		private PointF DetermineFocalPoint(MotionEvent e)
		{
			// Number of fingers on screen
            int pCount = e.PointerCount;
			float x = 0f;
			float y = 0f;
			for (int i = 0; i < pCount; i++)
			{
				x += e.GetX(i);
				y += e.GetY(i);
			}
			return new PointF(x / pCount, y / pCount);
		}

		public virtual float FocusX
		{
            get { return mFocusExternal.X; }
		}

		public virtual float FocusY
		{
            get { return mFocusExternal.Y; }
		}

		public virtual PointF FocusDelta
		{
            get { return mFocusDeltaExternal; }
		}
	}
}
