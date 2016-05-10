﻿//
//  Copyright 2016  R. Stanley Hum <r.stanley.hum@gmail.com>
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using HandbookApp.Actions;
using HandbookApp.States;
using Newtonsoft.Json;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using ModernHttpClient;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HandbookApp.Services
{

    [JsonObject(MemberSerialization.OptIn)]
    public class ServerMessage
    {
        [JsonProperty]
        public int ID { get; set; }

        public DateTime Time { get; set; }

        [JsonProperty]
        public string Action { get; set; }

        [JsonProperty]
        public string ArticleID { get; set; }

        [JsonProperty]
        public string ArticleTitle { get; set; }

        [JsonProperty]
        public string ArticleContent { get; set; }

        [JsonProperty]
        public string BookpageID { get; set; }

        [JsonProperty]
        public string BookpageArticleID { get; set; }

        [JsonProperty]
        public string BookpageLinkTitle { get; set; }

        [JsonProperty]
        public List<string> BookpageLinkIDs { get; set; }

        [JsonProperty]
        public string BookID { get; set; }

        [JsonProperty]
        public string BookTitle { get; set; }

        [JsonProperty]
        public string BookStartingID { get; set; }

        [JsonProperty]
        public int BookOrder { get; set; }
    }

    public class NewMessage
    {
        public string ID { get; set; }
        public string Action { get; set; }
    }

    public static class JsonServerService
    {
        public static string BaseAddress = "http://192.168.72.70:50051/";
        //public static string BaseAddress = "https://stanleyhum.azurewebsites.net/";
        public static string UpdateMessagesApi = "messages/";
        public const string ApplicationHeaderJson = "application/json";
        public const int TimeoutDurationInMilliseconds = 19000; // TODO: Needs 19 seconds timeout to go to external website and download need to retry on first load

        public static void JsonServerUpdate()
        {
            var client = new HttpClient(new NativeMessageHandler());
            client.BaseAddress = new Uri(BaseAddress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationHeaderJson));
            client.GetAsync(UpdateMessagesApi).ToObservable()
                .Timeout(TimeSpan.FromMilliseconds(TimeoutDurationInMilliseconds))
                .Finally(() => { client.Dispose(); })
                .Subscribe(
                    res => { processResponseMessage(res); },
                    ex => { System.Diagnostics.Debug.WriteLine("Exception: {0}", ex.Message); /* TODO: Log exception */ },
                    () => { System.Diagnostics.Debug.WriteLine("Completed"); /* TODO: Log completed */ }
                );
        }


        private static void processResponseMessage(HttpResponseMessage response)
        {
            response.Content.ReadAsStringAsync().ToObservable()
                .Timeout(TimeSpan.FromMilliseconds(TimeoutDurationInMilliseconds))
                .Subscribe(
                    x => { processJsonString(x); },
                    ex => { /* TODO: log error */ },
                    () => { /* TODO: continue with program */ }
                );
        }


        private static void processJsonString(string responseJson)
        {
            List<ServerMessage> messages = JsonConvert.DeserializeObject<List<ServerMessage>>(responseJson);

            var addArticles = messages
                .Where(x => x.Action == "AddArticleAction")
                .Select(x => new Article() { Id = x.ArticleID, Title = x.ArticleTitle, Content = x.ArticleContent });
            var updateArticleAction = new AddArticleRangeAction { Articles = addArticles.ToList() };
            App.Store.Dispatch(updateArticleAction);

            var addPages = messages
                .Where(x => x.Action == "AddBookpageAction")
                .Select(x => new Bookpage() { Id = x.BookpageID, ArticleId = x.BookpageArticleID, LinksTitle = x.BookpageLinkTitle, Links = x.BookpageLinkIDs });
            var updateBookpageAction = new AddBookpageRangeAction { Bookpages = addPages.ToList() };
            App.Store.Dispatch(updateBookpageAction);

            var addBooks = messages
                .Where(x => x.Action == "AddBookAction")
                .Select(x => new Book() { Id = x.BookID, Title = x.BookTitle, StartingBookpage = x.BookStartingID, OrderIndex = x.BookOrder });
            var updateBookAction = new AddBookRangeAction { Books = addBooks.ToList() };
            App.Store.Dispatch(updateBookAction);
        }
    }
}