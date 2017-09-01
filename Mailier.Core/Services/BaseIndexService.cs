using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mailier.Core.Services
{
    public class BaseIndexService
    {
        protected ElasticClient GetClient(string indexName)
        {
            //var uris = new Uri[]
            //{
            //    new Uri("http://localhost:9200"), 
            //    new Uri("http://localhost:9201"), 
            //    //new Uri(""), 
            //};

            //var pool = new StaticConnectionPool(uris);
            //var single = new SingleNodeConnectionPool(uris.FirstOrDefault());

            var nodes = new Node[]
            {
                new Node(new Uri("http://127.0.0.1:9200/")),
                new Node(new Uri("http://127.0.0.1:9201/")),
                //new Node(new Uri("")), 
            };

            //请求时随机发送请求到各个正常的节点，不请求异常节点,异常节点恢复后会重新被请求
            var pool = new StaticConnectionPool(nodes);

            ////false.创建客户端时，随机选择一个节点作为客户端的请求对象，该节点异常后不会切换其它节点
            ////true，请求时随机请求各个正常节点，不请求异常节点,但异常节点恢复后不会重新被请求
            //var poolSnd = new SniffingConnectionPool(nodes) { SniffedOnStartup = true };

            ////创建客户端时，选择第一个节点作为请求主节点，该节点异常后会切换其它节点，待主节点恢复后会自动切换回来
            //var poolThird = new StickyConnectionPool(nodes);

            var settings = new ConnectionSettings(pool).DefaultIndex(indexName);
            return new ElasticClient(settings);
        }
    }
}