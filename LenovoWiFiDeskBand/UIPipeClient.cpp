#include "stdafx.h"
#include "UIPipeClient.h"

CONST LPTSTR	PIPE_NAME = TEXT("\\\\.\\pipe\\LenovoWiFi");
CONST UINT		DEFAULT_PIPE_TIMEOUT = 20000u;
CONST INT		BUFFER_SIZE = 8;

CUIPipeClient::CUIPipeClient()
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
			break;
		}

		dwError = GetLastError();
		if (dwError != ERROR_PIPE_BUSY)
		{
			throw dwError;
		}

		if (!WaitNamedPipe(PIPE_NAME, DEFAULT_PIPE_TIMEOUT))
		{
			throw ERROR_SEM_TIMEOUT;
		}
	}
	
	m_hPipe = hPipe;
	
	DWORD dwMode = PIPE_READMODE_MESSAGE;
	if (!SetNamedPipeHandleState(
		m_hPipe,
		&dwMode,
		NULL,
		NULL))
	{
		throw GetLastError();
	}
}

CUIPipeClient::~CUIPipeClient()
{
	if (m_hPipe)
	{
		CloseHandle(m_hPipe);
	}
}

DWORD CUIPipeClient::PostMessage(LPCTSTR lpvMessage)
{
	if (!lpvMessage || !m_hPipe)
	{
		return ERROR_INVALID_HANDLE;
	}

	DWORD dwError, cbMessageLength, cbWritten, cbRead;
	cbMessageLength = (lstrlen(lpvMessage) + 1) * sizeof(TCHAR);

	BOOL fSuccess = WriteFile(
		m_hPipe,
		lpvMessage,
		cbMessageLength,
		&cbWritten,
		NULL);

	if (!fSuccess)
	{
		return GetLastError();
	}

	TCHAR chBuf[BUFFER_SIZE];
	dwError = ERROR_SUCCESS;

	do
	{
		fSuccess = ReadFile(
			m_hPipe,
			chBuf,
			BUFFER_SIZE * sizeof(TCHAR),
			&cbRead,
			NULL);

		if (!fSuccess)
		{
			dwError = GetLastError();
		}
	} while (!fSuccess && dwError == ERROR_MORE_DATA);

	return dwError;
}