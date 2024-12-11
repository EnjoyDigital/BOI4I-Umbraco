using BOI.Core.Constants;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace BOI.Core.Web.Extensions
{
    public static class ResponsiveImagesExtensions
    {
        /// <summary>
        /// Creates a Background css rule in a style block in the head.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="img">The Image</param>
        /// <param name="keysAndCrops">new {BreakPoint = "image Cropper alias", ...}</param>
        /// <param name="cssSelector">Css selector for the css to target</param>
        /// <param name="convertToPng8">Converts pngs to png8</param>
        /// <returns></returns>
        public static void ResponsiveBackground(this HttpContext context, IPublishedContent img, object keysAndCrops, string cssSelector, bool convertToPng8 = false)
        {
            var backgrounds = (Dictionary<string, string>)context.Items[HttpContextItems.BackgroundImages] ?? new Dictionary<string, string>();
            var cropAliasValues = HtmlHelper.AnonymousObjectToHtmlAttributes(keysAndCrops);
            if (cropAliasValues == null) throw new ArgumentException("Parsing object results in null", nameof(keysAndCrops));

            foreach (var crop in cropAliasValues)
            {
                var cssBlock = string.Concat(cssSelector, "{background-image:url('", convertToPng8 ? img.GetPng8CropUrl(crop.Value.ToString()) : img.GetCropUrl(crop.Value.ToString()), "');}");

                if (backgrounds.ContainsKey(crop.Key))
                {
                    var currentValue = backgrounds[crop.Key];
                    backgrounds[crop.Key] = string.Concat(currentValue, cssBlock);
                }
                else
                {
                    backgrounds.Add(crop.Key, cssBlock);
                }
            }

            context.Items[HttpContextItems.BackgroundImages] = backgrounds;
        }

        /// <summary>
        /// Creates a Background css rule in a style block in the head.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="img">The Image</param>
        /// <param name="widths">new {BreakPoint = width, ...}</param>
        /// <param name="cssSelector">Css selector for the css to target</param>
        /// <param name="convertToPng8">Converts pngs to png8</param>
        /// <returns></returns>
        public static IHtmlContent ResponsiveBackground(this IHtmlHelper helper, IPublishedContent img, Dictionary<string, int> widths, string cssSelector, bool convertToPng8 = false)
        {
            if (string.IsNullOrWhiteSpace(cssSelector) && !widths.Any())
            {
                throw new ArgumentException();
            }
            if (img == null) return new HtmlString(string.Empty);
            var backgrounds = new Dictionary<string, string>();

            foreach (var width in widths)
            {
                var cssBlock = string.Concat(cssSelector, "{background-image:url('", convertToPng8 ? img.GetPng8CropUrl(width.Value) : img.GetCropUrl(width.Value), "');}");
                if (backgrounds.ContainsKey(width.Key))
                {
                    var currentValue = backgrounds[width.Key];
                    backgrounds[width.Key] = string.Concat(currentValue, cssBlock);
                }
                else
                {
                    backgrounds.Add(width.Key, cssBlock);
                }
            }

            return new HtmlString(string.Empty);
        }
        /// <summary>
        /// Renders the backgroud css collection for the given breakpoint key
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="key">e.g Desktop</param>
        /// <returns></returns>
        public static IHtmlContent RenderBackgroundCss(this HttpContext context, string key)
        {
            var backgrounds = (Dictionary<string, string>)context.Items[HttpContextItems.BackgroundImages] ?? new Dictionary<string, string>();
            return backgrounds.ContainsKey(key) ? new HtmlString(backgrounds[key]) : new HtmlString(string.Empty);
        }

        /// <summary>
        /// Returns the Umbraco GetCropUrl method with format=png8 appended to the end to reduce PNG file size.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="cropSize"></param>
        /// <returns></returns>
        public static string GetPng8CropUrl(this IPublishedContent node, string cropSize = null)
        {
            if (node == null) return null;
            var croppedUrl = node.GetCropUrl(cropSize);
            var url = string.IsNullOrEmpty(croppedUrl) ? node.Url() : croppedUrl;
            return GetCropUrl(url, node);
        }

        internal static string GetCropUrl(string url, IPublishedContent image)
        {
            if (!image.Value<string>("umbracoExtension", string.Empty).InvariantEquals("png"))
            {
                return url;
            }
            var queryType = url.Contains('?') ? "&" : "?";
            return string.Concat(url, queryType, "format=png8");
        }

        /// <summary>
        /// Returns the Umbraco GetCropUrl method with format=png8 appended to the end to reduce PNG file size.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="cropWidth"></param>
        /// <returns>Url of media item with reduced file size if the file is a png</returns>
        public static string GetPng8CropUrl(this IPublishedContent node, int cropWidth = 0)
        {
            if (node == null) return null;
            var croppedUrl = node.GetCropUrl(cropWidth);
            var url = string.IsNullOrEmpty(croppedUrl) ? node.Url() : croppedUrl;
            return GetCropUrl(url, node);
        }
    }
}
