#include "stdafx.h"

LPCTSTR MONIKER = L"service:mexAddress=net.pipe://localhost/LenovoWiFi/HostedNetworkService/mex,"
				  L"address=net.pipe://localhost/LenovoWiFi/HostedNetworkService,"
				  L"binding=NetNamedPipeBinding_IHostedNetworkService, bindingNamespace=http://tempuri.org/,"
				  L"contract=IHostedNetworkService, contractNamespace=http://tempuri.org/";

CHostedNetworkClient::CHostedNetworkClient()
{
	DWORD dwError;
	HRESULT hr = CoInitializeEx(NULL, COINIT_MULTITHREADED);

	hr = CoGetObject(
		MONIKER,
		NULL,
		IID_IDispatch,
		reinterpret_cast<void **>(&m_pProxy));

	if (FAILED(hr))
	{
		BAIL_ON_HRESULT_ERROR(dwError, hr)
	}

	DISPID dispId;
	BSTR szFunc = SysAllocString(_T("StartHostedNetwork"));

	hr = m_pProxy->GetIDsOfNames(
		IID_NULL,
		&szFunc,
		1,
		GetUserDefaultLCID(),
		&dispId);

	BAIL_ON_FAILURE(hr)
	m_lStartFuncID = dispId;

	szFunc = SysAllocString(_T("StopHostedNetwork"));

	hr = m_pProxy->GetIDsOfNames(
		IID_NULL,
		&szFunc,
		1,
		GetUserDefaultLCID(),
		&dispId);

	BAIL_ON_FAILURE(hr)
	m_lStopFuncID = dispId;

	return;
ERROR_LABEL:
	CoUninitialize();
	throw dwError;
}

CHostedNetworkClient::~CHostedNetworkClient()
{
	CoUninitialize();
}

STDMETHODIMP CHostedNetworkClient::StartHostedNetwork()
{
	HRESULT hResult = S_OK;

	DISPPARAMS dispparamsNoArgs = { NULL, NULL, 0, 0 };
	EXCEPINFO excepInfo;
	memset(&excepInfo, 0, sizeof(EXCEPINFO));
	UINT uArgErr;

	VARIANTARG lpvVoid;
	VariantInit(&lpvVoid);

	return m_pProxy->Invoke(
		m_lStartFuncID,
		IID_NULL,
		GetUserDefaultLCID(),
		DISPATCH_METHOD,
		&dispparamsNoArgs, &lpvVoid, &excepInfo, &uArgErr);
}

STDMETHODIMP CHostedNetworkClient::StopHostedNetwork()
{
	HRESULT hResult;

	DISPPARAMS dispparamsNoArgs = { NULL, NULL, 0, 0 };
	EXCEPINFO excepInfo;
	memset(&excepInfo, 0, sizeof(EXCEPINFO));
	UINT uArgErr;

	VARIANTARG lpvVoid;
	VariantInit(&lpvVoid);

	return m_pProxy->Invoke(
		m_lStopFuncID,
		IID_NULL,
		GetUserDefaultLCID(),
		DISPATCH_METHOD,
		&dispparamsNoArgs, &lpvVoid, &excepInfo, &uArgErr);
}

STDMETHODIMP CHostedNetworkClient::RestartHostedNetwork()
{
	HRESULT hResult = StopHostedNetwork();

	BAIL_ON_FAILURE(hResult)

	hResult = StartHostedNetwork();

ERROR_LABEL:
	return hResult;
}