#pragma once

class IDeskbandListener
{
public:
	virtual void OnICS_Loading() = 0;
	virtual void OnICS_On() = 0;
	virtual void OnICS_Off() = 0;
	virtual void OnICS_ClientConnected() = 0;
};

class CUIPipeClient
{
public:
	CUIPipeClient();
	virtual ~CUIPipeClient();

	DWORD Connect();

	void RegisterListener(IDeskbandListener* listener)
	{
		m_pDeskbandListener = listener;
	}

	//BOOL Start();
	DWORD Send(LPCTSTR lpszMessage);
	VOID Disconnect();

private:
	HANDLE m_hPipe;
	BOOL m_fAvailable;
	IDeskbandListener* m_pDeskbandListener;
};

