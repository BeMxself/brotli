/* Copyright 2016 Simon Mourier. All Rights Reserved.
Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#define NOMINMAX
#include "windows.h"
#include "objidl.h"						// for ISequentialStream
#include "strsafe.h"

#include "../../dec/decode.h"
#include "../../enc/encode.h"

void CDECL qualityrmat(PCWSTR pszFormat, ...)
{
	WCHAR szTrace[0x2000];
	va_list args;
	va_start(args, pszFormat);
	StringCchVPrintf(szTrace, ARRAYSIZE(szTrace), pszFormat, args);
	va_end(args);
	OutputDebugString(szTrace);
}

// adapter classes 
class BrotliStreamIn : public brotli::BrotliIn
{
	ISequentialStream *_pStream;
	char *_pBuffer;
	int _bufferSize;

public:
	BrotliStreamIn(ISequentialStream *pStream, int bufferSize)
	{
		_pStream = pStream;
		_bufferSize = bufferSize;
		_pBuffer = new char[bufferSize];
	}

	BrotliStreamIn::~BrotliStreamIn(void)
	{
		if (_pBuffer) delete[] _pBuffer;
	}

	const void* Read(size_t n, size_t* bytes_read)
	{
		if (!_pBuffer)
		{
			return NULL;
		}

		if (n > _bufferSize)
		{
			n = _bufferSize;
		}

		ULONG read;
		_pStream->Read(_pBuffer, (ULONG)n, &read);
		*bytes_read = read;

		// brotli semantics are a bit strange, read the doc.
		if (!read && n)
		{
			delete[] _pBuffer;
			_pBuffer = NULL;
			return NULL;
		}

		return _pBuffer;
	}
};

class BrotliStreamOut : public brotli::BrotliOut
{
	ISequentialStream *_pStream;

public:
	BrotliStreamOut(ISequentialStream *pStream)
	{
		_pStream = pStream;
	}

	bool Write(const void* buf, size_t n)
	{
		return !_pStream->Write(buf, (ULONG)n, NULL);
	}
};

STDAPI CreateState(LPVOID *ppState)
{
	if (!ppState)
		return E_INVALIDARG;
	
	BrotliState* pState = BrotliCreateState(NULL, NULL, NULL);
	if (!pState)
		return E_OUTOFMEMORY;

	*ppState = pState;
	return S_OK;
}

STDAPI DestroyState(LPVOID pState)
{
	if (!pState)
		return E_INVALIDARG;

	BrotliDestroyState((BrotliState*)pState);
	return S_OK;
}

// note: we have added offset_in & offset_out so it's better suited for .NET p/invoke bindings (avoid unsafe casts and buffer copies)
STDAPI_(BrotliResult) DecompressStream(
	size_t* available_in, uint8_t* next_in, size_t* offset_in,
	size_t* available_out, uint8_t* next_out, size_t* offset_out,
	size_t* total_out, BrotliState* pState)
{
	const uint8_t* pin = next_in;
	uint8_t* pout = next_out;
	*offset_in = 0;
	*offset_out = 0;
	BrotliResult result =  BrotliDecompressStream(available_in, &pin, available_out, &pout, total_out, pState);
	*offset_in = (pin - next_in) / sizeof(uint8_t);
	*offset_out = (pout - next_out) / sizeof(uint8_t);
	return result;
}

STDAPI_(int) CompressStream(brotli::BrotliParams *bp, ISequentialStream *read, ISequentialStream *write)
{
	if (!bp || !read || !write)
		return E_INVALIDARG;

	BrotliStreamIn in(read, 1 << 16);
	BrotliStreamOut out(write);
	return brotli::BrotliCompress(*bp, &in, &out);
}
