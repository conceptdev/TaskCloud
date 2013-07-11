The Dropbox Sync API allows you to give your app its own private Dropbox client and leave the syncing to Dropbox.

- Focus on your data. The Sync API handles all the caching, retrying, and file change notifications.
- Writes are local so changes are immediate. The Sync API syncs to Dropbox behind the scenes.
- Your app works great even when offline and automatically syncs when it's back online.

### Datastore API (beta)

Keep your app's structured data in sync with Dropbox. These days, your app need to store and sync more than just files. With the Datastore API, structured data like contacts, to-do items, and game state can be synced effortlessly. Datastores work across platforms, offline, and even support automatic conflict resolution.

## Authenticating with Dropbox

Add the following lines of code to link a user's Dropbox account to your
app:

### In AppDelegate.cs

```csharp
using DropBoxSync.iOS;
...

// Get your own App Key and Secret from https://www.dropbox.com/developers/apps
const string DropboxSyncKey = "YOUR_APP_KEY";
const string DropboxSyncSecret = "YOUR_APP_SECRET";

public override bool FinishedLaunching (UIApplication app, NSDictionary options)
{
	
	// The account manager stores all the account info. Create this when your app launches
	var manager = new DBAccountManager (DropboxSyncKey, DropboxSyncSecret);
	DBAccountManager.SharedManager = manager;

	var account = manager.LinkedAccount;
	if (account != null) {
		var filesystem = new DBFilesystem (account);
		DBFilesystem.SharedFilesystem = filesystem;
	}	

	// ...
}

public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
{
	var account = DBAccountManager.SharedManager.HandleOpenURL (url);
	if (account != null) {
		var filesystem = new DBFilesystem (account);
		DBFilesystem.SharedFilesystem = filesystem;
		Console.WriteLine ("App linked successfully!");
		return true;
	} else {
		Console.WriteLine ("App is not linked");
		return false;
	}
}

```

### In Info.plist

You'll need to register the url scheme "db-DropboxSyncKey" to complete the
authentication flow. Double-click on your app's Info.plist file, select
the Advanced Tab, find the URL Types Section, then click Add URL Type
and set URL Schemes to db-DropboxSyncKey (i.e.	"db-aaa111bbb2222").

### Link the user

Once you've added the code above, you're ready to link the user's
Dropbox account from your UI. For example, add this snippet to a UI
event handler in one of your controllers:

```csharp
DBAccountManager.SharedManager.LinkFromController (myController)
```

This will show the Dropbox OAuth screen and ask the user to link their
account.
