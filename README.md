TaskCloud
=========

Contains THREE different 'cloud-based' todo list examples, an example using Dropbox's Datastore API, a cross-platform Azure example and the Apple-specific iCloud:


Dropbox (Xamarin Component)
-------

You can read about the support for Dropbox's latest Datastore API in [Xamarin's blog post](http://blog.xamarin.com/a-quick-look-at-dropbox%E2%80%99s-new-datastore-api%E2%80%A6-in-c-sharp/).

![screenshot](https://raw.github.com/conceptdev/TaskCloud/master/Dropbox/Screenshots/iOS1.png)

Azure (Xamarin Component)
-----
A Xamarin.iOS example built for this introductory Azure Web Services [YouTube video](http://www.youtube.com/watch?v=3O7eFwyiS8Q). 

![screenshot](https://raw.github.com/conceptdev/TaskCloud/master/QuickStartXamarin/Screenshots/iOS1.png)

The other Azure exmaple (below) uses Json and the REST web service directly.


iCloud
------

This sample demonstrates how to subclass UIDocument in iOS5 using MonoTouch, and access the files via iCloud.

NOTE: you need at least two devices with the same App Store login to play.

It is still a work-in-progress. It shows how to subclass UIDocument to provide a way to read/write
files to the iCloud data store and query that datastore.

The TaskDocument:UIDocument subclass creates a .task file for each 'task item'. These are saved-to and read-from 
the iCloud UbiquitousDataStore (see NSFileManager.DefaultManager.GetUrlForUbiquityContainer).

Other features of iCloud such as conflict resolution and moving to/from local storage have NOT been implemented.

![screenshot](http://1.bp.blogspot.com/-XfF8owsMCAo/T1QLNeOsV-I/AAAAAAAABWo/WYaR8hKpgx4/s1600/TaskCloud.png "Sample") 

Azure (first principles)
-----
Basic usage of Microsoft's Azure Mobile Services REST API. Accesses the REST service directly. Purely provided as an example of making it work - not necessarily ready for a production implementation - wait for the official toolkit!

This app works against the same database that you create in Microsoft's Windows 8 example, so you end up with multiple devices/platforms all sharing the same TodoList on the Azure server.

There are two example projects:

* iOS using MonoTouch

* Android using Mono-for-Android

![screenshot](https://raw.github.com/conceptdev/TaskCloud/master/Azure/Screenshots/Screenshots_sml.png)