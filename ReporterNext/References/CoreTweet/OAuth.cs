// The MIT License (MIT)
//
// CoreTweet - A .NET Twitter Library supporting Twitter API 1.1
// Copyright (c) 2013-2018 CoreTweet Development Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CoreTweet.Core;
using Newtonsoft.Json.Linq;

namespace CoreTweet
{
    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) methods for OAuth authentication.
    /// </summary>
    public static partial class OAuth
    {
        /// <summary>
        /// Represents an OAuth session.
        /// </summary>
        public class OAuthSession
        {
            /// <summary>
            /// Gets or sets the consumer key.
            /// </summary>
            public string ConsumerKey { get; set; }

            /// <summary>
            /// Gets or sets the consumer secret.
            /// </summary>
            public string ConsumerSecret { get; set; }

            /// <summary>
            /// Gets or sets the request token.
            /// </summary>
            public string RequestToken { get; set; }

            /// <summary>
            /// Gets or sets the request token secret.
            /// </summary>
            public string RequestTokenSecret { get; set; }

            /// <summary>
            /// Gets or sets the options of the connection.
            /// </summary>
            public ConnectionOptions ConnectionOptions { get; set; }

            /// <summary>
            /// Gets the authorize URL.
            /// </summary>
            public Uri AuthorizeUri
            {
                get
                {
                    var options = this.ConnectionOptions ?? ConnectionOptions.Default;
                    return new Uri(InternalUtils.GetUrl(options, options.ApiUrl, false, "oauth/authorize") + "?oauth_token=" + RequestToken);
                }
            }
        }

        private static Uri GetRequestTokenUrl(ConnectionOptions options)
        {
            if (options == null) options = ConnectionOptions.Default;
            return new Uri(InternalUtils.GetUrl(options, options.ApiUrl, false, "oauth/request_token"));
        }

        private static Uri GetAccessTokenUrl(ConnectionOptions options)
        {
            if (options == null) options = ConnectionOptions.Default;
            return new Uri(InternalUtils.GetUrl(options, options.ApiUrl, false, "oauth/access_token"));
        }
    }

    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) methods for OAuth 2 Authentication.
    /// </summary>
    public static partial class OAuth2
    {
        private static Uri GetAccessTokenUrl(ConnectionOptions options)
        {
            if (options == null) options = ConnectionOptions.Default;
            return new Uri(InternalUtils.GetUrl(options, options.ApiUrl, false, "oauth2/token"));
        }

        private static Uri GetInvalidateTokenUrl(ConnectionOptions options)
        {
            if (options == null) options = ConnectionOptions.Default;
            return new Uri(InternalUtils.GetUrl(options, options.ApiUrl, false, "oauth2/invalidate_token"));
        }

        private static string CreateCredentials(string consumerKey, string consumerSecret)
        {
            return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(consumerKey + ":" + consumerSecret));
        }
    }
}

