using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using SuggestionsService.Helpers;
using SuggestionsService.Model;

namespace SuggestionsService.Controllers.Api
{
    [Produces("application/json")]
    [Route("api/Suggestions")]
    public class SuggestionsController : Controller
    {
        // GET: api/Suggestions
        [HttpGet]
        public async Task<IEnumerable<Suggestion>> Get()
        {
            string accessToken = Request.Headers["X-AccessToken"];
            if (accessToken == null)
            {
                throw new ArgumentNullException("X-AccessToken");
            }

            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(
                async requestMessage =>
                {
                    // Append the access token to the request
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }));

            return await GetSuggestionsAsync(graphClient, HttpContext);
        }

        public static async Task<IEnumerable<Suggestion>> GetSuggestionsAsync(GraphServiceClient graphClient, HttpContext httpContext)
        {
            var events = await GraphService.GetCalendarEventsAsync(graphClient, httpContext);
            List<Suggestion> suggestions = new List<Suggestion>();
            foreach (var e in events)
            {
                suggestions.Add(CreateSuggestionFromEvent(e));
            }

            suggestions.AddRange(await GetPlannerSuggestionsAsync());

            // Add Alaska flight
            var alaskaFlightTime = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
            suggestions.Add(new Suggestion()
            {
                Time = new TimeInfo()
                {
                    Time = alaskaFlightTime
                },
                AppInfo = new SuggestionAppInfo()
                {
                    Name = "Alaska Airlines",
                    Icon = "https://lh3.googleusercontent.com/7KdxOhGsbkU7t_OJKqO_qGgvG8eH4W7H0CWG3ExpwJbdHk1B1YRmbzNXXMGMEZMxjaQ=s180"
                },
                AdaptiveCard = JObject.Parse(new AdaptiveCard()
                {
                    Version = "1.0",
                    Body =
                    {
                        new AdaptiveTextBlock()
                        {
                            Text = "Flight to Hawaii",
                            Size = AdaptiveTextSize.Medium,
                            Weight = AdaptiveTextWeight.Bolder
                        },

                        new AdaptiveTextBlock()
                        {
                            Text = alaskaFlightTime.ToString(),
                            Weight = AdaptiveTextWeight.Bolder
                        },

                        new AdaptiveColumnSet()
                        {
                            Separator = true,
                            Columns =
                            {
                                new AdaptiveColumn()
                                {
                                    Width = "1",
                                    Items =
                                    {
                                        new AdaptiveTextBlock()
                                        {
                                            Text = "Seattle",
                                            IsSubtle = true
                                        },
                                        new AdaptiveTextBlock()
                                        {
                                            Text = "SEA",
                                            Size = AdaptiveTextSize.ExtraLarge,
                                            Color = AdaptiveTextColor.Accent,
                                            Spacing = AdaptiveSpacing.None
                                        }
                                    }
                                },
                                new AdaptiveColumn()
                                {
                                    Width = "1",
                                    Items =
                                    {
                                        new AdaptiveTextBlock()
                                        {
                                            Text = "Hawaii",
                                            IsSubtle = true,
                                            HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                                        },
                                        new AdaptiveTextBlock()
                                        {
                                            Text = "HAI",
                                            Size = AdaptiveTextSize.ExtraLarge,
                                            Color = AdaptiveTextColor.Accent,
                                            Spacing = AdaptiveSpacing.None,
                                            HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Actions =
                    {
                        new AdaptiveOpenUrlAction()
                        {
                            Title = "Check in",
                            Url = new Uri("https://msn.com")
                        }
                    }
                }.ToJson())
            });

            suggestions.Sort();
            return suggestions;

            return new Suggestion[]
            {
                new Suggestion()
                {
                    Time = new TimeInfo()
                    {
                        Time = DateTime.UtcNow.AddHours(1)
                    },
                    AdaptiveCard = JObject.Parse("{ \"type\": \"AdaptiveCard\" }")
                }
            };
        }

        public static async Task<IEnumerable<Suggestion>> GetPlannerSuggestionsAsync()
        {
            return new Suggestion[]
            {
                CreatePlannerSuggestion("Northwind HR Training Video Part I", DateTime.UtcNow.Date.AddHours(15).AddDays(1)),

                CreatePlannerSuggestion("Office move planning", DateTime.UtcNow.Date.AddHours(16).AddDays(2))
            };
        }

        private static Suggestion CreatePlannerSuggestion(string title, DateTime dueDate)
        {
            return new Suggestion()
            {
                Time = new TimeInfo()
                {
                    Time = dueDate
                },
                AppInfo = new SuggestionAppInfo()
                {
                    Name = "Microsoft Planner",
                    Icon = "https://lh3.googleusercontent.com/ugG0T_9j488sN9eq4gI7rvHHwmL2CfT-YwOrm18lt7PgPxRT3ateHwpU0-8c_w3Fmht5=s180"
                },
                AdaptiveCard = JObject.Parse(new AdaptiveCard()
                {
                    Version = "1.0",
                    Body =
                        {
                            new AdaptiveTextBlock()
                            {
                                Text = title,
                                Weight = AdaptiveTextWeight.Bolder,
                                Wrap = true,
                                Size = AdaptiveTextSize.Medium
                            },

                            new AdaptiveTextBlock()
                            {
                                Text = "Due " + dueDate.ToString()
                            }
                        },
                    Actions =
                        {
                            new AdaptiveOpenUrlAction()
                            {
                                Title = "Mark complete",
                                Url = new Uri("https://msn.com")
                            }
                        }
                }.ToJson())
            };
        }

        private static Suggestion CreateSuggestionFromEvent(Event e)
        {
            var card = new AdaptiveCard()
            {
                Version = "1.0",
                Body =
                {
                    new AdaptiveTextBlock()
                    {
                        Text = e.Subject,
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Medium,
                        Wrap = true,
                        MaxLines = 2
                    }
                }
            };

            try
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = DateTime.Parse(e.Start.DateTime).ToShortTimeString(),
                    Weight = AdaptiveTextWeight.Bolder
                });
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(e.BodyPreview))
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = e.BodyPreview,
                    Wrap = true
                });
            }

            var suggestion = new Suggestion()
            {
                Time = new TimeInfo()
                {
                    Time = DateTime.Parse(e.Start.DateTime)
                },
                AppInfo = new SuggestionAppInfo()
                {
                    Name = "Outlook Calendar",
                    Icon = "https://d3rnbxvnd0hlox.cloudfront.net/images/channels/554351061/icons/on_color_large.png"
                },
                AdaptiveCard = JObject.Parse(card.ToJson())
            };

            return suggestion;
        }

        // GET: api/Suggestions/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/Suggestions
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/Suggestions/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
