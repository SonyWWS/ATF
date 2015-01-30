//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Linq;

namespace Sce.Atf
{
    /// <summary>
    /// Standard embedded resources</summary>
    public static class Resources
    {
        #region Standard Command Images

        /// <summary>
        /// Computer icon name</summary>
        [ImageResource("Computer.ico")]
        public static readonly string ComputerImage;

        /// <summary>
        /// Disk drive icon name</summary>
        [ImageResource("diskdrive16.png", "diskdrive24.png", "diskdrive32.png")]
        public static readonly string DiskDriveImage;

        /// <summary>
        /// File pin image</summary>
        [ImageResource("pin_green16.png", "pin_green24.png", "pin_green32.png")]
        public static readonly string PinGreenImage;

        /// <summary>
        /// File unpinned image</summary>
        [ImageResource("pin_grey16.png", "pin_grey24.png", "pin_grey32.png")]
        public static readonly string PinGreyImage;

        /// <summary>
        /// Folder icon name</summary>
        [ImageResource("Folder.ico")]
        public static readonly string FolderIcon;

        /// <summary>
        ///  Folder icon name</summary>
        [ImageResource("Folder16.png", "Folder24.png", "Folder32.png")]
        public static readonly string FolderImage;

        /// <summary>
        /// Open Folder reference icon name</summary>
        [ImageResource("folderOpenReference16.png")]
        public static readonly string ReferenceFolderOpen;

        /// <summary>
        /// Closed Folder reference icon name</summary>
        [ImageResource("folderClosedReference16.png")]
        public static readonly string ReferenceFolderClosed;


        /// <summary>
        /// Document icon name</summary>
        [ImageResource("Document16.png", "Document24.png", "Document32.png")]
        public static readonly string DocumentImage;

        /// <summary>
        /// Printer icon name</summary>
        [ImageResource("printer16.png", "printer24.png", "printer32.png")]
        public static readonly string PrinterImage;

        /// <summary>
        /// Printer preferences icon name</summary>
        [ImageResource("printer_preferences16.png", "printer_preferences24.png", "printer_preferences32.png")]
        public static readonly string PrinterPreferencesImage;

        /// <summary>
        /// Printer view icon name</summary>
        [ImageResource("printer_view16.png", "printer_view24.png", "printer_view32.png")]
        public static readonly string PrinterViewImage;

        /// <summary>
        /// Preferences icon name</summary>
        [ImageResource("Preferences16.png", "Preferences24.png", "Preferences32.png")]
        public static readonly string PreferencesImage;

        /// <summary>
        /// Help icon name</summary>
        [ImageResource("Help16.png", "Help24.png", "Help32.png")]
        public static readonly string HelpImage;

        /// <summary>
        /// "Data" icon name</summary>
        [ImageResource("Data16.png", "Data24.png", "Data32.png")]
        public static readonly string DataImage;

        /// <summary>
        /// "Select" icon name</summary>
        [ImageResource("Selection16.png", "Selection24.png", "Selection32.png")]
        public static readonly string SelectionImage;

        /// <summary>
        /// "Save" icon name</summary>
        [ImageResource("Save16.png", "Save24.png", "Save32.png")]
        public static readonly string SaveImage;

        /// <summary>
        /// "Save As" icon name</summary>
        [ImageResource("SaveAs16.png", "SaveAs24.png", "SaveAs32.png")]
        public static readonly string SaveAsImage;

        /// <summary>
        /// "Save All" icon name</summary>
        [ImageResource("SaveAll16.png", "SaveAll24.png", "SaveAll32.png")]
        public static readonly string SaveAllImage;

        /// <summary>
        /// "Undo" icon name</summary>
        [ImageResource("Undo16.png", "Undo24.png", "Undo32.png")]
        public static readonly string UndoImage;

        /// <summary>
        /// "Redo" icon name</summary>
        [ImageResource("Redo16.png", "Redo24.png", "Redo32.png")]
        public static readonly string RedoImage;

        /// <summary>
        /// "Copy" icon name</summary>
        [ImageResource("Copy16.png", "Copy24.png", "Copy32.png")]
        public static readonly string CopyImage;

        /// <summary>
        /// "Cut" icon name</summary>
        [ImageResource("Cut16.png", "Cut24.png", "Cut32.png")]
        public static readonly string CutImage;

        /// <summary>
        /// "Paste" icon name</summary>
        [ImageResource("Paste16.png", "Paste24.png", "Paste32.png")]
        public static readonly string PasteImage;

        /// <summary>
        /// "Delete" icon name</summary>
        [ImageResource("Delete16.png", "Delete24.png", "Delete32.png")]
        public static readonly string DeleteImage;

        /// <summary>
        /// "Group" icon name</summary>
        [ImageResource("Group16.png", "Group24.png", "Group32.png")]
        public static readonly string GroupImage;

        /// <summary>
        /// "Ungroup" icon name</summary>
        [ImageResource("Ungroup16.png", "Ungroup24.png", "Ungroup32.png")]
        public static readonly string UngroupImage;

        /// <summary>
        /// "Lock" icon name</summary>
        [ImageResource("edit_locked16x16.png", "edit_locked24x24.png", "edit_locked32x32.png")]
        public static readonly string LockImage;

        /// <summary>
        /// "Unlock" icon name</summary>
        [ImageResource("edit_unlocked16x16.png", "edit_unlocked24x24.png", "edit_unlocked32x32.png")]
        public static readonly string UnlockImage;

        /// <summary>
        /// "Find" icon name</summary>
        [ImageResource("Find16.png", "Find24.png", "Find32.png")]
        public static readonly string FindImage;

        /// <summary>
        /// "Zoom In" icon name</summary>
        [ImageResource("ZoomIn16.png", "ZoomIn24.png", "ZoomIn32.png")]
        public static readonly string ZoomInImage;

        /// <summary>
        /// "Zoom Out" icon name</summary>
        [ImageResource("ZoomOut16.png", "ZoomOut24.png", "ZoomOut32.png")]
        public static readonly string ZoomOutImage;

        /// <summary>
        /// "Fit To Size" icon name</summary>
        [ImageResource("FitToSize16.png", "FitToSize24.png", "FitToSize32.png")]
        public static readonly string FitToSizeImage;

        /// <summary>
        /// "Align Lefts" icon name</summary>
        [ImageResource("AlignLefts16.png", "AlignLefts24.png", "AlignLefts32.png")]
        public static readonly string AlignLeftsImage;

        /// <summary>
        /// "Align Rights" icon name</summary>
        [ImageResource("AlignRights16.png", "AlignRights24.png", "AlignRights32.png")]
        public static readonly string AlignRightsImage;

        /// <summary>
        /// "Align Centers" icon name</summary>
        [ImageResource("AlignCenters16.png", "AlignCenters24.png", "AlignCenters32.png")]
        public static readonly string AlignCentersImage;

        /// <summary>
        /// "Align Tops" icon name</summary>
        [ImageResource("AlignTops16.png", "AlignTops24.png", "AlignTops32.png")]
        public static readonly string AlignTopsImage;

        /// <summary>
        /// "Align Bottoms" icon name</summary>
        [ImageResource("AlignBottoms16.png", "AlignBottoms24.png", "AlignBottoms32.png")]
        public static readonly string AlignBottomsImage;

        /// <summary>
        /// "Align Middles" icon name</summary>
        [ImageResource("AlignMiddles16.png", "AlignMiddles24.png", "AlignMiddles32.png")]
        public static readonly string AlignMiddlesImage;

        /// <summary>
        /// "Hide" icon name</summary>
        [ImageResource("HideSelection16.png", "HideSelection24.png", "HideSelection32.png")]
        public static readonly string HideImage;

        /// <summary>
        /// "Show" icon name</summary>
        [ImageResource("ShowSelection16.png", "ShowSelection24.png", "ShowSelection32.png")]
        public static readonly string ShowImage;

        /// <summary>
        /// "Show Last" icon name</summary>
        [ImageResource("ShowSelection16.png", "ShowSelection24.png", "ShowSelection32.png")]
        public static readonly string ShowLastImage;

        /// <summary>
        /// "Show All" icon name</summary>
        [ImageResource("ShowAll16.png", "ShowAll24.png", "ShowAll32.png")]
        public static readonly string ShowAllImage;

        /// <summary>
        /// "Isolate" icon name</summary>
        [ImageResource("Isolate16.png", "Isolate24.png", "Isolate32.png")]
        public static readonly string IsolateImage;

        /// <summary>
        /// "Search" icon name</summary>
        [ImageResource("search.ico")]
        public static readonly string SearchImage;

        /// <summary>
        /// "Filter" icon name</summary>
        [ImageResource("filter.ico")]
        public static readonly string FilterImage;

        /// <summary>
        /// "Lock User Interface" icon name</summary>
        [ImageResource("ui_locked16x16.png", "ui_locked24x24.png", "ui_locked32x32.png")]
        public static readonly string LockUIImage;

        /// <summary>
        /// "Unlock User Interface" icon name</summary>
        [ImageResource("ui_unlocked16x16.png", "ui_unlocked24x24.png", "ui_unlocked32x32.png")]
        public static readonly string UnlockUIImage;
        #endregion

        #region Misc Images

        /// <summary>
        /// Alphabetical icon name</summary>
        [ImageResource("Alphabetical.png")]
        public static readonly string AlphabeticalImage;

        /// <summary>
        /// By Category icon name</summary>
        [ImageResource("ByCategory.png")]
        public static readonly string ByCategoryImage;

        /// <summary>
        /// Navigate Left icon name</summary>
        [ImageResource("nav_left.png")]
        public static readonly string NavLeftImage;

        /// <summary>
        /// Navigate Right icon name</summary>
        [ImageResource("nav_right.png")]
        public static readonly string NavRightImage;

        /// <summary>
        /// SCEA icon name</summary>
        [ImageResource("SCEA.png")]
        public static readonly string SceaImage;

        /// <summary>
        /// ATF large logo name</summary>
        [ImageResource("AtfLogo.png")]
        public static readonly string AtfLogoImage;

        /// <summary>
        /// ATF icon name</summary>
        [ImageResource("AtfIcon.ico")]
        public static readonly string AtfIconImage;

        /// <summary>
        /// Unsorted icon name</summary>
        [ImageResource("Unsorted.png")]
        public static readonly string UnsortedImage;

        /// <summary>
        /// Factory image</summary>
        [ImageResource("factory.png")]
        public static readonly string FactoryImage;

        /// <summary>
        /// Package image</summary>
        [ImageResource("package.png")]
        public static readonly string PackageImage;

        /// <summary>
        /// Package image</summary>
        [ImageResource("package_error.png")]
        public static readonly string PackageErrorImage;


        /// <summary>
        /// Layer image</summary>
        [ImageResource("layer.png")]
        public static readonly string LayerImage;

        /// <summary>
        /// Add image</summary>
        [ImageResource("add.png")]
        public static readonly string AddImage;

        /// <summary>
        /// Remove image</summary>
        [ImageResource("delete.png")]
        public static readonly string RemoveImage;

        /// <summary>
        /// Arrow Up image</summary>
        [ImageResource("arrow_up_blue.png")]
        public static readonly string ArrowUpImage;

        /// <summary>
        /// Arrow Down image</summary>
        [ImageResource("arrow_down_blue.png")]
        public static readonly string ArrowDownImage;

        /// <summary>
        /// Sort Ascending image</summary>
        [ImageResource("sort_ascending.png")]
        public static readonly string SortAscendingImage;

        /// <summary>
        /// Sort Descending image</summary>
        [ImageResource("sort_descending.png")]
        public static readonly string SortDescendingImage;

        /// <summary>
        /// Checked Items image</summary>
        [ImageResource("checked_items.png")]
        public static readonly string CheckedItemsImage;

        /// <summary>
        /// component image</summary>
        [ImageResource("component16.png", "component24.png", "component32.png")]
        public static readonly string ComponentImage;

        /// <summary>
        /// components image</summary>
        [ImageResource("components.png")]
        public static readonly string ComponentsImage;

        /// <summary>
        /// Reset image</summary>
        [ImageResource("Reset16.png")]
        public static readonly string ResetImage;

        #endregion

        #region Indicator Images

        /// <summary>
        /// Green Plus Indicator image</summary>
        [ImageResource("greenPlus13.png")]
        public static readonly string GreenPlusIndicatorImage;

        /// <summary>
        /// Lock Indicator image</summary>
        [ImageResource("lock13.png")]
        public static readonly string LockIndicatorImage;

        /// <summary>
        /// Red Check Indicator image</summary>
        [ImageResource("redCheck13.png")]
        public static readonly string RedCheckIndicatorImage;

        /// <summary>
        /// Unknown Indicator image</summary>
        [ImageResource("unknown13.png")]
        public static readonly string UnknownIndicatorImage;

        #endregion

        #region Cursors

        /// <summary>
        /// Four-way sizing cursor name</summary>
        [CursorResource("4WAY05.CUR")]
        public static readonly string FourWayCursor;

        /// <summary>
        /// Horizontal sizing cursor name</summary>
        [CursorResource("HO_SPLIT.CUR")]
        public static readonly string HorizSizeCursor;

        /// <summary>
        /// Vertical sizing cursor name</summary>
        [CursorResource("VE_SPLIT.CUR")]
        public static readonly string VerticalSizeCursor;

        #endregion

        #region Version Control Images

        /// <summary>
        /// Document Add icon name</summary>
        [ImageResource("document_add.png")]
        public static readonly string DocumentAddImage;

        /// <summary>
        /// Document Check Out icon name</summary>
        [ImageResource("document_check.png")]
        public static readonly string DocumentCheckOutImage;

        /// <summary>
        /// Document Get Latest icon name</summary>
        [ImageResource("document_into.png")]
        public static readonly string DocumentGetLatestImage;

        /// <summary>
        /// Document Lock icon name</summary>
        [ImageResource("document_lock.png")]
        public static readonly string DocumentLockImage;

        /// <summary>
        /// Document Refresh icon name</summary>
        [ImageResource("document_refresh.png")]
        public static readonly string DocumentRefreshImage;

        /// <summary>
        /// Document Revert icon name</summary>
        [ImageResource("document_revert.png")]
        public static readonly string DocumentRevertImage;

        /// <summary>
        /// Document Warning icon name</summary>
        [ImageResource("document_warning.png")]
        public static readonly string DocumentWarningImage;

        /// <summary>
        /// Document Unknown icon name</summary>
        [ImageResource("document_unknown.png")]
        public static readonly string DocumentUnknownImage;

        /// <summary>
        /// Source Control Enable icon name</summary>
        [ImageResource("sourceControl_enable.png")]
        public static readonly string SourceControlEnableImage;
        /// <summary>
        /// Source Control Enable icon name</summary>
        [ImageResource("sourceControl_disable.png")]
        public static readonly string SourceControlDisableImage;

        /// <summary>
        /// Source Control Add icon name</summary>
        [ImageResource("sourceControl_add.png")]
        public static readonly string SourceControlAddImage;

        /// <summary>
        /// Source Control Check-in icon name</summary>
        [ImageResource("sourceControl_checkin.png")]
        public static readonly string SourceControlCheckInImage;

        /// <summary>
        /// Source Control Check-out icon name</summary>
        [ImageResource("sourceControl_checkout.png")]
        public static readonly string SourceControlCheckOutImage;

        /// <summary>
        /// Source Control Reconcile icon name</summary>
        [ImageResource("sourceControl_reconcile.png")]
        public static readonly string SourceControlReconcileImage;

        /// <summary>
        /// Source Control Connection icon name</summary>
        [ImageResource("sourceControl_connect.png")]
        public static readonly string SourceControlConnectionImage;

        /// <summary>
        /// Source Control Connection icon name</summary>
        [ImageResource("sourceControl_getLatest.png")]
        public static readonly string SourceControlGetLatestImage;

        /// <summary>
        /// Source Control Refresh icon name</summary>
        [ImageResource("sourceControl_refresh.png")]
        public static readonly string SourceControlRefreshImage;

        /// <summary>
        /// Source Control Revert icon name</summary>
        [ImageResource("sourceControl_revert.png")]
        public static readonly string SourceControlRevertImage;
        #endregion

        #region Resource Images

        /// <summary>
        /// Resource icon name</summary>
        [ImageResource("resource.png")]
        public static readonly string ResourceImage;

        /// <summary>
        /// Resource Folder icon name</summary>
        [ImageResource("resourceFolder.png")]
        public static readonly string ResourceFolderImage;

        #endregion

        #region Reference Images

        /// <summary>
        /// Reference icon name</summary>
        [ImageResource("reference.png")]
        public static readonly string ReferenceImage;

        /// <summary>
        /// Reference Null icon name</summary>
        [ImageResource("referenceNull.png")]
        public static readonly string ReferenceNullImage;

        /// <summary>
        /// Reference Override icon name</summary>
        [ImageResource("referenceOverride.png")]
        public static readonly string ReferenceOverrideImage;

        #endregion

        /// <summary>
        /// Constructor</summary>
        static Resources()
        {
            // GUI framework-specific registration is done in assemblies on which this library shouldn't depend.
            // Regardless, said assemblies still need to register the resources defined in here.
            //
            // Use reflection to:
            //   - locate the any registration classes (hopefully there's at least one)
            //   - determine whether they've begun registration (then nothing needs be done here)
            //   - if they haven't, call their registration method from here

            const string kRegistrationStarted = "RegistrationStarted";
            const string kRegister = "Register";

            // all types named ResourceUtil
            var types = (from a in AppDomain.CurrentDomain.GetAssemblies()
                        where !a.IsDynamic
                        from t in a.GetExportedTypes()
                        where t.Name == "ResourceUtil"
                        select t).ToArray();

            // the results of calling ResourceUtil.RegistrationStarted, for each type
            var regStarted =    from t in types 
                                from p in t.GetProperties()
                                where 
                                    p.Name == kRegistrationStarted &&
                                    p.PropertyType == typeof(bool) &&
                                    p.GetGetMethod().IsPublic &&
                                    p.GetGetMethod().IsStatic
                                select (bool)(p.GetGetMethod().Invoke(null, new object[] {}));

            // if registration has already begun in one of these, we don't need to initiate it
            if (regStarted.Any(p => p))
                return;

            // otherwise, get the registration methods
            var registerMethods =   (from t in types
                                    from m in t.GetMethods()
                                    where m.Name == kRegister &&
                                            m.IsStatic &&
                                            m.IsPublic &&
                                            m.ReturnType == typeof(void) &&
                                            m.GetParameters().Length == 1 &&
                                            m.GetParameters()[0].ParameterType == typeof(Type)
                                    select m).ToArray();

            // if there's more than one registration method available, we have no idea which should be used.
            // The application will have to determine this, by calling one of the methods explicitly,
            // before execution arrives at this call path.
            if (registerMethods.Length > 1)
            {
                throw new InvalidOperationException(
                    "More than one library has implemented a ResourceUtil.Register(Type type).\n" +
                    "This is allowed, but one or the other method must be called explicitly,\n" +
                    "before the app calls this static constructor.");
            }

            // Initiate registration. Set the RegistrationStarted property so we only auto-initiate once.
            var setRegStarted = from t in types
                                from p in t.GetProperties()
                                where
                                     p.Name == kRegistrationStarted &&
                                     p.PropertyType == typeof(bool) &&
                                     p.GetSetMethod().IsPublic &&
                                     p.GetSetMethod().IsStatic
                                select p;
            setRegStarted.First().GetSetMethod().Invoke(null, new object[] { true });

            var paramQueue = new System.Collections.Queue(1);
            paramQueue.Enqueue(typeof(Resources));
            registerMethods.First().Invoke(null, paramQueue.ToArray());
        }
    }
}
