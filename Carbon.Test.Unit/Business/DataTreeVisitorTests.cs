using System.Collections.Generic;
using Carbon.Business.Domain.DataTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Test.Unit.Business
{
    [TestClass]
    public class DataTreeVisitorTests
    {
        [TestMethod]
        public void VisitRoot()
        {
            var app = new DataNode("app", NodeType.App);

            var visits = Visit(app);

            Assert.AreEqual(visits[app], new DataNodePath());
        }

        [TestMethod]
        public void VisitFirstPrimitiveRoot()
        {
            var app = new DataNode("app", NodeType.App);
            var page = app.AddChild("page", NodeType.Page);

            var visits = Visit(app);

            Assert.AreEqual(visits[page], new DataNodePath(page.Id, page.Id));
        }

        [TestMethod]
        public void VisitElementInFirstPrimitiveRoot()
        {
            var app = new DataNode("app", NodeType.App);
            var page = app.AddChild("page", NodeType.Page);
            var element = page.AddChild("element", NodeType.Text);

            var visits = Visit(app);

            Assert.AreEqual(visits[element], new DataNodePath(page.Id, element.Id));
        }

        [TestMethod]
        public void VisitGrandElementInFirstPrimitiveRoot()
        {
            var app = new DataNode("app", NodeType.App);
            var page = app.AddChild("page", NodeType.Page);
            var container = page.AddChild("container", NodeType.Container);
            var element = container.AddChild("element", NodeType.Text);

            var visits = Visit(app);

            Assert.AreEqual(visits[element], new DataNodePath(page.Id, element.Id));
        }

        [TestMethod]
        public void VisitSecondPrimitiveRoot()
        {
            var app = new DataNode("app", NodeType.App);
            var page = app.AddChild("page", NodeType.Page);
            var artboard = page.AddChild("artboard", NodeType.Artboard);

            var visits = Visit(app);

            Assert.AreEqual(visits[artboard], new DataNodePath(page.Id, artboard.Id, artboard.Id));
        }

        [TestMethod]
        public void VisitChildSecondPrimitiveRoot()
        {
            var app = new DataNode("app", NodeType.App);
            var page = app.AddChild("page", NodeType.Page);
            var artboard = page.AddChild("artboard", NodeType.Artboard);
            var element = artboard.AddChild("text", NodeType.Text);

            var visits = Visit(app);

            Assert.AreEqual(visits[element], new DataNodePath(page.Id, artboard.Id, element.Id));
        }

        [TestMethod]
        public void VisitGrandChildSecondPrimitiveRoot()
        {
            var app = new DataNode("app", NodeType.App);
            var page = app.AddChild("page", NodeType.Page);
            var artboard = page.AddChild("artboard", NodeType.Artboard);
            var container = artboard.AddChild("container", NodeType.Container);
            var element = container.AddChild("text", NodeType.Text);

            var visits = Visit(app);

            Assert.AreEqual(visits[element], new DataNodePath(page.Id, artboard.Id, element.Id));
        }

        [TestMethod]
        public void VisitChildrenInTwoFirstPrimitiveRoots()
        {
            var app = new DataNode("app", NodeType.App);
            var page1 = app.AddChild("page1", NodeType.Page);
            var element1 = page1.AddChild("element1", NodeType.Text);
            var page2 = app.AddChild("page2", NodeType.Page);
            var element2 = page2.AddChild("element2", NodeType.Text);

            var visits = Visit(app);

            Assert.AreEqual(visits[element1], new DataNodePath(page1.Id, element1.Id));
            Assert.AreEqual(visits[element2], new DataNodePath(page2.Id, element2.Id));
        }

        [TestMethod]
        public void VisitChildrenInTwoSecondPrimitiveRoots()
        {
            var app = new DataNode("app", NodeType.App);
            var page1 = app.AddChild("page1", NodeType.Page);
            var artboard1 = page1.AddChild("artboard1", NodeType.Artboard);
            var element1 = artboard1.AddChild("element1", NodeType.Text);

            var page2 = app.AddChild("page2", NodeType.Page);
            var artboard2 = page2.AddChild("artboard2", NodeType.Artboard);
            var element2 = artboard2.AddChild("element2", NodeType.Text);

            var visits = Visit(app);

            Assert.AreEqual(visits[element1], new DataNodePath(page1.Id, artboard1.Id, element1.Id));
            Assert.AreEqual(visits[element2], new DataNodePath(page2.Id, artboard2.Id, element2.Id));
        }

        [TestMethod]
        public void VisitGrandChildrenInTwoSecondPrimitiveRoots()
        {
            var app = new DataNode("app", NodeType.App);
            var page1 = app.AddChild("page1", NodeType.Page);
            var artboard1 = page1.AddChild("artboard1", NodeType.Artboard);
            var container1 = artboard1.AddChild("container1", NodeType.Container);
            var element1 = container1.AddChild("element1", NodeType.Text);

            var page2 = app.AddChild("page2", NodeType.Page);
            var artboard2 = page2.AddChild("artboard2", NodeType.Artboard);
            var container2 = artboard2.AddChild("container2", NodeType.Container);
            var element2 = container2.AddChild("element2", NodeType.Text);

            var visits = Visit(app);

            Assert.AreEqual(visits[element1], new DataNodePath(page1.Id, artboard1.Id, element1.Id));
            Assert.AreEqual(visits[element2], new DataNodePath(page2.Id, artboard2.Id, element2.Id));
        }

        private Dictionary<DataNode, DataNodePath> Visit(DataNode node)
        {
            var visitor = new PathRecorder();
            node.Visit(visitor);
            return visitor.Visits;
        }

        private class PathRecorder : DataTreeVisitor
        {
            public Dictionary<DataNode, DataNodePath> Visits { get; } = new Dictionary<DataNode, DataNodePath>();

            public override bool Visit(DataNode element, DataNodePath path)
            {
                Visits.Add(element, path.Clone());
                return true;
            }
        }
    }
}