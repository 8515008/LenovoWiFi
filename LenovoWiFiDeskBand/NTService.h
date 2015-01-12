#pragma once
class CNTService
{
public:
	CNTService(LPCTSTR lpszServiceName);
	~CNTService();

	BOOL Exists();
	DWORD GetCurrentState();

private:
	SC_HANDLE m_handle;
	BOOL m_fExists;
	SERVICE_STATUS_PROCESS m_sspStatus;
};

