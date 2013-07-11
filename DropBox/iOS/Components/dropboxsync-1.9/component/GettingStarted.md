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

You'll need to register the url scheme "db-APP_KEY" to complete the
authentication flow. Double-click on your app's Info.plist file, select
the Advanced Tab, find the URL Types Section, then click Add URL Type
and set URL Schemes to db-APP_KEY (i.e.	"db-aaa111bbb2222").

### Link the user

Once you've added the code above, you're ready to link the user's
Dropbox account from your UI. For example, add this snippet to a UI
event handler in one of your controllers:

```csharp
DBAccountManager.SharedManager.LinkFromController (myController)
```

This will show the Dropbox OAuth screen and ask the user to link their
account.

## Listing folders

Once you've linked your app to a Dropbox account, you may want to list
the contents of your app's exclusive Dropbox folder. If you used the
sample code above, once you're authenticated you should have a properly
authorized `DBFilesystem` instance stored in
`DBFilesystem.SharedFilesystem`, which is the object that allows you to
list folders; and open, move or delete files.

```csharp
void ListFiles (string path)
{
	DBError error;

	var contents = DBFilesystem.SharedFilesystem.ListFolder (path, out error);
	foreach (DBFileInfo info in contents) {
		Console.WriteLine (info.Path);
	}	
}
```

Sync API method calls involving file reads are synchronous, meaning they wait until the
requested data is available, or until an error occurs and an exception
is thrown. You should make sure all DBFilesystem and DBFile calls are
done from a background thread to keep your UI responsive.

## Working with files

Initially, your app's folder in your user's Dropbox won't contain any
files, so you'll need to create one:

```csharp
void CreateFile ()
{
	DBError error;

	var dbpath = DBPath.Root.ChildPath ("hello.txt");
	var file = DBFilesystem.SharedFilesystem.CreateFile (dbpath, out error);
	file.WriteString ("Hello World!", out error);
}
```

Writing to the file will succeed immediately, and the Sync API will sync
the file to Dropbox asynchronously. Even if you are offline, the write
will succeed and it will be automatically synced to the server once your
app comes back online.

Reading a file is just as easy: you can call `DBFile.ReadString` to get
a file's contents as a UTF8 string. If the file is not cached, this
operation can take a while, so always call this method on a background
thread.

## Watching for changes

Many objects in the Sync API allow you to register a callback
that will get called when something about a file changes. Here's an
example of how to find out when a file has changed:

```csharp
void CreateAndWatchFile ()
{
	// First, create a file to change for demo purposes.
	DBError err; 
	DBPath path = DBPath.Root.ChildPath ("change-me.txt");
	DBFile file = DBFilesystem.SharedFilesystem.CreateFile (path, out err);

	// Next, register for changes on that file.
	file.AddObserver (this, () => {
			DBFileStatus status = file.NewerStatus;

			// If file.NewerStatus is null, the file hasn't changed.
			if (status == null) return;

			if (status.Cached) {
				DBError error;
				file.Update (out error);
				Console.WriteLine ("The updated file has finished downloading");
			} else {
				Console.WriteLine ("The file is still downloading");
			}
	});
}
```

In the example above, every time you edit "change-me.txt" in your app's
Dropbox folder, the callback will print to the console when the file
starts downloading,
and print again when it finishes downloading.

To stop listening for updates:

```csharp
file.RemoveObserver (this);
```

## Creating a datastore and your first table

With a DBAccount in hand, the next step is to open the default datastore. Each app has its own default datastore per user.

```csharp
DBDatastore store = DBDatastore.OpenDefaultStoreForAccount (account, out error);
```

In order to store records in a datastore, you'll need to put them in a table. Let's define a table named "tasks":

```csharp
DBTable tasksTbl = store.GetTable ("people");
```

You've got a datastore manager, a datastore for your app, and a table for all the tasks you're about to make. Let's start storing some data.

## Working with records

A record is a set of name and value pairs called fields, similar in concept to a dictionary. Records in the same table can have different combinations of fields; there's no schema on the table which contains them. In fact, the record is created by first creating a dictionary.

```csharp
var keys = new NSString[] {
	new NSString("taskname"),
	new NSString("completed")
};
var values = new NSString[] {
	new NSString("Buy milk"),
	new NSString("No")
};

NSDictionary data = NSDictionary.FromObjectsAndKeys (values, keys);

DBRecord firstTask = tasksTbl.Insert (data);
```

This task is now in memory, but hasn't been persisted to storage or synced to Dropbox. Thankfully, that's simple:

```csharp
store.Sync (null);
```

Sync may be a straightforward method, but it wraps some powerful functionality. Sync both saves all of your local changes and applies any remote changes as well, automatically merging and dealing with conflicts along the way. Sync even works offline; you won't apply any remote changes, but your local changes will be saved to persistent storage and synced to Dropbox when the device comes back online.

Once syncing completes, visit the [datastore web inspector](https://www.dropbox.com/developers/apps/datastores) and you should see your newly created task.

Accessing data from a record is straightforward:

```csharp
string taskname = (NSString) firstTask ["taskname"];
```

Editing tasks is just as easy. This is how you can mark the first result as completed:

```csharp
firstTask["completed"] = "Yes";
store.Sync (null);
```

After the edit, calling sync will commit the edits locally and then sync them to Dropbox.

Finally, if you want to remove the record completely, just call DeleteRecord ():

```csharp
firstTask.DeleteRecord ();
store.Sync (null);
```

## Querying records

You can query the records in a table to get a subset of records that match a set of field names and values you specify. The query method takes a set of conditions that the fields of a record must match to be returned in the result set. For each included condition, all records must have a field with that name and that field's value must be exactly equal to the specified value. For strings, this is a case-sensitive comparison (e.g. "abc" won't match "ABC").

```csharp
DBRecord [] results = tasksTbl.Query (NSDictionary.FromObjectAndKey (new NSString ("No"), new NSString ("completed")), error);
DBRecord firstResult = results [0];
```

The records that meet the specified query are not returned in any guaranteed order. The entire result set is returned so you may apply sort in memory after the request completes.

If no condition set is provided, the query will return every record in the table.

```csharp
DBRecord [] results = tasksTbl.Query (null, error);
```

## Using observers

A datastore will receive changes from other instances of your app when you call sync. For some apps, the frequency of updates will be low; others may be rapid-fire. In either case, your app should respond as soon as those changes happen by updating the state of your app. You can do this by registering sync status observers.

```csharp
store.AddObserver (store, () => {
	if (store.Status == DBDatastoreStatus.Incoming) {
		// Handle the updated data
	}
});
```

The observer's handler is called whenever the status of the datastore changes, which includes downloading or uploading changes. It is also called when there are changes ready to be applied to the local state.

Checking store.Status in the handler lets you figure out when there are new changes that your app should respond to. If there are changes, calling sync will apply those changes to the datastore. sync will also return a dictionary of table IDs to sets of records that changed as a result of the sync. Your app can update based on the set of changed records or you can simply query the new states of the tables and update your app's views with the results.

## Records and fields

The record is the smallest grouping of data in a datastore. It combines a set of fields to make a useful set of information within a table.

## Record IDs

Each record has a string ID. An ID can be provided when a record is created, or one will be automatically generated and assigned if none is provided. Once a record is created, the ID cannot be changed.

Other records can refer to a given record by storing its ID. This is similar to the concept of a foreign key in SQL databases.

## Field types

Records can contain a variety of field types. Earlier in this tutorial, you saw strings and booleans, but you can also specify a number of other types. Here is a complete list of all supported types:

- String
- Boolean
- Integer – 64 bits, signed
- Floating point – IEEE double
- Date – POSIX-like timestamp stored with millisecond precision.
- Bytes – Arbitrary data, which is treated as binary, such as thumbnail images and compressed data. Individual records can be up to 100KB, which limits the size of the blob. If you want to store larger files, you should use the Sync API and reference the paths to those files in your records. If you want to store larger files, you should use the Sync API and reference the paths to those files in your records.
- List – A special value that can contain other values, though not other lists. Lists are described in more detail below.

## Lists and List Operations

Lists are special field values. They contain an ordered list of other values, though not other lists. Lists can be manipulated via four list operations: put (i.e. replace), move, insert, and delete. These list operations allow Dropbox to handle merging changes to the structure of the list automatically.

## Storage size limits

The overall size of a datastore is calculated by summing the sizes of all values of all fields. Your app can store up to 5MB of data across all its datastores without counting against the user's storage quota. Any data beyond the first 5MB is factored into the user's Dropbox storage quota, and writing can be limited in these cases when a user is over quota.

## Customizing conflict resolution

Datastores automatically merge changes on a per-field basis. If, for example, a user were to edit the taskname of a task on one device and the completed status of that same task on another device, the Datastore API would merge these changes without a conflict.

Sometimes, however, there will be simultaneous changes to the same field of the same record, and this requires conflict resolution. For example, if a user were to edit the completed status of a task on two different devices while offline, it's unclear how those changes should be merged when the devices come back online. Because of this ambiguity, app developers can choose what conflict resolution rule they want to follow.

To set the conflict resolution rule, call the SetResolutionRule () method on a table, and pass in the name of a field and the resolution rule you want to apply to that field.

```csharp
tasksTbl.SetResolutionRule (DBResolutionRule.Local, "completed");
```

There are five available resolution rules that affect what happens when a remote change conflicts with a local change:

- **DBResolutionRule.Remote** – The remote value will be chosen. This is the default behavior for all fields.
- **DBResolutionRule.Local** – The local value of the field will be chosen.
- **DBResolutionRule.Max** – The greater of the two changes will be chosen.
- **DBResolutionRule.Min** – The lesser of the two changes will be chosen.
- **DBResolutionRule.Sum** – Additions and subtractions to the value will be preserved and combined.

**Note that resolution rules don't persist, so you should set any custom resolution rules after opening a datastore but before the first time you sync.**

## Documentation

To explore the full Dropbox Sync API, check out our [iOS SDK documentation](https://www.dropbox.com/developers/sync/docs/ios).
