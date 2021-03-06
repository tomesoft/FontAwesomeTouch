// 
// AppDelegate.cs
//  
// Author:
//       Junichi OKADOME (tome@tomesoft.net)
// 
// AppDelegate.cs
//  
// Author:
//       Junichi OKADOME (tome@tomesoft.net)
// 
// Copyright 2013 tomesoft.net
//	
//	Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//		
//		http://www.apache.org/licenses/LICENSE-2.0
//		
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace TestAppForMonoMac
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		MainWindowController mainWindowController;
		
		public AppDelegate ()
		{
		}

		public override void FinishedLaunching (NSObject notification)
		{
			mainWindowController = new MainWindowController ();
			mainWindowController.Window.MakeKeyAndOrderFront (this);
		}

		partial void exportGlyphsAsPNG (NSObject sender)
		{
			mainWindowController.DoExportGlyphsAsPNG ();
		}
	}
}

