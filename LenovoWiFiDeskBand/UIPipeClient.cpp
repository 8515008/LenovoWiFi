#include "stdafx.h"
#include <iostream>

CONST LPTSTR	PIPE_NAME = TEXT("\\\\.\\pipe\\LenovoWiFi");
CONST UINT		DEFAULT_PIPE_TIMEOUT = 20000u;
CONST INT		BUFFER_SIZE = 8;

#define BUFSIZE 512

#define ICS_LOADING L"ics_loading"
#define ICS_ON L"ics_on"
#define ICS_OFF L"ics_off"
#define ICS_CLIENTCONNECTED L"ics_clientconnected"

CUIPipeClient::CUIPipeClient()
:m_hPipe(NULL), m_pDeskbandListener(NULL)
{
}

CUIPipeClient::~CUIPipeClient()
{
	if (m_hPipe)
	{
		CloseHandle(m_hPipe);
	}
}

DWORD CUIPipeClient::Connect()
{
	DWORD dwError;
	HANDLE hPipe;

	while (true)
	{
		hPipe = CreateFile(
			PIPE_NAME,
			GENERIC_READ | GENERIC_WRITE,
			0,
			NULL,
			OPEN_EXISTING,
			0,
			NULL);

		if (hPipe != INVALID_HANDLE_VALUE)
		{
			m_hPipe = hPipe;
			break;
		}

		dwError = GetLastError();
		if (dwError != ERROR_PIPE_BUSY)
		{
			break;
		}

		if (!WaitNamedPipe(PIPE_NAME, DEFAULT_PIPE_TIMEOUT))
		{
			break;
		}
	}

	if (hPipe == INVALID_HANDLE_VALUE)
	{
		return dwError;
	}

	DWORD dwMode = PIPE_READMODE_MESSAGE;
	if (!SetNamedPipeHandleState(
		m_hPipe,
		&dwMode,
		NULL,
		NULL))
	{
		return GetLastError();
	}


	BOOL   fSuccess = FALSE;
	DWORD  cbRead;
	TCHAR  chBuf[BUFSIZE];
	do
	{
		// Read from the pipe. 

		if (!m_pDeskbandListener) break;

		fSuccess = ReadFile(
			hPipe,    // pipe handle 
			chBuf,    // buffer to receive reply 
			BUFSIZE*sizeof(TCHAR),  // size of buffer 
			&cbRead,  // number of bytes read 
			NULL);    // not overlapped 

		if (!fSuccess && GetLastError() != ERROR_MORE_DATA)
			break;

		_tprintf(TEXT("\"%s\"\n"), chBuf);

		std::wstring strRead = chBuf;

		if (ICS_LOADING == strRead)
		{
			m_pDeskbandListener->OnICS_Loading();
		}
		else if (ICS_ON == strRead)
		{
			m_pDeskbandListener->OnICS_On();
		}
		else if (ICS_OFF == strRead)
		{
			m_pDeskbandListener->OnICS_Off();
		}
		else if (ICS_CLIENTCONNECTED == strRead)
		{
			m_pDeskbandListener->OnICS_ClientConnected();
		}
	} while (!fSuccess);  // repeat loop if ERROR_MORE_DATA 

	if (!fSuccess)
	{
		_tprintf(TEXT("ReadFile from pipe failed. GLE=%d\n"), GetLastError());
		return -1;
	}

	return ERROR_SUCCESS;
}

DWORD CUIPipeClient::Send(LPCTSTR lpvMessage)
{
	if (!lpvMessage || !m_hPipe)
	{
		return ERROR_INVALID_HANDLE;
	}

	//DWORD dwError = Connect();

	//if (dwError != ERROR_SUCCESS)
	//{
	//	return dwError;
	//}

	DWORD cbMessageLength, cbWritten;
	cbMessageLength = lstrlen(lpvMessage) * sizeof(TCHAR);

	BOOL fSuccess = WriteFile(
		m_hPipe,
		lpvMessage,
		cbMessageLength,
		&cbWritten,
		NULL);

	DWORD dwError = fSuccess ? ERROR_SUCCESS : GetLastError();

	//Disconnect();

	return dwError;
}

VOID CUIPipeClient::Disconnect()
{
	if (m_hPipe)
	{
		CloseHandle(m_hPipe);
	}
}