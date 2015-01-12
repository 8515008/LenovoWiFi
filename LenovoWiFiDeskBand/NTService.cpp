#include "stdafx.h"
#include "NTService.h"

#pragma comment(lib, "Advapi32.lib")

CNTService::CNTService(LPCTSTR lpszServiceName)
	: m_fExists(FALSE)
{
	DWORD dwError;
	SC_HANDLE schSCManager = OpenSCManager(NULL, NULL, SC_MANAGER_CONNECT);

	if (!schSCManager)
	{
		throw GetLastError();
	}

	SC_HANDLE schService = OpenService(schSCManager, lpszServiceName, SERVICE_QUERY_STATUS);

	if (!schService)
	{
		dwError = GetLastError();

		if (dwError != ERROR_SERVICE_DOES_NOT_EXIST)
		{
			CloseServiceHandle(schSCManager);
			throw GetLastError();
		}
	}
	else
	{
		m_fExists = TRUE;
	}

	m_handle = schService;

	if (m_fExists)
	{
		SERVICE_STATUS_PROCESS sspStatus;
		DWORD dwBytesNeeded;

		if (!QueryServiceStatusEx(
			schService,
			SC_STATUS_PROCESS_INFO,
			(LPBYTE)&sspStatus,
			sizeof(SERVICE_STATUS_PROCESS),
			&dwBytesNeeded))
		{
			CloseServiceHandle(schSCManager);
			CloseServiceHandle(schService);
			throw GetLastError();
		}

		m_sspStatus = sspStatus;
	}
}


CNTService::~CNTService()
{
	if (m_handle)
	{
		CloseServiceHandle(m_handle);
	}
}

BOOL CNTService::Exists()
{
	return m_handle != NULL;
}

DWORD CNTService::GetCurrentState()
{
	if (Exists())
	{
		return m_sspStatus.dwCurrentState;
	}

	return 0;
}