//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Extension methods for ItemInfo instances</summary>
    public static class ItemInfos
    {
        /// <summary>
        /// Obtains the ImageList associated with the ItemInfo instance</summary>
        /// <param name="info">ItemInfo, must be WinFormsItemInfo</param>
        /// <returns>ImageList associated with the ItemInfo instance</returns>
        public static ImageList GetImageList(this ItemInfo info)
        {
            return GetWinFormsItemInfo(info).ImageList;
        }

        /// <summary>
        /// Obtains the ImageList for an ItemInfo instance and looks up the index for an image by name</summary>
        /// <param name="info">ItemInfo, must be WinFormsItemInfo</param>
        /// <param name="name">Name of the image; use string constants declared in Resources classes</param>
        /// <returns>Index of the image in the ImageList if successful, -1 otherwise</returns>
        public static int GetImageIndex(this ItemInfo info, string name)
        {
            int index = -1;
            ImageList imageList = GetImageList(info);
            if (imageList != null)
                index = imageList.Images.IndexOfKey(name);
            return index;
        }

        /// <summary>
        /// Gets the StateImageList associated with the ItemInfo instance</summary>
        /// <param name="info">ItemInfo, must be WinFormsItemInfo</param>
        /// <returns>StateImageList associated with the ItemInfo instance</returns>
        public static ImageList GetStateImageList(this ItemInfo info)
        {
            return GetWinFormsItemInfo(info).StateImageList;
        }

        /// <summary>
        /// Obtains the StateImageList for an ItemInfo instance and looks up the index for an image by name</summary>
        /// <param name="info">ItemInfo, must be WinFormsItemInfo</param>
        /// <param name="name">Name of the image; use string constants declared in Resources classes</param>
        /// <returns>Index of the image in the StateImageList if successful, -1 otherwise</returns>
        public static int GetStateImageIndex(this ItemInfo info, string name)
        {
            int index = -1;
            ImageList imageList = GetImageList(info);
            if (imageList != null)
                index = imageList.Images.IndexOfKey(name);
            return index;
        }

        /// <summary>
        /// Gets the CheckState from an ItemInfo instance</summary>
        /// <param name="info">ItemInfo, must be WinFormsItemInfo</param>
        /// <returns>CheckState</returns>
        public static CheckState GetCheckState(this ItemInfo info)
        {
            return GetWinFormsItemInfo(info).CheckState;
        }

        /// <summary>
        /// Sets the CheckState on an ItemInfo instance</summary>
        /// <param name="info">ItemInfo, must be WinFormsItemInfo</param>
        /// <param name="checkState">New CheckState value</param>
        public static void SetCheckState(this ItemInfo info, CheckState checkState)
        {
            GetWinFormsItemInfo(info).CheckState = checkState;
        }

        private static WinFormsItemInfo GetWinFormsItemInfo(ItemInfo itemInfo)
        {
            var WinFormsItemInfo = (WinFormsItemInfo)itemInfo;
            if (WinFormsItemInfo == null)
                throw new InvalidOperationException("ItemInfo is not an instance of, or derived from, WinFormsItemInfo");

            return WinFormsItemInfo;
        }
    }
}