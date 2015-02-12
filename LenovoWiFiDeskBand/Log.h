#pragma once

#include <vector>
#include <memory>
#include <algorithm>
#include <time.h>
#include <io.h>
#include <mutex>
#include <locale>
#include <tchar.h>
#include <time.h>

#include "str.h"

using namespace lenovowifi;

enum LOG_LEVEL
{
	LOG_LEVEL_TRACE = 0,
	LOG_LEVEL_DEBUG	= 1,
	LOG_LEVEL_INFO	= 2,
	LOG_LEVEL_WARN	= 3,
	LOG_LEVEL_ERROR	= 4,
	LOG_LEVEL_FATAL	= 5,
	LOG_LEVEL_NONE	= 6
};

#define LOG_LINE_MAX_BYTES (16 * 1024)

class ILogger
{
public:
	virtual void print(LOG_LEVEL level, const char* log) = 0;
	virtual void print(LOG_LEVEL level, const wchar_t* log) = 0;

	virtual int type() = 0;
};

class Logger
{
public:
	static Logger& Instance()
	{
		static Logger log;
		return log;
	}

public:
	template<class Char>
	void trace(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_TRACE, tag, format, args);
		va_end(args);
	}

	template<class Char>
	void debug(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_DEBUG, tag, format, args);
		va_end(args);
	}

	template<class Char>
	void info(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_INFO, tag, format, args);
		va_end(args);
	}

	template<class Char>
	void warn(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_WARN, tag, format, args);
		va_end(args);
	}

	template<class Char>
	void error(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_ERROR, tag, format, args);
		va_end(args);
	}

	template<class Char>
	void fatal(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_FATAL, tag, format, args);
		va_end(args);
	}

public:
	template<class Char>
	void t(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_TRACE, tag, format, args);
		va_end(args);
	}

	template<class Char>
	void d(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_DEBUG, tag, format, args);
		va_end(args);
	}

	template<class Char>
	void i(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_INFO, tag, format, args);
		va_end(args);
	}

	template<class Char>
	void w(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_WARN, tag, format, args);
		va_end(args);
	}

	template<class Char>
	void e(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_ERROR, tag, format, args);
		va_end(args);
	}

	template<class Char>
	void f(const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(LOG_LEVEL_FATAL, tag, format, args);
		va_end(args);
	}

public:
	template<class Char>
	void output(LOG_LEVEL level, const Char* tag, const Char* format, ...)
	{
		va_list args;
		va_start(args, format);
		output(level, tag, format, args);
		va_end(args);
	}

public:
	void add(ILogger* logger)
	{
		if(logger == nullptr)
			return;

		for(auto& lgr : m_loggers)
		{
			if(lgr->type() == logger->type())
				return;
		}

		m_loggers.push_back(std::shared_ptr<ILogger>(logger));
	}

	void remove(ILogger* logger)
	{
		for(auto& lgr : m_loggers)
		{
			if(lgr.get() == logger)
			{
				m_loggers.erase(std::remove(m_loggers.begin(), m_loggers.end(), lgr), m_loggers.end());
				break;
			}
		}
	}

protected:
	void output(LOG_LEVEL level, const char* tag, const char* format, va_list args)
	{
		time_t t = 0;
		time(&t);
		struct tm* now = localtime(&t);

		long pId = 0;
		long threadId = 0;
#ifdef _WIN32
		pId = ::GetCurrentProcessId();
		threadId = ::GetCurrentThreadId();
#endif

		char prefix[128] = {0};
		sprintf(prefix, "%d-%02d-%02d %02d:%02d:%02d (%d|%d) %s ", 
			now->tm_year + 1900, now->tm_mon + 1, now->tm_mday, now->tm_hour, now->tm_min, now->tm_sec, pId, threadId,
			tag);
		int len = strlen(prefix);
		
		char log[LOG_LINE_MAX_BYTES + 4] = {0};
		int count = vsnprintf(log + len, LOG_LINE_MAX_BYTES, format, args);
		if (count <= 0)
			strcpy(log + len + LOG_LINE_MAX_BYTES, "...");

		memcpy(log, prefix, len * sizeof(char));

		for(auto& lgr : m_loggers)
		{
			std::unique_lock<std::mutex> locker(m_mutex);
			lgr->print(level,  log);
		}
	}

	void output(LOG_LEVEL level, const wchar_t* tag, const wchar_t* format, va_list args)
	{
		time_t t = 0;
		time(&t);
		struct tm* now = localtime(&t);

		long pId = 0;
		long threadId = 0;
#ifdef _WIN32
		pId = ::GetCurrentProcessId();
		threadId = ::GetCurrentThreadId();
#endif

		wchar_t prefix[128] = {0};
		wsprintf(prefix, L"%d-%02d-%02d %02d:%02d:%02d (%d|%d) %s ", 
			now->tm_year + 1900, now->tm_mon + 1, now->tm_mday, now->tm_hour, now->tm_min, now->tm_sec, pId, threadId, 
			tag);
		int len = wcslen(prefix);

		wchar_t log[LOG_LINE_MAX_BYTES + 4] = {0};
		int count = _vsnwprintf(log + len, LOG_LINE_MAX_BYTES, format, args);
		if (count <= 0)
			wcscpy(log + len + LOG_LINE_MAX_BYTES, L"...");

		memcpy(log, prefix, len * sizeof(wchar_t));

		for(auto& lgr : m_loggers)
		{
			std::unique_lock<std::mutex> locker(m_mutex);
			lgr->print(level, log);
		}
	}

protected:
	std::mutex m_mutex;
	std::vector<std::shared_ptr<ILogger>> m_loggers;
};

class FileLogger : public ILogger
{
public:
	FileLogger(const char* filename, LOG_LEVEL level = LOG_LEVEL_INFO, unsigned int maxFilesize = 3 * 1024 * 1024)
	{
		_tsetlocale(LC_CTYPE, (LPCWSTR)"");

		m_file = nullptr;
		m_level = level;
		memset(m_filename, 0, sizeof(m_filename) / sizeof(m_filename[0]));

		if(filename == nullptr || strlen(filename) > 260)
			return;

		strcpy(m_filename, filename);
		m_file = fopen(m_filename, "a+");
		if(m_file == nullptr)
			return;

		unsigned int size = _filelengthi64(m_file->_file);
		if(size == 0 || size > maxFilesize)
		{
			reset();
		}
	}

	virtual ~FileLogger()
	{
		if(m_file != nullptr)
			fclose(m_file);
	}

public:
	virtual void print(LOG_LEVEL level, const char* log)
	{
		if(level < m_level)
			return;

		if(m_file == nullptr)
			return;

		str lg(str(log).wstr(), CP_UTF8);
		fwrite(lg.cstr(), strlen(lg.cstr()) * sizeof(char), 1, m_file);
		fflush(m_file);
	}

	virtual void print(LOG_LEVEL level, const wchar_t* log)
	{
		if(level < m_level)
			return;

		if(m_file == nullptr)
			return;

		str lg(log, CP_UTF8);
		fwrite(lg.cstr(), strlen(lg.cstr()) * sizeof(char), 1, m_file);
		fflush(m_file);
	}

	virtual int type()
	{
		return 'fllg';
	}

protected:
	void reset()
	{
		if(m_file == nullptr)
			return;

		fclose(m_file);
		m_file = fopen(m_filename, "w+");

		if(m_file != nullptr)
		{
			char flag[] = { 0xEF, 0xBB, 0xBF };
			fwrite(flag, sizeof(flag), 1, m_file);
		}
	}

public:
	FILE* m_file;
	LOG_LEVEL m_level;
	char m_filename[260];
};

class DebugLogger : public ILogger
{
public:
	DebugLogger(LOG_LEVEL level = LOG_LEVEL_DEBUG)
	{
		m_level = level;
	}

public:
	virtual void print(LOG_LEVEL level, const char* log)
	{
#ifndef _DEBUG
		if(level > m_level)
			return;
#endif

		OutputDebugStringA(log);
	}

	virtual void print(LOG_LEVEL level, const wchar_t* log)
	{
#ifndef _DEBUG
		if(level > m_level)
			return;
#endif

		OutputDebugStringW(log);
	}

	virtual int type()
	{
		return 'dblg';
	}

protected:
	LOG_LEVEL m_level;
};

static Logger& Log = Logger::Instance();