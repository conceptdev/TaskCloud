# Parse

The official Parse.com Xamarin SDK. Add a cloud backend to your iOS and Android apps with this simple-to-use SDK.

### Initializing the SDK

* Sign up for a free [Parse](http://www.parse.com) account and create a new application.
* In Xamarin Studio or Visual Studio, create a new iOS or Android project.
* Using Xamarin Studio's built-in Component Store, add the free Parse component to your project.
* Find your Application ID and Windows Key on your Parse application dashboard. Select your application on [Parse](https://parse.com/apps), go to "Settings", then open the "Application Keys" tab.

If you're building an iOS app, add this to your `AppDelegate.cs` file:

```csharp
using Parse;
```

```csharp
public AppDelegate ()
{
    // Initialize the Parse client with your Application ID and Windows Key found on
    // your Parse dashboard
    ParseClient.Initialize("YOUR APPLICATION ID", "YOUR WINDOWS KEY");
}
```

If you're building an Android app, add a class called `App` to your project with the following code:

```csharp
using System;
using Android.App;
using Android.Runtime;
using Parse;

namespace ParseAndroidStarterProject
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

            // Initialize the Parse client with your Application ID and Windows Key found on
            // your Parse dashboard
            ParseClient.Initialize ("YOUR APPLICATION ID", "YOUR WINDOWS KEY");
        }
    }
}
```
* Replace `"YOUR APPLICATION ID"` and `"YOUR WINDOWS KEY"` with the keys you found in the previous step.

### Saving an Object

* Saving your first `ParseObject` to Parse is easy.  Add a button to your main ViewController (iOS) or Activity (Android) and add an async `TouchUpInside` (iOS) or `Click` (Android) event handler that saves a new `ParseObject`:

iOS (inside of your `ViewDidLoad` override):
    
```csharp
btn.TouchUpInside += async (sender, e) => {
    var obj = new ParseObject ("Note");
    obj ["text"] = "Hello, world!  This is a Xamarin app using Parse!";
    obj ["tags"] = new List<string> {"welcome", "xamarin", "parse"};
    await obj.SaveAsync ();
};
```

Android (inside of your `OnCreate` override):

```csharp
var btn = this.FindViewById<Button>(Resource.Id.button);
btn.Click += async (sender, e) => {
    var obj = new ParseObject("Note");
    obj ["text"] = "Hello, world!  This is a Xamarin app using Parse!";
    obj ["tags"] = new List<string> {"welcome", "xamarin", "parse"};
    await obj.SaveAsync ();
};
```

* Run the app, touch the button, and then head over to your [Parse](https://parse.com/apps) application's DataBrowser to see the new "Note" object that you've created and stored with Parse!

### User Management

* Adding a user account to your app is as simple as a call to `SignUpAsync()`:

```csharp
public async Task CreateUserAsync ()
{
    var user = new ParseUser ()
    {
        Username = "my name",
        Password = "my pass",
        Email = "email@example.com"
    };
 
    // other fields can be set just like with ParseObject
    user ["phone"] = "415-392-0202";
 
    await user.SignUpAsync ();
}
```

### Documentation

- Technical Docs: https://parse.com/docs/dotnet_guide
- Tutorials: https://parse.com/docs/tutorials
- Terms & Conditions: https://www.parse.com/about/terms

### Contact

- Contact: https://www.parse.com/about/contact
- Blog: http://blog.parse.com/
- Twitter: https://twitter.com/#!/parseit
