// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"

// {23ED1551-904E-4874-BA46-DBE1489D4D34}
CLSID CONST CLSIDLenovoWiFiDeskBand = 
	{ 0x23ed1551, 0x904e, 0x4874, { 0xba, 0x46, 0xdb, 0xe1, 0x48, 0x9d, 0x4d, 0x34 } };

TCHAR CONST LENOVO_WIFI_DESKBAND_NAME[] = L"Lenovo Wi-Fi Deskband";

HINSTANCE g_hInstance = NULL;
long g_cDllRef = 0;

STDAPI_(BOOL) DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		g_hInstance = hinstDLL;
		DisableThreadLibraryCalls(hinstDLL);
	}
	return TRUE;
}

STDAPI DllGetClassObject(REFCLSID rCLSID, REFIID rIID, void **ppv)
{
	HRESULT hr = CLASS_E_CLASSNOTAVAILABLE;

	if (IsEqualCLSID(CLSIDLenovoWiFiDeskBand, rCLSID))
	{
		hr = E_OUTOFMEMORY;

		CClassFactory *pClassFactory = new CClassFactory();
		if (pClassFactory)
		{
			hr = pClassFactory->QueryInterface(rIID, ppv);
			pClassFactory->Release();
		}
	}

	return hr;
}

STDAPI DllCanUnloadNow()
{
	return g_cDllRef > 0 ? S_FALSE : S_OK;
}

HRESULT RegisterServer()
{
	TCHAR szCLSID[MAX_PATH];
	StringFromGUID2(CLSIDLenovoWiFiDeskBand, szCLSID, ARRAYSIZE(szCLSID));

	TCHAR szSubkey[MAX_PATH];
	HKEY hKey;

	HRESULT hResult = StringCchPrintf(szSubkey, ARRAYSIZE(szSubkey), L"CLSID\\%s", szCLSID);
	if (SUCCEEDED(hResult))
	{
		hResult = E_FAIL;
		if (ERROR_SUCCESS == RegCreateKeyEx(
			HKEY_CLASSES_ROOT,
			szSubkey,
			0,
			NULL,
			REG_OPTION_NON_VOLATILE,
			KEY_WRITE,
			NULL,
			&hKey,
			NULL))
		{
			if (ERROR_SUCCESS == RegSetValueEx(hKey,
				NULL,
				0,
				REG_SZ,
				(LPBYTE)LENOVO_WIFI_DESKBAND_NAME,
				sizeof(LENOVO_WIFI_DESKBAND_NAME)))
			{
				hResult = S_OK;
			}

			RegCloseKey(hKey);
		}
	}

	if (SUCCEEDED(hResult))
	{
		hResult = StringCchPrintf(szSubkey, ARRAYSIZE(szSubkey), L"CLSID\\%s\\InprocServer32", szCLSID);
		if (SUCCEEDED(hResult))
		{
			hResult = HRESULT_FROM_WIN32(
						RegCreateKeyEx(
							HKEY_CLASSES_ROOT,
							szSubkey,
							0,
							NULL,
							REG_OPTION_NON_VOLATILE,
							KEY_WRITE,
							NULL,
							&hKey,
							NULL));

			if (SUCCEEDED(hResult))
			{
				TCHAR szModule[MAX_PATH];
				if (GetModuleFileNameW(g_hInstance, szModule, ARRAYSIZE(szModule)))
				{
					DWORD cch = lstrlen(szModule);
					hResult = HRESULT_FROM_WIN32(
								RegSetValueEx(
									hKey,
									NULL,
									0,
									REG_SZ,
									(LPBYTE)szModule,
									cch * sizeof(szModule[0])));
				}

				if (SUCCEEDED(hResult))
				{
					WCHAR CONST szModel[] = L"Apartment";
					hResult = HRESULT_FROM_WIN32(
								RegSetValueEx(
									hKey,
									L"ThreadingModel",
									0,
									REG_SZ,
									(LPBYTE)szModel,
									sizeof(szModel)));
				}

				RegCloseKey(hKey);
			}
		}
	}

	return hResult;
}

HRESULT RegisterComCat()
{
	ICatRegister *pCatRegister;
	HRESULT hr = CoCreateInstance(CLSID_StdComponentCategoriesMgr, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pCatRegister));
	if (SUCCEEDED(hr))
	{
		CATID catid = CATID_DeskBand;
		hr = pCatRegister->RegisterClassImplCategories(CLSIDLenovoWiFiDeskBand, 1, &catid);
		pCatRegister->Release();
	}
	return hr;
}

STDAPI DllRegisterServer()
{
	HRESULT hResult = RegisterServer();
	if (SUCCEEDED(hResult))
	{
		hResult = RegisterComCat();
	}

	return SUCCEEDED(hResult) ? S_OK : SELFREG_E_CLASS;
}

STDAPI DllUnregisterServer()
{
	TCHAR szCLSID[MAX_PATH];
	StringFromGUID2(CLSIDLenovoWiFiDeskBand, szCLSID, ARRAYSIZE(szCLSID));

	TCHAR szSubkey[MAX_PATH];
	HRESULT hResult = StringCchPrintf(szSubkey, ARRAYSIZE(szSubkey), L"CLSID\\%s", szCLSID);
	if (SUCCEEDED(hResult))
	{
		if (ERROR_SUCCESS != RegDeleteTree(HKEY_CLASSES_ROOT, szSubkey))
		{
			hResult = E_FAIL;
		}
	}

	return SUCCEEDED(hResult) ? S_OK : SELFREG_E_CLASS;
}