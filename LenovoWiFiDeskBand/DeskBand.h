#pragma once

class CDeskBand : public IDeskBand2, public IObjectWithSite, public IPersistStream, public IInputObject
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

protected:
	~CDeskBand();

	 LRESULT static CALLBACK WindowProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
	 void OnFocus(const BOOL bFocus);
	 void OnPaint(const HDC hDeviceContext);
	 void OnContextMenu(const HWND hWnd, const int xPos, const int yPos);

private:
	LONG m_cRef;
	HWND m_hWnd;
	HWND m_hParentWnd;
	BOOL m_fFocus;
	BOOL m_fCompositionEnabled;
	HICON m_hIcon;
	HMENU m_hMenu;
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

