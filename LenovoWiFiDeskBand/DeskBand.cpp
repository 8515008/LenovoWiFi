#include "stdafx.h"

extern HINSTANCE g_hInstance;
extern CLSID CLSIDLenovoWiFiDeskBand;

const TCHAR g_szDeskBandClassName[] = L"LenovoWiFiDeskBandWndClass";

CDeskBand::CDeskBand()
	: m_cRef(1),
	  m_hWnd(NULL),
	  m_hParentWnd(NULL),
	  m_fFocus(FALSE),
	  m_fCompositionEnabled(FALSE),
	  m_hIcon(NULL),
	  m_pSite(NULL),
	  m_uFirstCommand(0u)
{
}


CDeskBand::~CDeskBand()
{
	if (m_hIcon)
	{
		DestroyIcon(m_hIcon);
	}

	if (m_pSite)
	{
		m_pSite->Release();
	}
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
	else if (IsEqualIID(IID_IContextMenu, riid))
	{
		*ppvObject = static_cast<IContextMenu *>(this);
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
			pdbi->ptMinSize.x = 30;
			pdbi->ptMinSize.y = 30;
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
			pdbi->ptActual.x = 30;
			pdbi->ptActual.y = 30;
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
				WNDCLASS wndClass = { 0 };
				wndClass.style = CS_HREDRAW | CS_VREDRAW;
				wndClass.hCursor = LoadCursor(NULL, IDC_ARROW);
				wndClass.hInstance = g_hInstance;
				wndClass.lpfnWndProc = WindowProc;
				wndClass.lpszClassName = g_szDeskBandClassName;
				wndClass.hbrBackground = CreateSolidBrush(COLOR_WINDOW);

				RegisterClassW(&wndClass);

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

				if (!m_hWnd)
				{
					hResult = E_FAIL;
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

	CDeskBand *pDeskBand = reinterpret_cast<CDeskBand *>(GetWindowLongPtr(hWnd, GWLP_USERDATA));

	switch (uMsg)
	{
	case WM_CREATE:
		pDeskBand = reinterpret_cast<CDeskBand *>(reinterpret_cast<CREATESTRUCT *>(lParam)->lpCreateParams);
		pDeskBand->m_hWnd = hWnd;
		SetWindowLongPtr(hWnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(pDeskBand));
		break;
	case WM_SETFOCUS:
		pDeskBand->OnFocus(TRUE);
		break;
	case WM_KILLFOCUS:
		pDeskBand->OnFocus(FALSE);
		break;
	case WM_PAINT:
		pDeskBand->OnPaint(NULL);
		break;
	case WM_PRINTCLIENT:
		pDeskBand->OnPaint(reinterpret_cast<HDC>(wParam));
		break;
	case WM_ERASEBKGND:
		if (pDeskBand->m_fCompositionEnabled)
		{
			lResult = 1;
		}
		break;
	}

	if (uMsg != WM_ERASEBKGND)
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

	if (!hdc)
	{
		hdc = BeginPaint(m_hWnd, &ps);
	}

	if (hdc)
	{
		m_hIcon = LoadIcon(g_hInstance, MAKEINTRESOURCE(103));
		DrawIcon(hdc, 0, 0, m_hIcon);
	}

	if (!hDeviceContext)
	{
		EndPaint(m_hWnd, &ps);
	}
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

STDMETHODIMP CDeskBand::GetCommandString(UINT_PTR idCmd, UINT uFlags, UINT *pwReserved, LPSTR pszName, UINT cchMax)
{
	return S_OK;
}

STDMETHODIMP CDeskBand::InvokeCommand(LPCMINVOKECOMMANDINFO pici)
{
	if (!pici)
	{
		return E_INVALIDARG;
	}

	if (LOWORD(pici->lpVerb) == m_uFirstCommand + IDM_CAPTION_RESTART_WIFI)
	{
		
	}

	if (LOWORD(pici->lpVerb) == m_uFirstCommand + IDM_CAPTION_STOP_WIFI)
	{

	}

	if (LOWORD(pici->lpVerb) == m_uFirstCommand + IDM_CAPTION_SETTINGS)
	{

	}

	if (LOWORD(pici->lpVerb) == m_uFirstCommand + IDM_CAPTION_FEEDBACK)
	{

	}

	if (LOWORD(pici->lpVerb) == m_uFirstCommand + IDM_CAPTION_HELP)
	{

	}

	if (LOWORD(pici->lpVerb) == m_uFirstCommand + IDM_CAPTION_EXIT)
	{

	}

	return S_OK;
}

STDMETHODIMP CDeskBand::QueryContextMenu(HMENU hmenu, UINT indexMenu, UINT idCmdFirst, UINT idCmdLast, UINT uFlags)
{
	if (uFlags & CMF_DEFAULTONLY)
		return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, 0);

	m_uFirstCommand = idCmdFirst;

	CONST UINT CAPTION_LENGTH = 10;
	TCHAR pszCaption[CAPTION_LENGTH];

	ZeroMemory(pszCaption, CAPTION_LENGTH);
	LoadString(g_hInstance, IDS_RESTART_WIFI, pszCaption, CAPTION_LENGTH);
	InsertMenu(hmenu, indexMenu, MF_STRING | MF_BYPOSITION, idCmdFirst + IDM_CAPTION_RESTART_WIFI, pszCaption);

	ZeroMemory(pszCaption, CAPTION_LENGTH);
	LoadString(g_hInstance, IDS_STOP_WIFI, pszCaption, sizeof(pszCaption) / sizeof(TCHAR));
	InsertMenu(hmenu, indexMenu, MF_STRING | MF_BYPOSITION, idCmdFirst + IDM_CAPTION_STOP_WIFI, pszCaption);

	InsertMenu(hmenu, indexMenu, MF_SEPARATOR | MF_BYPOSITION, idCmdFirst + IDM_SEPARATOR_1_OFFSET, 0);

	ZeroMemory(pszCaption, CAPTION_LENGTH);
	LoadString(g_hInstance, IDS_SETTINGS, pszCaption, sizeof(pszCaption) / sizeof(TCHAR));
	InsertMenu(hmenu, indexMenu, MF_STRING | MF_BYPOSITION, idCmdFirst + IDM_CAPTION_SETTINGS, pszCaption);

	InsertMenu(hmenu, indexMenu, MF_SEPARATOR | MF_BYPOSITION, idCmdFirst + IDM_SEPARATOR_2_OFFSET, 0);

	ZeroMemory(pszCaption, CAPTION_LENGTH);
	LoadString(g_hInstance, IDS_FEEDBACK, pszCaption, sizeof(pszCaption) / sizeof(TCHAR));
	InsertMenu(hmenu, indexMenu, MF_STRING | MF_BYPOSITION, idCmdFirst + IDM_CAPTION_FEEDBACK, pszCaption);

	ZeroMemory(pszCaption, CAPTION_LENGTH);
	LoadString(g_hInstance, IDS_HELP, pszCaption, sizeof(pszCaption) / sizeof(TCHAR));
	InsertMenu(hmenu, indexMenu, MF_STRING | MF_BYPOSITION, idCmdFirst + IDM_CAPTION_HELP, pszCaption);

	InsertMenu(hmenu, indexMenu, MF_SEPARATOR | MF_BYPOSITION, idCmdFirst + IDM_SEPARATOR_3_OFFSET, 0);

	ZeroMemory(pszCaption, CAPTION_LENGTH);
	LoadString(g_hInstance, IDS_EXIT, pszCaption, sizeof(pszCaption) / sizeof(TCHAR));
	InsertMenu(hmenu, indexMenu, MF_STRING | MF_BYPOSITION, idCmdFirst + IDM_CAPTION_EXIT, pszCaption);

	return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, IDM_CAPTION_EXIT + 1);
}