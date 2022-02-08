#pragma once

#include <d3d11.h>

#include <d3d11_3.h>

#include <DirectXMath.h>
#include <DirectXPackedVector.h>

#define DirectX_NS DirectX         // Windows SDK requires a DirectX namespace for several types
#ifndef SAFE_DELETE
#define SAFE_DELETE(p)       { if (p) { delete (p);     (p)=NULL; } }
#endif
#ifndef SAFE_DELETE_ARRAY
#define SAFE_DELETE_ARRAY(p) { if (p) { delete[] (p);   (p)=NULL; } }
#endif
#ifndef SAFE_RELEASE
#define SAFE_RELEASE(p)      { if (p) { (p)->Release(); (p)=NULL; } }
#endif



//#include "Utils.h"
#define DXAPI_EXPORTS 1

#ifdef WIN32
#	ifdef DXAPI_EXPORTS
#	define DXAPI __declspec( dllexport )
#else
#	define DXAPI __declspec( dllimport )
#	endif
#else
#	define DXAPI
#endif

namespace myVRXamlComponent
{
	class DXAPI DeviceResources
	{
	public:
		DeviceResources();
		~DeviceResources();

	private:
		// DX11
		HRESULT     initDX(void* hwnd);
		HRESULT     destroyDX();
		HRESULT     resize(void* hwnd);
		void        setUpViewport();

	public:
		void Present();

		bool resizeRenderTarget(void* pResource, int x, int y, int width, int height)
		{
			rect.left = x;
			rect.top = y;
			rect.right = width;
			rect.bottom = y + height;
			firstInstance = true;
			return SUCCEEDED(resize(pResource));
		}

		bool initDeviceResources(void* pResource, int x, int y, int width, int height)
		{
			rect.left = x;
			rect.top = y;
			rect.right = width;
			rect.bottom = y + height;
			firstInstance = true;

		//	mD3dContext->OMSetRenderTargets(0, NULL, NULL);
			return SUCCEEDED(initDX(pResource));
		}

		void randClearColor(bool value)
		{
			mRandClearColor = value;
		}

		//	private:
				// Initial window resolution
		bool					mRandClearColor = false;
		int                     mWidth = 0;
		int                     mHeight = 0;
		RECT					rect{ 0,0,0,0 };

		// DX11
//		HINSTANCE				mInst = NULL;
		HWND					mHwnd = NULL;
		D3D_DRIVER_TYPE			mDriverType;
		D3D_FEATURE_LEVEL		mFeatureLevel;

		ID3D11Device* mD3dDevice = nullptr;
		//		ID3D11Device3*			mD3dDevice = nullptr;

		ID3D11DeviceContext* mD3dContext = nullptr;
		IDXGISwapChain1* mSwapChain = nullptr;

		ID3D11RenderTargetView* mRenderTargetView = nullptr;
		ID3D11DepthStencilView* mDepthStencilView = nullptr;
		ID3D11Texture2D* depthTexture = nullptr;

		bool				firstInstance = true;
	};
}