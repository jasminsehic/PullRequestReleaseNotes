﻿using System;
using System.Linq;
using System.Net;
using PullRequestReleaseNotes.Models;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace PullRequestReleaseNotes.Publishers
{
    public static class ConfluencePublisher
    {
        public static bool PublishMarkdownPage(string pageTitle, string markdownNotes, ProgramArgs programArgs)
        {
            var existingPage = FindConfluencePage(programArgs, pageTitle);
            if (existingPage == null)
                return PostConfluencePage(programArgs, pageTitle, markdownNotes);
            return UpdateConfluencePage(programArgs, existingPage, markdownNotes);
        }

        private static Content FindConfluencePage(ProgramArgs programArgs, string pageTitle)
        {
            var client = PrepareConfluenceClient(programArgs);
            var request = PrepareConfluenceRequest(new RestRequest("content", Method.Get));
            request.AddQueryParameter("type", "page");
            request.AddQueryParameter("spaceKey", programArgs.ConfluenceSpaceKey);            
            request.AddQueryParameter("title", pageTitle);
            request.AddQueryParameter("limit", "1");
            var response = client.Execute(request);
            ContentResults contentResult;
            try
            {
                contentResult = JsonConvert.DeserializeObject<ContentResults>(response.Content);
            }
            catch (JsonReaderException)
            {
                Console.WriteLine($"Error finding Confluence page. Response content:\n{response.Content}");
                throw;
            }
            if (response.StatusCode != HttpStatusCode.OK || contentResult == null || !contentResult.Results.Any() || contentResult.Results.First().Id == null)
                return null;
            request = PrepareConfluenceRequest(new RestRequest("content/{id}", Method.Get));
            request.AddUrlSegment("id", contentResult.Results.First().Id);
            var result = client.Execute<Content>(request);
            if (result.StatusCode == HttpStatusCode.OK)
                return result.Data;
            return null;
        }

        private static bool PostConfluencePage(ProgramArgs programArgs, string pageTitle, string markdown)
        {
            var page = new Content()
            {
                Type = "page",
                Title = pageTitle,
                Space = new Space { Key = programArgs.ConfluenceSpaceKey },
                Ancestors = new[] { new Ancestor() { Id = programArgs.ConfluenceReleaseParentPageId, Type = "page" } },
                Body = new Body { Storage = BuildMarkdownBodyContent(markdown) }
            };
            return PostConfluenceContent(programArgs, page).StatusCode == HttpStatusCode.OK;
        }

        private static bool UpdateConfluencePage(ProgramArgs programArgs, Content existingPage, string markdown)
        {
            var page = new Content()
            {
                Id = existingPage.Id,
                Version = new ContentVersion() { Number = existingPage.Version.Number + 1 },
                Type = "page",
                Title = existingPage.Title,
                Ancestors = new[] { new Ancestor() { Id = programArgs.ConfluenceReleaseParentPageId, Type = "page" } },
                Body = new Body { Storage = BuildMarkdownBodyContent(markdown) }
            };
            return UpdateConfluenceContent(programArgs, page).StatusCode == HttpStatusCode.OK;
        }

        private static BodyContent BuildMarkdownBodyContent(string markdown)
        {
            return new BodyContent
            {
                // depends on Markdown Render Confluence plugin being enabled
                Value = $@"<ac:structured-macro ac:name=""markdown""><ac:plain-text-body><![CDATA[
{markdown}
                        ]]></ac:plain-text-body></ac:structured-macro>",
                Representation = "storage"
            };
        }

        private static RestResponse<Content> PostConfluenceContent(ProgramArgs programArgs, Content content)
        {
            var client = PrepareConfluenceClient(programArgs);
            var request = PrepareConfluenceRequest(new RestRequest("content", Method.Post));
            AddJsonBodyToRequest(content, request);
            return client.Execute<Content>(request);
        }

        private static RestResponse<Content> UpdateConfluenceContent(ProgramArgs programArgs, Content content)
        {
            var client = PrepareConfluenceClient(programArgs);
            var request = PrepareConfluenceRequest(new RestRequest("content/{id}", Method.Put));
            request.AddUrlSegment("id", content.Id);
            AddJsonBodyToRequest(content, request);
            return client.Execute<Content>(request);
        }

        private static RestClient PrepareConfluenceClient(ProgramArgs programArgs)
        {
            var client = new RestClient(programArgs.ConfluenceApiUrl);
            client.Authenticator = new HttpBasicAuthenticator(programArgs.ConfluenceUser, programArgs.ConfluencePassword);
            return client;
        }

        private static RestRequest PrepareConfluenceRequest(RestRequest request)
        {
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-Atlassian-Token", "nocheck");
            return request;
        }
        private static void AddJsonBodyToRequest(Content page, RestRequest request)
        {
            var json = JsonConvert.SerializeObject(page);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
        }
    }
}