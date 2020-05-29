using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Synergy.Requests.Models
{
	/// <summary>
	/// A wrapper struct to store both Request object and Response object.
	/// <br>Used for returning value from an internal Function.</br>
	/// </summary>
	/// <typeparam name="TRequestType">The Request object instance</typeparam>
	/// <typeparam name="UResponseType">The Response object instance</typeparam>
	public struct InternalRequestAsObjectModel<TRequestType, UResponseType>
	{
		[JsonProperty]
		public readonly TRequestType RequestObject;

		[JsonProperty]
		public readonly UResponseType ResponseObject;

		public InternalRequestAsObjectModel(TRequestType _requestObj, UResponseType _responseObj)
		{
			RequestObject = _requestObj ?? throw new ArgumentNullException(nameof(_requestObj));
			ResponseObject = _responseObj ?? throw new ArgumentNullException(nameof(_responseObj));
		}

		public string GetRequestJson() => RequestObject != null ? JsonConvert.SerializeObject(RequestObject) : "";

		public string GetResponseJson() => ResponseObject != null ? JsonConvert.SerializeObject(ResponseObject) : "";
	}
}
