using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Business;
using Carbon.Data.Azure.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Test.Unit.Data
{
    [TestClass]
    public class FatEntitySlicingTests
    {
        [TestMethod]
        public void OneChar()
        {
            Validate("A");
        }

        [TestMethod]
        public void TwoSmallBuffers()
        {
            Validate("A", "B");
        }        

        [TestMethod]
        public void OneBufferTheSizeOfProperty()
        {
            var size = FatEntity.GetAvailablePropertySize();
            var buffer = RandomBuffer(size/4);
            Validate(buffer);
        }

        [TestMethod]
        public void OneBufferTheSizeOfPropertyPlusONe()
        {
            var size = FatEntity.GetAvailablePropertySize();
            var buffer = RandomBuffer(size/4 + 1);
            Validate(buffer);
        }

        [TestMethod]
        public void TwoBuffersTheSizeOfProperty()
        {
            var size = FatEntity.GetAvailablePropertySize();
            var buffer1 = RandomBuffer((int)Math.Floor(size/8d));
            var buffer2 = RandomBuffer((int)Math.Ceiling(size/8d));
            Validate(buffer1, buffer2);
        }

        [TestMethod]
        public void TwoBuffersTheSizeOfPropertyPlusOne()
        {
            var size = FatEntity.GetAvailablePropertySize();
            var buffer1 = RandomBuffer((int)Math.Floor(size / 8d));
            var buffer2 = RandomBuffer((int)Math.Ceiling(size / 8d) + 1);
            Validate(buffer1, buffer2);
        }

        [TestMethod]
        public void ManyBuffersAsProperties()
        {
            var buffers = new List<string>();
            var properties = FatEntity.MaxProperties - 1;
            for (var i = 0; i < properties; i++)
            {
                var buffer = RandomBuffer(10);
                buffers.Add(buffer);
            }
            Validate(buffers.ToArray());
        }

        [TestMethod]
        public void ManyBuffersAsPropertiesPlusOne()
        {
            var buffers = new List<string>();
            var properties = FatEntity.MaxProperties + 1;
            for (var i = 0; i < properties; i++)
            {
                var buffer = RandomBuffer(10);
                buffers.Add(buffer);
            }
            Validate(buffers.ToArray());
        }

        [TestMethod]
        public void OneHugeBuffer()
        {
            var buffer = RandomBuffer(2*1024*1024);
            Validate(buffer);
        }

        [TestMethod]
        public void TwoHugeBuffers()
        {
            var buffer1 = RandomBuffer(2 * 1024 * 1024);
            var buffer2 = RandomBuffer(4 * 1024 * 1024);
            Validate(buffer1, buffer2);
        }

        private void Validate(params string[] buffers)
        {            
            var pipe = new Pipe { Buffers = buffers.ToList() };
            var entities = FatEntityRepository<Pipe>.SliceObject(pipe).Select(x => x.WrappedEntity).ToList();
            var newPipe = FatEntityRepository<Pipe>.CombineFatEntities(entities, "P", "R");

            Assert.AreEqual(pipe.Buffers.Count, newPipe.Buffers.Count);
            for (var i = 0; i < pipe.Buffers.Count; i++)
            {
                var oldBuffer = pipe.Buffers[i];
                var newBuffer = newPipe.Buffers[i];
                Assert.AreEqual(oldBuffer.Length, newBuffer.Length);

                for (var j = 0; j < oldBuffer.Length; j++)
                {
                    Assert.AreEqual(oldBuffer[j], newBuffer[j]);
                }
            }
        }

        private string RandomBuffer(int size)
        {
            var random = new Random();
            var builder = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(random.Next(0x4e00, 0x4f80));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        private class Pipe : TableEntity, IPipe<string>
        {
            public List<string> Buffers { get; set; }

            public Pipe()
            {
                PartitionKey = "Partition";
            }

            public IEnumerable<string> Write()
            {
                return Buffers;
            }

            public void Read(IEnumerable<string> buffers)
            {
                Buffers = buffers.ToList();
            }

            public void GetStatistics(out int count, out int max, out int total)
            {
                count = Buffers.Count;
                max = Buffers.Max(x => x.Length);
                total = Buffers.Sum(x => x.Length);
            }
        }
    }
}
