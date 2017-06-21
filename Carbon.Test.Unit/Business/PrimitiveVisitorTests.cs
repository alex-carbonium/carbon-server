using System.Collections.Generic;
using System.Linq;
using Carbon.Business.Domain;
using Carbon.Business.Domain.DataTree;
using Carbon.Business.Sync;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Test.Unit.Business
{
    [TestClass]
    public class PrimitiveVisitorTests
    {
        [TestMethod]
        public void InsertFirstNode()
        {
            //arrange
            var model = new ProjectModel("app");
            var primitive = new DataNodeAddPrimitive();
            primitive.Path = new DataNodePath();
            primitive.Node = new DataNode("node2", NodeType.Page);
            primitive.Index = 0;

            //act
            Apply(model, primitive);

            //asser
            Assert.AreSame(primitive.Node, model.Children.Single());
        }

        [TestMethod]
        public void InsertSecondLevelNode()
        {
            //arrange
            var model = new ProjectModel("app");
            var page = model.AddChild("page", NodeType.Page);

            var primitive = new DataNodeAddPrimitive();
            primitive.Path = new DataNodePath(page.Id, page.Id);
            primitive.Node = new DataNode("text", NodeType.Text);
            primitive.Index = 0;

            //act
            Apply(model, primitive);

            //asser
            Assert.AreSame(primitive.Node, page.Children.Single());
        }

        [TestMethod]
        public void InsertTwoNodesTogether()
        {
            //arrange
            var model = new ProjectModel("app");

            var addPage = new DataNodeAddPrimitive();
            addPage.Path = new DataNodePath();
            addPage.Node = new DataNode("page", NodeType.Page);

            var addText = new DataNodeAddPrimitive();
            addText.Path = new DataNodePath(addPage.Node.Id, addPage.Node.Id);
            addText.Node = new DataNode("text", NodeType.Text);

            //act
            Apply(model, addPage, addText);

            //asser
            Assert.AreSame(addText.Node, model.Children.Single().Children.Single());
        }

        [TestMethod]
        public void InsertThirdLevelNode()
        {
            //arrange
            var model = new ProjectModel("app");
            var page = model.AddChild("page", NodeType.Page);
            var artboard = page.AddChild("artboard", NodeType.Artboard);

            var primitive = new DataNodeAddPrimitive();
            primitive.Path = new DataNodePath(page.Id, artboard.Id, artboard.Id);
            primitive.Node = new DataNode("text", NodeType.Text);
            primitive.Index = 0;

            //act
            Apply(model, primitive);

            //asser
            Assert.AreSame(primitive.Node, artboard.Children.Single());
        }

        [TestMethod]
        public void InsertEnd()
        {
            //arrange
            var model = new ProjectModel("app");
            model.AddChild("page1", NodeType.Page);
            model.AddChild("page2", NodeType.Page);

            var primitive = new DataNodeAddPrimitive();
            primitive.Path = new DataNodePath();
            primitive.Node = new DataNode("page3", NodeType.Page);
            primitive.Index = model.Children.Count;

            //act
            Apply(model, primitive);

            //asser
            Assert.AreSame(primitive.Node, model.Children.Last());
        }

        [TestMethod]
        public void InsertStart()
        {
            //arrange
            var model = new ProjectModel("app");
            model.AddChild("page1", NodeType.Page);
            model.AddChild("page2", NodeType.Page);

            var primitive = new DataNodeAddPrimitive();
            primitive.Path = new DataNodePath();
            primitive.Node = new DataNode("page3", NodeType.Page);
            primitive.Index = 0;

            //act
            Apply(model, primitive);

            //asser
            Assert.AreSame(primitive.Node, model.Children.First());
        }

        [TestMethod]
        public void ApplyByPageIdAndElementId()
        {
            //arrange
            var model = new ProjectModel("app");
            var page1 = model.AddChild("page1", NodeType.Page);
            page1.AddChild("text1", NodeType.Text).SetProp("x", 1);
            var page2 = model.AddChild("page2", NodeType.Page);
            var text2 = page2.AddChild("text1", NodeType.Text).SetProp("x", 2);

            var primitive = new DataNodeSetPropsPrimitive();
            primitive.Path = new DataNodePath(page2.Id, text2.Id);
            primitive.Props = new Dictionary<string, dynamic> { {"x", 3} };

            //act
            Apply(model, primitive);

            //asser
            Assert.AreEqual(1, page1.Children.Single().GetProp("x"), "Should not touch page1");
            Assert.AreEqual(3, page2.Children.Single().GetProp("x"), "Should change on page2");
        }

        [TestMethod]
        public void ReplaceNode()
        {
            //arrange
            var model = new ProjectModel("app");
            var page1 = model.AddChild("page1", NodeType.Page);
            var page2 = model.AddChild("page2", NodeType.Page);
            var page3 = model.AddChild("page3", NodeType.Page);
            var primitive = new DataNodeChangePrimitive();
            primitive.Path = new DataNodePath();
            primitive.Node = new DataNode(page2.Id, NodeType.Page);

            //act
            Apply(model, primitive);

            //asser
            Assert.AreSame(primitive.Node, model.Children[1]);
        }

        [TestMethod]
        public void ChangeNodePosition()
        {
            //arrange
            var model = new ProjectModel("app");
            var page1 = model.AddChild("page1", NodeType.Page);
            var page2 = model.AddChild("page2", NodeType.Page);
            var page3 = model.AddChild("page3", NodeType.Page);
            var primitive = new DataNodeChangePositionPrimitive();
            primitive.Path = new DataNodePath();
            primitive.ChildId = page3.Id;
            primitive.NewPosition = 0;

            //act
            Apply(model, primitive);

            //asser
            Assert.AreSame(page3, model.Children.First());
        }

        [TestMethod]
        public void RemoveNode()
        {
            //arrange
            var model = new ProjectModel("app");
            var page = model.AddChild("page1", NodeType.Page);
            var primitive = new DataNodeRemovePrimitive();
            primitive.Path = new DataNodePath();
            primitive.ChildId = page.Id;

            //act
            Apply(model, primitive);

            //asser
            Assert.AreEqual(0, model.Children.Count);
        }

        [TestMethod]
        public void PatchPropsChange_WithParsing()
        {
            //arrange
            var modelString = @"
{    
    type: 'App',
    props: {id: 'AppId', items: [{id: 'item1', value: 1}, {id: 'item2', value: 2}]}    
}";
            var primitiveString = @"
{    
    type: " + (int)PrimitiveType.DataNodePatchProps + @",
    patchType: " + (int)PatchType.Change + @",
    path: [],
    propName: 'items',
    item: {id: 'item2', value: 3}
}";

            var model = new ProjectModel();
            model.Read(modelString);

            var primitive = new DataNodePatchPropsPrimitive();
            primitive.Read(primitiveString);

            //act
            Apply(model, primitive);

            //asser
            Assert.AreEqual(3, (int)model.GetProp("items")[1].value);
        }

        [TestMethod]
        public void PatchPropsInsert_WithParsing()
        {
            //arrange
            var modelString = @"
{    
    type: 'App',
    props: {id: 'PageId', items: [{id: 'item1', value: 1}, {id: 'item2', value: 2}]}    
}";
            var primitiveString = @"
{    
    type: " + (int)PrimitiveType.DataNodePatchProps + @",
    patchType: " + (int)PatchType.Insert + @",
    path: [],
    propName: 'items',
    item: {id: 'item3', value: 3},
    index: 2
}";

            var model = new ProjectModel();
            model.Read(modelString);

            var primitive = new DataNodePatchPropsPrimitive();
            primitive.Read(primitiveString);

            //act
            Apply(model, primitive);

            //asser
            Assert.AreEqual(3, (int)model.GetProp("items").Count);
        }
        [TestMethod]
        public void PatchPropsInsertFirst()
        {
            //arrange
            var model = new ProjectModel();

            var primitive = new DataNodePatchPropsPrimitive();
            primitive.Path = new DataNodePath();
            primitive.PatchType = PatchType.Insert;
            primitive.PropName = "states";
            primitive.Item = new {id = "state1"};

            //act
            Apply(model, primitive);

            //asser
            Assert.AreEqual(1, (int)model.GetProp("states").Count);
        }

        [TestMethod]
        public void PatchPropsRemove_WithParsing()
        {
            //arrange
            var modelString = @"
{    
    type: 'App',
    props: {id: 'PageId', items: [{id: 'item1', value: 1}, {id: 'item2', value: 2}]}    
}";
            var primitiveString = @"
{    
    type: " + (int)PrimitiveType.DataNodePatchProps + @",
    patchType: " + (int)PatchType.Remove + @",
    path: [],
    propName: 'items',
    item: {id: 'item2', value: 2}
}";

            var model = new ProjectModel();
            model.Read(modelString);

            var primitive = new DataNodePatchPropsPrimitive();
            primitive.Read(primitiveString);

            //act
            Apply(model, primitive);

            //asser
            Assert.AreEqual(1, (int)model.GetProp("items").Count);
        }

        private void Apply(ProjectModel model, params DataNodeBasePrimitive[] primitives)
        {
            var visitor = new PrimitiveVisitor(primitives);
            model.Visit(visitor);
        }
    }
}
