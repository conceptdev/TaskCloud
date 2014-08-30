/* 
 * Android Gesture Detectors for C#
 *
 * Converted from https://github.com/almeros/android-gesture-detectors
 * by Zack Gramana <zack@xamarin.com>
 *
 * */

using System;
using Android.Content;
using Android.Views;

namespace Android.Views
{
	/// <author>Almer Thie (code.almeros.com)</author>
	public class RotateGestureDetector : TwoFingerGestureDetector
	{
		/// <summary>
		/// Listener which must be implemented which is used by RotateGestureDetector
		/// to perform callbacks to any implementing class which is registered to a
		/// RotateGestureDetector via the constructor.
		/// </summary>
		/// <remarks>
		/// Listener which must be implemented which is used by RotateGestureDetector
		/// to perform callbacks to any implementing class which is registered to a
		/// RotateGestureDetector via the constructor.
		/// </remarks>
		/// <seealso cref="SimpleOnRotateGestureListener">SimpleOnRotateGestureListener</seealso>
		public interface IOnRotateGestureListener
		{
			bool OnRotate(RotateGestureDetector detector);

			bool OnRotateBegin(RotateGestureDetector detector);

			void OnRotateEnd(RotateGestureDetector detector);
		}

		/// <summary>
		/// Helper class which may be extended and where the methods may be
		/// implemented.
		/// </summary>
		/// <remarks>
		/// Helper class which may be extended and where the methods may be
		/// implemented. This way it is not necessary to implement all methods
		/// of OnRotateGestureListener.
		/// </remarks>
		public class SimpleOnRotateGestureListener : RotateGestureDetector.IOnRotateGestureListener
		{
			public virtual bool OnRotate(RotateGestureDetector detector)
			{
				return false;
			}

			public virtual bool OnRotateBegin(RotateGestureDetector detector)
			{
				return true;
			}

			public virtual void OnRotateEnd(RotateGestureDetector detector)
			{
			}
			// Do nothing, overridden implementation may be used
		}

		private readonly RotateGestureDetector.IOnRotateGestureListener mListener;

		private bool mSloppyGesture;

		public RotateGestureDetector(Context context, RotateGestureDetector.IOnRotateGestureListener
			 listener) : base(context)
		{
			mListener = listener;
		}

		protected internal override void HandleStartProgressEvent(MotionEventActions actionCode, MotionEvent
			 evt)
		{
			switch (actionCode)
			{
				case MotionEventActions.PointerDown:
				{
					// At least the second finger is on screen now
					ResetState();
					// In case we missed an UP/CANCEL event
					mPrevEvent = MotionEvent.Obtain(evt);
					mTimeDelta = 0;
					UpdateStateByEvent(evt);
					// See if we have a sloppy gesture
					mSloppyGesture = IsSloppyGesture(evt);
					if (!mSloppyGesture)
					{
						// No, start gesture now
						mGestureInProgress = mListener.OnRotateBegin(this);
					}
					break;
				}

				case MotionEventActions.Move:
				{
					if (!mSloppyGesture)
					{
						break;
					}
					// See if we still have a sloppy gesture
					mSloppyGesture = IsSloppyGesture(evt);
					if (!mSloppyGesture)
					{
						// No, start normal gesture now
						mGestureInProgress = mListener.OnRotateBegin(this);
					}
					break;
				}

				case MotionEventActions.PointerUp:
				{
					if (!mSloppyGesture)
					{
						break;
					}
					break;
				}
			}
		}

		protected internal override void HandleInProgressEvent(MotionEventActions actionCode, MotionEvent
			 evt)
		{
			switch (actionCode)
			{
				case MotionEventActions.PointerUp:
				{
					// Gesture ended but 
					UpdateStateByEvent(evt);
					if (!mSloppyGesture)
					{
						mListener.OnRotateEnd(this);
					}
					ResetState();
					break;
				}

				case MotionEventActions.Cancel:
				{
					if (!mSloppyGesture)
					{
						mListener.OnRotateEnd(this);
					}
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
						bool updatePrevious = mListener.OnRotate(this);
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

		protected internal override void ResetState()
		{
			base.ResetState();
			mSloppyGesture = false;
		}

		/// <summary>
		/// Return the rotation difference from the previous rotate event to the current
		/// event.
		/// </summary>
		/// <remarks>
		/// Return the rotation difference from the previous rotate event to the current
		/// event.
		/// </remarks>
		/// <returns>The current rotation //difference in degrees.</returns>
		public virtual float RotationDegreesDelta
		{
            get {
                double diffRadians = Math.Atan2(mPrevFingerDiffY, mPrevFingerDiffX) - Math.Atan2(
                    mCurrFingerDiffY, mCurrFingerDiffX);
                return (float)(diffRadians * 180 / Math.PI);
            }
		}
	}
}
