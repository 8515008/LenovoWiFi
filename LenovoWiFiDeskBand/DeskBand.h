#pragma once

class CDeskBand : public IDeskBand2, public IObjectWithSite, public IPersistStream, public IInputObject, public IContextMenu
{
public:
	CDeskBand();

	STDMETHODIMP_(ULONG) AddRef();
	STDMETHODIMP QueryInterface(REFIID riid, void **ppvObject);
	STDMETHODIMP_(ULONG) Release();

	STDMETHODIMP ContextSensitiveHelp(BOOL fEnterMode);
	STDMETHODIMP GetWindow(HWND *phwnd);
	
	STDMETHODIMP CloseDW(DWORD dwReserved);
	STDMETHODIMP ResizeBorderDW(LPCRECT prcBorder, IUnknown *punkToolbarSite, BOOL fReserved);
	STDMETHODIMP ShowDW(BOOL bShow);

	STDMETHODIMP GetBandInfo(DWORD dwBandID, DWORD dwViewMode, DESKBANDINFO *pdbi);

	STDMETHODIMP CanRenderComposited(BOOL *pfCanRenderComposited);
	STDMETHODIMP GetCompositionState(BOOL *pfCompositionEnabled);
	STDMETHODIMP SetCompositionState(BOOL fCompositionEnabled);

	STDMETHODIMP GetSite(REFIID riid, void **ppvSite);
	STDMETHODIMP SetSite(IUnknown *pUnkSite);

	STDMETHODIMP GetClassID(CLSID *pClassID);

	STDMETHODIMP GetSizeMax(ULARGE_INTEGER *pcbSize);
	STDMETHODIMP IsDirty();
	STDMETHODIMP Load(IStream *pStm);
	STDMETHODIMP Save(IStream *pStm, BOOL fClearDirty);

	STDMETHODIMP HasFocusIO();
	STDMETHODIMP TranslateAcceleratorIO(LPMSG lpMsg);
	STDMETHODIMP UIActivateIO(BOOL fActivate, MSG *pMsg);

	STDMETHODIMP GetCommandString(UINT_PTR idCmd, UINT uFlags, UINT *pwReserved, LPSTR pszName, UINT cchMax);
	STDMETHODIMP InvokeCommand(LPCMINVOKECOMMANDINFO pici);
	STDMETHODIMP QueryContextMenu(HMENU hmenu, UINT indexMenu, UINT idCmdFirst, UINT idCmdLast, UINT uFlags);

protected:
	~CDeskBand();

	 LRESULT static CALLBACK WindowProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
	 void OnFocus(const BOOL bFocus);
	 void OnPaint(const HDC hDeviceContext);

private:
	LONG m_cRef;
	HWND m_hWnd;
	HWND m_hParentWnd;
	BOOL m_fFocus;
	BOOL m_fCompositionEnabled;
	HICON m_hIcon;
	UINT m_uFirstCommand;
	IInputObjectSite *m_pSite;

	UINT CONST IDM_CAPTION_RESTART_WIFI = 0;
	UINT CONST IDM_CAPTION_STOP_WIFI = 1;
	UINT CONST IDM_SEPARATOR_1_OFFSET = 2;
	UINT CONST IDM_CAPTION_SETTINGS = 3;
	UINT CONST IDM_SEPARATOR_2_OFFSET = 4;
	UINT CONST IDM_CAPTION_FEEDBACK = 5;
	UINT CONST IDM_CAPTION_HELP = 6;
	UINT CONST IDM_SEPARATOR_3_OFFSET = 7;
	UINT CONST IDM_CAPTION_EXIT = 8;
};

