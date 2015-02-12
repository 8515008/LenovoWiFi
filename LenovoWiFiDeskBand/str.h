#pragma once

//////////////////////////////////////////////////////////////////////////
/// \file 该文件的作用是:
///		1. 自动将unicode转成multibyte,utf8等等.
///		2. 自动将multibyte转成unicode,utf8等等.
//////////////////////////////////////////////////////////////////////////

namespace lenovowifi{

class str
{
public:
	str(const wchar_t* wstr, unsigned int codepage = CP_ACP, bool detach = false)
	{
		m_detach = detach;
		m_str = nullptr;
		m_wstr = nullptr;
		if(wstr == nullptr)
			return;

		trans(wstr, m_str, codepage);
	}

	str(const char* str, unsigned int codepage = CP_ACP, bool detach = false)
	{
		m_detach = detach;
		m_str = nullptr;
		m_wstr = nullptr;
		if(str == nullptr)
			return;

		trans(str, m_wstr, codepage);
	}

	~str()
	{
		if(!m_detach)
		{
			if(m_str != nullptr)
				delete[] m_str;

			if(m_wstr != nullptr)
				delete[] m_wstr;
		}
	}

public:
	void detach()
	{
		m_str = nullptr;
		m_wstr = nullptr;
	}

	const char* cstr()
	{
		return m_str;
	}

	const wchar_t* wstr()
	{
		return m_wstr;
	}

	char* data()
	{
		return m_str;
	}

	wchar_t* wdata()
	{
		return m_wstr;
	}

public:
	operator const char*()
	{
		return m_str;
	}

	operator const wchar_t*()
	{
		return m_wstr;
	}

public:
	static inline void trans(const wchar_t* wstr, char*& cstr, unsigned int codepage)
	{
		if(wstr == nullptr)
			return;

		int wlen = wcslen(wstr);
		int len = ::WideCharToMultiByte(codepage, 0, wstr, wlen, NULL, NULL, NULL, NULL);
		if(cstr == nullptr)
		{
			cstr = new char[len + 1];
			memset(cstr, 0, (len + 1) * sizeof(char));
		}

		int result = ::WideCharToMultiByte(codepage, 0, wstr, wlen, cstr, len, NULL, NULL);
		(void)result;
	}

	static inline void trans(const char* cstr, wchar_t*& wstr, unsigned int codepage)
	{
		if(cstr == nullptr)
			return;

		int len = strlen(cstr);
		int wlen = ::MultiByteToWideChar(codepage, 0, cstr, len, NULL, NULL);
		if(wstr == nullptr)
		{
			wstr = new wchar_t[wlen + 1];
			memset(wstr, 0, (wlen + 1) * sizeof(wchar_t));
		}

		int result = ::MultiByteToWideChar(codepage, 0, cstr, len, wstr, wlen);
		(void)result;
	}

protected:
	char* m_str;
	wchar_t* m_wstr;
	bool m_detach;
};

}
