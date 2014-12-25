#include "stdafx.h"

extern long g_cDllRef;

CClassFactory::CClassFactory()
	: m_cRef(1)
{
	InterlockedIncrement(&g_cDllRef);
}


CClassFactory::~CClassFactory()
{
	InterlockedDecrement(&g_cDllRef);
}

STDMETHODIMP_(ULONG) CClassFactory::AddRef()
{
	return InterlockedIncrement(&m_cRef);
}

STDMETHODIMP CClassFactory::QueryInterface(REFIID riid, void **ppvObject)
{
	HRESULT hr = S_OK;

	if (IsEqualIID(IID_IUnknown, riid) || IsEqualIID(IID_IClassFactory, riid))
	{
		*ppvObject = static_cast<IUnknown *>(this);
		AddRef();
	}
	else
	{
		hr = E_NOINTERFACE;
		*ppvObject = NULL;
	}

	return hr;
}

STDMETHODIMP_(ULONG) CClassFactory::Release()
{
	ULONG ref = InterlockedDecrement(&m_cRef);
	
	if (ref == 0UL)
	{
		delete this;
	}

	return ref;
}

STDMETHODIMP CClassFactory::CreateInstance(IUnknown *pUnkOuter, REFIID riid, void **ppvObject)
{
	HRESULT hr = CLASS_E_NOAGGREGATION;

	if (!pUnkOuter)
	{
		hr = E_OUTOFMEMORY;

		CDeskBand *pDeskBand = new CDeskBand();
		if (pDeskBand)
		{
			hr = pDeskBand->QueryInterface(riid, ppvObject);
			pDeskBand->Release();
		}
	}

	return hr;
}

STDMETHODIMP CClassFactory::LockServer(BOOL fLock)
{
	if (fLock)
	{
		InterlockedIncrement(&g_cDllRef);
	}
	else
	{
		InterlockedDecrement(&g_cDllRef);
	}

	return S_OK;
}