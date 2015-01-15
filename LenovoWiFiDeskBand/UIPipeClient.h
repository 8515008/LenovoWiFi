#pragma once

class CUIPipeClient
{
public:
	CUIPipeClient();
	~CUIPipeClient();

	BOOL IsAvailable();
	DWORD Connect();
	DWORD Send(LPCTSTR lpszMessage);

private:
	HANDLE m_hPipe;
	BOOL m_fAvailable;
};

