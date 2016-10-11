using System.Collections.Generic;
using System.Linq;
using Carbon.Business.Domain;
using Carbon.Business.Domain.DataTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Carbon.Test.Unit.Business
{
    [TestClass]
    public class ProjectModelStatisticsTests
    {
        private FontManager _fontManager;

        [TestInitialize]
        public void Setup()
        {
            _fontManager = new TestFontManager();
        }

        [TestMethod]
        public void FamilyUsedOnText()
        {            
            //arrange            
            var model = new ProjectModel();
            model.AddChild("page1", NodeType.Page)
                .AddChild("text1", NodeType.Text)
                .SetProp("font", JObject.Parse("{family: 'Arial'}"));

            //act
            var statistics = Visit(model);

            //assert
            var usage = statistics.FontUsage;
            Assert.AreEqual("Arial", usage.Single().Family);
        }

        [TestMethod]
        public void FamilyUsedOnTextTwice()
        {
            //arrange            
            var model = new ProjectModel();
            var page = model.AddChild("page1", NodeType.Page);
            page.AddChild("text1", NodeType.Text).SetProp("font", JObject.Parse("{family: 'Arial'}"));
            page.AddChild("text2", NodeType.Text).SetProp("font", JObject.Parse("{family: 'Arial'}"));

            //act
            var statistics = Visit(model);

            //assert
            var usage = statistics.FontUsage;
            Assert.AreEqual("Arial", usage.Single().Family);
        }

        [TestMethod]
        public void FamilyUsedOnTextTwiceOnceWithOtherWeight()
        {
            //arrange            
            var model = new ProjectModel();
            var page = model.AddChild("page1", NodeType.Page);
            page.AddChild("text1", NodeType.Text).SetProp("font", JObject.Parse("{family: 'Arial', weight: 400}"));
            page.AddChild("text2", NodeType.Text).SetProp("font", JObject.Parse("{family: 'Arial', weight: 600}"));

            //act
            var statistics = Visit(model);

            //assert            
            CollectionAssert.AreEquivalent(
                new List<FontUsage>
                {
                    new FontUsage { Family = "Arial", Weight = 400},
                    new FontUsage { Family = "Arial", Weight = 600}
                },
                statistics.FontUsage);            
        }

        [TestMethod]
        public void StyleUsedOnText()
        {
            //arrange            
            var model = new ProjectModel();
            model.AddChild("page1", NodeType.Page)
                .AddChild("text1", NodeType.Text)
                .SetProp("font", JObject.Parse("{style: 2}"));

            //act
            var statistics = Visit(model);

            //assert
            var usage = statistics.FontUsage;
            Assert.AreEqual(2, usage.Single().Style);
        }

        [TestMethod]
        public void WeightUsedOnText()
        {
            //arrange            
            var model = new ProjectModel();
            model.AddChild("page1", NodeType.Page)
                .AddChild("text1", NodeType.Text)
                .SetProp("font", JObject.Parse("{weight: 600}"));

            //act
            var statistics = Visit(model);

            //assert
            var usage = statistics.FontUsage;
            Assert.AreEqual(600, usage.Single().Weight);
        }

        [TestMethod]
        public void WeightUsedOnTextContentAndText()
        {
            //arrange            
            var model = new ProjectModel();
            model.AddChild("page1", NodeType.Page)
                .AddChild("text1", NodeType.Text)
                .SetProp("font", JObject.Parse("{weight: 600}"))
                .SetProp("content", JArray.Parse("[{text: 'hello', weight: 800}]"));

            //act
            var statistics = Visit(model);

            //assert            
            CollectionAssert.AreEquivalent(
                new List<FontUsage>
                {
                    new FontUsage { Weight = 600},
                    new FontUsage { Weight = 800}
                },
                statistics.FontUsage);
        }

        [TestMethod]
        public void WeightUsedTwiceOnContent()
        {
            //arrange            
            var model = new ProjectModel();
            model.AddChild("page1", NodeType.Page)
                .AddChild("text1", NodeType.Text)                
                .SetProp("content", JArray.Parse("[{text: 'hello', weight: 800}, {text: 'world', weight: 900}]"));

            //act
            var statistics = Visit(model);

            //assert            
            CollectionAssert.AreEquivalent(
                new List<FontUsage>
                {
                    new FontUsage { Weight = 800},
                    new FontUsage { Weight = 900}
                },
                statistics.FontUsage);
        }

        [TestMethod]
        public void StringContent()
        {
            //arrange            
            var model = new ProjectModel();
            model.AddChild("page1", NodeType.Page)
                .AddChild("text1", NodeType.Text)
                .SetProp("content", "hello");

            //act
            var statistics = Visit(model);

            //assert            
            Assert.AreEqual(0, statistics.FontUsage.Count);
        }

        [TestMethod]
        public void FontMetadataMustBeFetched()
        {
            //arrange            
            var model = new ProjectModel();
            model.AddChild("page1", NodeType.Page)
                .AddChild("text1", NodeType.Text)                
                .SetProp("content", JArray.Parse("[{text: 'hello', family: 'Blokk'}]"));

            //act
            var statistics = Visit(model);

            //assert                       
            Assert.AreEqual("Blokk", statistics.FontMetadata.Single()["name"]);
        }

        [TestMethod]
        public void FontMetadataMustNotBeFetchedTwice()
        {
            //arrange            
            var model = new ProjectModel();
            model.AddChild("page1", NodeType.Page)
                .AddChild("text1", NodeType.Text)
                .SetProp("content", JArray.Parse("[{text: 'hello', family: 'Blokk'}, {family: 'Blokk'}]"));

            //act
            var statistics = Visit(model);

            //assert                       
            Assert.AreEqual("Blokk", statistics.FontMetadata.Single()["name"]);
        }

        private ProjectModelStatistics Visit(ProjectModel model)
        {            
            var visitor = new ProjectModelStatisticsVisitor(_fontManager);
            model.Visit(visitor);
            return visitor.Statistics;
        }
    }
}