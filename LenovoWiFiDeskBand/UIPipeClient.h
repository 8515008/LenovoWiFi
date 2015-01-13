#pragma once
class CUIPipeClient
{
public:
	CUIPipeClient();
	~CUIPipeClient();

	DWORD PostMessage(LPCTSTR lpszMessage);

private:
	HANDLE m_hPipe;
};

