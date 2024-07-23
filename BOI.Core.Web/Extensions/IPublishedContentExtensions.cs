using BOI.Core.Extensions;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using System.Text;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;
using BOI.Umbraco.Models;

namespace BOI.Core.Web.Extensions
{
    public static class IPublishedContentExtensions
    {

        public static T As<T>(this IPublishedContent publishedContent)
            => (T)publishedContent;

        public static IPublishedContent GetSiteRoot(this IPublishedContent publishedContent)
            => publishedContent.AncestorOrSelf(SiteRoot.ModelTypeAlias);

        public static string GetImageAlternativeText(this IPublishedContent publishedContent, string defaultText = "")
            => publishedContent.IsComposedOf(Image.ModelTypeAlias) ? publishedContent.Value<string>("alternativeText") : defaultText;

      
        /// <summary>
        /// Generate all the html needed for a fully responsive, performant image
        /// </summary>
        /// 
        /// <param name="mediaItem">The IPublishedContent item.</param>
        /// <param name="classNames">CSS class names</param>
        /// <param name="fallbackAltText">Descriptive alt text. Don't leave this empty</param>
        /// <param name="cropMode">A specific crop mode as defined here: https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Processing.ResizeMode.html. The default is 'crop'. </param>
        /// <param name="lazyLoad">Lazy loads the image. Src & Srcset parameters will be prefixed with 'data-', Javascript will do the rest of the hard work.</param>
        /// <param name="blurUp">Adds classes to allow for lazy loaded images to be 'blurred' into view</param>
        /// /// <param name="cropSizes">
        /// An array of ("cropAlias", screenSize) pairs used to load different crops at or above specific screen sizes.
        /// Use 0 for your smallest crop size
        /// </param>
        public static HtmlString GetImageHtml(this IPublishedContent mediaItem, string classNames, string fallbackAltText, params (string cropAlias, int screenSize)[] cropDefs)
        {
            return mediaItem.GetImageHtml(classNames, fallbackAltText, ImageCropMode.Crop, false, false, cropDefs);
        }

        public static HtmlString GetImageHtml(this IPublishedContent mediaItem, string classNames, string fallbackAltText, ImageCropMode cropMode, params (string cropAlias, int screenSize)[] cropDefs)
        {
            return GetImageHtml(mediaItem, classNames, fallbackAltText, cropMode: cropMode, false, false, cropDefs);
        }

        public static HtmlString GetImageHtml(this IPublishedContent mediaItem, string classNames, string fallbackAltText, bool lazyLoad, params (string cropAlias, int screenSize)[] cropDefs)
        {
            return GetImageHtml(mediaItem, classNames, fallbackAltText, ImageCropMode.Crop, lazyLoad, false, cropDefs);
        }

        public static string Displayname(this IPublishedContent content, string displayTitleAlias = "displayTitle")
        {
            if (content == null)
            {
                return "";
            }
            return content.Value<string>(displayTitleAlias, fallback: Fallback.ToDefaultValue, defaultValue: content.Name);
        }

        public static HtmlString GetImageHtml(this IPublishedContent mediaItem, string classNames, string fallbackAltText, ImageCropMode cropMode = ImageCropMode.Crop, bool lazyLoad = false, bool blurUp = false, params (string cropAlias, int screenSize)[] cropDefs)
        {
            if (mediaItem != null)
            {
                if (cropDefs == null)
                {
                    throw new ArgumentException("Value of cropDefs cannot be null or empty.");
                }

                try
                {
                    // get the actual base umbracoFile data from IPublishedContent
                    var image = JsonConvert.DeserializeObject<ImageCropperValue>(mediaItem.Value<string>("umbracoFile").ToString());
                    var imageExtention = mediaItem.Value<string>("umbracoExtension");

                    // order the crop sizes largest first - makes things nice and easy later as the order of the crops in HTML matters
                    cropDefs = cropDefs.OrderByDescending(c => c.screenSize).ToArray();

                    if (image.Crops.NotNullAndAny() && cropDefs.NotNullAndAny())
                    {

                        // default crop, will be used later
                        var defaultCropAlias = cropDefs.FirstOrDefault().cropAlias;
                        var defaultCrop = image.Crops.Where(c => c.Alias == defaultCropAlias).FirstOrDefault();

                        if (defaultCrop != null)
                        {
                            // start building out HTML
                            var html = new StringBuilder();
                            html.AppendFormat("<picture>");

                            // create a <source> element for every crop-definition provided
                            foreach (var cropDef in cropDefs)
                            {
                                // get the umbraco crop size based on the alias provided in the definition
                                var crop = image.Crops.Where(c => c.Alias == cropDef.cropAlias).FirstOrDefault();
                                if (crop != null)
                                {
                                    //webp version first
                                    var webpOptions = "&format=webp&quality=80"; //quality set to 80 as leaving blank can sometimes cause webps to be larger than the original src:https://our.umbraco.com/forum/using-umbraco-and-getting-started/94579-how-to-use-webp-format-with-the-image-processor

                                    html.AppendFormat("<source ");
                                    if (cropDef.screenSize > 0)
                                    {
                                        html.AppendFormat("media=\"(min-width: {0}px)\" ", cropDef.screenSize); // set crop to be displayed only at the defined screen size
                                    }
                                    html.AppendFormat("{0}srcset=\"", lazyLoad ? "data-" : string.Empty);
                                    html.AppendFormat("{0} 1x,", mediaItem.GetCropUrl(width: crop.Width, height: crop.Height, cropAlias: crop.Alias, imageCropMode: cropMode, furtherOptions: webpOptions)); // create hdpi versions of the crop size for high definition screens
                                    html.AppendFormat("{0} 2x,", mediaItem.GetCropUrl(width: crop.Width * 2, height: crop.Height * 2, cropAlias: crop.Alias, imageCropMode: cropMode, furtherOptions: webpOptions));
                                    html.AppendFormat("{0} 3x\" ", mediaItem.GetCropUrl(width: crop.Width * 3, height: crop.Height * 3, cropAlias: crop.Alias, imageCropMode: cropMode, furtherOptions: webpOptions));
                                    html.AppendFormat("type=\"image/webp\" />");

                                    //fallback to jpeg if webp not supported
                                    html.AppendFormat("<source ");
                                    if (cropDef.screenSize > 0)
                                    {
                                        html.AppendFormat("media=\"(min-width: {0}px)\" ", cropDef.screenSize); // set crop to be displayed only at the defined screen size
                                    }
                                    html.AppendFormat("{0}srcset=\"", lazyLoad ? "data-" : string.Empty);
                                    html.AppendFormat("{0} 1x,", mediaItem.GetCropUrl(width: crop.Width, height: crop.Height, cropAlias: crop.Alias, imageCropMode: cropMode)); // create hdpi versions of the crop size for high definition screens
                                    html.AppendFormat("{0} 2x,", mediaItem.GetCropUrl(width: crop.Width * 2, height: crop.Height * 2, cropAlias: crop.Alias, imageCropMode: cropMode));
                                    html.AppendFormat("{0} 3x\" ", mediaItem.GetCropUrl(width: crop.Width * 3, height: crop.Height * 3, cropAlias: crop.Alias, imageCropMode: cropMode));
                                    html.AppendFormat("type=\"image/{0}\" />", imageExtention);


                                }
                                else
                                {
                                    //Current.Logger.Warn<IPublishedContent>(String.Format("Crop alias {0} not found", cropDef.cropAlias));
                                }
                            }

                            html.AppendFormat("<img ");

                            // fall back image
                            if (lazyLoad)
                            {
                                if (blurUp)
                                {
                                    // generate a tiny, blurry image for that initial load
                                    var initImageWidth = (defaultCrop.Width / 100) * 5; //take dimensions and reduce to 5%
                                    var initImageHeight = (defaultCrop.Height / 100) * 5;

                                    html.AppendFormat("src=\"{0}\" ", mediaItem.GetCropUrl(width: initImageWidth, height: initImageHeight, cropAlias: defaultCrop.Alias, imageCropMode: cropMode)); // This will load first and should be super quick to load on all devices/connections

                                    classNames = string.Concat("blur ", classNames); // add the blur class
                                }
                                else
                                {
                                    html.AppendFormat("src=\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {0} {1}'%3E%3C/svg%3E\" ", defaultCrop.Width, defaultCrop.Height); // else use a blank svg to reserve the space & stop CLS
                                }

                                html.AppendFormat("data-src=\"{0}\" ", mediaItem.GetCropUrl(cropAlias: defaultCrop.Alias, imageCropMode: cropMode)); // fallback image
                            }
                            else
                            {
                                html.AppendFormat("src=\"{0}\" ", mediaItem.GetCropUrl(cropAlias: defaultCrop.Alias, imageCropMode: cropMode)); // fallback image
                            }

                            html.AppendFormat("class=\"{0}\" ", classNames);
                            html.AppendFormat("alt=\"{0}\" />", mediaItem.HasValue("alternativeText") ? mediaItem.Value<string>("alternativeText") : fallbackAltText); // alt text is important so we should always fallback to something reasonable like the name of the current node
                            html.AppendFormat("</picture>");

                            return new HtmlString(html.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Current.Logger.Error<IPublishedContent>(ex, "There was an error generating responsive HTML markup for an image or crop size");

                    return new HtmlString(string.Empty);
                }
            }

            return new HtmlString(string.Empty);
        }
    }
}
