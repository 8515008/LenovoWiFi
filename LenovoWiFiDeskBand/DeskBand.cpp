#include "stdafx.h"
#include <thread>
#include <functional>
#include <Uxtheme.h>
#include <Shlwapi.h>


#pragma comment(lib, "UxTheme.lib")

extern HINSTANCE g_hInstance;
extern CLSID CLSIDLenovoWiFiDeskBand;

CONST TCHAR g_szLenovoWiFiServiceName[] = TEXT("LenovoWiFi");
CONST TCHAR g_szDeskBandClassName[]		= TEXT("LenovoWiFiDeskBandWndClass");

CDeskBand::CDeskBand()
: m_cRef(1),
m_hWnd(NULL),
m_hParentWnd(NULL),
m_fFocus(FALSE),
m_fCompositionEnabled(FALSE),
m_hIcon(NULL),
m_hMenu(NULL),
m_pSite(NULL),
m_fMouseEnter(FALSE),
m_dwIconID(IDI_LOADING0)
{
	//2015/2/13 jxzhou
	//remove this code, otherwise, will cause COM CoGetObject return error. don't know why!!!

	//Log.i(L"CDeskBand::CDeskBand", L"CDeskBand::CDeskBand()1\n");

	//CNTService *pService = new CNTService(g_szLenovoWiFiServiceName);
	//if (pService->Exists() && pService->GetCurrentState() == SERVICE_RUNNING)
	//{
	//	Log.i(L"CDeskBand::CDeskBand", L"CDeskBand::CDeskBand()2\n");
	//	m_pServiceClient = new CHostedNetworkClient();
	//	Log.i(L"CDeskBand::CDeskBand", L"CDeskBand::CDeskBand()3\n");
	//}

	m_pUIPipeClient = new CUIPipeClient();
	m_pUIPipeClient->RegisterListener(this);

}


CDeskBand::~CDeskBand()
{
	if (m_hIcon)
	{
		DestroyIcon(m_hIcon);
	}

	if (m_hMenu)
	{
		DestroyMenu(m_hMenu);
	}

	if (m_pSite)
	{
		m_pSite->Release();
	}

	UnregisterClass(g_szDeskBandClassName, g_hInstance);
}

STDMETHODIMP_(ULONG) CDeskBand::AddRef()
{
	return InterlockedIncrement(&m_cRef);
}

STDMETHODIMP CDeskBand::QueryInterface(REFIID riid, void **ppvObject)
{
	HRESULT hr = S_OK;

	if (IsEqualIID(IID_IUnknown, riid)
		|| IsEqualIID(IID_IOleWindow, riid)
		|| IsEqualIID(IID_IDockingWindow, riid)
		|| IsEqualIID(IID_IDeskBand, riid)
		|| IsEqualIID(IID_IDeskBand2, riid))
	{
		*ppvObject = static_cast<IOleWindow *>(this);
	}
	else if (IsEqualIID(IID_IObjectWithSite, riid))
	{
		*ppvObject = static_cast<IObjectWithSite *>(this);
	}
	else if (IsEqualIID(IID_IPersist, riid)
			|| IsEqualIID(IID_IPersistStream, riid))
	{
		*ppvObject = static_cast<IPersist *>(this);
	}
	else if (IsEqualIID(IID_IInputObject, riid))
	{
		*ppvObject = static_cast<IInputObject *>(this);
	}
	else
	{
		hr = E_NOINTERFACE;
		*ppvObject = NULL;
	}

	if (*ppvObject)
	{
		AddRef();
	}

	return hr;
}

STDMETHODIMP_(ULONG) CDeskBand::Release()
{
	return InterlockedDecrement(&m_cRef);
}

STDMETHODIMP CDeskBand::ContextSensitiveHelp(BOOL fEnterMode)
{
	return E_NOTIMPL;
}

STDMETHODIMP CDeskBand::GetWindow(HWND *phwnd)
{
	*phwnd = m_hWnd;

	return S_OK;
}

STDMETHODIMP CDeskBand::CloseDW(DWORD dwReserved)
{
	if (m_hWnd)
	{
		ShowWindow(m_hWnd, SW_HIDE);
		DestroyWindow(m_hWnd);
		m_hWnd = NULL;
	}

	return S_OK;
}

STDMETHODIMP CDeskBand::ResizeBorderDW(LPCRECT prcBorder, IUnknown *punkToolbarSite, BOOL fReserved)
{
	return E_NOTIMPL;
}

STDMETHODIMP CDeskBand::ShowDW(BOOL bShow)
{
	if (m_hWnd)
	{
		ShowWindow(m_hWnd, bShow ? SW_SHOW : SW_HIDE);

		Log.i(L"CDeskBand::ResizeBorderDW", L"ShowWindow()\n");

	}

	return S_OK;
}

STDMETHODIMP CDeskBand::GetBandInfo(DWORD dwBandID, DWORD dwViewMode, DESKBANDINFO *pdbi)
{
	HRESULT hResult = S_OK;

	if (pdbi)
	{
		if (pdbi->dwMask & DBIM_MINSIZE)
		{
			pdbi->ptMinSize.x = 32;
			pdbi->ptMinSize.y = 32;
		}

		if (pdbi->dwMask & DBIM_MAXSIZE)
		{
			pdbi->ptMaxSize.y = -1;
		}

		if (pdbi->dwMask & DBIM_INTEGRAL)
		{
			pdbi->ptIntegral.y = 1;
		}

		if (pdbi->dwMask & DBIM_ACTUAL)
		{
			pdbi->ptActual.x = 32;
			pdbi->ptActual.y = 32;
		}

		if (pdbi->dwMask & DBIM_TITLE)
		{
			pdbi->dwMask &= ~DBIM_TITLE;
		}

		if (pdbi->dwMask & DBIM_MODEFLAGS)
		{
			pdbi->dwModeFlags = DBIMF_NORMAL | DBIMF_NOGRIPPER | DBIMF_NOMARGINS;
		}

		if (pdbi->dwMask & DBIM_BKCOLOR)
		{
			pdbi->dwMask &= ~DBIM_BKCOLOR;
		}
	}
	else
	{
		hResult = E_INVALIDARG;
	}

	return hResult;
}

STDMETHODIMP CDeskBand::CanRenderComposited(BOOL *pfCanRenderComposited)
{
	*pfCanRenderComposited = TRUE;

	return S_OK;
}

STDMETHODIMP CDeskBand::GetCompositionState(BOOL *pfCompositionEnabled)
{
	*pfCompositionEnabled = m_fCompositionEnabled;

	return S_OK;
}

STDMETHODIMP CDeskBand::SetCompositionState(BOOL fCompositionEnabled)
{
	HRESULT hResult = S_OK;

	m_fCompositionEnabled = fCompositionEnabled;

	if (m_hWnd)
	{
		InvalidateRect(m_hWnd, NULL, TRUE);
		UpdateWindow(m_hWnd);
	}
	else
	{
		hResult = E_NOT_VALID_STATE;
	}
	

	return hResult;
}

STDMETHODIMP CDeskBand::GetSite(REFIID riid, void **ppvSite)
{
	HRESULT hResult = E_FAIL;

	if (m_pSite)
	{
		hResult = m_pSite->QueryInterface(riid, ppvSite);
	}
	else
	{
		*ppvSite = NULL;
	}

	return hResult;
}

STDMETHODIMP CDeskBand::SetSite(IUnknown *pUnkSite)
{
	HRESULT hResult = S_OK;

	m_hParentWnd = NULL;

	if (m_pSite)
	{
		m_pSite->Release();
	}

	if (pUnkSite)
	{
		IOleWindow *pOleWindow;
		hResult = pUnkSite->QueryInterface(IID_IOleWindow, reinterpret_cast<void **>(&pOleWindow));

		if (SUCCEEDED(hResult))
		{
			hResult = pOleWindow->GetWindow(&m_hParentWnd);
			if (SUCCEEDED(hResult))
			{
				Log.i(L"CDeskBand::SetSite", L"CreateWindowEx()\n");

				WNDCLASS wndClass = { 0 };
				wndClass.style = CS_HREDRAW | CS_VREDRAW;
				wndClass.hCursor = LoadCursor(NULL, IDC_ARROW);
				wndClass.hInstance = g_hInstance;
				wndClass.lpfnWndProc = WindowProc;
				wndClass.lpszClassName = g_szDeskBandClassName;
				wndClass.hbrBackground = CreateSolidBrush(COLOR_WINDOW);
				wndClass.lpszMenuName = MAKEINTRESOURCE(IDR_MENU);

				RegisterClass(&wndClass);

				HWND hWnd = 
				CreateWindowEx(
					WS_EX_LEFT,
					g_szDeskBandClassName,
					NULL,
					WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS,
					0, 0, 0, 0,
					m_hParentWnd,
					NULL,
					g_hInstance,
					this);

				if (!hWnd)
				{
					hResult = E_FAIL;
				}
				else
				{
					ShowWindow(hWnd, SW_SHOW);
				}
			}

			pOleWindow->Release();
		}

		hResult = pUnkSite->QueryInterface(IID_IInputObjectSite, reinterpret_cast<void **>(&m_pSite));
	}

	return hResult;
}

LRESULT CALLBACK CDeskBand::WindowProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	LRESULT lResult = 0;

	CDeskBand *pDeskband = reinterpret_cast<CDeskBand *>(GetWindowLongPtr(hWnd, GWLP_USERDATA));

	switch (uMsg)
	{
	case WM_CREATE:
		pDeskband = reinterpret_cast<CDeskBand *>(reinterpret_cast<CREATESTRUCT *>(lParam)->lpCreateParams);
		pDeskband->m_hWnd = hWnd;
		SetWindowLongPtr(hWnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(pDeskband));

		pDeskband->OnThreadSetupPipe();

		Log.i(L"CDeskBand::WindowProc", L"OnThreadSetupPipe()\n");


		break;
	case WM_SETFOCUS:
		pDeskband->OnFocus(TRUE);
		break;
	case WM_KILLFOCUS:
		pDeskband->OnFocus(FALSE);
		break;
	case WM_PAINT:
		pDeskband->OnPaint(NULL);
		break;
	case WM_PRINTCLIENT:
		pDeskband->OnPaint(reinterpret_cast<HDC>(wParam));
		break;
	case WM_CONTEXTMENU:
		pDeskband->OnContextMenu(hWnd, GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
		break;
	case WM_ERASEBKGND:
		if (pDeskband->m_fCompositionEnabled)
		{			
			lResult = 1;
		}
		break;
	case WM_MOUSEMOVE:
		if (!pDeskband->m_fMouseEnter)
		{
			TRACKMOUSEEVENT eventTrack;
			ZeroMemory(&eventTrack, sizeof(TRACKMOUSEEVENT));
			eventTrack.cbSize = sizeof(TRACKMOUSEEVENT);
			eventTrack.dwFlags = TME_LEAVE;
			eventTrack.hwndTrack = hWnd;

			TrackMouseEvent(&eventTrack);

			pDeskband->m_fMouseEnter = TRUE;
			pDeskband->OnMouseEnter();
		}
		break;
	case WM_MOUSELEAVE:
		pDeskband->m_fMouseEnter = FALSE;
		pDeskband->OnMouseLeave();
		break;
	case WM_LBUTTONDOWN:
		pDeskband->OnLeftButtonClick();
		break;
	}

	if (uMsg != WM_ERASEBKGND && uMsg != WM_CONTEXTMENU)
	{
		lResult = DefWindowProc(hWnd, uMsg, wParam, lParam);
	}

	return lResult;
}

void CDeskBand::OnFocus(const BOOL fFocus)
{
	m_fFocus = fFocus;

	if (m_pSite)
	{
		m_pSite->OnFocusChangeIS(static_cast<IOleWindow*>(this), m_fFocus);
	}
}

void CDeskBand::OnPaint(const HDC hDeviceContext)
{
	HDC hdc = hDeviceContext;
	PAINTSTRUCT ps;

	Log.i(L"CDeskBand::OnPaint", L"1()\n");

	if (!hdc)
	{
		hdc = BeginPaint(m_hWnd, &ps);
	}

	if (hdc)
	{
		RECT rc;
		GetClientRect(m_hWnd, &rc);
		BOOL b = DrawThemeParentBackground(m_hWnd, hdc, &rc);
		int err = GetLastError();
		Log.i(L"CDeskBand::OnPaint", L"2()\n");

		m_hIcon = LoadIcon(g_hInstance, MAKEINTRESOURCE(m_dwIconID));

		b = DrawIcon(hdc, 0, 0, m_hIcon);
		Log.i(L"CDeskBand::OnPaint", L"3()\n");
	}

	if (!hDeviceContext)
	{
		EndPaint(m_hWnd, &ps);
	}
}

void CDeskBand::DynamicContextMenu(const HWND hWnd, POINT point)
{
	HMENU hmenu = CreateMenu();
	HMENU hMenuPopup = CreateMenu();

	//Todo: 根据条件动态添加子项
	AppendMenu(hMenuPopup, MF_STRING, ID_RESTART_WIFI, TEXT("重启WIFI"));
	AppendMenu(hMenuPopup, MF_STRING, ID_EXIT, TEXT("退出"));

	AppendMenu(hmenu, MF_POPUP, (UINT_PTR)hMenuPopup, TEXT("弹出菜单"));

	UINT uMenuItemID = TrackPopupMenu(hMenuPopup,
		TPM_LEFTALIGN | TPM_RIGHTBUTTON | TPM_RETURNCMD,
		point.x, point.y, 0, hWnd, NULL);

	switch (uMenuItemID)
	{
	//TODO: check if client existed. if yes, send pipe cmd, else start it and send pipe cmd again.
	case ID_RESTART_WIFI:
		OnICS_Off();
		OnICS_Loading();
		//m_pServiceClient->RestartHostedNetwork();
		m_pUIPipeClient->Send(CMD_RESTARTWIFI);

		OnICS_On();

		break;
	case ID_STOP_WIFI:
		//m_pServiceClient->StopHostedNetwork();
		OnICS_Off();

		break;
	case ID_SETTINGS:
		break;
	case ID_FEEDBACK:
		break;
	case ID_HELP:
		break;
	case ID_EXIT:
		m_pUIPipeClient->Send(CMD_EXIT);
		break;
	default:
		break;
	}

	DestroyMenu(hmenu);
}

void CDeskBand::OnContextMenu(const HWND hWnd, const int xPos, const int yPos)
{
	m_pUIPipeClient->Send(CMD_RBUTTONCLICK);

	RECT rect;
	GetClientRect(hWnd, &rect);

	POINT point = { xPos, yPos };
	ScreenToClient(hWnd, &point);

	if (PtInRect(&rect, point))
	{
		ClientToScreen(hWnd, &point);
		
		DynamicContextMenu(hWnd, point);
/*
		HMENU hMenu = LoadMenu(g_hInstance, MAKEINTRESOURCE(IDR_MENU));

		if (hMenu == NULL)
		{
			return;
		}

		HMENU hMenuPopup = GetSubMenu(hMenu, 0);

		if (hMenuPopup == NULL)
		{
			return;
		}
		
		m_hMenu = hMenuPopup;

		UINT uMenuItemID = TrackPopupMenu(hMenuPopup,
			TPM_LEFTALIGN | TPM_RIGHTBUTTON | TPM_RETURNCMD,
			point.x, point.y, 0, hWnd, NULL);

		switch (uMenuItemID)
		{
		case ID_RESTART_WIFI:
			m_pServiceClient->RestartHostedNetwork();
			break;
		case ID_STOP_WIFI:
			m_pServiceClient->StopHostedNetwork();
			break;
		case ID_SETTINGS:
			break;
		case ID_FEEDBACK:
			break;
		case ID_HELP:
			break;
		case ID_EXIT:
			m_pUIPipeClient->Send(TEXT("exit\r\n"));
			break;
		default:
			break;
		}

		DestroyMenu(hMenu);
*/
	}
}

void CDeskBand::OnMouseEnter()
{
	m_pUIPipeClient->Send(CMD_MOUSEENTER);
}

void CDeskBand::OnLeftButtonClick()
{
	m_pUIPipeClient->Send(CMD_LBUTTONCLICK);
}

void CDeskBand::OnMouseLeave()
{
	m_pUIPipeClient->Send(CMD_MOUSELEAVE);
}

void CDeskBand::InitLog()
{
	// path: %temp%\lenovo\wifi\deskband.log
	char logpath[512] = { 0 };
	GetTempPathA(sizeof(logpath) / sizeof(logpath[0]), logpath);
	PathAppendA(logpath, "lenovo");
	CreateDirectoryA(logpath, NULL);

	PathAppendA(logpath, "wifi");
	CreateDirectoryA(logpath, NULL);

	PathAppendA(logpath, "deskband.log");

	//RegQueryValueEx(HKEY_CURRENT_USER,L"SOFTWARE\\Lenovo\\Easyplus",L"LogLevel",LOG_LEVEL_WARN); 

	Log.add(new DebugLogger());
	Log.add(new FileLogger(logpath));
}

void CDeskBand::OnThreadSetupPipe()
{

	std::thread thread(std::bind(&CDeskBand::OnThreadSetupPipe2, this));
	thread.detach();
}

void CDeskBand::OnThreadSetupPipe2()
{
	DWORD dwRet = m_pUIPipeClient->Connect();
	if (ERROR_SUCCESS == dwRet)
	{
		m_pUIPipeClient->Send(CMD_HANDSHAKE);
	}

}

void CDeskBand::OnICS_Loading()
{
	//TODO: set timer to update the icon in future.
	m_dwIconID = IDI_LOADING1;

	RECT rc;
	GetClientRect(m_hWnd, &rc);
	InvalidateRect(m_hWnd, &rc, TRUE);
}

void CDeskBand::OnICS_On()
{
	m_dwIconID = IDI_READY;

	RECT rc;
	GetClientRect(m_hWnd, &rc);
	InvalidateRect(m_hWnd, &rc, TRUE);
}

void CDeskBand::OnICS_Off()
{
	m_dwIconID = IDI_DISABLED;
	RECT rc;
	GetClientRect(m_hWnd, &rc);
	InvalidateRect(m_hWnd, &rc, TRUE);
}

void CDeskBand::OnICS_ClientConnected()
{
	m_dwIconID = IDI_CONECTED;
	RECT rc;
	GetClientRect(m_hWnd, &rc);
	InvalidateRect(m_hWnd, &rc, TRUE);
}

STDMETHODIMP CDeskBand::GetClassID(CLSID *pClassID)
{
	*pClassID = CLSIDLenovoWiFiDeskBand;

	return S_OK;
}

STDMETHODIMP CDeskBand::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

STDMETHODIMP CDeskBand::IsDirty()
{
	return E_NOTIMPL;
}

STDMETHODIMP CDeskBand::Load(IStream *pStm)
{
	return E_NOTIMPL;
}

STDMETHODIMP CDeskBand::Save(IStream *pStm, BOOL fClearDirty)
{
	return E_NOTIMPL;
}

STDMETHODIMP CDeskBand::HasFocusIO()
{
	return m_fFocus ? S_OK : S_FALSE;
}

STDMETHODIMP CDeskBand::TranslateAcceleratorIO(LPMSG lpMsg)
{
	return S_FALSE;
} 

STDMETHODIMP CDeskBand::UIActivateIO(BOOL fActivate, MSG *pMsg)
{
	if (fActivate)
	{
		SetFocus(m_hWnd);
	}

	return S_OK;
}