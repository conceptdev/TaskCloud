using System;
using System.Collections.Generic;

using Android.Content;
using Android.Widget;
using Android.OS;
using Android.App;

using DropboxSync.Android;

using MonkeyBox;
using System.Linq;
using Android.Graphics;
using Android.Util;
using Java.Util;
using Android.Views;
using Java.Security;
using Java.Lang.Reflect;
using Android.Accounts;

namespace MonkeyBox.Android
{
    [Activity (Label = "MonkeyBox.Android", MainLauncher = true)]
    public class MainActivity : Activity, ScaleGestureDetector.IOnScaleGestureListener, MoveGestureDetector.IOnMoveGestureListener, RotateGestureDetector.IOnRotateGestureListener, GestureDetector.IOnGestureListener
    {
        const string DropboxSyncKey = "YOUR_APP_KEY";
        const string DropboxSyncSecret = "YOUR_APP_SECRET";

        public DBAccountManager Account { get; private set; }

        public DBDatastore DropboxDatastore { get; set; }

        public IEnumerable<Monkey> Monkeys { get; set; }

        static readonly Dictionary<string,int> ResourceMap = new Dictionary<string, int> {
            { "Fred", Resource.Id.Fred },
            { "George", Resource.Id.George },
            { "Hootie", Resource.Id.Hootie },
            { "Julian", Resource.Id.Julian },
            { "Nim", Resource.Id.Nim },
            { "Pepe", Resource.Id.Pepe }
        };

        ScaleGestureDetector PinchDetector { get; set; }

        MoveGestureDetector MoveDetector { get; set; }

        RotateGestureDetector RotationDetector { get; set; }

        GestureDetector Detector { get; set; }

        Monkey CurrentMonkey {
            get {
                return CurrentFocus != null 
                    ? ((MonkeyView)CurrentFocus).Monkey 
                    : ((MonkeyView)MainLayout.GetChildAt(MainLayout.ChildCount - 1)).Monkey;
            }
        }

        MonkeyView CurrentView { get; set; }

        RelativeLayout MainLayout { get; set; }

        Dictionary<string, DBRecord> Records { get; set; }

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            // Add our touch handlers.
            PinchDetector = new ScaleGestureDetector(this, this);
            MoveDetector = new MoveGestureDetector(this, this);
            RotationDetector = new RotateGestureDetector(this, this);

            Detector = new GestureDetector(this, this);
            Detector.IsLongpressEnabled = false;

            MainLayout = (RelativeLayout)FindViewById (Resource.Id.main);
            MainLayout.Touch += HandleTouch;

            // Disable touch handling by the views themselves.
            for(var i = 0; i < MainLayout.ChildCount; i++) {
                var view = MainLayout.GetChildAt(i);
                Log("OnCreate", "View {0} disabled.", i);
                view.Focusable = true;
                view.FocusableInTouchMode = true;
                view.RequestFocus();
            }

            BootstrapDropbox ();
        }

        void HandleTouch (object sender, View.TouchEventArgs e)
        {
            var hit = new Rect((int)e.Event.GetX(), (int)e.Event.GetY(), (int)e.Event.RawX + 1, (int)e.Event.RawY + 1);

            // Figure out which view gets this touch event.

            // If a gesture is in progress, don't change the focus.
            if (!(MoveDetector.IsInProgress && PinchDetector.IsInProgress && RotationDetector.IsInProgress))
            {
                // See if there's another view that should respond to this event.
                var targetView = ViewRespondingToHitTest (hit);
                if (targetView != null && CurrentView != targetView && e.Event.Action == MotionEventActions.Down) {
                    targetView.RequestFocus();
                    CurrentView = targetView;
                }
            } else {
                Log("HandleTouch", "A gesture is currently in progress.");
            }

            var handled = false;

            if (CurrentView != null) {
                foreach (var result in ProcessDetectors(e.Event)) {
                    if (result) {
                        handled = true;
                    }
                }
            }

            e.Handled = handled;
        }

        MonkeyView ViewRespondingToHitTest (Rect hit)
        {
            var currentView = default(MonkeyView);

            for (var i = MainLayout.ChildCount - 1; i > -1; i--) {
                var view = (MonkeyView)MainLayout.GetChildAt (i);

                if (IsWithinCircularBounds(hit, view.CurrentBounds)) {
                    currentView = view;
                    currentView.RequestFocus ();
                    break;
                }
            }
            return currentView;
        }

        static bool IsWithinCircularBounds (Rect hit, Rect bounds)
        {
            if (!bounds.Contains(hit)) return false;

            // Forumula for a circle: (x-a)2 + (y-b)2 = r2
            var centerX = bounds.ExactCenterX() - bounds.Left;
            var centerY = bounds.ExactCenterY() - bounds.Top;
            var radius = centerX;

            double x = hit.Left - bounds.Left, 
                   y = hit.Top - bounds.Top, 
                   r2 = radius * radius,
                   xx = (x - centerX);

            var yy = Math.Sqrt(r2 - xx*xx) + 0.5;
            var upperBound = centerY - yy;
            var lowerBound = centerY + yy;

            var result = !Double.IsNaN (y) && y > upperBound && y < lowerBound;
            return result;
        }

        DisplayMetrics Metrics {
            get {
                var metrics = new DisplayMetrics();
                WindowManager.DefaultDisplay.GetMetrics(metrics);
                return metrics;
            }
        }

        IEnumerable<bool> ProcessDetectors(MotionEvent e) {

            RotationDetector.OnTouchEvent(e);
            yield return RotationDetector.IsInProgress;

            PinchDetector.OnTouchEvent(e);
            yield return PinchDetector.IsInProgress;

            MoveDetector.OnTouchEvent(e);
            yield return MoveDetector.IsInProgress;

            yield return Detector.OnTouchEvent(e);
        }

        #region IOnScaleGestureListener implementation

        public bool OnScale (ScaleGestureDetector detector)
        {
            Log("OnScale", "Scaling by a factor of {0}", detector.ScaleFactor);
            var view = (MonkeyView)MainLayout.FindFocus();
            var bounds = view.Drawable.Bounds;

            view.PivotX = bounds.ExactCenterX();
            view.PivotY = bounds.ExactCenterY();

            view.ScaleX *= detector.ScaleFactor;
            view.ScaleY *= detector.ScaleFactor;

            view.Monkey.Scale = view.ScaleX;

            return true;
        }

        public bool OnScaleBegin (ScaleGestureDetector detector)
        {
            return true;
        }

        public void OnScaleEnd (ScaleGestureDetector detector)
        {
            UpdateDropbox();
        }

        #endregion

        #region IOnMoveGestureListener implementation

        public bool OnMove (MoveGestureDetector detector)
        {
            var positionOffset = detector.FocusDelta;

            var view = CurrentView;

            view.TranslationX += positionOffset.X;
            view.TranslationY += positionOffset.Y;

            view.Monkey.X = view.TranslationX / (float)Metrics.WidthPixels;
            view.Monkey.Y = view.TranslationY / (float)Metrics.HeightPixels;

            Log("OnMove");
            return true;
        }

        public bool OnMoveBegin (MoveGestureDetector detector)
        {
            return true;
        }

        public void OnMoveEnd (MoveGestureDetector detector)
        {
            Log("OnMoveEnd");
            UpdateDropbox();
        }

        #endregion

        #region IOnRotateGestureListener implementation

        public bool OnRotate (RotateGestureDetector detector)
        {
            var view = CurrentView;
            var bounds = view.Drawable.Bounds;

            Log("OnRotate", "Rotating {0:F3} degrees to {1:F3}.", detector.RotationDegreesDelta, view.RotationX);

            view.PivotX = bounds.ExactCenterX();
            view.PivotY = bounds.ExactCenterY();

            view.Monkey.Rotation -= detector.RotationDegreesDelta ; // In decimal degrees.
            view.Rotation = view.Monkey.Rotation;

            return true;
        }

        public bool OnRotateBegin (RotateGestureDetector detector)
        {
            return true;
        }

        public void OnRotateEnd (RotateGestureDetector detector) {  }

        #endregion

        public bool OnDown (MotionEvent e)
        {
            return true; // Must be true, or we won't get future event notifications.
        }

        public bool OnFling (MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            return false;
        }

        public void OnLongPress (MotionEvent e)
        {
            return;
        }

        public bool OnScroll (MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return false;
        }

        public void OnShowPress (MotionEvent e)
        {
            return;
        }

        public bool OnSingleTapUp (MotionEvent e)
        {
            if (CurrentView == null) return true;

            Log("OnSingleTapUp", "Bringing {0} to the top.", CurrentView.Monkey.Name);

            CurrentView.BringToFront();
            CurrentView.RequestFocus();
            CurrentView.Invalidate();

            UpdateDropbox();

            return true;
        }

        void StartApp (DBAccount account = null)
        {
            InitializeDropbox (account);
            Log("StartApp", "Syncing monkies...");
            DropboxDatastore.Sync();
            Monkeys = GetMonkeys ();
            DrawMonkeys (Monkeys);
        }

        void DrawMonkeys (IEnumerable<Monkey> monkeys)
        {
            if (monkeys == null) {
                Log("DrawMonkeys", "Oops! Missing our monkies! Try again.");
                return;
            }
            var mainLayout = FindViewById (Resource.Id.main) as RelativeLayout;
            if (mainLayout == null) 
                throw new ApplicationException("Missing our main layout. Please ensure the layout xml is included in the project.");

            foreach (var monkey in monkeys.OrderBy(m => m.Z))
            {
                var id = ResourceMap[monkey.Name];
                var mv = mainLayout.FindViewById<MonkeyView>(id);

                mv.Monkey = monkey;

                mv.PivotX = mv.Drawable.Bounds.ExactCenterX();
                mv.PivotY = mv.Drawable.Bounds.ExactCenterY();

                mv.Rotation = monkey.Rotation;

                mv.ScaleX = monkey.Scale;
                mv.ScaleY = monkey.Scale;

                mv.TranslationX = monkey.X * (float)Metrics.WidthPixels;
                mv.TranslationY = monkey.Y * (float)Metrics.HeightPixels;

                mv.BringToFront();
                mv.RequestFocus();
            }
        }

        void BootstrapDropbox ()
        {
            // Setup Dropbox.
            Account = DBAccountManager.GetInstance (ApplicationContext, DropboxSyncKey, DropboxSyncSecret);
            Account.LinkedAccountChanged += (sender, e) => {
                if (e.P1.IsLinked)
                    Log("Account.LinkedAccountChanged", "Now linked to {0}", e.P1 != null ? e.P1.AccountInfo != null ? e.P1.AccountInfo.DisplayName : "nobody" : "null");
                else {
                    Log("Account.LinkedAccountChanged", "Now unlinked from {0}", e.P1 != null ? e.P1.AccountInfo != null ? e.P1.AccountInfo.DisplayName : "nobody" : "null");
                    Account.StartLink(this, (int)RequestCode.LinkToDropboxRequest);
                    return;
                }
                Account = e.P0;
                StartApp (e.P1);
            };
            // TODO: Restart auth flow.
            if (!Account.HasLinkedAccount) {
                Account.StartLink (this, (int)RequestCode.LinkToDropboxRequest);
            }
            else {
                StartApp ();
            }
        }

        void InitializeDropbox (DBAccount account)
        {
            Log("InitializeDropbox");
            if (DropboxDatastore == null || !DropboxDatastore.IsOpen || DropboxDatastore.Manager.IsShutDown) {
                DropboxDatastore = DBDatastore.OpenDefault (account ?? Account.LinkedAccount);
                DropboxDatastore.DatastoreChanged += HandleStoreChange;
            }
        }

        void HandleStoreChange (object sender, DBDatastore.SyncStatusEventArgs e)
        {
            if (e.P0.SyncStatus.HasIncoming)
            {
                if (!Account.HasLinkedAccount) {
                    Log("InitializeDropbox", "Account no longer linked, so abandoning.");
                    DropboxDatastore.DatastoreChanged -= HandleStoreChange;
                }
                Console.WriteLine ("Datastore needs to be re-synced.");
                DropboxDatastore.Sync ();
                Monkeys = GetMonkeys();
                DrawMonkeys(Monkeys);
            }
        }

        void UpdateDropbox ()
        {
            Log("Updating Dropbox");

            var table = DropboxDatastore.GetTable ("monkeys");

            // Update records in local cache.
            if (Records.Count == 0)
            {
                for(var i = 0; i < MainLayout.ChildCount; i++) {
                    var view = (MonkeyView)MainLayout.GetChildAt(i);
                    var monkey = view.Monkey;
                    monkey.Z = i;
                    var record = table.GetOrInsert(monkey.Name, monkey.ToFields());
                    Records[monkey.Name] = record;
                }
            } else {
                for(var i = 0; i < MainLayout.ChildCount; i++) {
                    var view = (MonkeyView)MainLayout.GetChildAt(i);
                    var monkey = view.Monkey;
                    monkey.Z = i;
                    DBRecord record;
                    var hasValue = Records.TryGetValue (monkey.Name, out record);
                    if (hasValue)
                        record.SetAll (monkey.ToFields());
                    else
                        table.GetOrInsert(monkey.Name, monkey.ToFields());
                }
            }

            if (!VerifyStore()) {
                RestartAuthFlow (); 
            } else {
                DropboxDatastore.Sync();
            }
        }

        bool VerifyStore ()
        {
            if (!DropboxDatastore.IsOpen) {
                Log ("VerifyStore", "Datastore is NOT open.");
                return false;
            }
            if (DropboxDatastore.Manager.IsShutDown) {
                Log ("VerifyStore", "Manager is shutdown.");
                return false;
            }
            if (!Account.HasLinkedAccount) {
                Log ("VerifyStore", "Account was unlinked while we weren't watching.");
                return false;
            }
            return true;
        }

        void RestartAuthFlow ()
        {
            if (Account.HasLinkedAccount)
                Account.Unlink ();
            else
                Account.StartLink (this, (int)RequestCode.LinkToDropboxRequest);
        }

        IEnumerable<Monkey> GetMonkeys ()
        {
            if (!VerifyStore()) {
                RestartAuthFlow();
                return null;
            }

            Log("GetMonkeys", "Getting monkies...");
            var table = DropboxDatastore.GetTable ("monkeys");
            var values = new List<Monkey>(6);
            var queryResults = table.Query ();

            var results = queryResults.ToEnumerable<DBRecord>().Where(r => !r.IsDeleted).ToList();

            // If we have more records than we should,
            // just start fresh.
            if (results.Count > 6) {
                foreach (var record in results) {
                    record.DeleteRecord();
                }
            }
            if (results.Count == 0) {
                // Generate random monkeys.
                values.AddRange(Monkey.GetAllMonkeys());
                Records = new Dictionary<string, DBRecord>();
            } else {
                Records = results.ToDictionary (x => x.GetString("Name"), x => x);

                // Process existing monkeys.
                foreach (var row in results) {

                    // Remove any MonkeyBox app data that's still in the old format.
                    var currentRow = row;
                    if (row.FieldNames().All(r => currentRow.GetFieldType(r).ToString() == "STRING")) {
                        row.DeleteRecord();
                        continue;
                    }

                    values.Add(new Monkey { 
                        Name = row.GetString("Name"),
                        Scale = Convert.ToSingle(row.GetDouble("Scale")),
                        Rotation = Convert.ToSingle(row.GetDouble("Rotation")),
                        X = Convert.ToSingle(row.GetDouble("X")),
                        Y = Convert.ToSingle(row.GetDouble("Y")),
                        Z = Convert.ToInt32(row.GetLong("Z"))
                    });
                }

                // Create new MonkeyBox app data if we just removed old data.
                if (values.Count == 0) {
                    // Generate random monkeys.
                    values.AddRange(Monkey.GetAllMonkeys());
                    foreach(var val in values) {
                        var record = table.GetOrInsert(val.Name, val.ToFields());
                        Records[val.Name] = record;
                    }
                    DropboxDatastore.Sync();
                }
            }

            return values;
        }

        protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
        {
            var code = (RequestCode)requestCode;

            if (code == RequestCode.LinkToDropboxRequest && resultCode != Result.Canceled) {
                StartApp();
            } else {
                Account.Unlink();
                BootstrapDropbox ();
            }
        }

        void Log (string location) {
            Log(location, String.Empty);           
        }

        void Log (string location, string format, params object[] objects)
        {
            var tag = String.Format("{0} {1}.{2}", GetType ().Name, CurrentMonkey == null ? String.Empty : CurrentMonkey.Name, location);
            global::Android.Util.Log.Debug (tag, format, objects);
        }
    }
}


