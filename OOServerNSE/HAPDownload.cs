/*
 * OptionsOracle NSE Interface Class Library
 * Copyright SamoaSky
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 */using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace OOServerNSE
{
	public class HAPDownload
	{
		public enum DownloadMethod { Get, Post };
		private static CookieContainer Cookies
		{
			get
			{
				return HAPDownload.FCookies;
			}
			set
			{
				HAPDownload.FCookies = value;
			}
		}

		public DownloadMethod downloadMethod
		{
			get
			{
				return this.FDownloadMethod;
			}
			set
			{
				this.FDownloadMethod = value;
			}
		}

		public bool IsAsyncDownload
		{
			get
			{
				return this.FIsAsyncDownload;
			}
			set
			{
				this.FIsAsyncDownload = value;
			}
		}

		public string SourceURL
		{
			get
			{
				return this.FSourceURL;
			}
			set
			{
				this.FSourceURL = value;
			}
		}

		public bool CheckCacheModified
		{
			get
			{
				return this.FCheckCacheModified;
			}
			set
			{
				this.FCheckCacheModified = value;
			}
		}

		public DateTime CacheLastModified
		{
			get
			{
				return this.FCacheLastModified;
			}
			set
			{
				this.FCacheLastModified = value;
			}
		}

		public string DestinationPath
		{
			get
			{
				return this.FDestinationPath;
			}
			set
			{
				this.FDestinationPath = value;
			}
		}

		public bool AddReferrer
		{
			get
			{
				return this.FAddReferrer;
			}
			set
			{
				this.FAddReferrer = value;
			}
		}

		public string ContentType
		{
			get
			{
				return this.FContentType;
			}
			set
			{
				this.FContentType = value;
			}
		}

		public int bytesDone
		{
			get
			{
				return this.FbytesDone;
			}
			set
			{
				this.FbytesDone = value;
			}
		}

		public int bytesTotal
		{
			get
			{
				return this.FbytesTotal;
			}
			set
			{
				this.FbytesTotal = value;
			}
		}

		public virtual void Start(BackgroundWorker worker, DoWorkEventArgs e)
		{
			this.FWorker = worker;
			try
			{
				this.Execute();
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public void AddParam(string name, string val)
		{
			this.FPostParams.Add(name, val);
		}

		private string GetParams()
		{
			string text = "";
			foreach (KeyValuePair<string, string> keyValuePair in this.FPostParams)
			{
				text = string.Concat(new string[]
				{
					text,
					keyValuePair.Key,
					"=",
					keyValuePair.Value,
					"&"
				});
			}
			return text.TrimEnd(new char[]
			{
				'&'
			});
		}

		public void AddHeader(string name, string val)
		{
			this.FHeaders.Add(name, val);
		}

		private bool IsCancelled()
		{
			return this.FWorker != null && this.FWorker.CancellationPending;
		}

		private void ReportProgress(int downloadedBytes, int totalBytes)
		{
			this.bytesDone = downloadedBytes;
			this.bytesTotal = totalBytes;
			if (this.FWorker != null && totalBytes != 0)
			{
				this.FWorker.ReportProgress(Convert.ToInt32(downloadedBytes * 100 / totalBytes), this);
			}
		}

		public void DoDownload()
		{
			try
			{
				HttpWebRequest httpWebRequest = null;
				this.SetupRequest(out httpWebRequest);
				this.SaveResponse(httpWebRequest);
			}
			catch (Exception)
			{
			}
		}

		private void SaveResponse(HttpWebRequest httpWebRequest)
		{
			HttpWebResponse httpWebResponse = null;
			Stream stream = null;
			this.memStream = null;
			this.FAlreadyDownloaded = false;
			try
			{
				try
				{
					foreach (object obj in HAPDownload.FCookies.GetCookies(new Uri("https://nseindia.com")))
					{
						Cookie cookie = (Cookie)obj;
						if (cookie.Name == "ak_bmsc")
						{
							cookie.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1.0));
						}
					}
					httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
					this.StatusCode = httpWebResponse.StatusCode;
					this.StatusDesc = httpWebResponse.StatusDescription;
					try
					{
						HAPDownload.Expires = DateTime.Now.AddMinutes(30.0);
						if (HAPDownload.FCookies.Count < 1)
						{
							HAPDownload.FCookies = new CookieContainer();
						}
						if (httpWebResponse.Cookies != null)
						{
							for (int i = 0; i < httpWebResponse.Headers.Count; i++)
							{
								string key = httpWebResponse.Headers.GetKey(i);
								string input = httpWebResponse.Headers.Get(i);
								if (key == "Set-Cookie")
								{
									Match match = Regex.Match(input, "(.+?)=(.+?);");
									Match match2 = Regex.Match(input, "(.omain=)(.+?);", RegexOptions.IgnoreCase);
									if (match.Captures.Count > 0)
									{
										HAPDownload.FCookies.Add(new Cookie(match.Groups[1].Value, match.Groups[2].Value, "/", match2.Groups[2].Value));
									}
								}
							}
						}
					}
					catch
					{
					}
				}
				catch (WebException ex)
				{
					this.ErrorStatus = ex.Status;
					if (ex.Status == WebExceptionStatus.ProtocolError)
					{
						this.StatusCode = ((HttpWebResponse)ex.Response).StatusCode;
						this.StatusDesc = ((HttpWebResponse)ex.Response).StatusDescription;
					}
					if (this.StatusCode != HttpStatusCode.NotModified)
					{
						throw ex;
					}
					this.FAlreadyDownloaded = true;
				}
				if (!this.FAlreadyDownloaded)
				{
					stream = this.GetStreamForResponse(httpWebResponse);
					this.memStream = new MemoryStream();
					byte[] array = new byte[10240];
					int num = 0;
					int totalBytes = (int)httpWebRequest.ContentLength;
					int num2;
					while ((num2 = stream.Read(array, 0, array.Length)) > 0 && (this.FWorker == null || !this.FWorker.CancellationPending))
					{
						this.memStream.Write(array, 0, num2);
						num += num2;
						if (this.FWorker != null)
						{
							this.ReportProgress(num, totalBytes);
						}
					}
					if (!this.UseMemoryStream)
					{
						this.CopyMemStreamToDestination();
					}
					this.memStream.Position = 0L;
				}
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
				}
			}
		}

		private Stream GetStreamForResponse(HttpWebResponse httpWebResponse)
		{
			string a = httpWebResponse.ContentEncoding.ToUpperInvariant();
			Stream result;
			if (!(a == "GZIP"))
			{
				if (!(a == "DEFLATE"))
				{
					result = httpWebResponse.GetResponseStream();
				}
				else
				{
					result = new DeflateStream(httpWebResponse.GetResponseStream(), CompressionMode.Decompress);
				}
			}
			else
			{
				result = new GZipStream(httpWebResponse.GetResponseStream(), CompressionMode.Decompress);
			}
			return result;
		}

		private bool ShouldUseCache()
		{
			return this.FCheckCacheModified && File.Exists(this.FDestinationPath);
		}

		// Token: 0x06000054 RID: 84 RVA: 0x0001205C File Offset: 0x0001025C
		private void SetupRequest(out HttpWebRequest httpWebRequest)
		{
			if ((HAPDownload.Expires < DateTime.Now || HAPDownload.FCookies == null) && this.FSourceURL.Contains("nseindia.com"))
			{
				this.GetCookies("https://www.nseindia.com/");
			}
			Uri requestUri = new Uri(this.FSourceURL);
			ServicePointManager.Expect100Continue = false;
			ServicePointManager.SecurityProtocol = (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);
			ServicePointManager.DefaultConnectionLimit = 9999;
			ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true));
			ServicePointManager.DefaultConnectionLimit = 2;
			ServicePointManager.MaxServicePoints = 4;
			ServicePointManager.MaxServicePointIdleTime = 10000;
			httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
			httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.102 Safari/537.36";
			httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
			httpWebRequest.KeepAlive = true;
			WebHeaderCollection headers = httpWebRequest.Headers;
			headers.Add("Upgrade-Insecure-Requests", "1");
			headers.Add("Sec-Fetch-Site", "none");
			headers.Add("Sec-Fetch-User", "?1");
			headers.Add("Sec-Fetch-Mode", "navigate");
			headers.Add("Accept-Encoding", "gzip, deflate, br");
			headers.Add("Accept-Language", "en-US,en;q=0.9,hi;q=0.8");
			httpWebRequest.CookieContainer = HAPDownload.FCookies;
			if (this.FAddReferrer)
			{
				httpWebRequest.Referer = this.Referrer;
			}
			foreach (KeyValuePair<string, string> keyValuePair in this.FHeaders)
			{
				httpWebRequest.Headers.Add(keyValuePair.Key, keyValuePair.Value);
			}
			httpWebRequest.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");
			httpWebRequest.Headers.Add(HttpRequestHeader.Pragma, "no-cache");
			if (this.FDownloadMethod == DownloadMethod.Post)
			{
				Stream stream = null;
				httpWebRequest.Method = "POST";
				if (this.FContentType == null)
				{
					throw new Exception("ContentType not set for POST download");
				}
				httpWebRequest.ContentType = this.FContentType;
				byte[] bytes = new ASCIIEncoding().GetBytes(this.GetParams());
				httpWebRequest.ContentLength = (long)bytes.Length;
				try
				{
					stream = httpWebRequest.GetRequestStream();
					stream.Write(bytes, 0, bytes.Length);
					return;
				}
				finally
				{
					if (stream != null)
					{
						stream.Close();
					}
				}
			}
			if (this.ShouldUseCache())
			{
				httpWebRequest.IfModifiedSince = this.FCacheLastModified;
			}
			httpWebRequest.Method = "GET";
		}

		private void GetCookies(string cookieUrl)
		{
			ServicePointManager.Expect100Continue = false;
			ServicePointManager.SecurityProtocol = (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);
			ServicePointManager.DefaultConnectionLimit = 9999;
			ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true));
			ServicePointManager.DefaultConnectionLimit = 2;
			ServicePointManager.MaxServicePoints = 4;
			ServicePointManager.MaxServicePointIdleTime = 10000;
			CookieContainer cookieContainer = new CookieContainer();
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(cookieUrl);
			httpWebRequest.KeepAlive = true;
			WebHeaderCollection headers = httpWebRequest.Headers;
			headers.Add("Upgrade-Insecure-Requests", "1");
			headers.Add("Sec-Fetch-Site", "none");
			headers.Add("Sec-Fetch-Mode", "navigate");
			headers.Add("Accept-Encoding", "gzip, deflate, br");
			headers.Add("Accept-Language", "en-US,en;q=0.9,hi;q=0.8");
			httpWebRequest.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");
			httpWebRequest.Headers.Add(HttpRequestHeader.Pragma, "no-cache");
			foreach (KeyValuePair<string, string> keyValuePair in this.FHeaders)
			{
				httpWebRequest.Headers.Add(keyValuePair.Key, keyValuePair.Value);
			}
			httpWebRequest.CookieContainer = cookieContainer;
			HAPDownload.FCookies = new CookieContainer();
			using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
			{
				if (httpWebResponse.Cookies != null)
				{
					for (int i = 0; i < httpWebResponse.Headers.Count; i++)
					{
						string key = httpWebResponse.Headers.GetKey(i);
						string text = httpWebResponse.Headers.Get(i);
						if (key == "Set-Cookie")
						{
							text = Regex.Replace(text, "(e|E)xpires=(.+?)(;|$)|(P|p)ath=(.+?);", "");
							foreach (string input in text.Split(new char[]
							{
								','
							}))
							{
								Match match = Regex.Match(input, "(.+?)=(.+?);");
								Match match2 = Regex.Match(input, "(.omain=)(.+?);", RegexOptions.IgnoreCase);
								if (match.Captures.Count > 0)
								{
									if (match2.Groups[2].Value != "")
									{
										string domain = match2.Groups[2].Value.TrimStart(new char[]
										{
											'.'
										});
										HAPDownload.FCookies.Add(new Cookie(match.Groups[1].Value, match.Groups[2].Value, "/", domain));
									}
									else
									{
										HAPDownload.FCookies.Add(new Cookie(match.Groups[1].Value, match.Groups[2].Value, "/", "nseindia.com"));
									}
								}
							}
						}
					}
				}
			}
		}

		private void CopyMemStreamToDestination()
		{
			if (this.FWorker == null || (this.FWorker != null && !this.FWorker.CancellationPending))
			{
				if (File.Exists(this.FDestinationPath))
				{
					File.Delete(this.FDestinationPath);
				}
				FileStream fileStream = File.OpenWrite(this.FDestinationPath);
				this.memStream.Position = 0L;
				try
				{
					byte[] array = new byte[32768];
					for (; ; )
					{
						int num = this.memStream.Read(array, 0, array.Length);
						if (num <= 0)
						{
							break;
						}
						fileStream.Write(array, 0, num);
					}
				}
				finally
				{
					fileStream.Close();
				}
			}
		}

		~HAPDownload()
		{
		}

		public bool IsAsync()
		{
			return this.FIsAsyncDownload;
		}

		public void Execute()
		{
			this.DoDownload();
		}

		public static DateTime Expires;

		public static CookieContainer FCookies;

		private DownloadMethod FDownloadMethod;

		private bool FIsAsyncDownload = true;

		private string FSourceURL;

		private bool FCheckCacheModified;

		private DateTime FCacheLastModified;

		private string FDestinationPath;

		private bool FAddReferrer = true;

		public string Referrer;

		private string FContentType;

		private int FbytesDone;

		private int FbytesTotal;

		public WebExceptionStatus ErrorStatus;

		public HttpStatusCode StatusCode;

		public string StatusDesc;

		public bool UseMemoryStream = true;

		public MemoryStream memStream;

		protected BackgroundWorker FWorker;

		private Dictionary<string, string> FPostParams = new Dictionary<string, string>();

		private Dictionary<string, string> FHeaders = new Dictionary<string, string>();

		private bool FAlreadyDownloaded;

		private const string tmpExtension = ".tmp";
	}
}
