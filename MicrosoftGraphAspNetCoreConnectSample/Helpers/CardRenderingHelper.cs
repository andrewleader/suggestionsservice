using AdaptiveCards;
using AdaptiveCards.Rendering.Html;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuggestionsService.Helpers
{
    public static class CardRenderingHelper
    {
        public static string RenderToHtml(JObject cardObject)
        {
            try
            {
                AdaptiveCardRenderer renderer = new AdaptiveCardRenderer();
                renderer.HostConfig.Actions.ActionAlignment = AdaptiveHorizontalAlignment.Left;
                var card = AdaptiveCard.FromJson(cardObject.ToString()).Card;
                var renderedTag = renderer.RenderCard(card).Html;
                return renderedTag.ToString();
            }
            catch { return "<p>Error</p>"; }
        }
    }
}
