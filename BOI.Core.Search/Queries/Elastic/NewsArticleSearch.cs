using BOI.Core.Constants;
using BOI.Core.Search.Constants;
using BOI.Core.Search.Extensions;
using BOI.Core.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;

namespace BOI.Core.Search.Queries.Elastic
{
    public class NewsArticleSearch
    {
        public int Page { get; set; }

        public int Size { get; set; }
    }

    public class NewsArticleSearcher
    {
        private readonly IConfiguration configuration;

        private readonly IElasticClient esClient;
        private IConfigurationRoot configuration1;

        public NewsArticleSearcher(IConfiguration configuration, IElasticClient esClient)
        {
            this.configuration = configuration;
            this.esClient = esClient;
        }

        public NewsArticleSearcher(IConfigurationRoot configuration1)
        {
            this.configuration1 = configuration1;
        }

        public NewsArticlesResults Execute(NewsArticleSearch model)
        {
            if (model.Size == 0)
            {
                model.Size = 8;
            }

            if (model.Page < 1)
            {
                model.Page = 1;
            }

            var results = new NewsArticlesResults();

            var search = esClient
                .Search<WebContent>(s => s
                    .Index(configuration[ConfigurationConstants.WebcontentIndexAliasKey])
                    .TrackTotalHits()
                    .From((model.Page - 1) * model.Size)
                    .Size(model.Size)
                    .Query(q => q
                        .Bool(b => b
                            .Must(
                               a =>
                               {
                                   return a.Term(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Value(DocTypeConstants.NewsArticle));
                               }
                            )
                        )
                    )

                );

            try
            {
                search = search.EnsureSuccess();
            }
            catch (Exception ex)
            {
                //logger.LogError(ex.Message);
            }

            if (search == null)
            {
                return results;
            }

            var queryResults = search.Documents.Select(r => new NewsArticleResult()
            {
                ArticleName = r.Name,
                ArticleListingSummary = r.ListingSummary,
                ArticleUrl = r.Url
            }).ToList();

            results = new NewsArticlesResults
            {
                QueryResults = queryResults,
                Total = Convert.ToInt32(search.Total),
                Page = model.Page,
                Size = model.Size,
            };

            return results;
        }
    }
}
