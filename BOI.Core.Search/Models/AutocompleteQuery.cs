using BOI.Core.Search.Constants;
using BOI.Umbraco.Models;
using Microsoft.Extensions.Configuration;
using Nest;

namespace BOI.Core.Search.Models
{
    public class AutocompleteQuery : IAutocompleteQuery
    {
        private readonly IElasticClient esClient;
        private readonly IConfiguration configuration;

        public AutocompleteQuery(IElasticClient esClient, IConfiguration configuration)
        {
            this.esClient = esClient;
            this.configuration = configuration;
            //Size = 3;
        }

        //[BindAlias(BaseQueryAliases.QueryString)]
        public string QueryString { get; set; }
        public string CriteriaType { get; set; }

        /// <summary>
        /// Searches the specified query string.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AutocompleteSuggestionSubClass> Execute()
        {
            var response = Search();

            var searchResponse = response.Documents.Select(doc => new AutocompleteSuggestionSubClass { value = doc.Name, data = "false" })
                .DistinctBy(doc => doc.value.ToLower())
                .Take(4).ToList();

            if (searchResponse.Any())
            {
                searchResponse.Add(new AutocompleteSuggestionSubClass { value = "See more results...", data = "true" });
            }

            return searchResponse;
        }

        /// <summary>
        /// Searches this instance.
        /// </summary>
        /// <returns></returns>
        protected ISearchResponse<WebContent> Search()
        {
            QueryString = QueryString.ToLower();

            var searchQuery = new SearchDescriptor<WebContent>()
                .Index(configuration["WebContentEsIndexAlias"])
               .Query(query =>
               {
                   const int boost = 3;

                   var starredQueryString = string.Concat(QueryString, "*");
                   //Max size of NGrams is 15 so use slower text search if query string is more than 15 characters
                   var container = ((query.Bool(match => match.Must(qs => qs.Prefix(c => c.Name, QueryString, boost))) ||
                        query.Bool(match => match.Must(qs => qs.QueryString(c => c.Fields(f => f.Field(x => x.Content)).Query(starredQueryString)))))
                        && query.Bool(b => b.MustNot(m => m.Term(ma => ma.Field(f => f.SearchExclude).Value(true)))));

                   return container;
               })
               .Size(3);

            return esClient.Search<WebContent>(descriptor => searchQuery);
        }

        /// <summary>
        /// Searches the specified query string.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AutocompleteSuggestionSubClass> SearchCriteriaLookup()
        {
            var response = SearchCriteria();

            var searchResponse = response.Documents.Select(doc => new AutocompleteSuggestionSubClass { value = doc.CriteriaName, data = "false" })
                .DistinctBy(doc => doc.value.ToLower())
                .Take(5).ToList();

            return searchResponse;
        }

        /// <summary>
        /// Searches this instance.
        /// </summary>
        /// <returns></returns>
        public ISearchResponse<WebContent> SearchCriteria()
        {
            QueryString = QueryString.ToLower();

            var searchQuery = new SearchDescriptor<WebContent>()
                .Index(configuration["WebContentEsIndexAlias"])
               .Query(query =>
               {
                   const int boost = 3;
                   QueryContainer container = null;

                   if (CriteriaType.Equals(FieldConstants.BespokeProductType, StringComparison.InvariantCultureIgnoreCase))
                   {
                       container = (query.Bool(match => match.Must(qs => qs.MatchPhrasePrefix(c => c.Field(a => a.Name).Query(QueryString)))))
                                           && query.Bool(b => b.MustNot(m => m.Term(ma => ma.Field(f => f.SearchExclude).Value(true))))
                                           && query.Bool(b => b.Must(m => m.Terms(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Terms(Criteria.ModelTypeAlias))))
                                           && query.Bool(b => b.Must(m => m.Term(t => t.Field(f => f.BespokeProduct).Value(true))));
                   }
                   else if (CriteriaType.Equals(FieldConstants.BuyToLetProductType, StringComparison.InvariantCultureIgnoreCase))
                   {
                       container = (query.Bool(match => match.Must(qs => qs.MatchPhrasePrefix(c => c.Field(a => a.Name).Query(QueryString)))))
                                           && query.Bool(b => b.MustNot(m => m.Term(ma => ma.Field(f => f.SearchExclude).Value(true))))
                                           && query.Bool(b => b.Must(m => m.Terms(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Terms(Criteria.ModelTypeAlias))))
                                           && query.Bool(b => b.Must(m => m.Term(t => t.Field(f => f.BuyToLetProduct).Value(true))));
                   }
                   else
                   {
                       container = (query.Bool(match => match.Must(qs => qs.MatchPhrasePrefix(c => c.Field(a => a.Name).Query(QueryString)))))
                                           && query.Bool(b => b.MustNot(m => m.Term(ma => ma.Field(f => f.SearchExclude).Value(true))))
                                           && query.Bool(b => b.Must(m => m.Terms(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Terms(Criteria.ModelTypeAlias))))
                                           && query.Bool(b => b.Must(m => m.Term(t => t.Field(f => f.ResidentialProduct).Value(true))));
                   }

                   return container;
               })
               .Size(5);

            return esClient.Search<WebContent>(descriptor => searchQuery);
        }

        public IEnumerable<AutocompleteSuggestionSubClass> SearchFAQs()
        {
            var response = SearchFAQ();

            var searchResponse = response.Documents.Select(doc => new AutocompleteSuggestionSubClass { value = doc.FaqQuestion, data = "false" })
                .DistinctBy(doc => doc.value.ToLower())
                .Take(5).ToList();

            return searchResponse;
        }

        /// <summary>
        /// Searches this instance.
        /// </summary>
        /// <returns></returns>
        public ISearchResponse<WebContent> SearchFAQ()
        {
            QueryString = QueryString.ToLower();

            var searchQuery = new SearchDescriptor<WebContent>()
                .Index(configuration["WebContentEsIndexAlias"])
               .Query(query =>
               {
                   const int boost = 3;
                   QueryContainer container = null;

                   container = (query.Bool(match => match.Must(qs => qs.MatchPhrasePrefix(c => c.Field(a => a.Name).Query(QueryString)))))
                                       && query.Bool(b => b.MustNot(m => m.Term(ma => ma.Field(f => f.SearchExclude).Value(true))))
                                       && query.Bool(b => b.Must(m => m.Terms(t => t.Field(tf => tf.NodeTypeAlias.Suffix("keyword")).Terms(FAQ.ModelTypeAlias))));

                   return container;
               })
               .Size(5);

            return esClient.Search<WebContent>(descriptor => searchQuery);
        }
    }
}
