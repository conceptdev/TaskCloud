TaskCloud
=========

This sample demonstrates how to subclass UIDocument in iOS5 using MonoTouch, and access the files via iCloud.

NOTE: you need at least two devices with the same App Store login to play.

It is still a work-in-progress. It shows how to subclass UIDocument to provide a way to read/write
files to the iCloud data store and query that datastore.

The TaskDocument:UIDocument subclass creates a .task file for each 'task item'. These are saved-to and read-from 
the iCloud UbiquitousDataStore (see NSFileManager.DefaultManager.GetUrlForUbiquityContainer).

Other features of iCloud such as conflict resolution and moving to/from local storage have NOT been implemented.

![screenshot](http://1.bp.blogspot.com/-XfF8owsMCAo/T1QLNeOsV-I/AAAAAAAABWo/WYaR8hKpgx4/s1600/TaskCloud.png "Sample") 