#include "stdafx.h"

CONST LPTSTR	PIPE_NAME = TEXT("\\\\.\\pipe\\LenovoWiFi");
CONST UINT		DEFAULT_PIPE_TIMEOUT = 20000u;
CONST INT		BUFFER_SIZE = 8;

CUIPipeClient::CUIPipeClient()
	: m_fAvailable(FALSE), m_hPipe(NULL)
{
	try
	{
		Connect();
	}
	catch (...)
	{
	}
}

CUIPipeClient::~CUIPipeClient()
{
	if (m_hPipe)
	{
		CloseHandle(m_hPipe);
	}
}

BOOL CUIPipeClient::IsAvailable()
{
	if (!m_fAvailable)
	{
		Connect();
	}

	return m_fAvailable;
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
			m_fAvailable = TRUE;
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

	m_hPipe = hPipe;

	if (m_fAvailable)
	{
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
}

DWORD CUIPipeClient::Send(LPCTSTR lpvMessage)
{
	if (!lpvMessage)
	{
		return ERROR_INVALID_HANDLE;
	}

	if (!m_fAvailable)
	{
		return ERROR_INVALID_STATE;
	}

	DWORD cbMessageLength, cbWritten;
	cbMessageLength = lstrlen(lpvMessage) * sizeof(TCHAR);

	BOOL fSuccess = WriteFile(
		m_hPipe,
		lpvMessage,
		cbMessageLength,
		&cbWritten,
		NULL);

	return fSuccess ? ERROR_SUCCESS : GetLastError();
}