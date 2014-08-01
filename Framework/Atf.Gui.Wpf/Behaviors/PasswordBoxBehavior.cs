//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Workaround to allow data binding to PasswordBox's Password property. Source code from:
    /// http://blog.functionalfun.net/2008/06/wpf-passwordbox-and-data-binding.html </summary>
    public static class PasswordBoxBehavior
    {
        /// <summary>
        /// Dependency property for the data bound password</summary>
        public static readonly DependencyProperty BoundPassword =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(SecureString), typeof(PasswordBoxBehavior),
                                                new PropertyMetadata(null, OnBoundPasswordChanged));

        /// <summary>
        /// Dependency property to enable data binding of the password</summary>
        public static readonly DependencyProperty BindPassword = DependencyProperty.RegisterAttached(
            "BindPassword", typeof(bool), typeof(PasswordBoxBehavior),
            new PropertyMetadata(false, OnBindPasswordChanged));

        private static readonly DependencyProperty UpdatingPassword =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxBehavior),
                                                new PropertyMetadata(false));

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox box = d as PasswordBox;

            // only handle this event when the property is attached to a PasswordBox
            // and when the BindPassword attached property has been set to true
            if (d == null || !GetBindPassword(d))
            {
                return;
            }

            // avoid recursive updating by ignoring the box's changed event
            box.PasswordChanged -= HandlePasswordChanged;

            SecureString newPassword = (SecureString)e.NewValue;

            if (!GetUpdatingPassword(box))
            {
                // PasswordBox.SecurePassword is read-only, so we need to set the value via the unencrypted 
                // PasswordBox.Password. Converting to an unencrypted string is a bit of a hack, but this allows
                // the data bound property BoundPassword to be of type SecureString so that it can be stored
                // securely in any code that uses this.
                box.Password = ConvertToUnsecureString(newPassword);
            }

            box.PasswordChanged += HandlePasswordChanged;
        }

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            // when the BindPassword attached property is set on a PasswordBox,
            // start listening to its PasswordChanged event

            PasswordBox box = dp as PasswordBox;

            if (box == null)
            {
                return;
            }

            bool wasBound = (bool) (e.OldValue);
            bool needToBind = (bool) (e.NewValue);

            if (wasBound)
            {
                box.PasswordChanged -= HandlePasswordChanged;
            }

            if (needToBind)
            {
                box.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox box = sender as PasswordBox;

            // set a flag to indicate that we're updating the password
            SetUpdatingPassword(box, true);
            // push the new password into the BoundPassword property
            SetBoundPassword(box, box.SecurePassword);
            SetUpdatingPassword(box, false);
        }

        /// <summary>
        /// Sets whether to enable password data binding</summary>
        /// <param name="dp">Dependency object to use for setting the value</param>
        /// <param name="value">True to enable, false to disable</param>
        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(BindPassword, value);
        }

        /// <summary>
        /// Gets whether password data binding is enabled</summary>
        /// <param name="dp">Dependency object to query for the value</param>
        /// <returns>True if enabled, false if disabled</returns>
        public static bool GetBindPassword(DependencyObject dp)
        {
            return (bool) dp.GetValue(BindPassword);
        }

        /// <summary>
        /// Gets the value of the data bound password</summary>
        /// <param name="dp">Dependency object to query for the value</param>
        /// <returns>The current value of the password</returns>
        public static SecureString GetBoundPassword(DependencyObject dp)
        {
            return (SecureString)dp.GetValue(BoundPassword);
        }

        /// <summary>
        /// Sets the value of the data bound password</summary>
        /// <param name="dp">Dependency object to use for setting the value</param>
        /// <param name="value">Password to set</param>
        public static void SetBoundPassword(DependencyObject dp, SecureString value)
        {
            dp.SetValue(BoundPassword, value);
        }

        private static bool GetUpdatingPassword(DependencyObject dp)
        {
            return (bool) dp.GetValue(UpdatingPassword);
        }

        private static void SetUpdatingPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(UpdatingPassword, value);
        }

        private static string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
                return null;

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
