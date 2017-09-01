using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mailier.Core.Entities
{
    [ElasticsearchType(IdProperty = "intHouseNo", Name = "housesnd")]
    public class HouseSndModel
    {
        public string RegionName { get; set; }//区域名称
        public string CycleName { get; set; }//生活圈
        [Keyword]
        public string GardenId { get; set; }//小区ID
        //[Text(Analyzer = "ik_max_word", SearchAnalyzer = "ik_smart")]
        public string GardenName { get; set; }//小区名称
        [Text(Analyzer = "by_max_word", SearchAnalyzer = "by_smart")]
        public string AsNameOne { get; set; }//别名一
        //[Text(Analyzer = "ik_max_word", SearchAnalyzer = "ik_smart")]
        public string AsNameTwo { get; set; }//别名二
        //[Text(Analyzer = "ik_max_word", SearchAnalyzer = "ik_smart")]
        public string AsNameThree { get; set; }//别名三
        public double GardenX { get; set; }//X坐标
        public double GardenY { get; set; }//Y坐标
        [GeoPoint]
        public string Point
        {
            get { return string.Format("{0}, {1}", GardenY, GardenX); }
        }
        public string SubwayList { get; set; }//周边地铁
        //地铁线路
        public IEnumerable<string> Lines {
            get {
                //14号线-阜通-3|14号线-望京-3|14号线-望京南-3|15号线-望京-3
                var array = SubwayList.Split(new[] { '|', '-' });
                if (array.Length == 0) return null;

                var lines = new List<string>();
                for (var i = 0; i < array.Length; i = i +3) {
                    lines.Add(array[i]);
                }
                return lines.Distinct();
                
            }
        }
        public IEnumerable<string> Stations {
            get {
                //14号线-阜通-3|14号线-望京-3|14号线-望京南-3|15号线-望京-3
                var array = SubwayList.Split(new[] { '|', '-' });
                if (array.Length < 3) return null;

                var stations = new List<string>();
                for (var i = 1; i < array.Length; i = i + 3)
                {
                    stations.Add(array[i]);
                }
                return stations.Distinct();
            }
        }
        [Keyword]
        public string IntHouseNo { get; set; }//房源编号
        public double HouseArea { get; set; }//面积
        public double PriceAll { get; set; }//总价
        public double Price { get; set; }//单价
        public string HouseForward { get; set; }//朝向
        public IEnumerable<string> Towards {
            get {
                return (HouseForward ?? "").Split(',');
            }
        }
        public double SleepRoomNum { get; set; }//卧室数
        public IEnumerable<string> Tags {
            get {
                //TODO:
                return new string[] { };
            }
        }

        public int IsFullFive { get; set; }//满五
        public int IsOnly { get; set; }//唯一
        public int IsFullTwo { get; set; }
        public int IsSubwayHouse { get; set; }//地铁房
        public int IsSchoolHouse { get; set; }//学区房
        public int AnytimeLook { get; set; }//随时可看
        public int IsFastSales { get; set; }//急售
        public string Exclusives { get; set; }//独家
        public int IsExclusives { get; set; }//是否是独家
        public int IsTerRace { get; set; }//露台
        public int IsBayWindow { get; set; }//飘窗
        public int IsGiveParkingLot { get; set; }//车位
        public int HousingType { get; set; }//房源类别(别墅//普通住宅)

        //此字段可以用集合，或者数组
        [Keyword]
        public string BrokerId { get; set; }//相关经纪人

        public IEnumerable<string> Brokers
        {
            get
            {
                var fieldname = nameof(BrokerId);
                if (!string.IsNullOrEmpty(BrokerId))
                {
                    return BrokerId.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList().Distinct();
                }
                return null;
            }
        }

        public double RankValue { get; set; }//排序
        public int Status { get; set; }//默认状态
        [Date(Format = "yyyy-MM-dd")]
        public string CreateTime { get; set; }//创建时间
        public int IsTop { get; set; }
        public string TopTime { get; set; }
        public int HouseAge { get; set; }//房龄
        public int IsRealSource { get; set; }//是否符合真房源
        public int IntHouseBuiltYear { get; set; }//建成年代
        public double IntHouseFloor { get; set; }//楼层
        public int HouseFLoorLevel { get; set; }//楼层（高/中低）
    }
}