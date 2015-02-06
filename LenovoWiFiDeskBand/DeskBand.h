#pragma once

class CDeskBand : public IDeskBand2, public IObjectWithSite, public IPersistStream, public IInputObject, public IDeskbandListener
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
	 void OnMouseEnter();
	 void OnLeftButtonClick();
	 void OnMouseLeave();
	 void DynamicContextMenu(const HWND hWnd, POINT point);

	 void OnThreadSetupPipe();
	 void OnThreadSetupPipe2();
	 virtual void OnICS_Loading();
	 virtual void OnICS_On();
	 virtual void OnICS_Off();
	 virtual void OnICS_ClientConnected();
private:
	LONG m_cRef;
	HWND m_hWnd;
	HWND m_hParentWnd;
	BOOL m_fFocus;
	BOOL m_fCompositionEnabled;
	HICON m_hIcon;
	HMENU m_hMenu;
	IInputObjectSite *m_pSite;
	CHostedNetworkClient *m_pServiceClient;
	CUIPipeClient *m_pUIPipeClient;
	BOOL m_fMouseEnter;

	DWORD m_dwIconID;
};

