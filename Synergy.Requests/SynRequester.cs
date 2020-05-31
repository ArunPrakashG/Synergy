using Newtonsoft.Json;
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
	public class SynRequester : IDisposable {
		private const int MAX_TRIES = 3;
		private static readonly Random Random = new Random();
		private static readonly SemaphoreSlim Sync = new SemaphoreSlim(1, 1);
		private readonly string InstanceID;
		private readonly ILogger Logger;
		private readonly int DELAY_BETWEEN_REQUESTS = 5; // secs
		private readonly int DELAY_BETWEEN_FAILED_REQUESTS = 10; // secs		
		private readonly HttpClientHandler ClientHandler;
		private readonly HttpClient Client;
		private readonly CookieContainer Cookies;

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

		public async Task<string> InternelRequestGet(string requestUrl, Dictionary<string, string> data, int maxTries = MAX_TRIES) {
			if (string.IsNullOrEmpty(requestUrl)) {
				return default;
			}

			bool success = false;
			for (int i = 0; i < maxTries; i++) {
				try {
					using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl)) {
						foreach (KeyValuePair<string, string> p in data) {
							if (string.IsNullOrEmpty(p.Key) || string.IsNullOrEmpty(p.Value)) {
								continue;
							}

							request.Headers.Add(p.Key, p.Value);
						}

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

		public async Task<T> InternalRequestAsObject<T>(
			HttpMethod method, string requestUrl, Dictionary<string, string> postData, int maxTries = MAX_TRIES) {
			if (string.IsNullOrEmpty(requestUrl)) {
				return default;
			}

			bool success = false;
			for (int i = 0; i < maxTries; i++) {
				try {
					using (HttpRequestMessage request = new HttpRequestMessage(method, requestUrl)) {
						foreach (KeyValuePair<string, string> p in postData) {
							if (string.IsNullOrEmpty(p.Key) || string.IsNullOrEmpty(p.Value)) {
								continue;
							}

							request.Headers.Add(p.Key, p.Value);
						}

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

		public async Task<UResponseType> InternalRequestAsObject<TRequestType, UResponseType>(
			TRequestType requestJsonContent, HttpMethod method, string requestUrl, Dictionary<string, string> data, int maxTries = MAX_TRIES) {
			if (string.IsNullOrEmpty(requestUrl) || requestJsonContent == null) {
				return default;
			}

			bool success = false;
			for (int i = 0; i < maxTries; i++) {
				try {
					using (HttpRequestMessage request = new HttpRequestMessage(method, requestUrl)) {
						foreach (KeyValuePair<string, string> p in data) {
							if (string.IsNullOrEmpty(p.Key) || string.IsNullOrEmpty(p.Value)) {
								continue;
							}

							request.Headers.Add(p.Key, p.Value);
						}

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

		public async Task<InternalRequestAsObjectModel<TRequestType, UResponseType>> InternalRequestAsObject<TRequestType, UResponseType>(
			HttpMethod method, string requestUrl, Dictionary<string, string> data, TRequestType requestJsonContent, int maxTries = MAX_TRIES) {
			if (string.IsNullOrEmpty(requestUrl) || requestJsonContent == null) {
				return default;
			}

			bool success = false;
			for (int i = 0; i < maxTries; i++) {
				try {
					using (HttpRequestMessage request = new HttpRequestMessage(method, requestUrl)) {
						foreach (KeyValuePair<string, string> p in data) {
							if (string.IsNullOrEmpty(p.Key) || string.IsNullOrEmpty(p.Value)) {
								continue;
							}

							request.Headers.Add(p.Key, p.Value);
						}

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

		public void Dispose() {
			ClientHandler?.Dispose();
			Client?.Dispose();
		}
	}
}
