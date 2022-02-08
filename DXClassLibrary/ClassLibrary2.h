#pragma once
#include "Device.h"
#include <windows.h>
#include <vcclr.h>
#using <System.dll>
#using <WindowsBase.dll>
#include <vector>

using namespace System;
using namespace myVRXamlComponent;

public ref class DXDeviceResources
{
public:
	// Allocate the native object on the C++ Heap via a constructor
	DXDeviceResources() : m_Impl(new DeviceResources) {}

	// Deallocate the native object on a destructor
	~DXDeviceResources()
	{
		delete m_Impl;
	}

protected:
	// Deallocate the native object on the finalizer just in case no destructor is called
	!DXDeviceResources() 
	{
		delete m_Impl;
	}

public:

	void Present() 
	{ 
		m_Impl->Present(); 
	}

	bool ResizeRenderTarget(void* pResource, int x, int y, int width, int height)
	{
		return m_Impl->resizeRenderTarget(pResource, x, y, width, height);
	}

	bool InitDeviceResources(void* pResource, int x, int y, int width, int height)
	{
		return m_Impl->initDeviceResources(pResource, x, y, width, height);
	}

	void randClearColor(bool value)
	{
		m_Impl->randClearColor(value);
	}

	DeviceResources* m_Impl;
};

namespace ClassLibrary2 {
	public ref class Class1
	{
		// TODO: Add your methods for this class here.
		DXDeviceResources^ contextMap;
	};
}
