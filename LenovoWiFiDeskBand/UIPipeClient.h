#pragma once

class CUIPipeClient
{
public:
	CUIPipeClient();
	~CUIPipeClient();

	DWORD Connect();
	DWORD Send(LPCTSTR lpszMessage);
	VOID Disconnect();

private:
	HANDLE m_hPipe;
	BOOL m_fAvailable;
};

