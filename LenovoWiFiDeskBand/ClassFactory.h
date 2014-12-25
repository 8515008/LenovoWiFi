#pragma once

class CClassFactory : public IClassFactory
{
public:
	CClassFactory();

	STDMETHODIMP_(ULONG) AddRef();
	STDMETHODIMP QueryInterface(REFIID riid, void **ppvObject);
	STDMETHODIMP_(ULONG) Release();

	STDMETHODIMP CreateInstance(IUnknown *pUnkOuter, REFIID riid, void **ppvObject);
	STDMETHODIMP LockServer(BOOL fLock);

protected:
	~CClassFactory();

private:
	LONG m_cRef;
};

