using Mailier.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Nest;
using DBModel;
using Mailier.Core.Providers;
using Mailier.Core.Utils;

namespace Mailier.Core.Services
{
    public class HouseSndService : BaseIndexService
    {
        private const string IndexName = "housesnd";
        private const string AliasName = "index_" + IndexName;
        public ElasticClient Client;

        public HouseSndService()
        {
            Client = GetClient(IndexName);
            Initialize();
        }        

        public List<HouseSndModel> Query(Dictionary<string, string> dic, params string[] includeFields)
        {
            Func<SearchDescriptor<HouseSndModel>, ISearchRequest> condition;
            var mustQuery = new List<Func<QueryContainerDescriptor<HouseSndModel>, QueryContainer>>();

            var isRealSourceOn = true;
            if (isRealSourceOn)
            {
                mustQuery.Add(q => q.Term(h => h.IsRealSource, 1));
            }

            mustQuery.Add(q => q.Term(h => h.Status, 1));

            //多字段匹配
            var keyword = dic.TryGetString("RS");
            if (!string.IsNullOrEmpty(keyword))
            {
                mustQuery.Add(q => q.MultiMatch(h => h.Fields(fd => fd.Fields(
                        t => t.IntHouseNo,
                        t => t.RegionName,
                        t => t.CycleName,
                        t => t.GardenName,
                        t => t.AsNameOne,
                        t => t.AsNameTwo,
                        t => t.AsNameThree
                        ))
                      .Query(dic["RS"])
                    ));
            }

            //建成年代
            var builtYear = dic.TryGetString("Y") ;
            if (int.TryParse(builtYear, out int intBuiltYear))
            {
                SearchItem searchYear = HouseSndOptions.BuiltYearCollection.FirstOrDefault(i => i.No == "Y" + builtYear);
                if (searchYear != null)
                {
                    var year = Convert.ToDouble(DateTime.Now.Year);
                    mustQuery.Add(q => q.Range(h => h
                          .Field(fd => fd.IntHouseBuiltYear)
                          .GreaterThanOrEquals(year + searchYear.ItemStart)
                          .LessThanOrEquals(year + searchYear.ItemEnd)
                        ));
                }
            }

            //楼层高
            var floor = dic.TryGetString("L");
            if (!string.IsNullOrEmpty(floor)) {
                mustQuery.Add(q => q.Term(h => h.HouseFLoorLevel, floor));
            }

            //新上房源
            if (dic.ContainsKey("N")) {
                mustQuery.Add(q => q.DateRange(h =>
                    h.Field(f=>f.CreateTime)
                    .GreaterThanOrEquals(DateTime.Now.AddDays(-7))
                    .LessThanOrEquals(DateTime.Now)
                ));
            }

            //搜索范围
            var range = dic.TryGetString("Range");
            if (range.Equals("Subway")) {
                //地铁线路
                var cacheLine = HouseSndOptions.SubwayCollection.FirstOrDefault(s => s.SubwayNO == "B" + dic.TryGetString("B"));
                if (cacheLine != null) {
                    mustQuery.Add(q => q.Term(h => h.Lines, cacheLine.SubwayName));

                    //地铁站点
                    var cacheStation = cacheLine.Station.FirstOrDefault(s => s.StationNO == "D" + dic.TryGetString("D"));
                    if (cacheStation != null) {
                        mustQuery.Add(q => q.Term(h=>h.Stations, cacheStation.StationName));
                    }
                }
            }
            if (range.Equals("Cycle")) {

                //新方法， 支持传入坐标点 + 距离
                var lat = dic.TryGetString("lat");//经度
                var lng = dic.TryGetString("lng");//纬度
                var distance = dic.TryGetString("distance");//单位: km

                if (double.TryParse(lat, out double tmpLat)
                    && double.TryParse(lng, out double tmpLng)
                    && double.TryParse(distance, out double tmpDis)){
                    mustQuery.Add(q => q.GeoDistance(g => g
                        .Field(fd=>fd.Point)
                        .Distance(double.Parse(distance) + "km")
                        .Location(double.Parse(lat), double.Parse(lng))
                        .DistanceType(GeoDistanceType.Arc)
                    ));
                }

                //原有方法，支持四角坐标
                var latBegin = dic.TryGetString("xb");
                var latEnd = dic.TryGetString("xe");
                var lngBegin = dic.TryGetString("yb");
                var lngEnd = dic.TryGetString("ye");

                if (double.TryParse(latBegin, out double tmpLatBegin)
                    && double.TryParse(latEnd, out double tmpLatEnd)
                    && double.TryParse(lngBegin, out double tmpLngBegin)
                    && double.TryParse(lngEnd, out double tmpLngEnd)) {

                    mustQuery.Add(q => q.GeoBoundingBox(g => g
                        .Field(fd=>fd.Point)
                        .BoundingBox(b=>b
                            .TopLeft(double.Parse(latBegin), double.Parse(lngBegin))
                            .BottomRight(double.Parse(latEnd), double.Parse(lngEnd)))
                        .Type(GeoExecution.Indexed)
                        .ValidationMethod(GeoValidationMethod.Strict)
                    ));
                }
            }

            //区域
            var region = dic.TryGetString("R");
            if (!string.IsNullOrEmpty(region)) {
                var cacheRegion = HouseSndOptions.RegionCollection.FirstOrDefault(r => r.RegionNO == "R" + region);
                if (cacheRegion != null) {
                    mustQuery.Add(q => q.Term(h => h.RegionName, cacheRegion.RegionName));

                    //商圈
                    var cycle = dic.TryGetString("C");
                    if (cacheRegion.Cycle != null && !string.IsNullOrEmpty(cycle)) {
                        var cacheCycle = cacheRegion.Cycle.FirstOrDefault(c => c.CycleNO == "C" + cycle);
                        if (cacheCycle != null) {
                            mustQuery.Add(q => q.Term(h => h.CycleName, cacheCycle.CycleName));
                        }
                    }
                }
            }

            //售价
            var price = dic.TryGetString("S");
            if (double.TryParse(price, out double priceTmp)) {
                var searchPrice = HouseSndOptions.PriceCollection.FirstOrDefault(i => i.DisplayNO == "S" +price);
                if (searchPrice != null)
                {
                    mustQuery.Add(q => q.Range(h =>
                        h.Field(fd => fd.PriceAll)
                        .GreaterThanOrEquals(searchPrice.PriceStart)
                        .LessThanOrEquals(searchPrice.PriceEnd)
                    ));
                }
            }

            //售价区间
            var priceBegin = dic.TryGetString("SB");
            var priceEnd = dic.TryGetString("SE");
            if (double.TryParse(priceBegin, out double tmpPriceBegin) && double.TryParse(priceEnd, out double tmpPriceEnd)) {
                mustQuery.Add(q => q.Range(h =>
                        h.Field(fd => fd.PriceAll)
                        .GreaterThanOrEquals(double.Parse(priceBegin))
                        .LessThanOrEquals(double.Parse(priceEnd))
                    ));
            }

            //面积
            var area = dic.TryGetString("A");
            if (int.TryParse(area, out int tmpArea)) {
                var searchArea = HouseSndOptions.AreaCollection.FirstOrDefault(i => i.DisplayNO == "A" + area);
                if (searchArea != null) {
                    mustQuery.Add(q => q.Range(h=>h
                        .Field(fd=>fd.HouseArea)
                        .GreaterThanOrEquals(searchArea.AreaStart)
                        .LessThanOrEquals(searchArea.AreaEnd)
                    ));
                }
            }

            //面积区间
            var areaBegin = dic.TryGetString("AB");
            var areaEnd = dic.TryGetString("AE");
            if (double.TryParse(areaBegin, out double tmpAreaBegin) && double.TryParse(areaEnd, out double tmpAreaEnd)) {
                mustQuery.Add(q => q.Range(h => h
                        .Field(fd => fd.HouseArea)
                        .GreaterThanOrEquals(double.Parse(areaBegin))
                        .LessThanOrEquals(double.Parse(areaEnd))
                    ));
            }

            //户型
            var roomNum = dic.TryGetString("H");
            if (int.TryParse(roomNum, out int tmpRoomNum)) {
                var houseType = HouseSndOptions.HouseTypeCollection.FirstOrDefault(i => i.DisplayNO == "H" + roomNum);
                if (houseType != null) {
                    mustQuery.Add(q => q.Range(h => h
                        .Field(fd=>fd.SleepRoomNum)
                        .GreaterThanOrEquals(houseType.HouseTypeStart)
                        .LessThanOrEquals(houseType.HouseTypeEnd)
                    ));
                }
            }

            //朝向
            var toward = dic.TryGetString("O");
            if (int.TryParse(toward, out int tmpToward)) {
                mustQuery.Add(q=>q.Term(h=>h.Towards, toward));
            }

            //卖点
            var tags = dic.TryGetString("T");
            if (!string.IsNullOrEmpty(tags)) {
                mustQuery.Add(q => q.Terms(h=>h
                    .Field(fd=>fd.Tags)
                    .Terms(tags.Split(','))
                ));
            }

            //小区
            var gardenIds = dic.TryGetString("GardenID");
            if (!string.IsNullOrEmpty(gardenIds)) {
                mustQuery.Add(q => q.Terms(h => h.Field(fd=>
                    fd.GardenId).Terms(gardenIds.Split(','))
                ));
            }

            //经纪人
            var brokerId = dic.TryGetString("BrokerID");
            if (!string.IsNullOrEmpty(brokerId)) {
                //usage one
                //mustQuery.Add(q => q.Wildcard(h=>h.Field(c=>c.BrokerId).Value(string.Format("*|{0}|*", dic["BrokerID"]))));

                //usage two
                mustQuery.Add(q => q.Term("brokers", brokerId));
            }

            

            //使用别名进行查询，方便变更数据
            condition = q => q.Index(AliasName);

            //加入搜索条件
            var shouldQuery = GetBoostQuery(dic);
            if (shouldQuery.Any()) {//计算评分
                condition += q => q.Query(d => d.Bool(f => f.Must(mustQuery).Should(shouldQuery)));
            }
            else {
                condition += q => q.Query(d => d.Bool(f => f.Filter(mustQuery)));
            }

            //返回哪些字段
            if (includeFields != null && includeFields.Any())
            {
                condition += q => q.Source(s => s.Includes(h => h.Fields(includeFields)));
            }
            else
            {
                condition += q => q.Source(s => s.Includes(h => h.Field(f => f.IntHouseNo)));
            }

            //计算评分的情况下，按积分倒序排序
            //如果无论如何你都要计算 _score ， 你可以将 track_scores 参数设置为 true 
            if (!shouldQuery.Any())
            {
                //排序规则
                var order = dic.TryGetString("OR");
                if (!string.IsNullOrEmpty(order))
                {
                    switch (order)
                    {
                        case "11":
                            condition += q => q.Sort(s => s.Ascending(h => h.PriceAll));// 总价 正序                        
                            break;
                        case "12":
                            condition += q => q.Sort(s => s.Descending(h => h.PriceAll));// 总价 倒序
                            break;
                        case "21":
                            condition += q => q.Sort(s => s.Ascending(h => h.Price));//单价 正序
                            break;
                        case "22":
                            condition += q => q.Sort(s => s.Descending(h => h.Price));// 单价 倒序
                            break;
                        case "31":
                            condition += q => q.Sort(s => s.Ascending(h => h.HouseArea));// 面积 正序                        
                            break;
                        case "32":
                            condition += q => q.Sort(s => s.Descending(h => h.HouseArea));//面积 倒序
                            break;
                        default:
                            condition += q => q.Sort(s => s.Descending(h => h.RankValue)); //默认按照Rank 倒序
                            break;
                    }
                }
                else
                {
                    condition += q => q.Sort(s => s.Descending(h => h.RankValue)); //默认按照Rank 倒序 
                }
            }
            

            //page
            if (!int.TryParse(dic.TryGetString("PG"), out int pageIndex))
            {
                pageIndex = 1;
            }
            if (!int.TryParse(dic.TryGetString("PS"), out int pageSize))
            {
                pageSize = 10;
            }
            condition += q => q.From((pageIndex - 1) * pageSize).Size(pageSize);

            var response = Client.Search(condition);
            var houses = response.Documents;
            return houses.AsList();
        }

        /// <summary>
        /// 设置计算匹配度字段权重
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        private List<Func<QueryContainerDescriptor<HouseSndModel>, QueryContainer>> GetBoostQuery(Dictionary<string, string> dic) {                                  
            var shouldQuery = new List<Func<QueryContainerDescriptor<HouseSndModel>, QueryContainer>>();

            //小区ID  
            var customGarden = dic.TryGetString("GI");
            if (!string.IsNullOrEmpty(customGarden))
            {
                shouldQuery.Add(q => q.Match(m=>m.Boost(51).Field("gardenId").Query(customGarden)));//(h => h.GardenId, customGarden, 51));
            }

            //区域
            var customRegion = dic.TryGetString("MR");
            if (!string.IsNullOrEmpty(customRegion))
            {
                var searchRegion = HouseSndOptions.RegionCollection.FirstOrDefault(i => i.RegionNO == "R" + customRegion);
                if (searchRegion != null)
                {
                    Func<QueryContainerDescriptor<HouseSndModel>, QueryContainer> item = q => q.Term(h => h.RegionName, customRegion, 50, "region");
                    //商圈
                    var customCycle = dic.TryGetString("MC");
                    if (!string.IsNullOrEmpty(customCycle) && searchRegion.Cycle != null)
                    {
                        SearchCycle searchCycle = searchRegion.Cycle.FirstOrDefault(i => i.CycleNO == "C" + customCycle);
                        if (searchCycle != null)
                        {
                            shouldQuery.Add(q => q.Term(h => h.CycleName, customCycle, 50));
                            shouldQuery.Remove(item);
                        }
                    }
                }

            }

            //售价
            var customPrice = dic.TryGetString("MS");
            if (int.TryParse(customPrice, out int tmpCstPrice)) {
                var searchPrice = HouseSndOptions.PriceCollection.FirstOrDefault(i => i.DisplayNO == "S" + customPrice);
                if (searchPrice != null)
                {
                    shouldQuery.Add(q => q.Range(r => r
                      .Field(fd => fd.PriceAll)
                      .GreaterThanOrEquals(searchPrice.PriceStart)
                      .LessThanOrEquals(searchPrice.PriceEnd)
                      .Boost(50)
                    ));
                }
            }


            //面积
            var customArea = dic.TryGetString("MA");
            if (int.TryParse(customArea, out int tmpCstArea))
            {
                var searchArea = HouseSndOptions.AreaCollection.FirstOrDefault(i => i.DisplayNO == "A" + customArea);
                if (searchArea != null)
                {
                    shouldQuery.Add(q => q.Range(r => r
                       .Field(fd => fd.PriceAll)
                       .GreaterThanOrEquals(searchArea.AreaStart)
                       .LessThanOrEquals(searchArea.AreaEnd)
                       .Boost(30)
                     ));
                }
            }
            //楼龄
            //if (dic.ContainsKey("MAGEB") && dic["MAGEB"] != null && dic["MAGEB"] != "")
            //{
            //    var q = LucenceHelper.GetQuery("Double", "HOUSEAGE", dic["MAGEB"], dic["MAGEE"], false, true);
            //    q.SetBoost(20);
            //    listQuery.Add(q);
            //    listQueryType.Add(BooleanClause.Occur.SHOULD);
            //}

            //户型
            var customHouseType = dic.TryGetString("MH");
            if (int.TryParse(customHouseType, out int tmpCstHouseType))
            {
                var searchHouseType = HouseSndOptions.HouseTypeCollection.FirstOrDefault(i => i.DisplayNO == "H" + customHouseType);
                if (searchHouseType != null)
                {
                    shouldQuery.Add(q => q.Range(r => r
                       .Field(fd => fd.PriceAll)
                       .GreaterThanOrEquals(searchHouseType.HouseTypeStart)
                       .LessThanOrEquals(searchHouseType.HouseTypeEnd)
                       .Boost(30)
                     ));
                }
            }



            //朝向
            //TODO:逻辑有问题
            var customToward = dic.TryGetString("MO");
            if (!string.IsNullOrEmpty(customToward))
            {
                shouldQuery.Add(q => q.Term(h => h.Towards, customToward, 20));
            }

            //卖点
            var customTags = dic.TryGetString("MT");
            if (!string.IsNullOrEmpty(customTags))
            {
                shouldQuery.Add(q => q.Terms(h => h
                    .Field(fd => fd.Tags)
                    .Terms(customTags.Split(','))
                    .Boost(5)
                ));
            }
            
            return shouldQuery;
        }

        public void CreateAll()
        {            
            Client.DeleteIndex(IndexName);
            Initialize();

            var pageIndex = 0;
            var pageSize = 2000;

            var list = new HouseSndProvider().GetData();

            while (list.Any()) {
                var data =list.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                if(data.Any())
                    Client.IndexMany(data);

                pageIndex++;
                if (pageSize > data.Count()) break;
            }  
        }

        private void Initialize()
        {
            if (Client.IndexExists(IndexName).Exists)
            {
                return;
            }
            var descriptor = new CreateIndexDescriptor(IndexName)
                .Aliases(s => s.Alias(AliasName))
                .Settings(s => s.Analysis(a => a
                    .TokenFilters(t => t
                        .Synonym("by_sfr", sy => sy.SynonymsPath("analysis/synonym.txt").Tokenizer("ik_smart"))
                        .Stop("by_tfr", stop => stop.StopWords(" "))
                    )
                    .Analyzers(aa => aa
                        .Custom("by_max_word", c => c
                            .Tokenizer("ik_max_word")
                            .Filters("by_tfr", "by_sfr")
                        )
                        .Custom("by_smart", c => c.Tokenizer("ik_smart")
                            .Filters("by_tfr", "by_sfr")
                        )
                    )
                  )
                )
                .Mappings(ms => ms
                    .Map<HouseSndModel>(
                        m => m.AutoMap()
                            .Properties(p => p
                                .Text(t => t
                                    .Name(h => h.GardenName)
                                    .Analyzer("by_max_word")
                                    .SearchAnalyzer("by_smart")))
                            .AllField(a => a.Store(false))
                    )
                );

            Client.CreateIndex(IndexName, c => descriptor);
        }        

        public List<HouseSndModel> QueryUsageOne(Dictionary<string, string> dic)
        {
            

            //var response = client.Search<HouseSndModel>(s => s.Query(
            //        q => q.Term(t => t.IntHouseNo, intHouseNo) 
            //        )//.Source(sc=>sc.Includes(i=>i.Field(f=>f.last_name)))
            //        );

            var isRealSourceOn = true;

            var mustQuery = new List<Func<QueryContainerDescriptor<HouseSndModel>, QueryContainer>>();

            QueryContainer query = new TermQuery();

            if (isRealSourceOn)
            {
                mustQuery.Add(q => q.Term(h => h.IsRealSource, 1, 50));
                query = query && new TermQuery { Field = "isRealSource", Value = 1 };
            }

            //var sort = new SortDescriptor<HouseSndModel>();
            //sort.Ascending(h => h.BrokerId);
            //sort.Ascending(h => h.IntHouseNo);

            var request = new SearchRequest<HouseSndModel>();
            request.Sort = new List<ISort>()
            {
                new SortField(){ Field = "", Order = SortOrder.Descending},
                //new SortField(){Field = Field<HouseSndModel>(f=>f.)}
            };
            request.Query = query;


            var response = Client.Search<HouseSndModel>(s => s
                .Query(q => q.Bool(b => b.Filter(mustQuery)))
                    );
            //var response = client.Search<HouseSndModel>(sortRequest);

            var houses = response.Documents;
            //var house = houses.FirstOrDefault();
            return houses.AsList();
        }
        public List<HouseSndModel> QueryTest() {
            Func<SearchDescriptor<HouseSndModel>, ISearchRequest> condition;
            var mustQuery = new List<Func<QueryContainerDescriptor<HouseSndModel>, QueryContainer>>();

            condition = c => c.Index(IndexName);
            mustQuery.Add(q => q.Term("brokers", "53455"));
            condition += c => c.Query(q => q.Bool(m => m.Must(mustQuery).Should(g =>g.Term("brokers", "53933", 30))));
            condition += c => c.Sort(q => q.Descending(h => h.RankValue)).TrackScores();

            var rsp =Client.Search(condition);
            return rsp.Documents.ToList();
        }
    }
}