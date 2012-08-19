#define USE_WEAKREFERENCE
using System;
using System.IO;
using System.Linq;
//using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
//using System.Threading.Tasks;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace FontAwesomeTouch
{
#if USE_WEAKREFERENCE
	public class WeakHandle<TObj>
		//where TObj : class
	{
		private TObj _strongReference;
		public System.WeakReference Reference { get; protected set; }
		
		public bool IsAlive { get { return this.Reference.IsAlive; } }
		
		public static implicit operator TObj (WeakHandle<TObj> wkh)
		{
			return (TObj)wkh.Reference.Target;
		}
		
		public TObj Target { get { return (TObj)Reference.Target; } }
		
		public bool HasStrongReference
		{
			get { return typeof (TObj).IsValueType ||
				_strongReference != null; }
		}
		
		public void MakeStrong ()
		{
			if (IsAlive)
				_strongReference = Target;
		}
		
		public void MakeWeak ()
		{
			_strongReference = default (TObj);
		}
		
		public WeakHandle (TObj obj) : this (obj, false)
		{
		}

		public WeakHandle (TObj obj, bool hasStrongInitially)
		{
			Reference = new WeakReference (obj);
			if (hasStrongInitially)
				MakeStrong ();
		}
	}
#endif

	public static class FontAwesomeUtil
	{
		const string TtfFileName = "fontawesome-webfont.ttf";
#if USE_WEAKREFERENCE
		static WeakHandle<CGFont> cachedFont;

		public static void MakeStrongCache ()
		{
			if (cachedFont != null || cachedFont.IsAlive)
				cachedFont.MakeStrong ();
		}
		public static void MakeWeakCache ()
		{
			if (cachedFont != null || cachedFont.IsAlive)
				cachedFont.MakeWeak ();
		}
#else
		internal static CGFont cachedFont;
#endif

		public static string PathToLoadFont { get; set; }

		static bool IsValidObject (object obj)
		{
			var nsobj = obj as NSObject;
			if (nsobj != null)
				return (nsobj.Handle != IntPtr.Zero);
			return (obj != null);
		}

		public static CGFont Font
		{
			get
			{
#if USE_WEAKREFERENCE
				if (cachedFont == null || !cachedFont.IsAlive || !IsValidObject (cachedFont.Target))
					LoadFont ();
				return (cachedFont != null) ? cachedFont.Target : null;
#else
				if (cachedFont == null || !IsValidObject (cachedFont))
					LoadFont ();
				return cachedFont;
#endif
			}
		}

		public static void ClearCache ()
		{
#if USE_WEAKREFERENCE
			if (cachedFont != null && cachedFont.IsAlive && IsValidObject (cachedFont.Target))
			{
				cachedFont.Target.Dispose ();
				cachedFont = null;
			}
#else
			if (cachedFont != null && cachedFont.Handle != IntPtr.Zero)
			{
				cachedFont.Dispose ();
				cachedFont = null;
			}
#endif
		}

		public static CGImage GetImage (string glyphName, int widthUnit, int heightUnit
		                                , float fontSizeUnit
		                                , CGColor textColor
		                                , CGColor backColor
		                                , PointF position = default (PointF)
		                                , float scale=1f)
		{
			CGFont font = FontAwesomeUtil.Font;
			if (font == null)
				throw new InvalidOperationException ("Font not loaded");

			int width = (int)(widthUnit * scale);
			int height = (int)(heightUnit * scale);
			float fontSize = fontSizeUnit * scale;
			PointF posScaled = new PointF (position.X * scale, position.Y * scale);

			CGImage cgImg = null;
			byte[] bytes = new byte[width*height*4];
			using (var context = new CGBitmapContext (bytes, width, height, 8, width*4
			                                          , CGColorSpace.CreateDeviceRGB ()
			                                          , CGImageAlphaInfo.PremultipliedFirst))
			{
				if (backColor.Alpha > 0f)
				{
					context.SetFillColor (backColor);
					context.FillRect (context.GetClipBoundingBox ());
				}

				context.SetAllowsFontSmoothing (true);
				context.SetAllowsAntialiasing (true);
				context.SetAllowsFontSubpixelQuantization (true);
				context.SetAllowsSubpixelPositioning (true);

				// draw bounding box
//				context.SetStrokeColor (1f, 1f);
//				context.BeginPath ();
//				context.AddRect (context.GetClipBoundingBox ());
//				context.StrokePath ();

				context.SetFillColor (textColor);
				context.SetTextDrawingMode (CGTextDrawingMode.Fill);
				context.SetFont (font);
				context.SetFontSize (fontSize);
				context.ShowGlyphsAtPositions(
					new ushort[] {font.GetGlyphWithGlyphName (glyphName)}
					, new PointF[] { posScaled }, 1);

				cgImg = context.ToImage ();
			}

			return cgImg;
		}

		public static UIImage GetUIImage (string glyphName, int width, int height
		                                  , float fontSize
		                                  , UIColor textColor
		                                  , UIColor backColor
		                                  , PointF position = default (PointF)
		                                  , float scale = 0f)
		{
			if (scale == 0f)
				scale = UIScreen.MainScreen.Scale;

			CGImage cgImg = GetImage (glyphName, width, height, fontSize, textColor.CGColor, backColor.CGColor, position, scale);
			return new UIImage (cgImg, scale, UIImageOrientation.Up);
		}


		// Get square sized image
		public static CGImage GetImageForBarItem (string glyphName, int sizeUnit=20, float scale=1f)
		{
			CGFont font = FontAwesomeUtil.Font;
			if (font == null)
				throw new InvalidOperationException ("Font not loaded");

			int size = (int)(sizeUnit * scale);

			float offsetX = (float)sizeUnit / 8.575f;
			float offsetY = (float)sizeUnit / 7.52f;

			CGImage cgImg = null;
			byte[] bytes = new byte[size * size * 4];
			using (var context = new CGBitmapContext (bytes, size, size, 8, size*4
			                                          , CGColorSpace.CreateDeviceRGB ()
			                                          , CGImageAlphaInfo.PremultipliedFirst))
			{
				context.SetAllowsFontSmoothing (true);
				context.SetAllowsAntialiasing (true);
				context.SetAllowsFontSubpixelQuantization (true);
				context.SetAllowsSubpixelPositioning (true);

				// draw bounding box
//				context.SetStrokeColor (1f, 1f);
//				context.BeginPath ();
//				context.AddRect (context.GetClipBoundingBox ());
//				context.StrokePath ();

				context.SetFillColor (1f, 1f);
				context.SetTextDrawingMode (CGTextDrawingMode.Fill);
				context.SetFont (font);
				context.SetFontSize ((float)size);
				// TODO: measure the glyph size and arrange
				context.ShowGlyphsAtPoint (offsetX*scale, offsetY*scale, new ushort[] {font.GetGlyphWithGlyphName (glyphName)});

				cgImg = context.ToImage ();
			}

			return cgImg;
		}

		public static UIImage GetUIImageForBarItem (string glyphName, int sizeUnit=20, float scale=0f)
		{
			if (scale == 0f)
				scale = UIScreen.MainScreen.Scale;

			CGImage cgImg = GetImageForBarItem (glyphName, sizeUnit, scale);
			return new UIImage (cgImg, scale, UIImageOrientation.Up);
		}

		static bool isLoading;
		static bool isLoaded;
		static void LoadFont ()
		{
			if (isLoading)
				return;
			isLoaded = false;
			isLoading = true;
			try
			{
				string path = PathToLoadFont;
				if (String.IsNullOrEmpty (path))
				{
					path = FindFontFilePath (NSBundle.MainBundle.BundlePath);
					if (String.IsNullOrEmpty (path))
						return;
				}

				cachedFont = null;
				LoadFontFrom (path);
				if (cachedFont != null)
				{
					isLoaded = true;
				}
			}
			finally
			{
				isLoading = false;
			}
		}

		static void LoadFontFrom (string path)
		{
			try
			{
				CGDataProvider provider = new CGDataProvider (path);
				CGFont font = CGFont.CreateFromProvider (provider);

				if (font.NumberOfGlyphs > 0)
				{
#if USE_WEAKREFERENCE
					cachedFont = new WeakHandle<CGFont> (font);
#else
					cachedFont = font;
#endif
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine ("Failed to load font : " + ex);
				throw ex;
			}
		}

		static string FindFontFilePath (string path)
		{
			return FindFilePathRecursively (path, (filePath) => {
				return StringComparer.InvariantCultureIgnoreCase.Compare (
					Path.GetFileName (filePath), TtfFileName) == 0;
			});
		}

		static string FindFilePathRecursively (string path, Predicate<string> pred)
		{
			string foundPath = (from filePath in Directory.EnumerateFiles (path)
								where pred (filePath)
			               		select filePath)
							.FirstOrDefault ();
			if (foundPath != null)
				return Path.Combine (path, foundPath);

			foreach (var dir in Directory.EnumerateDirectories (path))
			{
				string found = FindFilePathRecursively (Path.Combine (path, dir), pred);
				if (found != null)
					return found;
			}

			return null;
		}
	}
}
