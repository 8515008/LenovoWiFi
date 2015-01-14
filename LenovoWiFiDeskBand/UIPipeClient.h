#pragma once
class CUIPipeClient
{
public:
	CUIPipeClient();
	~CUIPipeClient();

	DWORD Send(LPCTSTR lpszMessage);

private:
	HANDLE m_hPipe;
};

