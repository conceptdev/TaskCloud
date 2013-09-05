
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;

namespace Parse
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
		// TODO: delete database!
		private static TaskDatabase _Database;
		public static TaskDatabase Database { 
			get {
				if (_Database == null) {
					string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
					string libraryPath = Path.Combine (documentsPath, "../Library/"); // Library folder
					var path = Path.Combine (libraryPath, "taskdatabase.db3");
					_Database = new TaskDatabase (path);
				}
				return _Database;
			} 
		}

		// TODO: add ctor
//		public AppDelegate ()
//		{
//			// https://parse.com/apps/YOUR_APP_NAME/edit#app_keys
//			// ApplicationId, Windows/.NET/Client key
//			//ParseClient.Initialize ("APPLICATION_ID", "WINDOWS_KEY");
//			
//		}

		UINavigationController navController;
		UIWindow window;
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			UINavigationBar.Appearance.TintColor = UIColor.FromRGB (11, 97, 206);
			
			window = new UIWindow (UIScreen.MainScreen.Bounds);	
			window.BackgroundColor = UIColor.White;
			window.Bounds = UIScreen.MainScreen.Bounds;
		
			var u = new TaskListScreen();
			navController = new UINavigationController();
			navController.PushViewController (u, false);

			window.RootViewController = navController;
            window.MakeKeyAndVisible ();

			return true;
		}
	}
}