//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sce.Atf.Wpf.Controls;
using System.ComponentModel;
using Microsoft.Win32;
using System.Windows;

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// User actions in FindFile Dialogs</summary>
    public enum FindFileAction
    {
        /// <summary>
        /// Accept suggestion for this file</summary>
        [DisplayString("Accept suggestion")]
        [Description("Accept suggestion for this file.")]
        AcceptSuggestion,

        /// <summary>
        /// Accept suggestion for all files</summary>
        [DisplayString("Accept all suggestion")]
        [Description("Accept suggestion for all files.")]
        AcceptAllSuggestions,

        /// <summary>
        /// Search for file in a user-specified directory and its sub-directories</summary>
        [DisplayString("Search directory")]
        [Description("Search for file in a specified directory and its sub-directories.")]
        SearchDirectory,

        /// <summary>
        /// Search a user-specified directory and its sub-directories for all missing files.</summary>
        [DisplayString("Search directory for all")]
        [Description("Search a specified directory and its sub-directories for all missing files.")]
        SearchDirectoryForAll,

        /// <summary>
        /// Let the user find file</summary>
        [DisplayString("Specify a new location")]
        [Description("Manually specify a new location for the file.")]
        UserSpecify,

        /// <summary>
        /// Ignore this missing file (don't try to find it)</summary>
        [DisplayString("Ignore")]
        [Description("Ignore this missing file (don't try to find it).")]
        Ignore,

        /// <summary>
        /// Ignore all missing files (don't try to find any)</summary>
        [DisplayString("Ignore all")]
        [Description("Ignore all missing files (don't prompt user again).")]
        IgnoreAll,

        /// <summary>
        /// Ignore all missing files (don't try to find any)</summary>
        [DisplayString("Quit")]
        [Description("Quit searching for remaining files.")]
        Quit
    }


    /// <summary>
    /// Options used by the FindFileResolver when prompting the user to select
    /// a replacement file </summary>
    public enum SelectFileFilterOptions
    {
        /// <summary>
        /// Any file can be selected </summary>
        Any,

        /// <summary>
        /// Only a file witht he same name and extension as the original can be selected </summary>
        ExactMatch,

        /// <summary>
        /// Any file with a matching extension to the original can be selected </summary>
        ExtensionMatch
    }

    /// <summary>
    /// XmlUrlResolver that attempts to repair broken file system URIs with user assistance</summary>
    public class FindFileResolver : XmlUrlResolver
    {
        /// <summary>
        /// Maps a URI to an object containing the actual resource</summary>
        /// <param name="absoluteUri">The URI returned from System.Xml.XmlResolver.ResolveUri(System.Uri,System.String)</param>
        /// <param name="role">The current implementation does not use this parameter when resolving URIs</param>
        /// <param name="returnType">The type of object to return. The current implementation only returns System.IO.Stream objects</param>
        /// <returns>A System.IO.Stream object or null if a type other than stream is specified</returns>
        public override object GetEntity(Uri absoluteUri, string role, Type returnType)
        {
            // if it's a file URI that can't be found, get user help
            if (absoluteUri != null &&
                absoluteUri.IsFile &&
                !File.Exists(absoluteUri.LocalPath))
            {
                Uri newUri;
                if (Find(absoluteUri, out newUri, SelectFileFilterOptions.Any) == true)
                {
                    absoluteUri = newUri;
                }
                else
                {
                    return null;
                }
            }

            return base.GetEntity(absoluteUri, role, returnType);
        }

        /// <summary>
        /// Gets and sets whether the user interface can ever be displayed for any instance
        /// of any FindFileResolver. If this is always 'false' then the FindFileResolvers are
        /// completely useless.</summary>
        public static bool UIEnabled
        {
            get { return s_lastAction == FindFileAction.IgnoreAll; }
            set { s_lastAction = value ? FindFileAction.IgnoreAll : FindFileAction.UserSpecify; }
        }

        /// <summary>
        /// Attempts to find the file specified by the given uri. A new replacement uri may be
        /// created, based on a user's decisions.</summary>
        /// <param name="uri">The uri to find, which may or may not be valid. It must not be null.</param>
        /// <param name="newUri">Will either be 'uri' or a new valid uri. It will not be set to null.</param>
        /// <returns>true if 'uri' exists or if a new uri was found. Otherwise, false.</returns>
        public static bool? Find(Uri uri, out Uri newUri, SelectFileFilterOptions options)
        {
            bool? result = false;
            lock (s_lockObject)
            {
                // Default new uri.
                newUri = uri;

                // Check for an exact match that the user has already made. This is very fast, so might
                //  as well do this first.
                Uri mappedUri;
                if (s_localPathMap.TryGetValue(uri.LocalPath, out mappedUri))
                {
                    newUri = mappedUri;
                    return true;
                }

                // Check if 'uri' is valid as-is.
                if (File.Exists(uri.LocalPath))
                    return true;

                // Then check if we can automatically correct the uri based on directory mapping.
                Uri suggestedUri = FindSuggestion(uri);
                bool accept = s_lastAction == FindFileAction.AcceptAllSuggestions
                              || s_lastAction == FindFileAction.SearchDirectoryForAll
                              || s_lastAction == FindFileAction.IgnoreAll;
                if (suggestedUri != null && accept)
                {
                    newUri = suggestedUri;
                    return true;
                }

                // Check if we can automatically do a file search.
                if (s_lastAction == FindFileAction.SearchDirectoryForAll ||
                    s_lastAction == FindFileAction.IgnoreAll)
                {
                    bool fileFound = false;
                    Uri foundUri = null;

                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        fileFound = SearchForFile(uri, out foundUri, false);
                    }));

                    if (fileFound)
                    {
                        newUri = foundUri;
                        return true;
                    }
                }

                if (s_lastAction != FindFileAction.IgnoreAll)
                {
                    Uri tempUri = null;
                    
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        result = QueryUser(uri, suggestedUri, out tempUri, options);
                    }));
                    
                    newUri = tempUri;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Resets user choices, cached data, etc. Useful for when loading a new document, for example.</summary>
        public static void Reset()
        {
            s_lastAction = FindFileAction.UserSpecify;
            s_searchRoot = string.Empty;
            s_localPathMap.Clear();
        }

        // Implements interactive portions of Find(), but is called from a separate thread to
        //  avoid recursion due to the dialog boxes.
        private static bool? QueryUser(Uri uri, Uri suggestedUri, out Uri newUri, SelectFileFilterOptions options)
        {
            // If the user cancels a sub-dialog box, reopen the first dialog box.
            while (true)
            {
                // Ask the user what we should do.
                // There are a two fewer options and slightly reorganized dialog box if there
                //  is no suggested replacement for the missing file.
                var vm = new FindFileDialogViewModel()
                    {
                        Action = s_lastAction,
                        OriginalPath = uri.LocalPath,
                        SuggestedPath = suggestedUri != null ? suggestedUri.LocalPath : null,
                    };

                bool? res = DialogUtils.ShowDialogWithViewModel<FindFileDialog>(vm);

                s_lastAction = res == false ? FindFileAction.Quit : vm.Action;

                switch (s_lastAction)
                {
                    case FindFileAction.AcceptSuggestion:
                        newUri = suggestedUri;
                        return true;

                    case FindFileAction.AcceptAllSuggestions:
                        newUri = suggestedUri;
                        return true;

                    case FindFileAction.SearchDirectory:
                        if (SearchForFile(uri, out newUri, true))
                            return true;
                        continue;

                    case FindFileAction.SearchDirectoryForAll:
                        if (SearchForFile(uri, out newUri, false))
                            return true;
                        continue;

                    case FindFileAction.UserSpecify:
                        if (UserFindFile(uri, out newUri, options))
                            return true;
                        continue;

                    case FindFileAction.Ignore:
                        newUri = uri;
                        return false;

                    case FindFileAction.IgnoreAll:
                        newUri = uri;
                        return false;
                    
                    case FindFileAction.Quit:
                        newUri = uri;
                        return null;
                }

                throw new InvalidOperationException("unhandled FindFileAction enum");
            }
        }

        /// <summary>
        /// Searches all the files, starting from a user-selected directory, for the filename
        /// specified by 'uri'</summary>
        /// <param name="uri"></param>
        /// <param name="newUri">either 'uri' or a replacement URI</param>
        /// <param name="askUser">asks the user for the search directory. If false, the user
        /// will only be asked the first time.</param>
        /// <returns>true if a replacement was chosen, otherwise false</returns>
        private static bool SearchForFile(Uri uri, out Uri newUri, bool askUser)
        {
            // default return value
            newUri = uri;

            // ask the user for a directory to search in or use last search
            bool doSearch = true;
            if (askUser || s_searchRoot == string.Empty)
            {
                var dlg = new BrowseForFolderDialog
                              {
                                  InitialFolder = 
                                    s_searchRoot != string.Empty 
                                        ? s_searchRoot 
                                        : Directory.GetCurrentDirectory()
                              };

                if (dlg.ShowDialog(Application.Current.MainWindow) == true)
                    s_searchRoot = dlg.SelectedFolder;
                else
                    doSearch = false;
            }

            // do the search and if successful, add the result to our mapping
            if (doSearch)
            {
                string searchName = Path.GetFileName(uri.LocalPath);
                foreach (string newPath in DirectoryUtil.GetFilesIteratively(s_searchRoot, searchName))
                {
                    // The search can be stopped after the first match.
                    newUri = new Uri(newPath);
                    s_localPathMap.Add(uri.LocalPath, newUri);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Given a URI that can't be found, this method will search directories that the user has
        /// chosen previously.</summary>
        /// <param name="uri"></param>
        /// <returns>a suggested replacement uri of a file with the same name (but in a different
        /// location) or null if no replacement file was found.</returns>
        private static Uri FindSuggestion(Uri uri)
        {
            string findDir = Path.GetDirectoryName(uri.LocalPath);
            string findFile = Path.GetFileName(uri.LocalPath);

            foreach (var pair in s_localPathMap)
            {
                // look for moving of a directory hierarchy
                string sourceDir = Path.GetDirectoryName(pair.Key);
                string destDir = Path.GetDirectoryName(pair.Value.LocalPath);
                string relativePath = PathUtil.GetRelativePath(findDir, sourceDir);
                string testDir = PathUtil.GetAbsolutePath(relativePath, destDir);
                string suggestion = Path.Combine(testDir, findFile);
                if (File.Exists(suggestion))
                {
                    Uri newUri = new Uri(suggestion);
                    s_localPathMap.Add(uri.LocalPath, newUri);
                    return newUri;
                }

                // look for flattening of a directory hierarchy
                suggestion = Path.Combine(destDir, findFile);
                if (File.Exists(suggestion))
                {
                    Uri newUri = new Uri(suggestion);
                    s_localPathMap.Add(uri.LocalPath, newUri);
                    return newUri;
                }
            }

            return null;
        }

        private static bool UserFindFile(Uri uri, out Uri newUri, SelectFileFilterOptions options)
        {
            newUri = uri;

            var dlg = new OpenFileDialog
                          {
                              CheckFileExists = true,
                              CheckPathExists = true,
                              Multiselect = false,
                              Title = string.Format(
                                  "Find or Replace {0}",
                                  Path.GetFileName(uri.LocalPath))
                          };

            if (options == SelectFileFilterOptions.ExactMatch)
            {
                string fileName = Path.GetFileName(uri.LocalPath);
                dlg.Filter = fileName + "|" + fileName;
            }
            else if (options == SelectFileFilterOptions.ExtensionMatch)
            {
                string extension = Path.GetExtension(uri.LocalPath);
                dlg.Filter = extension + "|" + extension;
            }

            if (dlg.ShowDialog(DialogUtils.GetActiveWindow()) == true)
            {
                newUri = new Uri(dlg.FileName);
                s_localPathMap.Add(uri.LocalPath, newUri);
                return true;
            }

            return false;
        }

        // The following are to solve a problem with 1) reentrancy (main thread responding to a Paint event,
        //  for example, and 2) multiple threads (the Asset Lister creating thumbnails, for example).
        private static readonly object s_lockObject = new object();

        private static string s_searchRoot = string.Empty;
        private static FindFileAction s_lastAction = FindFileAction.UserSpecify;

        //maps Uri.LocalPath from original location to new Uri
        private static readonly Dictionary<string, Uri> s_localPathMap = new Dictionary<string, Uri>();
    }
}
