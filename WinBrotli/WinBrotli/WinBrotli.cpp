#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#define NOMINMAX
#include "windows.h"

#include "../../dec/decode.h"
#include "../../enc/encode.h"

HRESULT APIENTRY Compress(HMODULE hModule, DWORD dwReason, LPVOID lpReserved)
{
	//brotli::BrotliParams p = { 0 };
	//int res = brotli::BrotliCompress(p, nullptr, nullptr);
	return 0;
}


