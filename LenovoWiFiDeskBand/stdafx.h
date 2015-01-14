// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <tchar.h>
#include <windows.h>
#include <Unknwn.h>
#include <OleCtl.h>
#include <Shobjidl.h>
#include <strsafe.h>
#include <ShlGuid.h>
#include <Windowsx.h>

// TODO: reference additional headers your program requires here
#include "resource.h"
#include "common.h"
#include "ClassFactory.h"
#include "HostedNetworkClient.h"
#include "DeskBand.h"
#include "NTService.h"
#include "UIPipeClient.h"