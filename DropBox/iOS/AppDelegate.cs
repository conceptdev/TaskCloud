
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;
using DropBoxSync.iOS;

namespace TaskyDrop
{
	/// <summary>Kick everything off</summary>
	public class Application
	{
		static void Main (string[] args)
		{
			UIApplication.Main (args, null, "AppDelegate");
		}
	}

	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		const string DropboxSyncKey = "YOUR_APP_ID";
		const string DropboxSyncSecret = "YOUR_APP_SECRET";
		// Visit https://www.dropbox.com/developers to get these values. 
		// DON'T FORGET to add the key to the iOS Application Properties URL Scheme (with 'db-' prefix)

		UINavigationController navController;

		UIWindow window;
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{

			UINavigationBar.Appearance.TintColor = UIColor.FromRGB (29, 131, 219);

			window = new UIWindow (UIScreen.MainScreen.Bounds);	
			window.BackgroundColor = UIColor.White;
			window.Bounds = UIScreen.MainScreen.Bounds;

			var u = new TaskListScreen();
			navController = new UINavigationController();
			navController.PushViewController (u, false);

			window.RootViewController = navController;
			window.MakeKeyAndVisible ();




			// DROPBOX STUFF
			// The account manager stores all the account info. Create this when your app launches
			var manager = new DBAccountManager (DropboxSyncKey, DropboxSyncSecret);
			DBAccountManager.SharedManager = manager;

			var account = manager.LinkedAccount;
			if (account != null) {
				SetupDropbox ();
			} else
				manager.LinkFromController (window.RootViewController);	
			//--


			return true;
		}

		public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			var account = DBAccountManager.SharedManager.HandleOpenURL (url);
			SetupDropbox ();
			return account != null;
		}

		void SetupDropbox ()
		{
			System.Threading.Tasks.Task.Factory.StartNew (() => {
				DropboxDatabase.Shared.Init ();
			});
		}
	}
}