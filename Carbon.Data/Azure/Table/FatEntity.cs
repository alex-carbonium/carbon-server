using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Data.Azure.Table
{
    public class FatEntity
    {
        private const int EnvelopOverhead = 1500;
        public const int MaxRowSize = 1024 * 1024 - EnvelopOverhead;
        public const int MaxProperties = 252;
        public const int MaxPropertySize = 64 * 1024;
        public const int MetataPropertySize = 4096;
        public static readonly int PropertyNameOverhead = CalculatePropertyNameOverhead("PXXX", EdmType.Binary);

        private int _payloadSize;
        private readonly DynamicTableEntity _entity;

        private int _spaceLeft;
        private int _filledProperties;

        private StringBuilder _metaTable;      
        private MemoryStream _currentStream;
        private int _propertySpaceLeft;

        public FatEntity(string partionKey, string rowKey)
        {            
            _entity = new DynamicTableEntity(partionKey, rowKey);

            _spaceLeft = MaxRowSize - MetataPropertySize;
            _filledProperties = 1;
            _payloadSize = EnvelopOverhead;
        }

        public DynamicTableEntity WrappedEntity 
        { 
            get { return _entity; }
        }

        public bool Fill(byte[] data, ref int position)
        {            
            while (_spaceLeft > 0 && position < data.Length && _filledProperties < MaxProperties)
            {
                if (_propertySpaceLeft <= 0)
                {
                    Flush(false);
                    
                    _currentStream = new MemoryStream(4096);

                    _propertySpaceLeft = GetAvailablePropertySize();
                    _spaceLeft -= PropertyNameOverhead;
                }

                var length = Math.Min(_propertySpaceLeft, data.Length - position);
                length = Math.Min(length, _spaceLeft);

                if (length > 0)
                {                    
                    _currentStream.Write(data, position, length);

                    if (position == 0)
                    {
                        WriteMetadata(data.Length);
                    }                    

                    position += length;
                    _payloadSize += length;
                    _spaceLeft -= length;
                    _propertySpaceLeft -= length;
                }
            }

            var usable = _spaceLeft > 0 && _filledProperties < MaxProperties;            
            return usable;
        }

        public void FillSimple(IEnumerable<string> strings)
        {            
            _metaTable = new StringBuilder("T");
            foreach (var data in strings)
            {
                var name = $"P{_filledProperties:D3}";
                var property = new EntityProperty(data);
                _entity.Properties.Add(name, property);
                ++_filledProperties;
                _payloadSize += data.Length*4;
            }
            Flush(true);
        }

        private void WriteMetadata(int bufferLength)
        {
            if (_metaTable == null)
            {
                _metaTable = new StringBuilder(256);
                _metaTable.Append("B");
            }
            if (_metaTable.Length > 0)
            {
                _metaTable.Append(",");
            }
            _metaTable.Append(bufferLength);
        }

        public void Flush(bool final)
        {
            if (_currentStream != null)
            {
                var name = $"P{_filledProperties:D3}";
                var property = new EntityProperty(_currentStream.ToArray());
                _entity.Properties.Add(name, property);
                ++_filledProperties;
                _currentStream = null;
            }
            if (final)
            {
                if (_metaTable != null)
                {
                    var metadata = _metaTable.ToString();
                    _entity.Properties.Add("M", new EntityProperty(metadata));
                    _payloadSize += metadata.Length*2;
                }
                _payloadSize += _filledProperties*PropertyNameOverhead;
            }
        }

        public static bool CanUseSimpleFormat(int count, int max, int total)
        {            
            return count <= MaxProperties - 1 
                && max * 4 <= MaxPropertySize - PropertyNameOverhead
                && total * 4 <= MaxRowSize - MetataPropertySize;
        }

        public static IEnumerable<string> GetPayload(IList<DynamicTableEntity> entities)
        {
            var first = entities[0];
            var metadata = first.Properties["M"].StringValue;
            if (metadata[0] == 'T')
            {
                return GetPayloadSimple(entities);
            }
            return GetPayloadComplex(entities);
        }

        public static IEnumerable<string> GetPayloadSimple(IList<DynamicTableEntity> entities)
        {
            var entity = entities[0];
            var properties = entity.Properties
                .Where(x => x.Key.StartsWith("P"))
                .OrderBy(x => x.Key)
                .Select(x => x.Value);

            foreach (var property in properties)
            {
                yield return property.StringValue;
            }
        }

        private static IEnumerable<string> GetPayloadComplex(IList<DynamicTableEntity> entities)
        {
            byte[] buffer = null;
            var sourcePos = 0;
            var propertyIndex = 0;
            var entityIndex = 0;
            List<EntityProperty> properties = null;
            DynamicTableEntity entity = null;
            var queue = new Queue<int>();
            EnqueueMetadata(queue, entities[0]);

            while (queue.Count > 0)
            {
                var length = queue.Dequeue();
                var result = new byte[length];
                var targetPos = 0;

                do
                {
                    if (buffer == null)
                    {
                        if (entity == null)
                        {
                            entity = entities[entityIndex++];
                            properties = entity.Properties
                                .Where(x => x.Key.StartsWith("P"))
                                .OrderBy(x => x.Key)
                                .Select(x => x.Value)
                                .ToList();
                            propertyIndex = 0;

                            if (entityIndex > 1)
                            {
                                EnqueueMetadata(queue, entity);
                            }
                        }
                        var property = properties[propertyIndex++];
                        buffer = property.BinaryValue;
                        sourcePos = 0;
                    }

                    var available = Math.Min(length, buffer.Length - sourcePos);
                    available = Math.Min(available, result.Length - targetPos);
                    Buffer.BlockCopy(buffer, sourcePos, result, targetPos, available);
                    targetPos += available;
                    sourcePos += available;

                    if (targetPos == length)
                    {
                        yield return Framework.Defs.Encoding.GetString(result);
                    }
                    else
                    {
                        buffer = null;
                        if (propertyIndex == properties.Count)
                        {
                            entity = null;
                        }
                    }
                } while (targetPos < length);
            }
        }

        private static void EnqueueMetadata(Queue<int> queue, DynamicTableEntity entity)
        {
            EntityProperty property;
            if (entity.Properties.TryGetValue("M", out property))
            {
                var metadata = property.StringValue;
                var lengths = metadata.Split(',').Skip(1).Select(int.Parse).ToList();
                foreach (var length in lengths)           
                {
                    queue.Enqueue(length);
                }
            }            
        }

        public static int GetAvailablePropertySize()
        {
            return MaxPropertySize - PropertyNameOverhead;
        }

        private static int CalculatePropertyNameOverhead(string name, EdmType type)
        {
            const int propertyOverhead = 8;
            int propertyNameSize = name.Length * 2;
            int propertyTypeSize;

            switch (type)
            {
                case EdmType.Binary:
                case EdmType.Int32:
                case EdmType.String:
                    propertyTypeSize = 4;
                    break;
                case EdmType.Boolean:
                    propertyTypeSize = 1;
                    break;
                case EdmType.DateTime:
                case EdmType.Double:
                case EdmType.Int64:
                    propertyTypeSize = 8;
                    break;
                case EdmType.Guid:
                    propertyTypeSize = 16;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return propertyOverhead + propertyNameSize + propertyTypeSize;
        }

        public int GetPayloadSize()
        {
            return _payloadSize + EnvelopOverhead;
        }
    }
}
