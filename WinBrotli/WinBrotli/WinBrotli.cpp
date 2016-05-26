#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#define NOMINMAX
#include "windows.h"

#include "../../dec/decode.h"
#include "../../enc/encode.h"

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

STDAPI_(BrotliResult) DecompressStream(size_t* available_in, const uint8_t** next_in,
	size_t* available_out, uint8_t** next_out,
	size_t* total_out, BrotliState* pState)
{
	return BrotliDecompressStream(available_in, next_in, available_out, next_out, total_out, pState);
}
