#include "stdafx.h"

CONST LPTSTR	PIPE_NAME = TEXT("\\\\.\\pipe\\LenovoWiFi");
CONST UINT		DEFAULT_PIPE_TIMEOUT = 20000u;
CONST INT		BUFFER_SIZE = 8;

CUIPipeClient::CUIPipeClient()
	:m_hPipe(NULL)
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
			GENERIC_WRITE,
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

	return ERROR_SUCCESS;
}

DWORD CUIPipeClient::Send(LPCTSTR lpvMessage)
{
	if (!lpvMessage)
	{
		return ERROR_INVALID_HANDLE;
	}

	DWORD dwError = Connect();

	if (dwError != ERROR_SUCCESS)
	{
		return dwError;
	}

	DWORD cbMessageLength, cbWritten;
	cbMessageLength = lstrlen(lpvMessage) * sizeof(TCHAR);

	BOOL fSuccess = WriteFile(
		m_hPipe,
		lpvMessage,
		cbMessageLength,
		&cbWritten,
		NULL);

	dwError = fSuccess ? ERROR_SUCCESS : GetLastError();

	Disconnect();

	return dwError;
}

VOID CUIPipeClient::Disconnect()
{
	if (m_hPipe)
	{
		CloseHandle(m_hPipe);
	}
}