//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf.Controls;

namespace Sce.Atf
{
    /// <summary>
    /// XmlUrlResolver that attempts to repair broken file system URIs with user assistance</summary>
    public class FindFileResolver : XmlUrlResolver
    {
        /// <summary>
        /// Maps a URI to an object containing the actual resource</summary>
        /// <param name="absoluteUri">The URI returned from System.Xml.XmlResolver.ResolveUri(System.Uri,System.String)</param>
        /// <param name="role">The current implementation does not use this parameter when resolving URIs</param>
        /// <param name="returnType">The type of object to return. The current implementation only returns System.IO.Stream objects</param>
        /// <returns>System.IO.Stream object or null if a type other than stream is specified</returns>
        public override object GetEntity(Uri absoluteUri, string role, Type returnType)
        {
            // if it's a file URI that can't be found, get user help
            if (absoluteUri != null &&
                absoluteUri.IsFile &&
                !File.Exists(absoluteUri.LocalPath))
            {
                Uri newUri;
                if (Find(absoluteUri, out newUri))
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
        /// of any FindFileResolver. If this is always 'false', the FindFileResolvers are
        /// completely useless.</summary>
        public static bool UIEnabled
        {
            get { return !s_ignoreAll; }
            set { s_ignoreAll = !value; }
        }

        /// <summary>
        /// Attempts to find the file specified by the given URI. A new replacement URI may be
        /// created, based on a user's decisions.</summary>
        /// <param name="uri">The URI to find, which may or may not be valid. It must not be null.</param>
        /// <param name="newUri">Is either be 'uri' or a new valid URI. It is not set to null.</param>
        /// <returns><c>True</c> if 'uri' exists or if a new URI was found. Otherwise, false.</returns>
        public static bool Find(Uri uri, out Uri newUri)
        {
            bool result = false;
            lock (s_lockObject)
            {
                // Default new uri.
                newUri = uri;

                // Reentrancy check. This can happen because the main thread does a Paint() while
                //  blocking on a Join(), because neither Join() nor Monitor.Wait() stop message pumping.
                //  So, let's let the dialog box finish before continuing so that s_localPathMap is up-to-date.
                if (s_finderThread != null)
                    s_finderThread.Join();

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

                // Has the user chosen to ignore all missing files?
                if (s_ignoreAll)
                    return false;

                // Then check if we can automatically correct the uri based on directory mapping.
                Uri suggestedUri = FindSuggestion(uri);
                if (suggestedUri != null && (s_acceptAll || s_searchAll))
                {
                    newUri = suggestedUri;
                    return true;
                }

                // Check if we can automatically do a file search.
                if (s_searchAll)
                {
                    if (SearchForFile(uri, out newUri, false))
                        return true;
                }

                try
                {
                    // Create a separate thread and then join (i.e., wait) on that thread. This solves
                    //  a problem of opening a dialog box and causing an OnPaint call which eventually
                    //  results in the same thread reentering this critical section.
                    Uri newUriFromThread = uri;
                    s_finderThread = new Thread(delegate()
                    {
                        result = QueryUser(uri, suggestedUri, out newUriFromThread);
                    });
                    s_finderThread.Name = "FindFileService";
                    s_finderThread.IsBackground = true; //so that the thread can be killed if app dies.
                    s_finderThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                    s_finderThread.SetApartmentState(ApartmentState.STA);//so that OLE dialog boxes can be opened
                    s_finderThread.Start();
                    s_finderThread.Join();

                    newUri = newUriFromThread;
                }
                finally
                {
                    s_finderThread = null;
                }
            }
            return result;
        }

        /// <summary>
        /// Resets user choices, cached data, etc. Useful for when loading a new document, for example.</summary>
        public static void Reset()
        {
            s_ignoreAll = false;
            s_acceptAll = false;
            s_searchAll = false;
            s_searchRoot = string.Empty;
            s_localPathMap.Clear();
        }

        // Implements interactive portions of Find(), but is called from a separate thread to
        //  avoid recursion due to the dialog boxes.
        private static bool QueryUser(Uri uri, Uri suggestedUri, out Uri newUri)
        {
            FindFileAction userAction;

            // If the user cancels a sub-dialog box, reopen the first dialog box.
            while (true)
            {
                // Ask the user what we should do. There are two possible dialog boxes to use.
                if (suggestedUri == null)
                {
                    // There are a two fewer options and slightly reorganized dialog box if there
                    //  is no suggested replacement for the missing file.
                    FindFileDialog dialog = new FindFileDialog(uri.LocalPath);
                    if (dialog.ShowDialog() == DialogResult.Cancel)
                        userAction = FindFileAction.Ignore;
                    else
                        userAction = dialog.Action;
                }
                else
                {
                    // We have a suggested replacement already, so allow the user to accept the
                    //  suggestion.
                    FindFileWithSuggestionDialog dialog =
                        new FindFileWithSuggestionDialog(uri.LocalPath, suggestedUri.LocalPath);
                    if (dialog.ShowDialog() == DialogResult.Cancel)
                        userAction = FindFileAction.Ignore;
                    else
                        userAction = dialog.Action;
                }

                switch (userAction)
                {
                    case FindFileAction.AcceptSuggestion:
                        newUri = suggestedUri;
                        return true;

                    case FindFileAction.AcceptAllSuggestions:
                        s_acceptAll = true;
                        newUri = suggestedUri;
                        return true;

                    case FindFileAction.SearchDirectory:
                        if (SearchForFile(uri, out newUri, true))
                            return true;
                        continue;

                    case FindFileAction.SearchDirectoryForAll:
                        s_searchAll = true;
                        if (SearchForFile(uri, out newUri, false))
                            return true;
                        continue;

                    case FindFileAction.UserSpecify:
                        if (UserFindFile(uri, out newUri))
                            return true;
                        continue;

                    case FindFileAction.Ignore:
                        newUri = uri;
                        return false;

                    case FindFileAction.IgnoreAll:
                        s_ignoreAll = true;
                        newUri = uri;
                        return false;
                }

                throw new InvalidOperationException("unhandled FindFileAction enum");
            }
        }

        /// <summary>
        /// Searches all the files, starting from a user selected directory, for the filename
        /// specified by 'uri'</summary>
        /// <param name="uri">Filename searched for</param>
        /// <param name="newUri">Either 'uri' or a replacement URI</param>
        /// <param name="askUser">Asks the user for the search directory. If false, the user
        /// is only asked the first time.</param>
        /// <returns><c>True</c> if a replacement was chosen</returns>
        private static bool SearchForFile(Uri uri, out Uri newUri, bool askUser)
        {
            // default return value
            newUri = uri;

            // ask the user for a directory to search in or use last search
            bool doSearch = true;
            if (askUser || s_searchRoot == string.Empty)
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                if (s_searchRoot != string.Empty)
                    dlg.SelectedPath = s_searchRoot;
                else
                    dlg.SelectedPath = Directory.GetCurrentDirectory();
                DialogResult result = dlg.ShowDialog();
                if (result == DialogResult.OK)
                    s_searchRoot = dlg.SelectedPath;
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
        /// Given a URI that can't be found, this method searches directories that the user has
        /// chosen previously</summary>
        /// <param name="uri">URI searched for</param>
        /// <returns>Suggested replacement URI of a file with the same name (but in a different
        /// location) or null if no replacement file was found</returns>
        private static Uri FindSuggestion(Uri uri)
        {
            string findDir = Path.GetDirectoryName(uri.LocalPath);
            string findFile = Path.GetFileName(uri.LocalPath);

            foreach (KeyValuePair<string, Uri> pair in s_localPathMap)
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

        private static bool UserFindFile(Uri uri, out Uri newUri)
        {
            newUri = uri;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = false;
            dlg.Title = string.Format(
                "Find or Replace {0}",
                Path.GetFileName(uri.LocalPath));
            DialogResult result = dlg.ShowDialog();

            if (result == DialogResult.OK)
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
        private static Thread s_finderThread;

        private static bool s_ignoreAll;
        private static bool s_acceptAll;
        private static bool s_searchAll;
        private static string s_searchRoot = string.Empty;

        //maps Uri.LocalPath from original location to new Uri
        private static readonly Dictionary<string, Uri> s_localPathMap = new Dictionary<string, Uri>();
    }
}
