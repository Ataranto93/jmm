﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using NLog;

namespace AniDBAPI
{
	public class APIUtils
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

	    public const int LastYear = 2050;

	    public static string DownloadWebPage(string url)
		{
			try
			{
				HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
				webReq.Timeout = 20000; // 20 seconds
				webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                webReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				HttpWebResponse WebResponse = (HttpWebResponse)webReq.GetResponse();

				Stream responseStream = WebResponse.GetResponseStream();
                String enco = WebResponse.CharacterSet;
                Encoding encoding = null;
                if (!String.IsNullOrEmpty(enco))
                    encoding = Encoding.GetEncoding(WebResponse.CharacterSet);
                if (encoding == null)
                    encoding = Encoding.Default;
			    StreamReader Reader = new StreamReader(responseStream, encoding);
                
				string output = Reader.ReadToEnd();

				WebResponse.Close();
				responseStream.Close();

				return output;
			}
			catch (Exception ex)
			{
				logger.ErrorException("Error in APIUtils.DownloadWebPage: {0}", ex);
				return "";
			}
		}

		public static Stream DownloadWebBinary(string url)
		{
			try
			{
				HttpWebResponse response = null;
				HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
				// Note: some network proxies require the useragent string to be set or they will deny the http request
				// this is true for instance for EVERY thailand internet connection (also needs to be set for banners/episodethumbs and any other http request we send)
				webReq.UserAgent = "Anime2MP";
				webReq.Timeout = 20000; // 20 seconds
				response = (HttpWebResponse)webReq.GetResponse();

				if (response != null) // Get the stream associated with the response.
					return response.GetResponseStream();
				else
					return null;
			}
			catch (Exception ex)
			{
				logger.ErrorException("Error in APIUtils.DownloadWebBinary: {0}", ex);
				return null;
			}
		}



		

		
	}
}
