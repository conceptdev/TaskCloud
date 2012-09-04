// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Tasky
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTableView TaskTable { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField TaskTitle { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton SaveButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton DeleteButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TaskTable != null) {
				TaskTable.Dispose ();
				TaskTable = null;
			}

			if (TaskTitle != null) {
				TaskTitle.Dispose ();
				TaskTitle = null;
			}

			if (SaveButton != null) {
				SaveButton.Dispose ();
				SaveButton = null;
			}

			if (DeleteButton != null) {
				DeleteButton.Dispose ();
				DeleteButton = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
