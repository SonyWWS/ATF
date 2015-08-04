//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "stdafx.h"

using namespace System;
using namespace System::Reflection;
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
	public ref class TestDllImports
	{
	public:
		[Test]
		static void TestSendMessage()
		{
			array<Type^> ^params = gcnew array<Type^>(4);
			params[0] = IntPtr::typeid;
			params[1] = int::typeid; //should have used uint, but it's safe to use int instead
			params[2] = IntPtr::typeid;
			params[3] = IntPtr::typeid;

			MethodInfo ^m = (User32::typeid)->GetMethod("SendMessage", params);

			Assert::IsNotNull(m);

			// Native version:
			//LRESULT
			//	SendMessage(
			//	HWND hWnd,
			//	UINT Msg,
			//	WPARAM wParam,
			//	LPARAM lParam
			//	)
			
			// Return type:
			Assert::AreEqual(Marshal::SizeOf(m->ReturnType), (int)sizeof(LRESULT));

			// Parameter types:
			Assert::AreEqual(Marshal::SizeOf(params[0]), (int)sizeof(HWND));
			Assert::AreEqual(Marshal::SizeOf(params[1]), (int)sizeof(UINT));
			Assert::AreEqual(Marshal::SizeOf(params[2]), (int)sizeof(WPARAM));
			Assert::AreEqual(Marshal::SizeOf(params[3]), (int)sizeof(LPARAM));
		}

		[Test]
		static void TestPostMessage()
		{
			array<Type^> ^params = gcnew array<Type^>(4);
			params[0] = IntPtr::typeid;
			params[1] = UInt32::typeid;
			params[2] = IntPtr::typeid;
			params[3] = IntPtr::typeid;

			MethodInfo ^m = (User32::typeid)->GetMethod("PostMessage", params);

			Assert::IsNotNull(m);

			// Native version:
			//WINUSERAPI
			//	BOOL
			//	WINAPI
			//	PostMessageW(
			//	__in_opt HWND hWnd,
			//	__in UINT Msg,
			//	__in WPARAM wParam,
			//	__in LPARAM lParam);

			// Return type:
			Assert::AreEqual(Marshal::SizeOf(m->ReturnType), (int)sizeof(BOOL));

			// Parameter types:
			Assert::AreEqual(Marshal::SizeOf(params[0]), (int)sizeof(HWND));
			Assert::AreEqual(Marshal::SizeOf(params[1]), (int)sizeof(UINT));
			Assert::AreEqual(Marshal::SizeOf(params[2]), (int)sizeof(WPARAM));
			Assert::AreEqual(Marshal::SizeOf(params[3]), (int)sizeof(LPARAM));
		}

		[Test]
		static void TestSetWindowsHookEx()
		{
			array<Type^> ^params = gcnew array<Type^>(4);
			params[0] = Sce::Atf::User32::HookType::typeid;
			params[1] = Sce::Atf::User32::WindowsHookCallback::typeid;
			params[2] = IntPtr::typeid;
			params[3] = int::typeid;

			MethodInfo ^m = (User32::typeid)->GetMethod("SetWindowsHookEx", params);

			Assert::IsNotNull(m);

			// Native version:
			//WINUSERAPI
			//	HHOOK
			//	WINAPI
			//	SetWindowsHookExW(
			//	__in int idHook,
			//	__in HOOKPROC lpfn,
			//	__in_opt HINSTANCE hmod,
			//	__in DWORD dwThreadId);

			// Return type:
			Assert::AreEqual(Marshal::SizeOf(m->ReturnType), (int)sizeof(IntPtr));

			// Parameter types:
			// Annoying. Marshal won't calculate the size of an enum, even though it's stored as an int.
			Assert::AreEqual(Marshal::SizeOf(int::typeid), (int)sizeof(int));
			// Annoying. Marshal won't calculate the size of a function pointer.
			Assert::AreEqual(Marshal::SizeOf(IntPtr::typeid), (int)sizeof(HOOKPROC));
			Assert::AreEqual(Marshal::SizeOf(params[2]), (int)sizeof(HINSTANCE));
			Assert::AreEqual(Marshal::SizeOf(params[3]), (int)sizeof(DWORD));
		}

		[Test]
		static void TestCopyMemory()
		{
			array<Type^> ^params = gcnew array<Type^>(3);
			params[0] = IntPtr::typeid;
			params[1] = IntPtr::typeid;
			params[2] = UIntPtr::typeid;

			MethodInfo ^m = (Kernel32::typeid)->GetMethod("CopyMemory", params);

			Assert::IsNotNull(m);

			// _CRT_INSECURE_DEPRECATE_MEMORY(memcpy_s) void *  __cdecl memcpy(_Out_opt_bytecapcount_(_Size) void * _Dst, _In_opt_bytecount_(_Size) const void * _Src, _In_ size_t _Size);
			Assert::AreEqual(Marshal::SizeOf(params[0]), (int)sizeof(void*));
			Assert::AreEqual(Marshal::SizeOf(params[1]), (int)sizeof(const void*));
			Assert::AreEqual(Marshal::SizeOf(params[2]), (int)sizeof(size_t));
		}
	};
}
