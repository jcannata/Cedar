﻿namespace Cedar.Testing
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cedar.Testing.Printing.Console;
    using FluentAssertions;
    using Microsoft.Owin;
    using Xunit;
    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task
        >, System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task
            >
        >;

    public class MiddlewareTests
    {
        [Fact]
        public async Task a_passing_middleware_test_should()
        {
            var result = await Scenario.ForMiddleware(Middleware)
                .When(() => new HttpRequestMessage(HttpMethod.Put, "/some-resource")
                {
                    Content = new StringContent("stuff")
                })
                .ThenShould(response => response.StatusCode == HttpStatusCode.Created)
                .When(response => new HttpRequestMessage(HttpMethod.Get, response.Headers.Location))
                .ThenShould(response => response.StatusCode == HttpStatusCode.OK);

            result.Print(new ConsolePrinter()).Wait();

            result.Passed.Should().BeTrue();
        }

        [Fact]
        public async Task a_failing_middleware_test_should()
        {
            var result = await Scenario.ForMiddleware(Middleware)
                .When(() => new HttpRequestMessage(HttpMethod.Put, "/some-resource")
                {
                    Content = new StringContent("stuff")
                })
                .ThenShould(response => response.StatusCode == HttpStatusCode.Accepted)
                .When(response => new HttpRequestMessage(HttpMethod.Get, response.Headers.Location))
                .ThenShould(response => response.StatusCode == HttpStatusCode.OK);

            result.Print(new ConsolePrinter()).Wait();

            result.Passed.Should().BeFalse();
        }

        [Fact]
        public async Task a_passing_middleware_test_with_timeouts_should()
        {
            var inABit = DateTimeOffset.UtcNow.Add(TimeSpan.FromMilliseconds(200));

            var result = await Scenario.ForMiddleware(Middleware)
                .When(() => new HttpRequestMessage(HttpMethod.Put, "/some-resource")
                {
                    Content = new StringContent("stuff")
                })
                .ThenShould(response => response.StatusCode == HttpStatusCode.Created)
                .When(response => new HttpRequestMessage(HttpMethod.Get, response.Headers.Location)
                {
                    Headers =
                    {
                        IfModifiedSince = inABit
                    }
                }, response => response.StatusCode != HttpStatusCode.NotModified)
                .ThenShould(response => response.StatusCode == HttpStatusCode.OK);

            result.Passed.Should().BeTrue();
        }

        [Fact]
        public async Task a_failing_middleware_test_with_timeouts_should()
        {
            var inABit = DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(5));

            var result = await Scenario.ForMiddleware(Middleware)
                .When(() => new HttpRequestMessage(HttpMethod.Put, "/some-resource")
                {
                    Content = new StringContent("stuff")
                })
                .ThenShould(response => response.StatusCode == HttpStatusCode.Created)
                .When(response => new HttpRequestMessage(HttpMethod.Get, response.Headers.Location)
                {
                    Headers =
                    {
                        IfModifiedSince = inABit
                    }
                }, response => response.StatusCode != HttpStatusCode.NotModified, TimeSpan.FromMilliseconds(10))
                .ThenShould(response => response.StatusCode == HttpStatusCode.OK);

            result.Passed.Should().BeFalse();
            result.Results.Should().BeOfType<ScenarioException>();
        }

        private static MidFunc Middleware
        {
            get
            {
                var resources = new ConcurrentDictionary<string, byte[]>();
                
                return next => async env =>
                {
                    var context = new OwinContext(env);

                    var id = context.Request.Path.Value;

                    byte[] resource;
                    
                    if(context.Request.Method == "PUT")
                    {
                        context.Response.StatusCode = 201;
                        context.Response.ReasonPhrase = "Created";
                        context.Response.Headers["Location"] = id;

                        using(var stream = new MemoryStream())
                        {
                            await context.Request.Body.CopyToAsync(stream);
                            resource = stream.ToArray();
                            resources.AddOrUpdate(id, resource, (_, previous) => resource);
                        }
                        return;
                    }

                    if(!resources.TryGetValue(id, out resource))
                    {
                        return;
                    }

                    if(context.Request.Method == "GET")
                    {
                        string[] dates;
                        DateTimeOffset date;

                        var now = DateTimeOffset.UtcNow;

                        if(context.Request.Headers.TryGetValue("If-Modified-Since", out dates)
                           && dates.Any()
                           && DateTimeOffset.TryParse(dates.First(), out date)
                           && date > now)
                        {
                            context.Response.StatusCode = 304;
                            context.Response.ReasonPhrase = "Not Modified";

                            return;
                        }

                        context.Response.StatusCode = 200;
                        context.Response.ReasonPhrase = "OK";

                        await context.Response.WriteAsync(resource);
                        return;
                    }

                    context.Response.StatusCode = 405;
                    context.Response.ReasonPhrase = "Method Not Allowed";
                };
            }
        }
    }
}