﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Elasticsearch.Net;
using Nest.DSL.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nest.Resolvers.Converters;
using System.Linq.Expressions;
using Nest.DSL.Descriptors;
using Nest.Resolvers;

namespace Nest
{
	/// <summary>
	/// A descriptor wich describes a search operation for _search and _msearch
	/// </summary>
	/// <remarks>Doesn't inherit from QueryPathDescriptorBase because it already needs an untyped supperclass 
	/// that has specifics that we can push to QueryPathDescriptorBase</remarks>
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	public partial class SearchDescriptor<T> :
		SearchDescriptorBase
		, IPathInfo<SearchRequestParameters>
		where T : class
	{
		internal override SearchTypeOptions? _SearchType
		{
			get { return this._QueryString.GetQueryStringValue<SearchTypeOptions?>("search_type");  }
		}
		internal override string _Preference
		{
			get { return this._QueryString.GetQueryStringValue<string>("preference"); }
		}

		internal override string _Routing
		{
			get
			{
				var routing = this._QueryString.GetQueryStringValue<string[]>("routing");
				return routing == null
					? null
					: string.Join(",", routing);
			}
		}

		internal override Type _ClrType { get { return typeof(T); } }

		/// <summary>
		/// Whether conditionless queries are allowed or not
		/// </summary>
		internal bool _Strict { get; set; }

		/// <summary>
		/// The indices to execute the search on. Defaults to the default index
		/// </summary>
		public SearchDescriptor<T> Indices(IEnumerable<string> indices)
		{
			if (indices == null) return this;
			return this.Indices(indices.ToArray());
		}
		/// <summary>
		/// The indices to execute the search on. Defaults to the default index
		/// </summary>
		public SearchDescriptor<T> Indices(IEnumerable<Type> indices)
		{
			if (indices == null) return this;
			return this.Indices(indices.ToArray());
		}
		/// <summary>
		/// The indices to execute the search on. Defaults to the default index
		/// </summary>
		public SearchDescriptor<T> Indices(params string[] indices)
		{
			if (indices == null) return this;
			this._Indices = indices.Select(s => (IndexNameMarker)s);
			return this;
		}
		/// <summary>
		/// The indices to execute the search on. Defaults to the default index
		/// </summary>
		public SearchDescriptor<T> Indices(params Type[] indices)
		{
			if (indices == null) return this;
			this._Indices = indices.Select(s => (IndexNameMarker)s);
			return this;
		}
		/// <summary>
		/// The index to execute the search on. Defaults to the default index
		/// </summary>
		public SearchDescriptor<T> Index(string index)
		{
			return this.Indices(index);
		}

		internal SearchDescriptor<T> Index(IndexNameMarker index)
		{
			if (index == null) return this;
			this._Indices = new[] { index };
			return this;
		}
		/// <summary>
		/// The index to execute the search on, using the default index for typeof TAlternative. Defaults to the default index
		/// </summary>
		public SearchDescriptor<T> Index<TAlternative>() where TAlternative : class
		{
			return this.Indices(typeof(Type));
		}
		/// <summary>
		/// The index to execute the search on using the inferred default for 'type'. Defaults to the default index
		/// </summary>
		public SearchDescriptor<T> Index(Type type)
		{
			return this.Indices(type);
		}
		/// <summary>
		/// The types to execute the search on. Defaults to the inferred typename of T 
		/// unless T is dynamic then a type (or AllTypes()) MUST be specified.
		/// </summary>
		public SearchDescriptor<T> Types(IEnumerable<string> types)
		{
			if (types == null) return this;
			this._Types = types.Select(s => (TypeNameMarker)s);
			return this;
		}
		/// <summary>
		/// The types to execute the search on. Defaults to the inferred typename of T 
		/// unless T is dynamic then a type (or AllTypes()) MUST be specified.
		/// </summary>
		public SearchDescriptor<T> Types(params string[] types)
		{
			return this.Types((IEnumerable<string>)types);
		}
		/// <summary>
		/// The types to execute the search on. Defaults to the inferred typename of T 
		/// unless T is dynamic then a type (or AllTypes()) MUST be specified.
		/// </summary>
		public SearchDescriptor<T> Types(IEnumerable<Type> types)
		{
			if (types == null) return this;
			this._Types = types.Select(s => (TypeNameMarker)s);
			return this;

		}
		/// <summary>
		/// The types to execute the search on. Defaults to the inferred typename of T 
		/// unless T is dynamic then a type (or AllTypes()) MUST be specified.
		/// </summary>
		public SearchDescriptor<T> Types(params Type[] types)
		{
			return this.Types((IEnumerable<Type>)types);
		}
		/// <summary>
		/// The type to execute the search on. Defaults to the inferred typename of T 
		/// unless T is dynamic then a type (or AllTypes()) MUST be specified.
		/// </summary>
		public SearchDescriptor<T> Type(string type)
		{
			if (type == null) return this;
			this._Types = new[] { (TypeNameMarker)type };
			return this;
		}
		/// <summary>
		/// The type to execute the search on. Defaults to the inferred typename of T 
		/// unless T is dynamic then a type (or AllTypes()) MUST be specified.
		/// </summary>
		public SearchDescriptor<T> Type(Type type)
		{
			if (type == null) return this;
			this._Types = new[] { (TypeNameMarker)type };
			return this;
		}

		internal SearchDescriptor<T> Type(TypeNameMarker type)
		{
			if (type == null) return this;
			this._Types = new[] { type };
			return this;
		}
		/// <summary>
		/// Execute search over all indices
		/// </summary>
		public SearchDescriptor<T> AllIndices()
		{
			this._AllIndices = true;
			return this;
		}
		/// <summary>
		/// Execute search over all types
		/// </summary>
		public SearchDescriptor<T> AllTypes()
		{
			this._AllTypes = true;
			return this;
		}

		/// <summary>
		/// When strict is set, conditionless queries are treated as an exception. 
		public SearchDescriptor<T> Strict(bool strict = true)
		{
			this._Strict = strict;
			return this;
		}

		[JsonProperty(PropertyName = "timeout")]
		internal string _Timeout { get; set; }
		[JsonProperty(PropertyName = "from")]
		internal int? _From { get; set; }
		[JsonProperty(PropertyName = "size")]
		internal int? _Size { get; set; }
		[JsonProperty(PropertyName = "explain")]
		internal bool? _Explain { get; set; }
		[JsonProperty(PropertyName = "version")]
		internal bool? _Version { get; set; }
		[JsonProperty(PropertyName = "track_scores")]
		internal bool? _TrackScores { get; set; }

		[JsonProperty(PropertyName = "min_score")]
		internal double? _MinScore { get; set; }

		[JsonProperty(PropertyName = "indices_boost")]
		[JsonConverter(typeof(DictionaryKeysAreNotPropertyNamesJsonConverter))]
		internal IDictionary<string, double> _IndicesBoost { get; set; }

		[JsonProperty(PropertyName = "sort")]
		[JsonConverter(typeof(DictionaryKeysAreNotPropertyNamesJsonConverter))]
		internal IDictionary<PropertyPathMarker, object> _Sort { get; set; }

		[JsonProperty(PropertyName = "facets")]
		[JsonConverter(typeof(DictionaryKeysAreNotPropertyNamesJsonConverter))]
		internal IDictionary<PropertyPathMarker, FacetDescriptorsBucket<T>> _Facets { get; set; }

		[JsonProperty(PropertyName = "suggest")]
		internal IDictionary<string, object> _Suggest { get; set; }

		[JsonProperty(PropertyName = "query")]
		internal RawOrQueryDescriptor<T> _QueryOrRaw
		{
			get
			{
				if (this._RawQuery == null && this._Query == null)
					return null;
				return new RawOrQueryDescriptor<T>
				{
					Raw = this._RawQuery,
					Descriptor = this._Query
				};
			}
		}

		[JsonProperty(PropertyName = "filter")]
		internal RawOrFilterDescriptor<T> _FilterOrRaw
		{
			get
			{
				if (this._RawFilter == null && this._Filter == null)
					return null;
				return new RawOrFilterDescriptor<T>
				{
					Raw = this._RawFilter,
					Descriptor = this._Filter
				};
			}
		}

		[JsonProperty(PropertyName = "highlight")]
		internal HighlightDescriptor<T> _Highlight { get; set; }

		[JsonProperty(PropertyName = "rescore")]
		internal RescoreDescriptor<T> _Rescore { get; set; }

		internal string _RawQuery { get; set; }
		internal BaseQuery _Query { get; set; }

		internal string _RawFilter { get; set; }
		internal BaseFilter _Filter { get; set; }

		[JsonProperty(PropertyName = "fields")]
		internal IList<PropertyPathMarker> _Fields { get; set; }

		[JsonProperty(PropertyName = "script_fields")]
		[JsonConverter(typeof(DictionaryKeysAreNotPropertyNamesJsonConverter))]
		internal FluentDictionary<string, ScriptFilterDescriptor> _ScriptFields { get; set; }

		[JsonProperty(PropertyName = "_source")]
		internal object _Source { get; set; }


		[JsonProperty(PropertyName = "aggs")]
		[JsonConverter(typeof(DictionaryKeysAreNotPropertyNamesJsonConverter))]
		internal IDictionary<string, AggregationDescriptor<T>> _Aggregations { get; set; }

		public SearchDescriptor<T> Aggregations(Func<AggregationDescriptor<T>, AggregationDescriptor<T>> aggregationsSelector)
		{
			var aggs = aggregationsSelector(new AggregationDescriptor<T>());
			if (aggs == null) return this;
			this._Aggregations = aggs._Aggregations;
			return this;
		}


		public SearchDescriptor<T> Source(bool include = true)
		{
			this._Source = include;
			return this;
		}

		public SearchDescriptor<T> Source(Func<SearchSourceDescriptor<T>, SearchSourceDescriptor<T>> sourceSelector)
		{
			this._Source = sourceSelector(new SearchSourceDescriptor<T>());
			return this;
		}
		/// <summary>
		/// The number of hits to return. Defaults to 10. When using scroll search type 
		/// size is actually multiplied by the number of shards!
		/// </summary>
		public SearchDescriptor<T> Size(int size)
		{
			this._Size = size;
			return this;
		}
		/// <summary>
		/// The number of hits to return. Defaults to 10.
		/// </summary>
		public SearchDescriptor<T> Take(int take)
		{
			return this.Size(take);
		}
		/// <summary>
		/// The starting from index of the hits to return. Defaults to 0.
		/// </summary>
		public SearchDescriptor<T> From(int from)
		{
			this._From = from;
			return this;
		}
		/// <summary>
		/// The starting from index of the hits to return. Defaults to 0.
		/// </summary>
		public SearchDescriptor<T> Skip(int skip)
		{
			return this.From(skip);
		}
		/// <summary>
		/// A search timeout, bounding the search request to be executed within the 
		/// specified time value and bail with the hits accumulated up
		/// to that point when expired. Defaults to no timeout.
		/// </summary>
		public SearchDescriptor<T> Timeout(string timeout)
		{
			this._Timeout = timeout;
			return this;
		}
		/// <summary>
		/// Enables explanation for each hit on how its score was computed. 
		/// (Use .DocumentsWithMetaData on the return results)
		/// </summary>
		public SearchDescriptor<T> Explain(bool explain = true)
		{
			this._Explain = explain;
			return this;
		}
		/// <summary>
		/// Returns a version for each search hit. (Use .DocumentsWithMetaData on the return results)
		/// </summary>
		public SearchDescriptor<T> Version(bool version = true)
		{
			this._Version = version;
			return this;
		}
		/// <summary>
		/// Make sure we keep calculating score even if we are sorting on a field.
		/// </summary>
		public SearchDescriptor<T> TrackScores(bool trackscores = true)
		{
			this._TrackScores = trackscores;
			return this;
		}
		/// <summary>
		/// Allows to filter out documents based on a minimum score:
		/// </summary>
		public SearchDescriptor<T> MinScore(double minScore)
		{
			this._MinScore = minScore;
			return this;
		}

		/// <summary>
		/// <para>
		/// Controls a preference of which shard replicas to execute the search request on. 
		/// By default, the operation is randomized between the each shard replicas.
		/// </para>
		/// <para>
		/// The operation will go and be executed only on the primary shards.
		/// </para>
		/// </summary>
		public SearchDescriptor<T> ExecuteOnPrimary()
		{
			return this.Preference("_primary");
		}
		/// <summary>
		/// <para>
		/// Controls a preference of which shard replicas to execute the search request on. 
		/// By default, the operation is randomized between the each shard replicas.
		/// </para>
		/// <para>
		/// The operation will go and be executed on the primary shard, and if not available (failover), 
		/// will execute on other shards.
		/// </para>
		/// </summary>
		public SearchDescriptor<T> ExecuteOnPrimaryFirst()
		{
			return this.Preference("_primary_first");
		}
		/// <summary>
		/// <para>
		/// Controls a preference of which shard replicas to execute the search request on. 
		/// By default, the operation is randomized between the each shard replicas.
		/// </para>
		/// <para>
		/// The operation will prefer to be executed on a local allocated shard is possible.
		/// </para>
		/// </summary>
		public SearchDescriptor<T> ExecuteOnLocalShard()
		{
			return this.Preference("_local");
		}
		/// <summary>
		/// <para>
		/// Controls a preference of which shard replicas to execute the search request on. 
		/// By default, the operation is randomized between the each shard replicas.
		/// </para>
		/// <para>
		/// Restricts the search to execute only on a node with the provided node id
		/// </para>
		/// </summary>
		public SearchDescriptor<T> ExecuteOnNode(string node)
		{
			node.ThrowIfNull("node");
			return this.Preference("_only_node:" + node);
		}
		/// <summary>
		/// <para>
		/// Controls a preference of which shard replicas to execute the search request on. 
		/// By default, the operation is randomized between the each shard replicas.
		/// </para>
		/// <para>
		/// Prefers execution on the node with the provided node id if applicable.
		/// </para>
		/// </summary>
		public SearchDescriptor<T> ExecuteOnPreferredNode(string node)
		{
			node.ThrowIfNull("node");
			this.Preference(string.Format("_prefer_node:{0}", node));
			return this;
		}
		/// <summary>
		/// Allows to configure different boost level per index when searching across 
		/// more than one indices. This is very handy when hits coming from one index
		/// matter more than hits coming from another index (think social graph where each user has an index).
		/// </summary>
		public SearchDescriptor<T> IndicesBoost(
			Func<FluentDictionary<string, double>, FluentDictionary<string, double>> boost)
		{
			boost.ThrowIfNull("boost");
			this._IndicesBoost = boost(new FluentDictionary<string, double>());
			return this;
		}
		/// <summary>
		/// Allows to selectively load specific fields for each document 
		/// represented by a search hit. Defaults to load the internal _source field.
		/// </summary>
		public SearchDescriptor<T> Fields(params Expression<Func<T, object>>[] expressions)
		{
			this._Fields = expressions.Select(e => (PropertyPathMarker)e).ToList();
			return this;
		}
		
		/// <summary>
		/// Allows to selectively load specific fields for each document 
		/// represented by a search hit. Defaults to load the internal _source field.
		/// </summary>
		public SearchDescriptor<T> Fields(Func<FluentFieldList<T>, FluentFieldList<T>> properties)
		{
			this._Fields = properties(new FluentFieldList<T>()).ToList();
			return this;
		}
		/// <summary>
		/// Allows to selectively load specific fields for each document 
		/// represented by a search hit. Defaults to load the internal _source field.
		/// </summary>
		public SearchDescriptor<T> Fields(params string[] fields)
		{
			this._Fields = fields.Select(f => (PropertyPathMarker)f).ToList();
			return this;
		}

		public SearchDescriptor<T> ScriptFields(
				Func<FluentDictionary<string, Func<ScriptFilterDescriptor, ScriptFilterDescriptor>>,
				 FluentDictionary<string, Func<ScriptFilterDescriptor, ScriptFilterDescriptor>>> scriptFields)
		{
			scriptFields.ThrowIfNull("scriptFields");
			var scriptFieldDescriptors = scriptFields(new FluentDictionary<string, Func<ScriptFilterDescriptor, ScriptFilterDescriptor>>());
			if (scriptFieldDescriptors == null || scriptFieldDescriptors.All(d => d.Value == null))
			{
				this._ScriptFields = null;
				return this;
			}
			this._ScriptFields = new FluentDictionary<string, ScriptFilterDescriptor>();
			foreach (var d in scriptFieldDescriptors)
			{
				if (d.Value == null)
					continue;
				this._ScriptFields.Add(d.Key, d.Value(new ScriptFilterDescriptor()));
			}
			return this;
		}


		/// <summary>
		/// <para>Allows to add one or more sort on specific fields. Each sort can be reversed as well.
		/// The sort is defined on a per field level, with special field name for _score to sort by score.
		/// </para>
		/// <para>
		/// Sort ascending.
		/// </para>
		/// </summary>
		public SearchDescriptor<T> SortAscending(Expression<Func<T, object>> objectPath)
		{
			if (this._Sort == null)
				this._Sort = new Dictionary<PropertyPathMarker, object>();

			this._Sort.Add(objectPath, "asc");
			return this;
		}
		/// <summary>
		/// <para>Allows to add one or more sort on specific fields. Each sort can be reversed as well.
		/// The sort is defined on a per field level, with special field name for _score to sort by score.
		/// </para>
		/// <para>
		/// Sort descending.
		/// </para>
		/// </summary>
		public SearchDescriptor<T> SortDescending(Expression<Func<T, object>> objectPath)
		{
			if (this._Sort == null)
				this._Sort = new Dictionary<PropertyPathMarker, object>();

			this._Sort.Add(objectPath, "desc");
			return this;
		}
		/// <summary>
		/// <para>Allows to add one or more sort on specific fields. Each sort can be reversed as well.
		/// The sort is defined on a per field level, with special field name for _score to sort by score.
		/// </para>
		/// <para>
		/// Sort ascending.
		/// </para>
		/// </summary>
		public SearchDescriptor<T> SortAscending(string field)
		{
			if (this._Sort == null)
				this._Sort = new Dictionary<PropertyPathMarker, object>();
			this._Sort.Add(field, "asc");
			return this;
		}
		/// <summary>
		/// <para>Allows to add one or more sort on specific fields. Each sort can be reversed as well.
		/// The sort is defined on a per field level, with special field name for _score to sort by score.
		/// </para>
		/// <para>
		/// Sort descending.
		/// </para>
		/// </summary>
		public SearchDescriptor<T> SortDescending(string field)
		{
			if (this._Sort == null)
				this._Sort = new Dictionary<PropertyPathMarker, object>();

			this._Sort.Add(field, "desc");
			return this;
		}
		/// <summary>
		/// <para>Sort() allows you to fully describe your sort unlike the SortAscending and SortDescending aliases.
		/// </para>
		/// </summary>
		public SearchDescriptor<T> Sort(Func<SortDescriptor<T>, SortDescriptor<T>> sortSelector)
		{
			if (this._Sort == null)
				this._Sort = new Dictionary<PropertyPathMarker, object>();

			sortSelector.ThrowIfNull("sortSelector");
			var descriptor = new SortDescriptor<T>();
			sortSelector(descriptor);
			this._Sort.Add(descriptor._Field, descriptor);
			return this;
		}
		/// <summary>
		/// <para>SortGeoDistance() allows you to sort by a distance from a geo point.
		/// </para>
		/// </summary>
		public SearchDescriptor<T> SortGeoDistance(Func<SortGeoDistanceDescriptor<T>, SortGeoDistanceDescriptor<T>> sortSelector)
		{
			if (this._Sort == null)
				this._Sort = new Dictionary<PropertyPathMarker, object>();

			sortSelector.ThrowIfNull("sortSelector");
			var descriptor = new SortGeoDistanceDescriptor<T>();
			sortSelector(descriptor);
			this._Sort.Add("_geo_distance", descriptor);
			return this;
		}
		/// <summary>
		/// <para>SortScript() allows you to sort by a distance from a geo point.
		/// </para>
		/// </summary>
		public SearchDescriptor<T> SortScript(Func<SortScriptDescriptor<T>, SortScriptDescriptor<T>> sortSelector)
		{
			if (this._Sort == null)
				this._Sort = new Dictionary<PropertyPathMarker, object>();

			sortSelector.ThrowIfNull("sortSelector");
			var descriptor = new SortScriptDescriptor<T>();
			sortSelector(descriptor);
			this._Sort.Add("_script", descriptor);
			return this;
		}

		private SearchDescriptor<T> _Facet<F>(
			string name,
			Func<F, F> facet,
			Func<F, PropertyPathMarker> inferedFieldNameSelector,
			Action<FacetDescriptorsBucket<T>, F> fillBucket
			)
			where F : IFacetDescriptor, new()
		{
			facet.ThrowIfNull("facet");
			inferedFieldNameSelector.ThrowIfNull("inferedFieldNameSelector");
			fillBucket.ThrowIfNull("fillBucket");

			if (this._Facets == null)
				this._Facets = new Dictionary<PropertyPathMarker, FacetDescriptorsBucket<T>>();

			var descriptor = new F();
			var f = facet(descriptor);
			var key = string.IsNullOrWhiteSpace(name) ? inferedFieldNameSelector(descriptor) : name;
			if (key.IsConditionless())
			{
				throw new DslException(
					"Couldn't infer name for facet of type {0}".F(typeof(F).Name)
				);
			}
			var bucket = new FacetDescriptorsBucket<T>();
			bucket.Global = f.IsGlobal;
			bucket.FacetFilter = f.FacetFilter;
			bucket.Nested = f.Nested;
			bucket.Scope = f.Scope;
			fillBucket(bucket, descriptor);
			this._Facets.Add(key, bucket);

			return this;
		}


		/// <summary>
		/// Allow to specify field facets that return the N most frequent terms.
		/// </summary>
		public SearchDescriptor<T> FacetTerm(string name, Func<TermFacetDescriptor<T>, TermFacetDescriptor<T>> facet)
		{
			return this.FacetTerm(facet, Name: name);
		}

		/// <summary>
		/// Allow to specify field facets that return the N most frequent terms.
		/// </summary>
		public SearchDescriptor<T> FacetTerm(Func<TermFacetDescriptor<T>, TermFacetDescriptor<T>> facet, string Name = null)
		{
			return this._Facet<TermFacetDescriptor<T>>(
				Name,
				facet,
				(d) => d._Field,
				(b, d) => b.Terms = d
			);
		}

		/// <summary>
		/// range facet allow to specify a set of ranges and get both the number of docs (count) 
		/// that fall within each range, and aggregated data either based on the field, or using another field
		/// </summary>
		/// <typeparam name="K">struct, (int, double, string, DateTime)</typeparam>
		public SearchDescriptor<T> FacetRange<K>(string name, Func<RangeFacetDescriptor<T, K>, RangeFacetDescriptor<T, K>> facet) where K : struct
		{
			return this.FacetRange<K>(facet, Name: name);
		}
		/// <summary>
		/// range facet allow to specify a set of ranges and get both the number of docs (count) 
		/// that fall within each range, and aggregated data either based on the field, or using another field
		/// </summary>
		/// <typeparam name="K">struct, (int, double, string, DateTime)</typeparam>
		public SearchDescriptor<T> FacetRange<K>(Func<RangeFacetDescriptor<T, K>, RangeFacetDescriptor<T, K>> facet, string Name = null) where K : struct
		{
			return this._Facet<RangeFacetDescriptor<T, K>>(
				Name,
				facet,
				(d) => d._Field,
				(b, d) => b.Range = d
			);
		}
		/// <summary>
		/// The histogram facet works with numeric data by building a histogram across intervals 
		/// of the field values. Each value is “rounded” into an interval (or placed in a bucket), 
		/// and statistics are provided per interval/bucket (count and total). 
		/// </summary>
		public SearchDescriptor<T> FacetHistogram(string name, Func<HistogramFacetDescriptor<T>, HistogramFacetDescriptor<T>> facet)
		{
			return this.FacetHistogram(facet, Name: name);
		}
		/// <summary>
		/// The histogram facet works with numeric data by building a histogram across intervals 
		/// of the field values. Each value is “rounded” into an interval (or placed in a bucket), 
		/// and statistics are provided per interval/bucket (count and total). 
		/// </summary>
		public SearchDescriptor<T> FacetHistogram(Func<HistogramFacetDescriptor<T>, HistogramFacetDescriptor<T>> facet, string Name = null)
		{
			return this._Facet<HistogramFacetDescriptor<T>>(
				Name,
				facet,
				(d) => d._Field,
				(b, d) => b.Histogram = d
			);
		}
		/// <summary>
		/// A specific histogram facet that can work with date field types enhancing it over the regular histogram facet.
		/// </summary>
		public SearchDescriptor<T> FacetDateHistogram(string name, Func<DateHistogramFacetDescriptor<T>, DateHistogramFacetDescriptor<T>> facet)
		{
			return this.FacetDateHistogram(facet, Name: name);
		}
		/// <summary>
		/// A specific histogram facet that can work with date field types enhancing it over the regular histogram facet.
		/// </summary>
		public SearchDescriptor<T> FacetDateHistogram(Func<DateHistogramFacetDescriptor<T>, DateHistogramFacetDescriptor<T>> facet, string Name = null)
		{
			return this._Facet<DateHistogramFacetDescriptor<T>>(
				Name,
				facet,
				(d) => d._Field,
				(b, d) => b.DateHistogram = d
			);
		}

		/// <summary>
		/// Statistical facet allows to compute statistical data on a numeric fields. 
		/// The statistical data include count, total, sum of squares, 
		/// mean (average), minimum, maximum, variance, and standard deviation. 
		/// </summary>
		public SearchDescriptor<T> FacetStatistical(string name, Func<StatisticalFacetDescriptor<T>, StatisticalFacetDescriptor<T>> facet)
		{
			return this.FacetStatistical(facet, Name: name);
		}

		/// <summary>
		/// Statistical facet allows to compute statistical data on a numeric fields. 
		/// The statistical data include count, total, sum of squares, 
		/// mean (average), minimum, maximum, variance, and standard deviation. 
		/// </summary>
		public SearchDescriptor<T> FacetStatistical(Func<StatisticalFacetDescriptor<T>, StatisticalFacetDescriptor<T>> facet, string Name = null)
		{
			return this._Facet<StatisticalFacetDescriptor<T>>(
				Name,
				facet,
				(d) => d._Field,
				(b, d) => b.Statistical = d
			);
		}

		/// <summary>
		/// The terms_stats facet combines both the terms and statistical allowing 
		/// to compute stats computed on a field, per term value driven by another field.
		/// </summary>
		public SearchDescriptor<T> FacetTermsStats(string name, Func<TermsStatsFacetDescriptor<T>, TermsStatsFacetDescriptor<T>> facet)
		{
			return this.FacetTermsStats(facet, Name: name);
		}

		/// <summary>
		/// The terms_stats facet combines both the terms and statistical allowing 
		/// to compute stats computed on a field, per term value driven by another field.
		/// </summary>
		public SearchDescriptor<T> FacetTermsStats(Func<TermsStatsFacetDescriptor<T>, TermsStatsFacetDescriptor<T>> facet, string Name = null)
		{
			return this._Facet<TermsStatsFacetDescriptor<T>>(
				Name,
				facet,
				(d) => d._KeyField,
				(b, d) => b.TermsStats = d
			);
		}
		/// <summary>
		/// The geo_distance facet is a facet providing information for ranges of distances
		/// from a provided geo_point including count of the number of hits that fall 
		/// within each range, and aggregation information (like total).
		/// </summary>
		public SearchDescriptor<T> FacetGeoDistance(string name, Func<GeoDistanceFacetDescriptor<T>, GeoDistanceFacetDescriptor<T>> facet)
		{
			return this.FacetGeoDistance(facet, Name: name);
		}

		/// <summary>
		/// The geo_distance facet is a facet providing information for ranges of distances
		/// from a provided geo_point including count of the number of hits that fall 
		/// within each range, and aggregation information (like total).
		/// </summary>
		public SearchDescriptor<T> FacetGeoDistance(Func<GeoDistanceFacetDescriptor<T>, GeoDistanceFacetDescriptor<T>> facet, string Name = null)
		{
			return this._Facet<GeoDistanceFacetDescriptor<T>>(
					Name,
					facet,
					(d) => d._ValueField ?? d._Field,
					(b, d) => b.GeoDistance = d
				);
		}

		/// <summary>
		/// A facet query allows to return a count of the hits matching 
		/// the facet query. The query itself can be expressed using the Query DSL.
		/// </summary>
		public SearchDescriptor<T> FacetQuery(string name, Func<QueryDescriptor<T>, BaseQuery> querySelector, bool? Global = null)
		{
			name.ThrowIfNullOrEmpty("name");
			querySelector.ThrowIfNull("query");
			if (this._Facets == null)
				this._Facets = new Dictionary<PropertyPathMarker, FacetDescriptorsBucket<T>>();

			var query = new QueryDescriptor<T>();
			var q = querySelector(query);
			this._Facets.Add(name, new FacetDescriptorsBucket<T> { Query = q });

			return this;
		}
		/// <summary>
		/// A filter facet (not to be confused with a facet filter) allows you to return a count of the h
		/// its matching the filter. The filter itself can be expressed using the Query DSL.
		/// Note, filter facet filters are faster than query facet when using native filters (non query wrapper ones).
		/// </summary>
		public SearchDescriptor<T> FacetFilter(string name, Func<FilterDescriptor<T>, BaseFilter> filterSelector)
		{
			name.ThrowIfNullOrEmpty("name");
			filterSelector.ThrowIfNull("filterSelector");

			if (this._Facets == null)
				this._Facets = new Dictionary<PropertyPathMarker, FacetDescriptorsBucket<T>>();

			var filter = new FilterDescriptor<T>();
			var f = filterSelector(filter);
			this._Facets.Add(name, new FacetDescriptorsBucket<T> { Filter = f });

			return this;
		}

		/// <summary>
		/// To avoid repetition of the suggest text, it is possible to define a global text.
		/// </summary>
		public SearchDescriptor<T> SuggestGlobalText(string globalSuggestText)
		{
			if (this._Suggest == null)
				this._Suggest = new Dictionary<string, object>();
			this._Suggest.Add("text", globalSuggestText);
			return this;
		}

		/// <summary>
		/// The term suggester suggests terms based on edit distance. The provided suggest text is analyzed before terms are suggested. 
		/// The suggested terms are provided per analyzed suggest text token. The term suggester doesn’t take the query into account that is part of request.
		/// </summary>
		public SearchDescriptor<T> SuggestTerm(string name, Func<TermSuggestDescriptor<T>, TermSuggestDescriptor<T>> suggest)
		{
			name.ThrowIfNullOrEmpty("name");
			suggest.ThrowIfNull("suggest");
			if (this._Suggest == null)
				this._Suggest = new Dictionary<string, object>();
			var desc = new TermSuggestDescriptor<T>();
			var item = suggest(desc);
			var bucket = new SuggestDescriptorBucket<T> { _Text = item._Text, TermSuggest = item };
			this._Suggest.Add(name, bucket);
			return this;
		}

		/// <summary>
		/// The phrase suggester adds additional logic on top of the term suggester to select entire corrected phrases 
		/// instead of individual tokens weighted based on ngram-langugage models. 
		/// </summary>
		public SearchDescriptor<T> SuggestPhrase(string name, Func<PhraseSuggestDescriptor<T>, PhraseSuggestDescriptor<T>> suggest)
		{
			name.ThrowIfNullOrEmpty("name");
			suggest.ThrowIfNull("suggest");
			if (this._Suggest == null)
				this._Suggest = new Dictionary<string, object>();

			var desc = new PhraseSuggestDescriptor<T>();
			var item = suggest(desc);
			var bucket = new SuggestDescriptorBucket<T> { _Text = item._Text, PhraseSuggest = item };
			this._Suggest.Add(name, bucket);
			return this;
		}

		/// <summary>
		/// The completion suggester is a so-called prefix suggester. 
		/// It does not do spell correction like the term or phrase suggesters but allows basic auto-complete functionality.
		/// </summary>
		public SearchDescriptor<T> SuggestCompletion(string name, Func<CompletionSuggestDescriptor<T>, CompletionSuggestDescriptor<T>> suggest)
		{
			name.ThrowIfNullOrEmpty("name");
			suggest.ThrowIfNull("suggest");
			if (this._Suggest == null)
				this._Suggest = new Dictionary<string, object>();

			var desc = new CompletionSuggestDescriptor<T>();
			var item = suggest(desc);
			var bucket = new SuggestDescriptorBucket<T> { _Text = item._Text, CompletionSuggest = item };
			this._Suggest.Add(name, bucket);
			return this;
		}

		/// <summary>
		/// Describe the query to perform using a query descriptor lambda
		/// </summary>
		public SearchDescriptor<T> Query(Func<QueryDescriptor<T>, BaseQuery> query)
		{
			query.ThrowIfNull("query");
			var q = new QueryDescriptor<T>() { IsStrict = this._Strict };

			var bq = query(q);
			return this.Query(bq);

		}
		/// <summary>
		/// Describe the query to perform using the static Query class
		/// </summary>
		public SearchDescriptor<T> Query(BaseQuery query)
		{
			if (query == null)
				return this;

			if (this._Strict && query.IsConditionless)
				throw new DslException("Query resulted in a conditionless query:\n{0}".F(JsonConvert.SerializeObject(query, Formatting.Indented)));

			else if (query.IsConditionless)
				return this;
			this._Query = query;
			return this;

		}

		/// <summary>
		/// Shortcut to .Query(q=>q.QueryString(qs=>qs.Query("string"))
		/// Does a match_all if the userInput string is null or empty;
		/// </summary>
		public SearchDescriptor<T> QueryString(string userInput)
		{
			var q = new QueryDescriptor<T>();
			BaseQuery bq;
			if (userInput.IsNullOrEmpty())
				bq = q.MatchAll();
			else
				bq = q.QueryString(qs => qs.Query(userInput));
			this._Query = bq;
			return this;
		}

		/// <summary>
		/// Describe the query to perform as a raw json string
		/// </summary>
		public SearchDescriptor<T> QueryRaw(string rawQuery)
		{
			rawQuery.ThrowIfNull("rawQuery");
			this._RawQuery = rawQuery;
			return this;
		}

		/// <summary>
		/// Filter search using a filter descriptor lambda
		/// </summary>
		public SearchDescriptor<T> Filter(Func<FilterDescriptor<T>, BaseFilter> filter)
		{
			filter.ThrowIfNull("filter");
			var f = new FilterDescriptor<T>().Strict(this._Strict);

			var bf = filter(f);
			if (bf == null)
				return this;
			if (this._Strict && bf.IsConditionless)
				throw new DslException("Filter resulted in a conditionless filter:\n{0}".F(JsonConvert.SerializeObject(bf, Formatting.Indented)));

			else if (bf.IsConditionless)
				return this;


			this._Filter = bf;
			return this;
		}
		/// <summary>
		/// Filter search
		/// </summary>
		public SearchDescriptor<T> Filter(BaseFilter filter)
		{
			filter.ThrowIfNull("filter");
			this._Filter = filter;
			return this;
		}

		/// <summary>
		/// Filter search using a raw json string
		/// </summary>
		public SearchDescriptor<T> FilterRaw(string rawFilter)
		{
			rawFilter.ThrowIfNull("rawFilter");
			this._RawFilter = rawFilter;
			return this;
		}


		/// <summary>
		/// Allow to highlight search results on one or more fields. The implementation uses the either lucene fast-vector-highlighter or highlighter. 
		/// </summary>
		public SearchDescriptor<T> Highlight(Action<HighlightDescriptor<T>> highlightDescriptor)
		{
			highlightDescriptor.ThrowIfNull("highlightDescriptor");
			this._Highlight = new HighlightDescriptor<T>();
			highlightDescriptor(this._Highlight);
			return this;
		}

		/// <summary>
		/// Allows you to specify a rescore query
		/// </summary>
		public SearchDescriptor<T> Rescore(Action<RescoreDescriptor<T>> rescoreSelector)
		{
			rescoreSelector.ThrowIfNull("rescoreSelector");
			this._Rescore = new RescoreDescriptor<T>();
			rescoreSelector(this._Rescore);
			return this;
		}

		/// <summary>
		/// Shorthand for a match_all query without having to specify .Query(q=>q.MatchAll())
		/// </summary>
		public SearchDescriptor<T> MatchAll()
		{
			return this.Query(q => q.MatchAll());
		}

		public SearchDescriptor<T> ConcreteTypeSelector(Func<dynamic, Hit<dynamic>, Type> typeSelector)
		{
			this._ConcreteTypeSelector = typeSelector;
			return this;
		}

		ElasticsearchPathInfo<SearchRequestParameters> IPathInfo<SearchRequestParameters>.ToPathInfo(IConnectionSettingsValues settings)
		{
			var pathInfo = new ElasticsearchPathInfo<SearchRequestParameters>();
			pathInfo.HttpMethod = this._QueryString.ContainsKey("source")
				? PathInfoHttpMethod.GET
				: PathInfoHttpMethod.POST;

			pathInfo.RequestParameters = this._QueryString;

			var inferrer = new ElasticInferrer(settings);
			string indices;
			if (this._AllIndices.GetValueOrDefault(false))
				indices = !this._AllTypes.GetValueOrDefault(false) ? "_all" : null;
			else if (this._Indices.HasAny())
				indices = inferrer.IndexNames(this._Indices);
			else
				indices = inferrer.IndexName<T>();

			string types;
			if (this._AllTypes.GetValueOrDefault(false))
				types = null;
			else if (this._Types.HasAny())
				types = inferrer.TypeNames(this._Types);
			else
				types = inferrer.TypeName<T>();

			pathInfo.Index = indices;
			pathInfo.Type = types;

			return pathInfo;
		}
	}
}
