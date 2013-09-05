The official Parse.com Xamarin SDK. Add a cloud backend to your iOS and Android apps with this simple-to-use SDK.

## Saving an Object

Here's all it takes to save an object to Parse:

```csharp
using Parse;
...

async void MakePost ()
{
    var post = new ParseObject ("Post");
    post ["title"] = "I love Parse with Xamarin";
    post ["body"] = "Need I say more?";
    await post.SaveAsync ();
}
```

## Querying for Data

Use Parse's rich querying features to retrieve your data:

```csharp
// Find the 5 nearest points of interest to the Evolve conference in Austin, TX
// with a rating greater than 2.5 stars.
var query = ParseObject.GetQuery ("PointOfInterest")
    .WhereNear ("location", new ParseGeoPoint (30.265348, -97.738613))
    .WhereGreaterThan ("rating", 2.5)
    .Limit (5);
IEnumerable<ParseObject> results = await query.FindAsync ();
```

## Saving a File

Easily save files to Parse and associate them with your `ParseObject`s:

```csharp
Stream profilePic = /* Take a picture */;
var file = new ParseFile ("profilePic.png", profilePic);
await file.SaveAsync ();
ParseUser.CurrentUser ["profilePic"] = file;
await ParseUser.CurrentUser.SaveAsync ();
```

## User Management

Adding a user account to your app is as simple as a call to `SignUpAsync()`:

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

You can even create and log in users using Facebook:

```csharp
string sessionToken = /* Authenticate the user with Facebook and fetch a session token */;
DateTime expiration = /* The expiration time for the session token */;
string facebookId = /* The user's Facebook ID */;
await ParseFacebookUtils.LogInAsync (facebookId, sessionToken, expiration);
```

See our full [API documentation](http://parse.com/docs/dotnet/api/Index.html) or our [guide](https://parse.com/docs/dotnet_guide)
for more info, and learn more about Parse [here](https://parse.com).
