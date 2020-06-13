using Newtonsoft.Json;
using System;

namespace Synergy.Requests.Models {
	/// <summary>
	/// A wrapper struct to store both Request object and Response object.
	/// <br>Used for returning value from an internal Function.</br>
	/// </summary>
	/// <typeparam name="TRequestType">The Request object instance</typeparam>
	/// <typeparam name="UResponseType">The Response object instance</typeparam>
	public struct InternalRequestAsObjectModel<TRequestType, UResponseType>
	{
		[JsonProperty]
		/// <summary>
		/// The request object.
		/// </summary>
		public readonly TRequestType RequestObject;

		[JsonProperty]
		/// <summary>
		/// The response object.
		/// </summary>
		public readonly UResponseType ResponseObject;

		/// <summary>
		/// The constructor.
		/// </summary>
		/// <param name="_requestObj">Sets the request object.</param>
		/// <param name="_responseObj">Sets the response object.</param>
		public InternalRequestAsObjectModel(TRequestType _requestObj, UResponseType _responseObj)
		{
			RequestObject = _requestObj ?? throw new ArgumentNullException(nameof(_requestObj));
			ResponseObject = _responseObj ?? throw new ArgumentNullException(nameof(_responseObj));
		}

		/// <summary>
		/// Gets the request object, if not null, as a JSON string.
		/// </summary>
		/// <returns></returns>
		public string GetRequestJson() => RequestObject != null ? JsonConvert.SerializeObject(RequestObject) : "";

		/// <summary>
		/// Gets the response object, if not null, as a JSON string.
		/// </summary>
		/// <returns></returns>
		public string GetResponseJson() => ResponseObject != null ? JsonConvert.SerializeObject(ResponseObject) : "";
	}
}
