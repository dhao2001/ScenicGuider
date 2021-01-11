#pragma once

using namespace System;

namespace GraphLibrary
{
	public ref class GraphNotFullyConnectedException : public ApplicationException
	{
	public:
		String^ Message;
		GraphNotFullyConnectedException() : ApplicationException()
		{
			Message = "This Graph is Not Fully Connected.";
		}

		GraphNotFullyConnectedException(String^ message) : ApplicationException(message)
		{

		}
	};
}