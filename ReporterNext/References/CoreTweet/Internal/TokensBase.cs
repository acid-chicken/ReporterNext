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
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using CoreTweet.Rest;
using CoreTweet.Streaming;

namespace CoreTweet.Core
{
    /// <summary>
    /// Represents an OAuth token. This is an <c>abstract</c> class.
    /// </summary>
    public abstract partial class TokensBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokensBase"/> class.
        /// </summary>
        protected TokensBase()
        {
            this.ConnectionOptions = new ConnectionOptions();
        }

        /// <summary>
        /// Gets or sets the consumer key.
        /// </summary>
        public string ConsumerKey { get; set; }
        /// <summary>
        /// Gets or sets the consumer secret.
        /// </summary>
        public string ConsumerSecret { get; set; }

        #region Endpoints for Twitter API

        /// <summary>
        /// Gets the wrapper of account.
        /// </summary>
        public Account Account => new Account(this);
        /// <summary>
        /// Gets the wrapper of application.
        /// </summary>
        public Application Application => new Application(this);
        /// <summary>
        /// Gets the wrapper of blocks.
        /// </summary>
        public Blocks Blocks => new Blocks(this);
        /// <summary>
        /// Gets the wrapper of collections.
        /// </summary>
        public Collections Collections => new Collections(this);
        /// <summary>
        /// Gets the wrapper of direct_messages.
        /// </summary>
        public DirectMessages DirectMessages => new DirectMessages(this);
        /// <summary>
        /// Gets the wrapper of favorites.
        /// </summary>
        public Favorites Favorites => new Favorites(this);
        /// <summary>
        /// Gets the wrapper of friends.
        /// </summary>
        public Friends Friends => new Friends(this);
        /// <summary>
        /// Gets the wrapper of followers.
        /// </summary>
        public Followers Followers => new Followers(this);
        /// <summary>
        /// Gets the wrapper of friendships.
        /// </summary>
        public Friendships Friendships => new Friendships(this);
        /// <summary>
        /// Gets the wrapper of geo.
        /// </summary>
        public Geo Geo => new Geo(this);
        /// <summary>
        /// Gets the wrapper of help.
        /// </summary>
        public Help Help => new Help(this);
        /// <summary>
        /// Gets the wrapper of lists.
        /// </summary>
        public Lists Lists => new Lists(this);
        /// <summary>
        /// Gets the wrapper of media.
        /// </summary>
        public Media Media => new Media(this);
        /// <summary>
        /// Gets the wrapper of mutes.
        /// </summary>
        public Mutes Mutes => new Mutes(this);
        /// <summary>
        /// Gets the wrapper of search.
        /// </summary>
        public Search Search => new Search(this);
        /// <summary>
        /// Gets the wrapper of saved_searches.
        /// </summary>
        public SavedSearches SavedSearches => new SavedSearches(this);
        /// <summary>
        /// Gets the wrapper of statuses.
        /// </summary>
        public Statuses Statuses => new Statuses(this);
        /// <summary>
        /// Gets the wrapper of trends.
        /// </summary>
        public Trends Trends => new Trends(this);
        /// <summary>
        /// Gets the wrapper of tweets.
        /// </summary>
        public Tweets Tweets => new Tweets(this);
        /// <summary>
        /// Gets the wrapper of users.
        /// </summary>
        public Users Users => new Users(this);
        /// <summary>
        /// Gets the wrapper of the Streaming API.
        /// </summary>
        public StreamingApi Streaming => new StreamingApi(this);
        #endregion

        /// <summary>
        /// Gets or sets the options of the connection.
        /// </summary>
        public ConnectionOptions ConnectionOptions { get; set; }

        internal const string JsonContentType = "application/json; charset=UTF-8";

        /// <summary>
        /// When overridden in a descendant class, creates a string for Authorization header.
        /// </summary>
        /// <param name="type">Type of HTTP request.</param>
        /// <param name="url">The URL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A string for Authorization header.</returns>
        public abstract string CreateAuthorizationHeader(MethodType type, Uri url, IEnumerable<KeyValuePair<string, object>> parameters);

        private static Uri CreateUri(MethodType type, string url, IEnumerable<KeyValuePair<string, object>> formattedParameters)
        {
            var ub = new UriBuilder(url);
            if (type != MethodType.Post)
            {
                var old = ub.Query;
                var s = Request.CreateQueryString(formattedParameters);
                ub.Query = !string.IsNullOrEmpty(old)
                    ? old.TrimStart('?') + "&" + s
                    : s;
            }
            // Windows.Web.Http.HttpClient reads Uri.OriginalString, so we have to re-construct an Uri instance.
            return new Uri(ub.Uri.AbsoluteUri);
        }

        private static bool ContainsBinaryData(KeyValuePair<string, object>[] parameters)
        {
            return Array.Exists(parameters, x =>
            {
                var v = x.Value;

                if (v is string) return false;

                return v is Stream || v is IEnumerable<byte> || v is ArraySegment<byte> || v is FileInfo;
            });
        }
    }
}
