//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Windows.Forms;
//using System.Diagnostics;

namespace Sce.Atf
{
    using IComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
    using IDataObject = System.Windows.Forms.IDataObject;

    /// <summary>
    /// Provides extended functionality for Drag and Drop.
    /// for internal use only.</summary>
    [ComVisible(true)]
    internal sealed class DragDropDataObject : IComDataObject, IDataObject, IDisposable
    {
        #region -- IComDataObject ---------------------------------------------

        /// <summary>
        /// Creates a connection between a data object and an advisory sink. This method is called by an object
        /// that supports an advisory sink and enables the advisory sink to be notified of changes in the object's data.</summary>
        /// <returns>This method supports the standard return values E_INVALIDARG, E_UNEXPECTED, and E_OUTOFMEMORY,
        /// as well as the following:
        /// ValueDescriptionS_OK -- The advisory connection was created.
        /// E_NOTIMPL -- This method is not implemented on the data object.
        /// DV_E_LINDEX -- There is an invalid value for <see cref="F:System.Runtime.InteropServices.ComTypes.FORMATETC.lindex"/>;
        ///   currently, only -1 is supported.
        /// DV_E_FORMATETC -- There is an invalid value for the <paramref name="pFormatetc"/> parameter.
        /// OLE_E_ADVISENOTSUPPORTED -- The data object does not support change notification.</returns>
        /// <param name="pFormatetc">A <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC"/> structure,
        /// passed by reference, that defines the format, target device, aspect, and medium that will be used for
        /// future notifications.</param>
        /// <param name="advf">One of the ADVF values that specifies a group of flags for controlling the advisory
        /// connection.</param>
        /// <param name="adviseSink">A pointer to the IAdviseSink interface on the advisory sink that will receive
        /// the change notification.</param>
        /// <param name="connection">When this method returns, contains a pointer to a DWORD token that identifies
        /// this connection. You can use this token later to delete the advisory connection by passing it to
        /// <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.DUnadvise(System.Int32)"/>.
        /// If this value is zero, the connection was not established. This parameter is passed uninitialized.</param>
        public int DAdvise(ref FORMATETC pFormatetc, ADVF advf, IAdviseSink adviseSink, out int connection)
        {
            // Check that the specified advisory flags are supported.
            const ADVF c_advfAllowed = ADVF.ADVF_NODATA | ADVF.ADVF_ONLYONCE | ADVF.ADVF_PRIMEFIRST;
            if ((int)((advf | c_advfAllowed) ^ c_advfAllowed) != 0)
            {
                connection = 0;
                return OLE_E_ADVISENOTSUPPORTED;
            }

            // Create and insert an entry for the connection list
            var entry = new AdviseEntry
            {
                Format = pFormatetc,
                Advf = advf,
                Sink = adviseSink,
            };
            m_connections.Add(m_nextConnectionId, entry);
            connection = m_nextConnectionId;
            m_nextConnectionId++;

            // If the ADVF_PRIMEFIRST flag is specified and the data exists,
            // raise the DataChanged event now.
            if ((advf & ADVF.ADVF_PRIMEFIRST) == ADVF.ADVF_PRIMEFIRST)
            {
                OleData dataEntry;
                if (GetDataEntry(ref pFormatetc, out dataEntry))
                    RaiseDataChanged(connection, ref dataEntry);
            }

            return 0;
        }

        /// <summary>
        /// Destroys a notification connection that had been previously established.</summary>
        /// <param name="connection">A DWORD token that specifies the connection to remove. Use the value
        /// returned by DAdvise() when the connection was originally established.</param>
        public void DUnadvise(int connection)
        {
            m_connections.Remove(connection);
        }

        /// <summary>
        /// Creates an object that can be used to enumerate the current advisory connections.</summary>
        /// <returns>This method supports the standard return value E_OUTOFMEMORY, as well as the following:
        /// S_OK -- The enumerator object is successfully instantiated or there are no connections.
        /// OLE_E_ADVISENOTSUPPORTED -- This object does not support advisory notifications.</returns>
        /// <param name="enumAdvise">When this method returns, contains an <see cref="T:System.Runtime.InteropServices.ComTypes.IEnumSTATDATA"/>
        /// that receives the interface pointer to the new enumerator object. If the implementation sets
        /// <paramref name="enumAdvise"/> to null, there are no connections to advisory sinks at this time.
        /// This parameter is passed uninitialized.</param>
        public int EnumDAdvise(out IEnumSTATDATA enumAdvise)
        {
            enumAdvise = null;
            return (OLE_E_ADVISENOTSUPPORTED);
        }

        /// <summary>
        /// Creates an object for enumerating the <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC"/>
        /// structures for a data object. These structures are used in calls to GetData() or SetData().</summary>
        /// <returns>This method supports the standard return values E_INVALIDARG and E_OUTOFMEMORY, as well as the following:
        /// S_OK -- The enumerator object was successfully created.
        /// E_NOTIMPL -- The direction specified by the <paramref name="direction"/> parameter is not supported.
        /// OLE_S_USEREG -- Requests that OLE enumerate the formats from the registry.</returns>
        /// <param name="direction">Specifies the direction of the data.</param>
        public IEnumFORMATETC EnumFormatEtc(DATADIR direction)
        {
            if (direction == DATADIR.DATADIR_GET)
            {
                var list = m_oleStorage.Select(data => data.Format).ToArray();
                return new FormatEnumerator(list);
            }
            throw new NotImplementedException("OLE_S_USEREG");
        }

        /// <summary>
        /// Provides a standard FORMATETC structure that is logically equivalent to a more complex structure.
        /// Use this method to determine whether two different FORMATETC structures would return the same data,
        /// removing the need for duplicate rendering.</summary>
        /// <returns>This method supports the standard return values E_INVALIDARG, E_UNEXPECTED, and E_OUTOFMEMORY,
        /// as well as the following: 
        /// S_OK -- The returned FORMATETC structure is different from the one that was passed.
        /// DATA_S_SAMEFORMATETC -- The structures are the same and null is returned in the "formatOut" parameter.
        /// DV_E_LINDEX -- There is an invalid value for <see cref="F:System.Runtime.InteropServices.ComTypes.FORMATETC.lindex"/>;
        ///   currently, only -1 is supported.
        /// DV_E_FORMATETC -- There is an invalid value for the "pFormatetc" parameter.
        /// OLE_E_NOTRUNNINGThe application is not running.</returns>
        /// <param name="formatIn">A pointer to a <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC"/> structure, passed by reference, that defines the format, medium, and target device that the caller would like to use to retrieve data in a subsequent call such as <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)"/>. The <see cref="T:System.Runtime.InteropServices.ComTypes.TYMED"/> member is not significant in this case and should be ignored.</param><param name="formatOut">When this method returns, contains a pointer to a <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC"/> structure that contains the most general information possible for a specific rendering, making it canonically equivalent to <paramref name="formatIn"/>. The caller must allocate this structure and the <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetCanonicalFormatEtc(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.FORMATETC@)"/> method must fill in the data. To retrieve data in a subsequent call such as <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)"/>, the caller uses the supplied value of <paramref name="formatOut"/>, unless the value supplied is null. This value is null if the method returns DATA_S_SAMEFORMATETC. The <see cref="T:System.Runtime.InteropServices.ComTypes.TYMED"/> member is not significant in this case and should be ignored. This parameter is passed uninitialized.</param>
        public int GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut)
        {
            formatOut = formatIn;
            return DV_E_FORMATETC;
        }

        /// <summary>
        /// Obtains data from a source data object. This method, which is called by a data consumer, renders the data
        /// described in the specified FORMATETC structure and transfers it through the specified STGMEDIUM structure.
        /// The caller then assumes responsibility for releasing the STGMEDIUM structure.</summary>
        /// <param name="format">A pointer to a FORMATETC structure, passed by reference, that defines the format,
        /// medium, and target device to use when passing the data. It is possible to specify more than one medium
        /// by using the Boolean OR operator, allowing the method to choose the best medium among those specified.</param>
        /// <param name="medium">When this method returns, contains a pointer to the STGMEDIUM structure that
        /// indicates the storage medium containing the returned data through its STGMEDIUM.tymed member, and the
        /// responsibility for releasing the medium through the value of its STGMEDIUM.pUnkForRelease member.  If
        /// STGMEDIUM.pUnkForRelease is null, the receiver of the medium is responsible for releasing it; otherwise,
        /// pUnkForRelease points to the IUnknown interface on the appropriate object so its Release method can be called.</param>
        public void GetData(ref FORMATETC format, out STGMEDIUM medium)
        {
            medium = new STGMEDIUM();
            GetDataHere(ref format, ref medium);
        }

        /// <summary>
        /// Obtains data from a source data object. This method, which is called by a data consumer, differs from the
        /// GetData() method in that the caller must allocate and free the specified storage medium.</summary>
        /// <param name="format">A pointer to a FORMATETC structure, passed by reference, that defines the format, medium,
        /// and target device to use when passing the data. Only one medium can be specified in
        /// <see cref="T:System.Runtime.InteropServices.ComTypes.TYMED"/>, and only the following values are valid:
        /// <see cref="F:System.Runtime.InteropServices.ComTypes.TYMED.TYMED_ISTORAGE"/>,
        /// <see cref="F:System.Runtime.InteropServices.ComTypes.TYMED.TYMED_ISTREAM"/>,
        /// <see cref="F:System.Runtime.InteropServices.ComTypes.TYMED.TYMED_HGLOBAL"/>, or
        /// <see cref="F:System.Runtime.InteropServices.ComTypes.TYMED.TYMED_FILE"/>.</param>
        /// <param name="medium">A STGMEDIUM, passed by reference, that defines the storage medium containing the data
        /// being transferred. The medium must be allocated by the caller and filled in by GetDataHere(). The caller
        /// must also free the medium. The implementation of this method must always supply a value of null for the
        /// STGMEDIUM.pUnkForRelease member of the STGMEDIUM structure that this parameter points to.</param>
        public void GetDataHere(ref FORMATETC format, ref STGMEDIUM medium)
        {
            OleData dataEntry;
            if (GetDataEntry(ref format, out dataEntry))
            {
                var source = dataEntry.Medium;
                medium = CopyMedium(ref source);
                return;
            }

            //object name = DataFormats.GetFormat(format.cfFormat).Name;
            //Debug.WriteLine("OLE GetDataHere: {0}, {1}, {2}", name, format.tymed, format.dwAspect);
            medium = default(STGMEDIUM);
        }

        /// <summary>
        /// Determines whether the data object is capable of rendering the data described in the FORMATETC structure.
        /// Objects attempting a paste or drop operation can call this method before calling GetData() to get an
        /// indication of whether the operation may be successful.</summary>
        /// <param name="format">A pointer to a FORMATETC structure, passed by reference, that defines the format,
        /// medium, and target device to use for the query.</param>
        /// <returns>This method supports the standard return values E_INVALIDARG, E_UNEXPECTED, and E_OUTOFMEMORY,
        /// as well as the following: 
        /// S_OK -- A subsequent call to GetData() would probably be successful.
        /// DV_E_LINDEX -- An invalid value for <see cref="F:System.Runtime.InteropServices.ComTypes.FORMATETC.lindex"/>;
        ///   currently, only -1 is supported.
        /// DV_E_FORMATETC -- An invalid value for the 'format' parameter.
        /// DV_E_TYMED -- An invalid FORMATETC.tymed value.
        /// DV_E_DVASPECT -- An invalid FORMATETC.dwAspect value.
        /// OLE_E_NOTRUNNING -- The application is not running.</returns>
        public int QueryGetData(ref FORMATETC format)
        {
            //var name = DataFormats.GetFormat(format.cfFormat).Name;
            if ((DVASPECT.DVASPECT_CONTENT & format.dwAspect) == 0)
            {
                //Debug.WriteLine("OLE QueryGetData: {0}, {1}, {2}", name, format.tymed, format.dwAspect);
                return DV_E_DVASPECT;
            }

            var ret = DV_E_TYMED;

            // Try to locate the data
            for (var i = 0; i < m_oleStorage.Count; ++i)
            {
                var pair = m_oleStorage[i];
                if ((pair.Format.tymed & format.tymed) > 0)
                {
                    if (pair.Format.cfFormat == format.cfFormat)
                    {
                        // Found it, return S_OK;
                        return 0;
                    }
                    // Found the medium type, but wrong format
                    ret = DV_E_CLIPFORMAT;
                }
                else
                {
                    // Mismatch on medium type
                    ret = DV_E_TYMED;
                }
            }

            //Debug.WriteLine("OLE QueryGetData: {0}, {1}, {2}", name, format.tymed, format.dwAspect);
            return ret;
        }

        /// <summary>
        /// Transfers data to the object that implements this method. This method is called by an object that
        /// contains a data source.</summary>
        /// <param name="formatIn">A FORMATETC structure, passed by reference, that defines the format used
        /// by the data object when interpreting the data contained in the storage medium.</param>
        /// <param name="medium">A STGMEDIUM structure, passed by reference, that defines the storage medium
        /// in which the data is being passed.</param>
        /// <param name="release">true to specify that the data object called, which implements SetData(),
        ///  owns the storage medium after the call returns. This means that the data object must free the
        /// medium after it has been used by calling the ReleaseStgMedium function. false to specify that the
        /// caller retains ownership of the storage medium, and the data object called uses the storage medium
        /// for the duration of the call only.</param>
        public void SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release)
        {
            // If the format exists in our storage, remove it prior to resetting it
            for (var i = 0; i < m_oleStorage.Count; ++i)
            {
                var pair = m_oleStorage[i];
                var format = pair.Format;
                if (IsFormatCompatible(ref formatIn, ref format))
                {
                    var releaseMedium = pair.Medium;
                    ReleaseStgMedium(ref releaseMedium);
                    m_oleStorage.Remove(pair);
                    break;
                }
            }

            // If release is true, we'll take ownership of the medium.
            // If not, we'll make a copy of it.
            var sm = medium;
            if (!release)
            {
                sm = CopyMedium(ref medium);
            }

            // Add it to the internal storage   
            var data = new OleData { Format = formatIn, Medium = sm };
            m_oleStorage.Add(data);

            RaiseDataChanged(ref data);
        }

        private static bool IsFormatEqual(string formatA, string formatB)
        {
            return string.CompareOrdinal(formatA, formatB) == 0;
        }

        #endregion

        #region -- IDataObject ------------------------------------------------
        // <see cref="System.Runtime.InteropServices.ComTypes.IDataObject"/>

        public object GetData(Type format)
        {
            var tymed = GetCompatibleFormat(format.FullName, format);
            if (tymed != TYMED.TYMED_NULL)
            {
                return GetData(format.FullName);
            }

            object obj;
            return m_managedStorage.TryGetValue(format.FullName, out obj) ? obj : null;
        }

        public object GetData(string format)
        {
            return GetData(format, true);
        }

        public object GetData(string format, bool autoConvert)
        {
            object obj;
            if (m_managedStorage.TryGetValue(format, out obj))
                return obj;

            var formatEtc = OleConverter.CreateFormat(format);
            if (QueryGetData(ref formatEtc) == 0)
            {
                STGMEDIUM medium;
                GetData(ref formatEtc, out medium);
                return OleConverter.Convert(format, ref medium);
            }

            return null;
        }

        public bool GetDataPresent(Type format)
        {
            var tymed = GetCompatibleFormat(format.FullName, format);
            if (tymed != TYMED.TYMED_NULL)
            {
                return GetDataPresent(format.FullName);
            }

            return m_managedStorage.ContainsKey(format.FullName);
        }

        public bool GetDataPresent(string format)
        {
            return GetDataPresent(format, true);
        }

        public bool GetDataPresent(string format, bool autoConvert)
        {
            if (m_managedStorage.ContainsKey(format))
                return true;

            var formatEtc = OleConverter.CreateFormat(format);
            if (QueryGetData(ref formatEtc) == 0)
                return true;

            return false;
        }

        public string[] GetFormats()
        {
            return GetFormats(true);
        }

        public string[] GetFormats(bool autoConvert)
        {
            var formats = new HashSet<string>(m_managedStorage.Keys);
            for (var i = 0; i < m_oleStorage.Count; ++i)
            {
                var item = m_oleStorage[i];
                var name = DataFormats.GetFormat(item.Format.cfFormat).Name;
                formats.Add(name);
            }
            return formats.ToArray();
        }

        public void SetData(object data)
        {
            if (data is ISerializable)
            {
                SetData(DataFormats.Serializable, data);
            }
            else
            {
                SetData(data.GetType(), data);
            }
        }

        public void SetData(Type format, object data)
        {
            SetData(format.FullName, format, true, data);
        }

        public void SetData(string format, object data)
        {
            SetData(format, true, data);
        }

        public void SetData(string format, bool autoConvert, object data)
        {
            var type = (data != null) ? data.GetType() : typeof(object);
            SetData(format, type, autoConvert, data);
        }

        public void SetData(string format, Type type, bool autoConvert, object data)
        {
            var tymed = GetCompatibleFormat(format, type);
            if (tymed != TYMED.TYMED_NULL)
            {
                var formatEtc = OleConverter.CreateFormat(format);
                formatEtc.tymed = tymed;

                // Set data on an empty DataObject instance
                var conv = new DataObject();
                conv.SetData(format, true, data);

                // Now retrieve the data, using the COM interface.
                // This will perform a managed to unmanaged conversion for us.
                STGMEDIUM medium;
                ((IComDataObject)conv).GetData(ref formatEtc, out medium);
                try
                {
                    // Now set the data on our data object
                    SetData(ref formatEtc, ref medium, true);
                }
                catch
                {
                    // On exceptions, release the medium
                    ReleaseStgMedium(ref medium);
                    throw;
                }
            }
            else
            {
                m_managedStorage[format] = data;
            }
        }

        private static TYMED GetCompatibleFormat(string format, Type type)
        {
            if (IsFormatEqual(format, DataFormats.Bitmap) && typeof(Bitmap).IsAssignableFrom(type))
                return TYMED.TYMED_GDI;
            if (IsFormatEqual(format, DataFormats.EnhancedMetafile))
                return TYMED.TYMED_ENHMF;
            if (typeof(Stream).IsAssignableFrom(type) ||
                IsFormatEqual(format, DataFormats.Html) ||
                IsFormatEqual(format, DataFormats.Text) ||
                IsFormatEqual(format, DataFormats.Rtf) ||
                IsFormatEqual(format, DataFormats.OemText) ||
                IsFormatEqual(format, DataFormats.UnicodeText) ||
                IsFormatEqual(format, "ApplicationTrust") ||
                IsFormatEqual(format, DataFormats.FileDrop) ||
                IsFormatEqual(format, "FileName") ||
                IsFormatEqual(format, "FileNameW"))
                return TYMED.TYMED_HGLOBAL;
            if (IsFormatEqual(format, DataFormats.Dib) && typeof(Image).IsAssignableFrom(type))
                return TYMED.TYMED_NULL;
            if (IsFormatEqual(format, typeof(Bitmap).FullName))
                return TYMED.TYMED_HGLOBAL;
            if (IsFormatEqual(format, DataFormats.Serializable) ||
                typeof(ISerializable).IsAssignableFrom(type) ||
                type.IsSerializable)
                return TYMED.TYMED_HGLOBAL;

            return TYMED.TYMED_NULL;
        }

        #endregion

        #region -- IDisposable ------------------------------------------------

        public void Dispose()
        {
            ClearStorage();
            GC.SuppressFinalize(this);
        }

        #endregion

        ~DragDropDataObject()
        {
            ClearStorage();
        }

        [DllImport("urlmon.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int CopyStgMedium(ref STGMEDIUM pcstgmedSrc, ref STGMEDIUM pstgmedDest);

        [DllImport("ole32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);

        private void ClearStorage()
        {
            for (var i = 0; i < m_oleStorage.Count; ++i)
            {
                var pair = m_oleStorage[i];
                var medium = pair.Medium;
                ReleaseStgMedium(ref medium);
            }
            m_oleStorage.Clear();
            m_managedStorage.Clear();
        }

        private static STGMEDIUM CopyMedium(ref STGMEDIUM medium)
        {
            var sm = new STGMEDIUM();
            var hr = CopyStgMedium(ref medium, ref sm);
            if (hr != 0)
                throw Marshal.GetExceptionForHR(hr);
            return sm;
        }

        private static bool IsFormatCompatible(FORMATETC format1, FORMATETC format2)
        {
            return IsFormatCompatible(ref format1, ref format2);
        }

        private static bool IsFormatCompatible(ref FORMATETC format1, ref FORMATETC format2)
        {
            return ((format1.tymed & format2.tymed) > 0
                    && format1.dwAspect == format2.dwAspect
                    && format1.cfFormat == format2.cfFormat);
        }

        private bool GetDataEntry(ref FORMATETC pFormatetc, out OleData dataEntry)
        {
            for (var i = 0; i < m_oleStorage.Count; ++i)
            {
                var entry = m_oleStorage[i];
                var format = entry.Format;
                if (IsFormatCompatible(ref pFormatetc, ref format))
                {
                    dataEntry = entry;
                    return true;
                }
            }

            dataEntry = default(OleData);
            return false;
        }

        /// <summary>
        /// Raises the DataChanged event for the specified connection.
        /// </summary>
        private void RaiseDataChanged(int connection, ref OleData dataEntry)
        {
            var adviseEntry = m_connections[connection];
            var format = dataEntry.Format;
            var medium = (adviseEntry.Advf & ADVF.ADVF_NODATA) != ADVF.ADVF_NODATA ?
                dataEntry.Medium : default(STGMEDIUM);

            adviseEntry.Sink.OnDataChange(ref format, ref medium);

            if ((adviseEntry.Advf & ADVF.ADVF_ONLYONCE) == ADVF.ADVF_ONLYONCE)
            {
                m_connections.Remove(connection);
            }
        }

        /// <summary>
        /// Raises the DataChanged event for any advisory connections that
        /// are listening for it.
        /// </summary>
        private void RaiseDataChanged(ref OleData dataEntry)
        {
            foreach (var connection in m_connections)
            {
                if (IsFormatCompatible(connection.Value.Format, dataEntry.Format))
                    RaiseDataChanged(connection.Key, ref dataEntry);
            }
        }

        private class FormatEnumerator : IEnumFORMATETC
        {
            private readonly FORMATETC[] m_formats;
            private int m_currentIndex;

            public FormatEnumerator(IEnumerable<FORMATETC> data)
            {
                m_formats = data.ToArray();
            }

            public void Clone(out IEnumFORMATETC newEnum)
            {
                newEnum = new FormatEnumerator(m_formats)
                {
                    m_currentIndex = m_currentIndex
                };
            }

            public int Next(int celt, FORMATETC[] rgelt, int[] pceltFetched)
            {
                // Start with zero fetched, in case we return early
                if (pceltFetched != null && pceltFetched.Length > 0)
                    pceltFetched[0] = 0;

                // This will count down as we fetch elements
                var cReturn = celt;

                // Short circuit if they didn't request any elements, or didn't
                // provide room in the return array, or there are not more elements
                // to enumerate.
                if (celt <= 0 || rgelt == null || m_currentIndex >= m_formats.Length)
                    return 1; // S_FALSE

                // If the number of requested elements is not one, then we must
                // be able to tell the caller how many elements were fetched.
                if ((pceltFetched == null || pceltFetched.Length < 1) && celt != 1)
                    return 1; // S_FALSE

                // If the number of elements in the return array is too small, we
                // throw. This is not a likely scenario, hence the exception.
                if (rgelt.Length < celt)
                    throw new ArgumentException("The number of elements in the return array is less than the number of elements requested");

                // Fetch the elements.
                for (var i = 0; m_currentIndex < m_formats.Length && cReturn > 0; i++, cReturn--, m_currentIndex++)
                    rgelt[i] = m_formats[m_currentIndex];

                // Return the number of elements fetched
                if (pceltFetched != null && pceltFetched.Length > 0)
                    pceltFetched[0] = celt - cReturn;

                // cReturn has the number of elements requested but not fetched.
                // It will be greater than zero, if multiple elements were requested
                // but we hit the end of the enumeration.
                return (cReturn == 0) ? 0 : 1; // S_OK : S_FALSE
            }

            public int Reset()
            {
                m_currentIndex = 0;
                return 0; // S_OK
            }

            public int Skip(int celt)
            {
                if (m_currentIndex + celt > m_formats.Length)
                    return 1; // S_FALSE

                m_currentIndex += celt;
                return 0; // S_OK
            }
        }

        private struct AdviseEntry
        {
            public FORMATETC Format;
            public ADVF Advf;
            public IAdviseSink Sink;
        }

        private struct OleData
        {
            public FORMATETC Format;
            public STGMEDIUM Medium;
        }

        private const int OLE_E_ADVISENOTSUPPORTED = unchecked((int)0x80040003);
        private const int DV_E_FORMATETC = unchecked((int)0x80040064);
        private const int DV_E_TYMED = unchecked((int)0x80040069);
        private const int DV_E_CLIPFORMAT = unchecked((int)0x8004006A);
        private const int DV_E_DVASPECT = unchecked((int)0x8004006B);

        private readonly List<OleData> m_oleStorage = new List<OleData>();
        private readonly Dictionary<string, object> m_managedStorage = new Dictionary<string, object>();
        private readonly Dictionary<int, AdviseEntry> m_connections = new Dictionary<int, AdviseEntry>();
        private int m_nextConnectionId = 1;
    }
}
