#pragma once
class CUIPipeClient
{
public:
	CUIPipeClient();
	~CUIPipeClient();

	DWORD SendMessage(LPCTSTR lpszMessage);

private:
	HANDLE m_hPipe;
};

