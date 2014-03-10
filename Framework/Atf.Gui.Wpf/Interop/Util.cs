//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sce.Atf.Wpf.Interop
{
    /// <summary>
    /// Utilities for obtaining WPF information</summary>
    public static class Util
    {
        /// <summary>
        /// Attempts to find a WPF image resource matching embeddedImage.
        /// If none exists, creates a new image resource and adds it to application resources
        /// keyed by original embedded image, effectively creating a lookup map from an
        /// embedded image to its WPF image source counterpart in application resources.</summary>
        /// <param name="embeddedImage">Embedded image</param>
        /// <returns>WPF imagesource created or found</returns>
        public static ImageSource GetOrCreateResourceForEmbeddedImage(System.Drawing.Image embeddedImage)
        {
            var resource = Application.Current.TryFindResource(embeddedImage);
            if (resource == null)
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                var ms = new MemoryStream();
                embeddedImage.Save(ms, ImageFormat.Png); // TODO: more formats
                ms.Seek(0, SeekOrigin.Begin);
                bi.StreamSource = ms;
                bi.EndInit();
                Application.Current.Resources.Add(embeddedImage, bi);
                return bi;
            }

            var image = resource as ImageSource;
            if (image == null)
                throw new InvalidOperationException("Image resource key already exists but value is not of expected type");

            return image;
        }

        /// <summary>
        /// Converts an ATF key code that invokes a command to the WPF keyboard combination used to invoke that command</summary>
        /// <param name="atfKey">ATF key code</param>
        /// <returns>WPF keyboard combination to invoke the command</returns>
        public static KeyGesture ConvertKey(Sce.Atf.Input.Keys atfKey)
        {
            return ConvertKey(KeysInterop.ToWf(atfKey));
        }

        /// <summary>
        /// Converts an WinForms key code that invokes a command to the WPF keyboard combination used to invoke that command</summary>
        /// <param name="formsKey">WinForms key code</param>
        /// <returns>WPF keyboard combination to invoke the command</returns>
        public static KeyGesture ConvertKey(System.Windows.Forms.Keys formsKey)
        {
            var key = formsKey & System.Windows.Forms.Keys.KeyCode;
            Key wpfKey = KeyInterop.KeyFromVirtualKey((int)key);

            ModifierKeys wpfMods = ModifierKeys.None;
            if ((formsKey & System.Windows.Forms.Keys.Shift) > 0)
                wpfMods |= ModifierKeys.Shift;
            if ((formsKey & System.Windows.Forms.Keys.Control) > 0)
                wpfMods |= ModifierKeys.Control;
            if ((formsKey & System.Windows.Forms.Keys.Alt) > 0)
                wpfMods |= ModifierKeys.Alt;

            return new KeyGesture(wpfKey, wpfMods);
        }
    }
}
