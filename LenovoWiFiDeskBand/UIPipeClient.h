#pragma once

#define ICS_LOADING L"ics_loading"
#define ICS_ON L"ics_on"
#define ICS_OFF L"ics_off"
#define ICS_CLIENTCONNECTED L"ics_clientconnected"
#define CMD_EXIT L"exit\r\n"
#define CMD_MOUSEENTER L"mouseenter\r\n"
#define CMD_MOUSELEAVE L"mouseleave\r\n"
#define CMD_RBUTTONCLICK L"rbuttonclick\r\n"
#define CMD_LBUTTONCLICK L"lbuttonclick\r\n"
#define CMD_HANDSHAKE	L"handshake\r\n"


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

