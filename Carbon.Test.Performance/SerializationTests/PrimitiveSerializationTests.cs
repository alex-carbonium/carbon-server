using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Carbon.Business.Domain.DataTree;
using Carbon.Business.Sync;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Carbon.Test.Performance.SerializationTests
{
    [TestClass]
    public class PrimitiveSerializationTests
    {
        private Stopwatch _sw;
        private static TestContext _context;

        [ClassInitialize]
        public static void GlobalSetup(TestContext context)
        {
            _context = context;
        }

        [TestInitialize]
        public void Setup()
        {
            _sw = new Stopwatch();
        }

        [TestMethod]
        public void DeserializeArrayVsIndividually()
        {
            //Arrange
            var primitives = new List<DataNodeAddPrimitive>(500);
            for (var i = 0; i < 500; i++)
            {
                var primitive = new DataNodeAddPrimitive();
                primitive.Id = Guid();
                primitive.Path = new DataNodePath(Guid(), Guid(), Guid(), Guid());
                primitive.Node = CreateNode(10);
                for (int j = 0; j < 10; j++)
                {
                    primitive.Node.InsertChild(CreateNode(10), primitive.Node.Children.Count);
                }
                primitives.Add(primitive);
            }

            var strings = primitives.Select(x => x.Write()).ToArray();
            var arrayBuilder = new StringBuilder();
            arrayBuilder.Append("[");
            foreach (var primitive in primitives)
            {
                if (arrayBuilder.Length > 1)
                {
                    arrayBuilder.Append(",");
                }
                arrayBuilder.Append(primitive.Write());
            }
            var arrayString = arrayBuilder.ToString();

            //Act
            var listTime = Measure(() =>
            {
                PrimitiveFactory.CreateMany<DataNodeAddPrimitive>(strings).ToList();
            });

            var arrayTime = Measure(() =>
            {
                JsonConvert.DeserializeObject<JArray>(arrayString);
            });

            //Assert
            var msg = $"List {listTime}, array: {arrayTime}";
            _context.WriteLine(msg);
            Assert.IsTrue(listTime < arrayTime, msg);
        }

        private double Measure(Action action)
        {
            var results = new List<long>();
            for (int i = 0; i < 5; i++)
            {
                _sw.Restart();
                action();
                results.Add(_sw.ElapsedMilliseconds);
            }

            return results.Average();
        }

        private static DataNode CreateNode(int propCount)
        {
            var node = new DataNode(Guid(), NodeType.Container);
            for (var j = 0; j < propCount; j++)
            {
                var p = (char)('a' + j);
                var v = (char)('A' + j);
                node.SetProp(new string(p, 10), new string(v, 10));
            }
            node.Children = new List<DataNode>();
            return node;
        }

        private static string Guid()
        {
            return System.Guid.NewGuid().ToString();
        }
    }
}
