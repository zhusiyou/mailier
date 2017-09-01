using DBModel;
using Mailier.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mailier.Core.Services
{
    public class HouseSndOptions
    {
        /// <summary>
        /// 房屋面积
        /// </summary>
        public static readonly List<SearchArea> AreaCollection;
        /// <summary>
        /// 卖点
        /// </summary>
        public static readonly List<SearchTag> TagCollection;
        /// <summary>
        /// 房屋类型
        /// </summary>
        public static readonly List<SearchHouseType> HouseTypeCollection;
        /// <summary>
        /// 售价
        /// </summary>
        public static readonly List<SearchPrice> PriceCollection;
        /// <summary>
        /// 区域商圈
        /// </summary>
        public static readonly List<SearchRegion> RegionCollection;
        /// <summary>
        /// 地铁线路、站点
        /// </summary>
        public static readonly List<SearchSubway> SubwayCollection;
        /// <summary>
        /// 建成年代
        /// </summary>
        public static readonly List<SearchItem> BuiltYearCollection;
        /// <summary>
        /// 楼层
        /// </summary>
        public static readonly List<SearchItem> FloorCollection;
        /// <summary>
        /// 朝向
        /// </summary>
        public static readonly List<SearchNormal> FaceCollection;

        static HouseSndOptions()
        {
            var redis = new RedisHelper();
            AreaCollection = redis.StringGet<List<SearchArea>>("B_AREA");
            TagCollection = redis.StringGet<List<SearchTag>>("B_S_TAG");
            HouseTypeCollection = redis.StringGet<List<SearchHouseType>>("B_HOUSETYPE");
            PriceCollection = redis.StringGet<List<SearchPrice>>("B_S_PRICE");
            RegionCollection = redis.StringGet<List<SearchRegion>>("B_REGION_50E434A183EE40DB93ABAC8A91588DEA");
            SubwayCollection = redis.StringGet<List<SearchSubway>>("B_SUBWAY_C7100AD45B9249BF9DE27894AD034AAB");
            BuiltYearCollection = redis.StringGet<List<SearchItem>>("B_C_YEAR");
            FloorCollection = redis.StringGet<List<SearchItem>>("B_S_Floor");
            FaceCollection = redis.StringGet<List<SearchNormal>>("B_FACE");
        }
    }
}