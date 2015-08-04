//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "stdafx.h"

// These names conflict with the managed versions.
#undef BROWSEINFO
#undef HDITEM

// Couldn't get this fully working. I'll make the necessary types public instead. --Ron
//// This 'using' is because of an annoying C++/CLI limitation.
////	http://stackoverflow.com/questions/417842/internalsvisibleto-not-working-for-managed-c
//#using <Atf.Core.dll> as_friend

// This 'using' is because the dependency on Atf.Gui.OpenGL.dll is conditional on whether
//	or not we're compiling for the x86 (32-bit) processor.
#ifdef _M_IX86
#using <Atf.Gui.OpenGL.dll> as_friend
#endif

using namespace System;
using namespace System::Runtime::InteropServices;

using namespace NUnit::Framework;
using namespace Sce::Atf;

namespace UnitTests
{
	// ReSharper's unit test runner does not work with this C++/CLI project. Use
	//	Visual Studio 2013's Test -> Windows -> Test Explorer command or the VS context
	//	menu commands (e.g., ctrl+R, ctrl+T for the Debug Tests command) on the Test
	//	or TestFixture attributes.
	[TestFixture]
	public ref class TestMarshaledStructs
	{
	public:
		[Test]
		static void TestSHFILEINFO()
		{
			SHFILEINFOW nativeInfo;
			Shell32::SHFILEINFO managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "hIcon"),
				(int) ((UINT8*) (&nativeInfo.hIcon) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "iIcon"),
				(int) ((UINT8*) (&nativeInfo.iIcon) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwAttributes"),
				(int) ((UINT8*) (&nativeInfo.dwAttributes) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "szDisplayName"),
				(int) ((UINT8*) (&nativeInfo.szDisplayName) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int)Marshal::OffsetOf(managedInfo.GetType(), "szTypeName"),
				(int) ((UINT8*) (&nativeInfo.szTypeName) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestBROWSEINFO()
		{
			BROWSEINFOW nativeInfo;
			Shell32::BROWSEINFO managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "hwndOwner"),
				(int) ((UINT8*) (&nativeInfo.hwndOwner) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "pidlRoot"),
				(int) ((UINT8*) (&nativeInfo.pidlRoot) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "pszDisplayName"),
				(int) ((UINT8*) (&nativeInfo.pszDisplayName) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lpszTitle"),
				(int) ((UINT8*) (&nativeInfo.lpszTitle) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "ulFlags"),
				(int) ((UINT8*) (&nativeInfo.ulFlags) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lpfn"),
				(int) ((UINT8*) (&nativeInfo.lpfn) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lParam"),
				(int) ((UINT8*) (&nativeInfo.lParam) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "iImage"),
				(int) ((UINT8*) (&nativeInfo.iImage) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestBROWSEINFOW()
		{
			BROWSEINFOW nativeInfo;
			Sce::Atf::Wpf::Controls::BrowseForFolderDialog::BROWSEINFOW ^managedInfo =
				gcnew Sce::Atf::Wpf::Controls::BrowseForFolderDialog::BROWSEINFOW();
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "hwndOwner"),
				(int) ((UINT8*) (&nativeInfo.hwndOwner) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "pidlRoot"),
				(int) ((UINT8*) (&nativeInfo.pidlRoot) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "pszDisplayName"),
				(int) ((UINT8*) (&nativeInfo.pszDisplayName) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "lpszTitle"),
				(int) ((UINT8*) (&nativeInfo.lpszTitle) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "ulFlags"),
				(int) ((UINT8*) (&nativeInfo.ulFlags) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "lpfn"),
				(int) ((UINT8*) (&nativeInfo.lpfn) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "lParam"),
				(int) ((UINT8*) (&nativeInfo.lParam) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "iImage"),
				(int) ((UINT8*) (&nativeInfo.iImage) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestMEMORYSTATUSEX()
		{
			MEMORYSTATUSEX nativeInfo;
			Kernel32::MEMORYSTATUSEX ^managedInfo = gcnew Kernel32::MEMORYSTATUSEX();
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "dwLength"),
				(int) ((UINT8*) (&nativeInfo.dwLength) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "dwMemoryLoad"),
				(int) ((UINT8*) (&nativeInfo.dwMemoryLoad) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "ullTotalPhys"),
				(int) ((UINT8*) (&nativeInfo.ullTotalPhys) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "ullAvailPhys"),
				(int) ((UINT8*) (&nativeInfo.ullAvailPhys) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "ullTotalPageFile"),
				(int) ((UINT8*) (&nativeInfo.ullTotalPageFile) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "ullAvailPageFile"),
				(int) ((UINT8*) (&nativeInfo.ullAvailPageFile) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "ullTotalVirtual"),
				(int) ((UINT8*) (&nativeInfo.ullTotalVirtual) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "ullAvailVirtual"),
				(int) ((UINT8*) (&nativeInfo.ullAvailVirtual) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "ullAvailExtendedVirtual"),
				(int) ((UINT8*) (&nativeInfo.ullAvailExtendedVirtual) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestBITMAP()
		{
			::BITMAP nativeInfo;
			Sce::Atf::BITMAP managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "bmType"),
				(int) ((UINT8*) (&nativeInfo.bmType) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "bmWidth"),
				(int) ((UINT8*) (&nativeInfo.bmWidth) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "bmHeight"),
				(int) ((UINT8*) (&nativeInfo.bmHeight) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "bmWidthBytes"),
				(int) ((UINT8*) (&nativeInfo.bmWidthBytes) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "bmPlanes"),
				(int) ((UINT8*) (&nativeInfo.bmPlanes) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "bmBitsPixel"),
				(int) ((UINT8*) (&nativeInfo.bmBitsPixel) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "bmBits"),
				(int) ((UINT8*) (&nativeInfo.bmBits) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestBITMAPINFO_FLAT()
		{
			::BITMAPINFOHEADER nativeInfo;
			Sce::Atf::BITMAPINFO_FLAT managedInfo;
			// BITMAPINFO_FLAT has the header in the top plus space at the bottom.
			// Let's make sure the header portion matches.
			//Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "biSize"),
				(int) ((UINT8*) (&nativeInfo.biSize) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "biWidth"),
				(int) ((UINT8*) (&nativeInfo.biWidth) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "biHeight"),
				(int) ((UINT8*) (&nativeInfo.biHeight) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "biPlanes"),
				(int) ((UINT8*) (&nativeInfo.biPlanes) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "biBitCount"),
				(int) ((UINT8*) (&nativeInfo.biBitCount) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "biCompression"),
				(int) ((UINT8*) (&nativeInfo.biCompression) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "biSizeImage"),
				(int) ((UINT8*) (&nativeInfo.biSizeImage) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "biXPelsPerMeter"),
				(int) ((UINT8*) (&nativeInfo.biXPelsPerMeter) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "biYPelsPerMeter"),
				(int) ((UINT8*) (&nativeInfo.biYPelsPerMeter) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "biClrUsed"),
				(int) ((UINT8*) (&nativeInfo.biClrUsed) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "biClrImportant"),
				(int) ((UINT8*) (&nativeInfo.biClrImportant) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestBITMAPINFOHEADER()
		{
			::BITMAPINFOHEADER nativeInfo;
			Sce::Atf::BITMAPINFOHEADER ^managedInfo = gcnew Sce::Atf::BITMAPINFOHEADER();
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "biSize"),
				(int) ((UINT8*) (&nativeInfo.biSize) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "biWidth"),
				(int) ((UINT8*) (&nativeInfo.biWidth) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "biHeight"),
				(int) ((UINT8*) (&nativeInfo.biHeight) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "biPlanes"),
				(int) ((UINT8*) (&nativeInfo.biPlanes) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "biBitCount"),
				(int) ((UINT8*) (&nativeInfo.biBitCount) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "biCompression"),
				(int) ((UINT8*) (&nativeInfo.biCompression) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "biSizeImage"),
				(int) ((UINT8*) (&nativeInfo.biSizeImage) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "biXPelsPerMeter"),
				(int) ((UINT8*) (&nativeInfo.biXPelsPerMeter) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "biYPelsPerMeter"),
				(int) ((UINT8*) (&nativeInfo.biYPelsPerMeter) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "biClrUsed"),
				(int) ((UINT8*) (&nativeInfo.biClrUsed) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo->GetType(), "biClrImportant"),
				(int) ((UINT8*) (&nativeInfo.biClrImportant) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestHDITEM()
		{
			HDITEMW nativeInfo;
			Sce::Atf::User32::HDITEM managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "mask"),
				(int) ((UINT8*) (&nativeInfo.mask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "cxy"),
				(int) ((UINT8*) (&nativeInfo.cxy) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "pszText"),
				(int) ((UINT8*) (&nativeInfo.pszText) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "hbm"),
				(int) ((UINT8*) (&nativeInfo.hbm) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "cchTextMax"),
				(int) ((UINT8*) (&nativeInfo.cchTextMax) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "fmt"),
				(int) ((UINT8*) (&nativeInfo.fmt) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lParam"),
				(int) ((UINT8*) (&nativeInfo.lParam) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "iImage"),
				(int) ((UINT8*) (&nativeInfo.iImage) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "iOrder"),
				(int) ((UINT8*) (&nativeInfo.iOrder) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "type"),
				(int) ((UINT8*) (&nativeInfo.type) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "pvFilter"),
				(int) ((UINT8*) (&nativeInfo.pvFilter) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "state"),
				(int) ((UINT8*) (&nativeInfo.state) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestPOINT()
		{
			POINT nativeInfo;
			Sce::Atf::User32::POINT managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "X"),
				(int) ((UINT8*) (&nativeInfo.x) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "Y"),
				(int) ((UINT8*) (&nativeInfo.y) - (UINT8*) &nativeInfo));
		}

		
		[Test]
		static void TestRECT()
		{
			RECT nativeInfo;
			Sce::Atf::User32::RECT managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "Left"),
				(int) ((UINT8*) (&nativeInfo.left) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "Top"),
				(int) ((UINT8*) (&nativeInfo.top) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "Right"),
				(int) ((UINT8*) (&nativeInfo.right) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "Bottom"),
				(int) ((UINT8*) (&nativeInfo.bottom) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestMINMAXINFO()
		{
			MINMAXINFO nativeInfo;
			Sce::Atf::User32::MINMAXINFO managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "ptReserved"),
				(int) ((UINT8*) (&nativeInfo.ptReserved) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "ptMaxSize"),
				(int) ((UINT8*) (&nativeInfo.ptMaxSize) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "ptMaxPosition"),
				(int) ((UINT8*) (&nativeInfo.ptMaxPosition) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "ptMinTrackSize"),
				(int) ((UINT8*) (&nativeInfo.ptMinTrackSize) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "ptMaxTrackSize"),
				(int) ((UINT8*) (&nativeInfo.ptMaxTrackSize) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestMSG()
		{
			MSG nativeInfo;
			Sce::Atf::User32::MSG managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "hWnd"),
				(int) ((UINT8*) (&nativeInfo.hwnd) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "msg"),
				(int) ((UINT8*) (&nativeInfo.message) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "wParam"),
				(int) ((UINT8*) (&nativeInfo.wParam) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lParam"),
				(int) ((UINT8*) (&nativeInfo.lParam) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "time"),
				(int) ((UINT8*) (&nativeInfo.time) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "p"),
				(int) ((UINT8*) (&nativeInfo.pt) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestTRACKMOUSEEVENT()
		{
			TRACKMOUSEEVENT nativeInfo;
			Sce::Atf::User32::TRACKMOUSEEVENT managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "cbSize"),
				(int) ((UINT8*) (&nativeInfo.cbSize) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwFlags"),
				(int) ((UINT8*) (&nativeInfo.dwFlags) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "hwndTrack"),
				(int) ((UINT8*) (&nativeInfo.hwndTrack) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwHoverTime"),
				(int) ((UINT8*) (&nativeInfo.dwHoverTime) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestNMHDR()
		{
			NMHDR nativeInfo;
			Sce::Atf::User32::NMHDR managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "hwndFrom"),
				(int) ((UINT8*) (&nativeInfo.hwndFrom) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "idFrom"),
				(int) ((UINT8*) (&nativeInfo.idFrom) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "code"),
				(int) ((UINT8*) (&nativeInfo.code) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestWINDOWPOS()
		{
			WINDOWPOS nativeInfo;
			Sce::Atf::User32::WINDOWPOS managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "hwnd"),
				(int) ((UINT8*) (&nativeInfo.hwnd) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "hwndInsertAfter"),
				(int) ((UINT8*) (&nativeInfo.hwndInsertAfter) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "x"),
				(int) ((UINT8*) (&nativeInfo.x) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "y"),
				(int) ((UINT8*) (&nativeInfo.y) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "cx"),
				(int) ((UINT8*) (&nativeInfo.cx) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "cy"),
				(int) ((UINT8*) (&nativeInfo.cy) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "flags"),
				(int) ((UINT8*) (&nativeInfo.flags) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestNCCALCSIZE_PARAMS()
		{
			NCCALCSIZE_PARAMS nativeInfo;
			Sce::Atf::Applications::FormNcRenderer::NCCALCSIZE_PARAMS managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "rect0"),
				(int) ((UINT8*) (&nativeInfo.rgrc[0]) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "rect1"),
				(int) ((UINT8*) (&nativeInfo.rgrc[1]) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "rect2"),
				(int) ((UINT8*) (&nativeInfo.rgrc[2]) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lppos"),
				(int) ((UINT8*) (&nativeInfo.lppos) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestWINDOWINFO()
		{
			WINDOWINFO nativeInfo;
			Sce::Atf::CustomFileDialog::WINDOWINFO managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "cbSize"),
				(int) ((UINT8*) (&nativeInfo.cbSize) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "rcWindow"),
				(int) ((UINT8*) (&nativeInfo.rcWindow) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "rcClient"),
				(int) ((UINT8*) (&nativeInfo.rcClient) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwStyle"),
				(int) ((UINT8*) (&nativeInfo.dwStyle) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwExStyle"),
				(int) ((UINT8*) (&nativeInfo.dwExStyle) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwWindowStatus"),
				(int) ((UINT8*) (&nativeInfo.dwWindowStatus) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "cxWindowBorders"),
				(int) ((UINT8*) (&nativeInfo.cxWindowBorders) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "cyWindowBorders"),
				(int) ((UINT8*) (&nativeInfo.cyWindowBorders) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "atomWindowType"),
				(int) ((UINT8*) (&nativeInfo.atomWindowType) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "wCreatorVersion"),
				(int) ((UINT8*) (&nativeInfo.wCreatorVersion) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestOPENFILENAME()
		{
			OPENFILENAME nativeInfo;
			Sce::Atf::CustomFileDialog::OPENFILENAME managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lStructSize"),
				(int) ((UINT8*) (&nativeInfo.lStructSize) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "hwndOwner"),
				(int) ((UINT8*) (&nativeInfo.hwndOwner) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "hInstance"),
				(int) ((UINT8*) (&nativeInfo.hInstance) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lpstrFilter"),
				(int) ((UINT8*) (&nativeInfo.lpstrFilter) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lpstrCustomFilter"),
				(int) ((UINT8*) (&nativeInfo.lpstrCustomFilter) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "nMaxCustFilter"),
				(int) ((UINT8*) (&nativeInfo.nMaxCustFilter) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "nFilterIndex"),
				(int) ((UINT8*) (&nativeInfo.nFilterIndex) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lpstrFile"),
				(int) ((UINT8*) (&nativeInfo.lpstrFile) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "nMaxFile"),
				(int) ((UINT8*) (&nativeInfo.nMaxFile) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lpstrFileTitle"),
				(int) ((UINT8*) (&nativeInfo.lpstrFileTitle) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "nMaxFileTitle"),
				(int) ((UINT8*) (&nativeInfo.nMaxFileTitle) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lpstrInitialDir"),
				(int) ((UINT8*) (&nativeInfo.lpstrInitialDir) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lpstrTitle"),
				(int) ((UINT8*) (&nativeInfo.lpstrTitle) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "Flags"),
				(int) ((UINT8*) (&nativeInfo.Flags) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "nFileOffset"),
				(int) ((UINT8*) (&nativeInfo.nFileOffset) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "nFileExtension"),
				(int) ((UINT8*) (&nativeInfo.nFileExtension) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lpstrDefExt"),
				(int) ((UINT8*) (&nativeInfo.lpstrDefExt) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lCustData"),
				(int) ((UINT8*) (&nativeInfo.lCustData) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lpfnHook"),
				(int) ((UINT8*) (&nativeInfo.lpfnHook) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "lpTemplateName"),
				(int) ((UINT8*) (&nativeInfo.lpTemplateName) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "pvReserved"),
				(int) ((UINT8*) (&nativeInfo.pvReserved) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwReserved"),
				(int) ((UINT8*) (&nativeInfo.dwReserved) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "FlagsEx"),
				(int) ((UINT8*) (&nativeInfo.FlagsEx) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestDROPDESCRIPTION()
		{
			DROPDESCRIPTION nativeInfo;
			Sce::Atf::DropDescription managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "type"),
				(int) ((UINT8*) (&nativeInfo.type) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "szMessage"),
				(int) ((UINT8*) (&nativeInfo.szMessage) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "szInsert"),
				(int) ((UINT8*) (&nativeInfo.szInsert) - (UINT8*) &nativeInfo));
		}

		[Test]
		static void TestShDragImage()
		{
			SHDRAGIMAGE nativeInfo;
			Sce::Atf::ShDragImage managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "sizeDragImage"),
				(int) ((UINT8*) (&nativeInfo.sizeDragImage) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "ptOffset"),
				(int) ((UINT8*) (&nativeInfo.ptOffset) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "hbmpDragImage"),
				(int) ((UINT8*) (&nativeInfo.hbmpDragImage) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "crColorKey"),
				(int) ((UINT8*) (&nativeInfo.crColorKey) - (UINT8*) &nativeInfo));
		}

#ifdef _M_IX86
		[Test]
		static void TestDDSCAPS2()
		{
			DDSCAPS2 nativeInfo;
			Sce::Atf::Rendering::DdsImageLoader::DDSCAPS2 managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwCaps"),
				(int) ((UINT8*) (&nativeInfo.dwCaps) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwCaps2"),
				(int) ((UINT8*) (&nativeInfo.dwCaps2) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwCaps3"),
				(int) ((UINT8*) (&nativeInfo.dwCaps3) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwCaps4"),
				(int) ((UINT8*) (&nativeInfo.dwCaps4) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwVolumeDepth"),
				(int) ((UINT8*) (&nativeInfo.dwVolumeDepth) - (UINT8*) &nativeInfo));
		}
#endif

#ifdef _M_IX86
		[Test]
		static void TestDDCOLORKEY()
		{
			DDCOLORKEY nativeInfo;
			Sce::Atf::Rendering::DdsImageLoader::DDCOLORKEY managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwColorSpaceLowValue"),
				(int) ((UINT8*) (&nativeInfo.dwColorSpaceLowValue) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwColorSpaceHighValue"),
				(int) ((UINT8*) (&nativeInfo.dwColorSpaceHighValue) - (UINT8*) &nativeInfo));
		}
#endif

#ifdef _M_IX86
		[Test]
		static void TestDDPIXELFORMAT()
		{
			DDPIXELFORMAT nativeInfo;
			Sce::Atf::Rendering::DdsImageLoader::DDPIXELFORMAT managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwSize"),
				(int) ((UINT8*) (&nativeInfo.dwSize) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwFlags"),
				(int) ((UINT8*) (&nativeInfo.dwFlags) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwFourCC"),
				(int) ((UINT8*) (&nativeInfo.dwFourCC) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwRGBBitCount"),
				(int) ((UINT8*) (&nativeInfo.dwRGBBitCount) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwYUVBitCount"),
				(int) ((UINT8*) (&nativeInfo.dwYUVBitCount) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwZBufferBitDepth"),
				(int) ((UINT8*) (&nativeInfo.dwZBufferBitDepth) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwAlphaBitDepth"),
				(int) ((UINT8*) (&nativeInfo.dwAlphaBitDepth) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwLuminanceBitCount"),
				(int) ((UINT8*) (&nativeInfo.dwLuminanceBitCount) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwBumpBitCount"),
				(int) ((UINT8*) (&nativeInfo.dwBumpBitCount) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwPrivateFormatBitCount"),
				(int) ((UINT8*) (&nativeInfo.dwPrivateFormatBitCount) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwRBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwRBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwYBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwYBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwStencilBitDepth"),
				(int) ((UINT8*) (&nativeInfo.dwStencilBitDepth) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwLuminanceBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwLuminanceBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwBumpDuBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwBumpDuBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwOperations"),
				(int) ((UINT8*) (&nativeInfo.dwOperations) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwGBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwGBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwUBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwUBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwZBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwZBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwBumpDvBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwBumpDvBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "MultiSampleCaps"),
				(int) ((UINT8*) (&nativeInfo.MultiSampleCaps) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwBBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwBBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwVBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwVBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwStencilBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwStencilBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwBumpLuminanceBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwBumpLuminanceBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwRGBAlphaBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwRGBAlphaBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwYUVAlphaBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwYUVAlphaBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwLuminanceAlphaBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwLuminanceAlphaBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwRGBZBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwRGBZBitMask) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "dwYUVZBitMask"),
				(int) ((UINT8*) (&nativeInfo.dwYUVZBitMask) - (UINT8*) &nativeInfo));
		}
#endif //_M_IX86

#ifdef _M_IX86
		[Test]
		static void TestDDSURFACEDESC2()
		{
			DDSURFACEDESC2 nativeInfo;
			Sce::Atf::Rendering::DdsImageLoader::DDSURFACEDESC2 managedInfo;
			Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwSize"),
				(int) ((UINT8*) (&nativeInfo.dwSize) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwFlags"),
				(int) ((UINT8*) (&nativeInfo.dwFlags) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwHeight"),
				(int) ((UINT8*) (&nativeInfo.dwHeight) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwWidth"),
				(int) ((UINT8*) (&nativeInfo.dwWidth) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_lPitch"),
				(int) ((UINT8*) (&nativeInfo.lPitch) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwLinearSize"),
				(int) ((UINT8*) (&nativeInfo.dwLinearSize) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwBackBufferCount"),
				(int) ((UINT8*) (&nativeInfo.dwBackBufferCount) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwDepth"),
				(int) ((UINT8*) (&nativeInfo.dwDepth) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwMipMapCount"),
				(int) ((UINT8*) (&nativeInfo.dwMipMapCount) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwRefreshRate"),
				(int) ((UINT8*) (&nativeInfo.dwRefreshRate) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwSrcVBHandle"),
				(int) ((UINT8*) (&nativeInfo.dwSrcVBHandle) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwAlphaBitDepth"),
				(int) ((UINT8*) (&nativeInfo.dwAlphaBitDepth) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwReserved"),
				(int) ((UINT8*) (&nativeInfo.dwReserved) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_lpSurface"),
				(int) ((UINT8*) (&nativeInfo.lpSurface) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_ddckCKDestOverlay"),
				(int) ((UINT8*) (&nativeInfo.ddckCKDestOverlay) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwEmptyFaceColor"),
				(int) ((UINT8*) (&nativeInfo.dwEmptyFaceColor) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_ddckCKDestBlt"),
				(int) ((UINT8*) (&nativeInfo.ddckCKDestBlt) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_ddckCKSrcOverlay"),
				(int) ((UINT8*) (&nativeInfo.ddckCKSrcOverlay) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_ddckCKSrcBlt"),
				(int) ((UINT8*) (&nativeInfo.ddckCKSrcBlt) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_ddpfPixelFormat"),
				(int) ((UINT8*) (&nativeInfo.ddpfPixelFormat) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwFVF"),
				(int) ((UINT8*) (&nativeInfo.dwFVF) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_ddsCaps"),
				(int) ((UINT8*) (&nativeInfo.ddsCaps) - (UINT8*) &nativeInfo));
			Assert::AreEqual(
				(int) Marshal::OffsetOf(managedInfo.GetType(), "m_dwTextureStage"),
				(int) ((UINT8*) (&nativeInfo.dwTextureStage) - (UINT8*) &nativeInfo));
		}
#endif //_M_IX86

		// Throws an exception:
		//	Type 'Sce.Atf.Shell32+ITEMIDLIST' cannot be marshaled as an unmanaged structure; no meaningful size or offset can be computed.
		//[Test]
		//static void TestITEMIDLIST()
		//{
		//	ITEMIDLIST nativeInfo;
		//	Shell32::ITEMIDLIST managedInfo;
		//	Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
		//	Assert::AreEqual(
		//		(int) Marshal::OffsetOf(managedInfo.GetType(), "mkid"),
		//		(int) ((UINT8*) (&nativeInfo.mkid) - (UINT8*) &nativeInfo));
		//}

		// Throws an exception:
		//	Type 'Sce.Atf.Shell32+ITEMIDLIST' cannot be marshaled as an unmanaged structure; no meaningful size or offset can be computed.
		//[Test]
		//static void TestSHITEMID()
		//{
		//	SHITEMID nativeInfo;
		//	Shell32::SHITEMID managedInfo;
		//	Assert::AreEqual(Marshal::SizeOf(managedInfo), (int)sizeof(nativeInfo));
		//	Assert::AreEqual(
		//		(int) Marshal::OffsetOf(managedInfo.GetType(), "cb"),
		//		(int) ((UINT8*) (&nativeInfo.cb) - (UINT8*) &nativeInfo));
		//	Assert::AreEqual(
		//		(int) Marshal::OffsetOf(managedInfo.GetType(), "abID"),
		//		(int) ((UINT8*) (&nativeInfo.abID) - (UINT8*) &nativeInfo));
		//}


	};
}

