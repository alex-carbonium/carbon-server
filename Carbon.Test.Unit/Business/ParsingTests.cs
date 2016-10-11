using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Business.Domain.DataTree;
using Carbon.Business.Sync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Test.Unit.Business
{
    [TestClass]
    public class ParsingTests : BaseTest
    {
        [TestMethod]
        public void ParseAllPrimitiveTypes()
        {
            var types = Enum.GetValues(typeof(PrimitiveType))
                .Cast<PrimitiveType>()
                .Where(x => x != PrimitiveType.None);

            var failedTypes = new List<PrimitiveType>();
            foreach (var type in types)
            {
                var json = $"{{type: {(int)type}}}";
                try
                {
                    var primitive = PrimitiveFactory.Create<RawPrimitive>(json);
                    if (primitive == null)
                    {
                        failedTypes.Add(type);
                    }
                }
                catch
                {
                    failedTypes.Add(type);
                }
            }

            Assert.AreEqual("", string.Join(", ", failedTypes), "These types could not be parsed");
        }

        [TestMethod]
        public void ParsePrimitiveWithNode()
        {
            var primitive = new DataNodeAddPrimitive();
            primitive.Path = new DataNodePath("1", "2", "3");
            primitive.Node = new DataNode("node", NodeType.Page).SetProp("name", "page 1");

            var json = primitive.Write();
            primitive = PrimitiveFactory.Create<DataNodeAddPrimitive>(json);

            Assert.AreEqual(new DataNodePath("1", "2", "3"), primitive.Path);
            Assert.AreEqual("page 1", primitive.Node.GetProp("name"));
        }

        [TestMethod]
        public void ParsePrimitiveWithProps()
        {
            var primitive = new DataNodeSetPropsPrimitive();
            primitive.Path = new DataNodePath("1", "2", "3");
            primitive.Props = new Dictionary<string, dynamic> { {"a", 1} };

            var json = primitive.Write();
            primitive = PrimitiveFactory.Create<DataNodeSetPropsPrimitive>(json);

            Assert.AreEqual(new DataNodePath("1", "2", "3"), primitive.Path);
            Assert.AreEqual(1, primitive.Props["a"]);
        }

        [TestMethod]
        public void ParsePrimitiveWithUncommonOrder()
        {
            var json = "{childId: 'c1', type: " + (int)PrimitiveType.DataNodeRemove + ", id: 'p1'}";

            var primitive = PrimitiveFactory.Create<DataNodeRemovePrimitive>(json);

            Assert.AreEqual("p1", primitive.Id, "Wrong id");
            Assert.AreEqual("c1", primitive.ChildId, "Wrong property");
        }

        [TestMethod]
        public void ParsePrimitiveWithUnknownSimpleProperty()
        {
            var json = "{xxx: 1, childId: 'c1', type: " + (int)PrimitiveType.DataNodeRemove + ", id: 'p1'}";

            var primitive = PrimitiveFactory.Create<DataNodeRemovePrimitive>(json);

            Assert.AreEqual("p1", primitive.Id, "Wrong id");
            Assert.AreEqual("c1", primitive.ChildId, "Wrong property");
        }

        [TestMethod]
        public void ParsePrimitiveWithUnknownObjectProperty()
        {
            var json = "{xxx: {a: 1}, childId: 'c1', type: " + (int)PrimitiveType.DataNodeRemove + ", id: 'p1'}";

            var primitive = PrimitiveFactory.Create<DataNodeRemovePrimitive>(json);

            Assert.AreEqual("p1", primitive.Id, "Wrong id");
            Assert.AreEqual("c1", primitive.ChildId, "Wrong property");
        }

        [TestMethod]
        public void ParsePrimitiveWithUnknownArrayProperty()
        {
            var json = "{xxx: [1, 2, 3], childId: 'c1', type: " + (int)PrimitiveType.DataNodeRemove + ", id: 'p1'}";

            var primitive = PrimitiveFactory.Create<DataNodeRemovePrimitive>(json);

            Assert.AreEqual("p1", primitive.Id, "Wrong id");
            Assert.AreEqual("c1", primitive.ChildId, "Wrong property");
        }

        [TestMethod]
        public void ParseDataNodeWithEmptyChildren()
        {
            var node = new DataNode("app", NodeType.App);
            node.SetProp("name", "app");
            node.Children = new List<DataNode>();
            var json = node.Write();

            node = DataNode.Create(json);

            Assert.AreEqual(0, node.Children.Count);
            Assert.AreEqual("app", node.GetProp("name"));            
        }

        [TestMethod]
        public void ParseDataNodeWithOneChild()
        {
            var node = new DataNode("app", NodeType.App);
            node.SetProp("name", "app");
            node.AddChild("page1", NodeType.Page).SetProp("name", "page 1");            
            var json = node.Write();

            node = DataNode.Create(json);

            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual("app", node.GetProp("name"));
            Assert.AreEqual("page 1", node.Children[0].GetProp("name"));            
        }

        [TestMethod]
        public void ParseDataNodeWithTwoChildren()
        {
            var node = new DataNode("app", NodeType.App);
            node.SetProp("name", "app");
            node.AddChild("page1", NodeType.Page).SetProp("name", "page 1");
            node.AddChild("page2", NodeType.Page).SetProp("name", "page 2");
            var json = node.Write();

            node = DataNode.Create(json);

            Assert.AreEqual(2, node.Children.Count);
            Assert.AreEqual("app", node.GetProp("name"));
            Assert.AreEqual("page 1", node.Children[0].GetProp("name"));
            Assert.AreEqual("page 2", node.Children[1].GetProp("name"));
        }

        [TestMethod]
        public void ParseDataNodeWithTwoGrandChildren()
        {
            var node = new DataNode("app", NodeType.App);                        
            var page1 = node.AddChild("page1", NodeType.Page).SetProp("name", "page 1");
            page1.AddChild("text1", NodeType.Text).SetProp("name", "text 1");
            page1.AddChild("text2", NodeType.Text).SetProp("name", "text 2");
            var json = node.Write();

            node = DataNode.Create(json);
            page1 = node.Children.Single();

            Assert.AreEqual(2, page1.Children.Count);            
            Assert.AreEqual("text 1", page1.Children[0].GetProp("name"));
            Assert.AreEqual("text 2", page1.Children[1].GetProp("name"));
        }
    }
}
