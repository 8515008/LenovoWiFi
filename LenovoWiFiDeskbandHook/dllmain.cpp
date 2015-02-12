// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"

#ifdef _DEBUG
//LPCTSTR DIALOG_WINDOW_NAME = TEXT("Address");
LPCTSTR DIALOG_WINDOW_NAME = TEXT("Lenovo WiFi");
#else
LPCTSTR DIALOG_WINDOW_NAME = TEXT("Lenovo WiFi");
#endif
LPCTSTR PANEL_CLASS_NAME = TEXT("DirectUIHWND");
LPCTSTR SINK_CLASS_NAME = TEXT("CtrlNotifySink");
LPCTSTR BUTTON_CLASS_NAME = TEXT("Button");

INT CONST INDEX_SINK_HAS_BUTTON_YES = 6;

HINSTANCE g_hInstance;
HHOOK g_hHook;

STDAPI_(BOOL) APIENTRY DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	switch (fdwReason)
	{
	case DLL_PROCESS_ATTACH:
		g_hInstance = hinstDLL;
		break;
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

LRESULT CALLBACK CallWndProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	if (nCode == HC_ACTION)
	{
		PCWPRETSTRUCT pCWPStruct = (PCWPRETSTRUCT)lParam;

		if (pCWPStruct && pCWPStruct->message == WM_INITDIALOG)
		{
			TCHAR szCaption[32] = { 0 };
			GetWindowText(pCWPStruct->hwnd, szCaption, 32);

			if (lstrcmp(szCaption, DIALOG_WINDOW_NAME) == 0)
			{
				HWND panel = FindWindowEx(pCWPStruct->hwnd, NULL, PANEL_CLASS_NAME, TEXT(""));

				if (panel)
				{
					HWND sink = NULL;
					INT index = 0;

					while (index <= INDEX_SINK_HAS_BUTTON_YES)
					{
						sink = FindWindowEx(panel, sink, SINK_CLASS_NAME, NULL);
						index++;
					}

					if (sink)
					{
						HWND button = FindWindowEx(sink, NULL, BUTTON_CLASS_NAME, NULL);
						PostMessage(sink, WM_COMMAND, BN_CLICKED, (LPARAM)button);
					}
				}
			}
		}
	}

	return CallNextHookEx(g_hHook, nCode, wParam, lParam);
}

STDAPI_(VOID) StopHook()
{
	if (g_hHook)
	{
		UnhookWindowsHookEx(g_hHook);
	}
}

STDAPI_(VOID) StartHook()
{
	StopHook();

	HHOOK hook = SetWindowsHookEx(WH_CALLWNDPROCRET, CallWndProc, g_hInstance, NULL);

	if (hook)
	{
		g_hHook = hook;
	}
}

