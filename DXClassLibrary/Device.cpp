#include "pch.h"
#include "Device.h"
#include <iostream>

#define PRESENT1 0

using namespace myVRXamlComponent;

namespace myVRXamlComponent
{

	DeviceResources::DeviceResources()
	{
//		initDX();
	}

	DeviceResources::~DeviceResources()
	{
		destroyDX();
	}

	// DX11
	HRESULT    DeviceResources::initDX(void* hwnd)
	{
		mHwnd = (HWND)hwnd;

		RECT dimensions = rect;
//		GetClientRect(mHwnd, &dimensions);
//		unsigned int width = dimensions.right - dimensions.left;
		unsigned int width = rect.right;

		unsigned int height = dimensions.bottom - dimensions.top;

		D3D_DRIVER_TYPE driverTypes[] =
		{
			D3D_DRIVER_TYPE_HARDWARE, D3D_DRIVER_TYPE_WARP,
			D3D_DRIVER_TYPE_REFERENCE, D3D_DRIVER_TYPE_SOFTWARE
		};

		unsigned int totalDriverTypes = ARRAYSIZE(driverTypes);

		D3D_FEATURE_LEVEL featureLevels[] =
		{
			D3D_FEATURE_LEVEL_11_0,
			D3D_FEATURE_LEVEL_10_1,
			D3D_FEATURE_LEVEL_10_0
		};

		unsigned int totalFeatureLevels = ARRAYSIZE(featureLevels);

		DXGI_SWAP_CHAIN_DESC swapChainDesc;
		ZeroMemory(&swapChainDesc, sizeof(swapChainDesc));

		swapChainDesc.BufferCount = 2;
		swapChainDesc.BufferDesc.Width = width;
		swapChainDesc.BufferDesc.Height = height;
		swapChainDesc.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
		swapChainDesc.BufferDesc.RefreshRate.Numerator = 60;
		swapChainDesc.BufferDesc.RefreshRate.Denominator = 1;
		swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
		swapChainDesc.OutputWindow = mHwnd;
		swapChainDesc.Windowed = true;
		swapChainDesc.SampleDesc.Count = 1;
		swapChainDesc.SampleDesc.Quality = 0;
#if PRESENT1
		swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
#endif
		unsigned int creationFlags = 0;

#ifdef _DEBUG
		creationFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif

		HRESULT result;
		unsigned int driver = 0;
		
		IDXGISwapChain* tempSwapChain = nullptr;

		for (driver = 0; driver < totalDriverTypes; ++driver)
		{
			result = D3D11CreateDeviceAndSwapChain(0, driverTypes[driver], 0, creationFlags,
				featureLevels, totalFeatureLevels,
				D3D11_SDK_VERSION, &swapChainDesc, &tempSwapChain,
				&mD3dDevice, &mFeatureLevel, &mD3dContext);

			if (SUCCEEDED(result))
			{
				mDriverType = driverTypes[driver];
				break;
			}
		}

		tempSwapChain->QueryInterface(IID_IDXGISwapChain1, (void**)&mSwapChain);
//		mSwapChain = (IDXGISwapChain1*)tempSwapChain;

		if (FAILED(result))
		{
			std::cout << "Failed to create the Direct3D device!\n";
			return result;
		}

		ID3D11Texture2D* backBufferTexture;

		result = mSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (LPVOID*)&backBufferTexture);

		if (FAILED(result))
		{
			std::cout << ("Failed to get the swap chain back buffer!");
			return result;
		}

		result = mD3dDevice->CreateRenderTargetView(backBufferTexture, 0, &mRenderTargetView);

		if (backBufferTexture)
			backBufferTexture->Release();

		if (FAILED(result))
		{
			std::cout << ("Failed to create the render target view!");
			return result;
		}

		mD3dContext->OMSetRenderTargets(1, &mRenderTargetView, 0);

		D3D11_VIEWPORT viewport;
		viewport.Width = static_cast<float>(width);
		viewport.Height = static_cast<float>(height);
		viewport.MinDepth = 0.0f;
		viewport.MaxDepth = 1.0f;
		viewport.TopLeftX = 0.0f;
		viewport.TopLeftY = 0.0f;

		mD3dContext->RSSetViewports(1, &viewport);

		/////////////

		// Create a depth stencil view
		D3D11_TEXTURE2D_DESC depthTexDesc;
		ZeroMemory(&depthTexDesc, sizeof(depthTexDesc));
		depthTexDesc.Width = width;
		depthTexDesc.Height = height;
		depthTexDesc.MipLevels = 1;
		depthTexDesc.ArraySize = 1;
		depthTexDesc.Format = DXGI_FORMAT_D24_UNORM_S8_UINT;
		depthTexDesc.SampleDesc.Count = 1;
		depthTexDesc.SampleDesc.Quality = 0;
		depthTexDesc.Usage = D3D11_USAGE_DEFAULT;
		depthTexDesc.BindFlags = D3D11_BIND_DEPTH_STENCIL;
		depthTexDesc.CPUAccessFlags = 0;
		depthTexDesc.MiscFlags = 0;

		result = mD3dDevice->CreateTexture2D(&depthTexDesc, NULL, &depthTexture);
		if (FAILED(result))
		{
			std::cout << ("Failed to create the depth texture!\n");
			return result;
		}

		// Create the depth stencil view
		D3D11_DEPTH_STENCIL_VIEW_DESC descDSV;
		ZeroMemory(&descDSV, sizeof(descDSV));
		descDSV.Format = depthTexDesc.Format;
		descDSV.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;
		descDSV.Texture2D.MipSlice = 0;
		result = mD3dDevice->CreateDepthStencilView(depthTexture, &descDSV, &mDepthStencilView);
		if (FAILED(result))
		{
			std::cout << ("Failed to create the depth stencil target view!\n");
			return result;
		}

/*		float color[4] = {1.0f, 0.0f, 1.0f, 0.0f};
		mD3dContext->ClearRenderTargetView(mRenderTargetView, color);
		mSwapChain->Present(0, 0);
*/
		return S_OK;
	}

	HRESULT		DeviceResources::resize(void* hwnd)
	{
		mHwnd = (HWND)hwnd;

		ID3D11RenderTargetView* nullViews[] = { nullptr };
		mD3dContext->OMSetRenderTargets(ARRAYSIZE(nullViews), nullViews, nullptr);

		if (depthTexture) depthTexture->Release();
		if (mDepthStencilView) mDepthStencilView->Release();
		if (mRenderTargetView) mRenderTargetView->Release();

		depthTexture = 0;
		mDepthStencilView = 0;
		mRenderTargetView = 0;

		RECT dimensions = rect;
//		GetClientRect(mHwnd, &dimensions);
//		unsigned int width_ = dimensions.right - dimensions.left;
		unsigned int width_ = rect.right;
		unsigned int height_ = dimensions.bottom - dimensions.top;

		HRESULT result;
		if (mSwapChain != nullptr)
		{
			result = mSwapChain->ResizeBuffers(2, lround(width_ ? width_ : 1), lround(height_ ? height_ : 1), DXGI_FORMAT_R8G8B8A8_UNORM, 0);
			if (FAILED(result))
			{
				std::cout << ("Failed to resize the swap chain back buffer!");
				return result;
			}
		}


		ID3D11Texture2D* backBufferTexture;
		result = mSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (LPVOID*)&backBufferTexture);
		if (FAILED(result))
		{
			std::cout << ("Failed to get the swap chain back buffer!");
			return result;
		}
		result = mD3dDevice->CreateRenderTargetView(backBufferTexture, 0, &mRenderTargetView);

		if (backBufferTexture)
			backBufferTexture->Release();

		if (FAILED(result))
		{
			std::cout << ("Failed to create the render target view!");
			return result;
		}

		mD3dContext->OMSetRenderTargets(1, &mRenderTargetView, 0);

		D3D11_VIEWPORT viewport;
		viewport.Width = static_cast<float>(width_);
		viewport.Height = static_cast<float>(height_);
		viewport.MinDepth = 0.0f;
		viewport.MaxDepth = 1.0f;
		viewport.TopLeftX = (float)rect.left;// 0.0f;
		viewport.TopLeftY = (float)rect.top;// 0.0f;
		mD3dContext->RSSetViewports(1, &viewport);

		// Create a depth stencil view
		D3D11_TEXTURE2D_DESC depthTexDesc;
		ZeroMemory(&depthTexDesc, sizeof(depthTexDesc));
		depthTexDesc.Width = width_ ? width_ : 1;
		depthTexDesc.Height = height_ ? height_ : 1;
		depthTexDesc.MipLevels = 1;
		depthTexDesc.ArraySize = 1;
		depthTexDesc.Format = DXGI_FORMAT_D24_UNORM_S8_UINT;
		depthTexDesc.SampleDesc.Count = 1;
		depthTexDesc.SampleDesc.Quality = 0;
		depthTexDesc.Usage = D3D11_USAGE_DEFAULT;
		depthTexDesc.BindFlags = D3D11_BIND_DEPTH_STENCIL;
		depthTexDesc.CPUAccessFlags = 0;
		depthTexDesc.MiscFlags = 0;

		result = mD3dDevice->CreateTexture2D(&depthTexDesc, NULL, &depthTexture);
		if (FAILED(result))
		{
			std::cout << ("Failed to create the depth texture!\n");
			return result;
		}

		// Create the depth stencil view
		D3D11_DEPTH_STENCIL_VIEW_DESC descDSV;
		ZeroMemory(&descDSV, sizeof(descDSV));
		descDSV.Format = depthTexDesc.Format;
		descDSV.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;
		descDSV.Texture2D.MipSlice = 0;
		result = mD3dDevice->CreateDepthStencilView(depthTexture, &descDSV, &mDepthStencilView);
		if (FAILED(result))
		{
			std::cout << ("Failed to create the depth stencil target view!\n");
			return result;
		}

		return S_OK;
	}


	HRESULT     DeviceResources::destroyDX()
	{
		if (mD3dContext)
		{
			mD3dContext->ClearState();
		}

		mD3dContext->Release();
		mD3dDevice->Release();
		mRenderTargetView->Release();
		mDepthStencilView->Release();

		return S_OK;
	}

	void       DeviceResources::setUpViewport()
	{
		D3D11_VIEWPORT vp;
		vp.Width = (float)mWidth;
		vp.Height = (float)mHeight;
		vp.MinDepth = 0.0f;
		vp.MaxDepth = 1.0f;
		vp.TopLeftX = 0;
		vp.TopLeftY = 0;
		mD3dContext->RSSetViewports(1, &vp);
	}

	void DeviceResources::Present()
	{		
		float color[4] = { 1.0, 0.0, 1.0, 1.0f };

		if (mRandClearColor)
		{
			float r = (float)rand() / RAND_MAX;
			float g = (float)rand() / RAND_MAX;
			float b = (float)rand() / RAND_MAX;

			color[0] = r;
			color[1] = g;
			color[2] = b;
			color[3] = 1.0;
		}

		mD3dContext->ClearRenderTargetView(mRenderTargetView, color);
#if PRESENT1
		RECT dimensions = rect;
//		GetClientRect(mHwnd, &dimensions);
/*		dimensions.left = 0;
		dimensions.right = 110;
		dimensions.top = 0;
		dimensions.bottom = 200;//rect.bottom;
*/		
		RECT dimensions1 = dimensions;
		RECT array[2] = { dimensions, dimensions1 };
//*/
		DXGI_PRESENT_PARAMETERS parameters = { 0 };
		parameters.DirtyRectsCount = firstInstance ? 0 : 1 ;
		parameters.pDirtyRects = firstInstance ? nullptr : array;
//		parameters.pDirtyRects[0] = { rect.left, rect.right, rect.top, rect.bottom };
//		parameters.pDirtyRects = &rect;

		parameters.pScrollRect = nullptr;
		parameters.pScrollOffset = nullptr;

//		mSwapChain->Present(0, 0);
		mSwapChain->Present1(0, 0, &parameters);
		firstInstance = false;

/*		if (firstInstance)
		{
			mSwapChain->Present(0, 0);
			firstInstance = false;
		}
		else
			mSwapChain->Present1(0, 0, &parameters);
*/
			
#else
		mSwapChain->Present(0, 0);
#endif

	}
}