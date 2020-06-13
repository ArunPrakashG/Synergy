using Newtonsoft.Json;
using Synergy.Extensions;
using Synergy.Logging;
using Synergy.Logging.Interfaces;
using Synergy.Requests.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Synergy.Requests {
	/// <summary>
	/// The main <see cref="SynRequester" /> class.
	/// <para>Helpers to send requests and get their response, in multiple ways, with inbuilt request delays and retry mechanism</para>.
	/// <para>Inherits <see cref="IDisposable" />, all methods can be warpped inside using() blocks</para>.
	/// </summary>
	public sealed class SynRequester : IDisposable {
		/// <summary>
		/// The maximum number of tries before the request is considered as a failure.
		/// </summary>
		private const int MAX_TRIES = 3;

		/// <summary>
		/// A static instance of <see cref="Random" /> ensures that there is least chance of any repeated values.
		/// <para>This helpers in generating unique identifiers for our <see cref="SynRequester" /> Instances.</para>
		/// </summary>
		/// <returns></returns>
		private static readonly Random Random = new Random();

		/// <summary>
		/// A static semaphore ensures that the requesting process is in sync accross all the instances of <see cref="SynRequster" />.
		/// <para>By this way, it makes it easier to implement timeouts between multiple requests and such.</para>
		/// </summary>
		/// <returns></returns>
		private static readonly SemaphoreSlim Sync = new SemaphoreSlim(1, 1);

		/// <summary>
		/// The generated unique identifier for this particuler <see cref="SynRequester" /> instance.
		/// </summary>
		private readonly string InstanceID;

		/// <summary>
		/// The <see cref="ILogger"/> instance of this <see cref="SynRequester"/> instance.
		/// </summary>
		private readonly ILogger Logger;

		/// <summary>
		/// The delay in seconds to wait before another request is executed.
		/// </summary>
		private readonly int DELAY_BETWEEN_REQUESTS = 5; // secs

		/// <summary>
		/// The delay in seconds to wait before another request is executed after a failed request.
		/// </summary>
		private readonly int DELAY_BETWEEN_FAILED_REQUESTS = 10; // secs

		/// <summary>
		/// The underlying <see cref="HttpClientHandler"/> instance.
		/// </summary>		
		private readonly HttpClientHandler ClientHandler;

		/// <summary>
		/// The underlying <see cref="HttpClient"/> instance.
		/// </summary>		
		private readonly HttpClient Client;

		/// <summary>
		/// The underlying <see cref="CookieContainer"/> instance.
		/// </summary>		
		private readonly CookieContainer Cookies;

        /// <summary>
        /// The constructor of <see cref="SynRequester"/>.
		/// <para>Initializes underlying <see cref="HttpClient"/> instances and others.</para>
        /// </summary>
        /// <param name="_httpClientHandler">The <see cref="HttpClientHandler"/> instance to use for the underlying <see cref="HttpClient"/> instance</param>.
        /// <param name="_logger">The <see cref="ILogger"/> instance to use for Logging inside the current instance of <see cref="SynRequester"/></param>.
        /// <param name="_delayBetweenRequests">Sets the delay between each request. (in seconds)</param>
        /// <param name="_delayBetweenFailedRequests">Sets the delay between each failed request. (in seconds)</param>
		public SynRequester(HttpClientHandler _httpClientHandler, ILogger _logger, int _delayBetweenRequests = 3, int _delayBetweenFailedRequests = 10) {
			ClientHandler = _httpClientHandler ?? new HttpClientHandler();
			InstanceID = (Random.Next(10 + _httpClientHandler.GetHashCode() + _httpClientHandler.CookieContainer.GetHashCode()) ^
				Random.Next(_logger != null ? 35 : 15) ^ _delayBetweenFailedRequests + _delayBetweenRequests).GetHashCode().ToString();
			Logger = _logger ?? new Logger($"{nameof(SynRequester)}|{InstanceID}");
			DELAY_BETWEEN_REQUESTS = _delayBetweenRequests;
			DELAY_BETWEEN_FAILED_REQUESTS = _delayBetweenFailedRequests;
			Cookies = ClientHandler.CookieContainer;
			Client = new HttpClient(ClientHandler, false);		
		}

		/// <summary>
		/// Send a GET request to the specified URL and return the result as string.
		/// </summary>
		/// <param name="requestUrl">The URL the send the request to.</param>
		/// <param name="data">The Headers to append with the requests.</param>
		/// <param name="maxTries">The maximum number of tries before the request is considered as a fail.</param>
		/// <returns>The string result of the request if success, else null.</returns>
		public async Task<string> InternelRequestGet(string requestUrl, Dictionary<string, string> data = null, int maxTries = MAX_TRIES) {
			if (string.IsNullOrEmpty(requestUrl)) {
				return default;
			}

			bool success = false;
			for (int i = 0; i < maxTries; i++) {
				try {
					using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl)) {
						data.ForEachElement((s, v) => {
							request.Headers.Add(s, v);
						}, true);

						using (HttpResponseMessage response = await ExecuteRequest(async () => await Client.SendAsync(request).ConfigureAwait(false)).ConfigureAwait(false)) {
							if (!response.IsSuccessStatusCode) {
								continue;
							}

							using (HttpContent responseContent = response.Content) {
								string jsonContent = await responseContent.ReadAsStringAsync().ConfigureAwait(false);

								if (string.IsNullOrEmpty(jsonContent)) {
									continue;
								}

								success = true;
								return jsonContent;
							}
						}
					}
				}
				catch (Exception e) {
					Logger.Exception(e);
					success = false;
					continue;
				}
				finally {
					if (!success) {
						await Task.Delay(TimeSpan.FromSeconds(DELAY_BETWEEN_FAILED_REQUESTS)).ConfigureAwait(false);
					}
				}
			}

			if (!success) {
				Logger.Error("Internal request failed.");
			}

			return default;
		}

		/// <summary>
		/// Sends a <see cref="HttpRequestMessage"/> request and returns result object.
		/// </summary>
		/// <param name="request">The <see cref="HttpRequestMessage"/> request instance.</param>
		/// <param name="maxTries">The maximum number of tries before the request is considered as a fail.</param>
		/// <typeparam name="T">The type of result object, used to deserialize the result json.</typeparam>
		/// <returns>The result object.</returns>
		public async Task<T> InternalRequestAsObject<T>(
			HttpRequestMessage request, int maxTries = MAX_TRIES) {
			if (request == null) {
				return default;
			}

			bool success = false;
			for (int i = 0; i < maxTries; i++) {
				try {
					using (HttpResponseMessage response = await ExecuteRequest(async () => await Client.SendAsync(request).ConfigureAwait(false)).ConfigureAwait(false)) {
						if (!response.IsSuccessStatusCode) {
							continue;
						}

						using (HttpContent responseContent = response.Content) {
							string jsonContent = await responseContent.ReadAsStringAsync().ConfigureAwait(false);

							if (string.IsNullOrEmpty(jsonContent)) {
								continue;
							}

							success = true;
							return JsonConvert.DeserializeObject<T>(jsonContent);
						}
					}
				}
				catch (Exception e) {
					Logger.Exception(e);
					success = false;
					continue;
				}
				finally {
					if (!success) {
						await Task.Delay(TimeSpan.FromSeconds(DELAY_BETWEEN_FAILED_REQUESTS)).ConfigureAwait(false);
					}
				}
			}

			if (!success) {
				Logger.Error("Internal request failed.");
			}

			return default;
		}

		/// <summary>
		/// Send a generic HTTP request to the specified url with the specified method and headers.
		/// </summary>
		/// <param name="method">the <see cref="HttpMethod"/> to use for the request.</param>
		/// <param name="requestUrl">The URL the send the request to.</param>
		/// <param name="data">headers which should be appended to the request, if any.</param>
		/// <param name="maxTries">The maximum number of tries before the request is considered as a fail.</param>
		/// <typeparam name="T">The type of result object, used to deserialize the result json.</typeparam>
		/// <returns>The result object.</returns>
		public async Task<T> InternalRequestAsObject<T>(
			HttpMethod method, string requestUrl, Dictionary<string, string> data, int maxTries = MAX_TRIES) {
			if (string.IsNullOrEmpty(requestUrl)) {
				return default;
			}

			bool success = false;
			for (int i = 0; i < maxTries; i++) {
				try {
					using (HttpRequestMessage request = new HttpRequestMessage(method, requestUrl)) {
						data.ForEachElement((s, v) => {
							request.Headers.Add(s, v);
						}, true);

						using (HttpResponseMessage response = await ExecuteRequest(async () => await Client.SendAsync(request).ConfigureAwait(false)).ConfigureAwait(false)) {
							if (!response.IsSuccessStatusCode) {
								continue;
							}

							using (HttpContent responseContent = response.Content) {
								string jsonContent = await responseContent.ReadAsStringAsync().ConfigureAwait(false);

								if (string.IsNullOrEmpty(jsonContent)) {
									continue;
								}

								success = true;
								return JsonConvert.DeserializeObject<T>(jsonContent);
							}
						}
					}
				}
				catch (Exception e) {
					Logger.Exception(e);
					success = false;
					continue;
				}
				finally {
					if (!success) {
						await Task.Delay(TimeSpan.FromSeconds(DELAY_BETWEEN_FAILED_REQUESTS)).ConfigureAwait(false);
					}
				}
			}

			if (!success) {
				Logger.Error("Internal request failed.");
			}

			return default;
		}

		/// <summary>
		/// Send a generic, unparsed, JSON serializable object as an HTTP request to the specified url with the specified method and headers.
		/// </summary>
		/// <param name="requestJsonContent">the JSON serializable object of the request.</param>
		/// <param name="method">the <see cref="HttpMethod"/> to use for the request.</param>
		/// <param name="requestUrl">The URL the send the request to.</param>
		/// <param name="data">headers which should be appended to the request, if any.</param>
		/// <param name="maxTries">The maximum number of tries before the request is considered as a fail.</param>
		/// <typeparam name="TRequestType">the type of request object.</typeparam>
		/// <typeparam name="UResponseType">the type of response object.</typeparam>
		/// <returns>the result, Deserialized as <see cref="UResponseType"/> type.</returns>
		public async Task<UResponseType> InternalRequestAsObject<TRequestType, UResponseType>(
			TRequestType requestJsonContent, HttpMethod method, string requestUrl, Dictionary<string, string> data, int maxTries = MAX_TRIES) {
			if (string.IsNullOrEmpty(requestUrl) || requestJsonContent == null) {
				return default;
			}

			bool success = false;
			for (int i = 0; i < maxTries; i++) {
				try {
					using (HttpRequestMessage request = new HttpRequestMessage(method, requestUrl)) {
						data.ForEachElement((s, v) => {
							request.Headers.Add(s, v);
						}, true);

						request.Content = new StringContent(JsonConvert.SerializeObject(requestJsonContent));

						using (HttpResponseMessage response = await ExecuteRequest(async () => await Client.SendAsync(request).ConfigureAwait(false)).ConfigureAwait(false)) {
							if (!response.IsSuccessStatusCode) {
								continue;
							}

							using (HttpContent responseContent = response.Content) {
								string jsonContent = await responseContent.ReadAsStringAsync().ConfigureAwait(false);

								if (string.IsNullOrEmpty(jsonContent)) {
									continue;
								}

								success = true;
								return JsonConvert.DeserializeObject<UResponseType>(jsonContent);
							}
						}
					}
				}
				catch (Exception e) {
					Logger.Exception(e);
					success = false;
					continue;
				}
				finally {
					if (!success) {
						await Task.Delay(TimeSpan.FromSeconds(DELAY_BETWEEN_FAILED_REQUESTS)).ConfigureAwait(false);
					}
				}
			}

			if (!success) {
				Logger.Error("Internal request failed.");
			}

			return default;
		}

		/// <summary>
		/// Send a generic, unparsed, JSON serializable object as an HTTP request to the specified url with the specified method and headers and get the response and the request objects in a struct.
		/// </summary>
		/// <param name="method">the <see cref="HttpMethod"/> to use for the request.</param>
		/// <param name="requestUrl">The URL the send the request to.</param>
		/// <param name="data">headers which should be appended to the request, if any.</param>
		/// <param name="requestJsonContent">the JSON serializable object of the request.</param>
		/// <param name="maxTries">The maximum number of tries before the request is considered as a fail.</param>
		/// <typeparam name="TRequestType">the type of request object.</typeparam>
		/// <typeparam name="UResponseType">the type of response object.</typeparam>
		/// <returns>a struct containing both request (<see cref="TRequestType"/>) and response (<see cref="UResponseType"/>) objects.</returns>
		public async Task<InternalRequestAsObjectModel<TRequestType, UResponseType>> InternalRequestAsObject<TRequestType, UResponseType>(
			HttpMethod method, string requestUrl, Dictionary<string, string> data, TRequestType requestJsonContent, int maxTries = MAX_TRIES) {
			if (string.IsNullOrEmpty(requestUrl) || requestJsonContent == null) {
				return default;
			}

			bool success = false;
			for (int i = 0; i < maxTries; i++) {
				try {
					using (HttpRequestMessage request = new HttpRequestMessage(method, requestUrl)) {
						data.ForEachElement((s, v) => {
							request.Headers.Add(s, v);
						}, true);

						request.Content = new StringContent(JsonConvert.SerializeObject(requestJsonContent));

						using (HttpResponseMessage response = await ExecuteRequest(async () => await Client.SendAsync(request).ConfigureAwait(false)).ConfigureAwait(false)) {
							if (!response.IsSuccessStatusCode) {
								continue;
							}

							using (HttpContent responseContent = response.Content) {
								string jsonContent = await responseContent.ReadAsStringAsync().ConfigureAwait(false);

								if (string.IsNullOrEmpty(jsonContent)) {
									continue;
								}

								success = true;
								return new InternalRequestAsObjectModel<TRequestType, UResponseType>(requestJsonContent, JsonConvert.DeserializeObject<UResponseType>(jsonContent));
							}
						}
					}
				}
				catch (Exception e) {
					Logger.Exception(e);
					success = false;
					continue;
				}
				finally {
					if (!success) {
						await Task.Delay(TimeSpan.FromSeconds(DELAY_BETWEEN_FAILED_REQUESTS)).ConfigureAwait(false);
					}
				}
			}

			if (!success) {
				Logger.Error("Internal request failed.");
			}

			return default;
		}

		/// <summary>
		/// A wrapper for all internal requests.
		/// <para>Requests are rate limited through this function().</para>
		/// </summary>
		/// <param name="function">the request function()</param>
		/// <typeparam name="T">the type of the response of the request.</typeparam>
		/// <returns>the result</returns>
		private async Task<T> ExecuteRequest<T>(Func<Task<T>> function) {
			if (function == null) {
				return default;
			}

			await Sync.WaitAsync().ConfigureAwait(false);

			try {
				return await function().ConfigureAwait(false);
			}
			finally {
				await Task.Delay(TimeSpan.FromSeconds(DELAY_BETWEEN_REQUESTS));
				Sync.Release();
			}
		}
		
		/// <summary>
		/// Disposes Semaphores and Clients used internally.
		/// </summary>
		public void Dispose() {
			ClientHandler?.Dispose();
			Client?.Dispose();
		}
	}
}
