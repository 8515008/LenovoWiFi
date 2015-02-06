#pragma once

class CHostedNetworkClient
{
public:
	CHostedNetworkClient();
	~CHostedNetworkClient();

	STDMETHODIMP StartHostedNetwork();
	STDMETHODIMP StopHostedNetwork();
	STDMETHODIMP RestartHostedNetwork();
	STDMETHODIMP QueryHostedNetworkState();

private:
	IDispatch *m_pProxy;
	DISPID m_lStartFuncID;
	DISPID m_lStopFuncID;
};

