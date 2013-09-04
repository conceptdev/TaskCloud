using System;
using Android.App;
using Android.Runtime;
using Parse;

namespace Parse
{
	[Application]
	public class App : Application
	{
		public App (IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) 
		{
		}

		public override void OnCreate ()
		{
			base.OnCreate ();

			// https://parse.com/apps/YOUR_APP_NAME/edit#app_keys
			// ApplicationId, Windows/.NET/Client key key
			//ParseClient.Initialize ("APPLICATION_ID", "WINDOWS_KEY");

		}
	}
}