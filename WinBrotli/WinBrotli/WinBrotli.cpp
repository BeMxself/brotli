/* Copyright 2016 Simon Mourier. All Rights Reserved.
Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#define NOMINMAX
#include "windows.h"
#include "strsafe.h"

#include "../../dec/decode.h"
#include "../../enc/encode.h"

void CDECL TraceFormat(PCWSTR pszFormat, ...)
{
	WCHAR szTrace[0x2000];
	va_list args;
	va_start(args, pszFormat);
	StringCchVPrintf(szTrace, ARRAYSIZE(szTrace), pszFormat, args);
	va_end(args);
	OutputDebugString(szTrace);
}

STDAPI CreateState(LPVOID *ppState)
{
	if (!ppState)
		return E_INVALIDARG;
	
	BrotliState* pState = BrotliCreateState(NULL, NULL, NULL);
	if (!pState)
		return E_OUTOFMEMORY;

	*ppState = pState;
	return 0;
}

STDAPI DestroyState(LPVOID pState)
{
	if (!pState)
		return E_INVALIDARG;

	BrotliDestroyState((BrotliState*)pState);
	return 0;
}

// note: we have added offset_in & offset_out so it's better suited for .NET p/invoke bindings (avoid unsafe casts)
STDAPI_(BrotliResult) DecompressStream(
	size_t* available_in, uint8_t* next_in, size_t* offset_in,
	size_t* available_out, uint8_t* next_out, size_t* offset_out,
	size_t* total_out, BrotliState* pState)
{
	TraceFormat(L"DecompressStream: available_in:%p (%u)",
		available_in, (*available_in));
	const uint8_t* pin = next_in;
	uint8_t* pout = next_out;
	*offset_in = 0;
	*offset_out = 0;
	BrotliResult result =  BrotliDecompressStream(available_in, &pin, available_out, &pout, total_out, pState);
	*offset_in = (pin - next_in) / sizeof(uint8_t);
	*offset_out = (pout - next_out) / sizeof(uint8_t);
	return result;
}
