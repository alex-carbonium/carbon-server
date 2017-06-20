using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Carbon.Business.Sync;
using Newtonsoft.Json;

namespace Carbon.Business.Domain.DataTree
{
    public class DataNode : JsonObject
    {
        private DataNode()
        {
        }
        public DataNode(string id, string type)
        {
            Props = new Dictionary<string, dynamic>();
            Type = type;
            Id = id;
        }

        protected override bool ReadProperty(string property, JsonReader reader)
        {
            switch (property)
            {
                case "t":
                    Type = ReadString(reader);
                    return true;
                case "props":
                    Props = ReadProps(reader);
                    return true;
                case "children":
                    Children = new List<DataNode>();

                    ReadAssert(reader, JsonToken.PropertyName);
                    ReadAssert(reader, JsonToken.StartArray);

                    while (reader.TokenType == JsonToken.StartObject)
                    {
                        var child = Create(reader);
                        Children.Add(child);
                    }

                    ReadAssert(reader, JsonToken.EndArray);

                    return true;
                default:
                    return false;
            }
        }

        protected override void WriteProperties(JsonWriter writer)
        {
            writer.WritePropertyName("t");
            writer.WriteValue(Type);

            writer.WritePropertyName("props");
            WriteProps(writer, Props);

            if (Children != null)
            {
                writer.WritePropertyName("children");
                writer.WriteStartArray();
                foreach (var child in Children)
                {
                    child.Write(writer);
                }
                writer.WriteEndArray();
            }
        }

        public string Id
        {
            get { return GetProp("id"); }
            set { SetProp("id", value); }
        }

        public string Type { get; set; }

        public Dictionary<string, dynamic> Props { get; set; }

        public IList<DataNode> Children { get; set; }

        public dynamic GetProp(string name)
        {
            dynamic value;
            Props.TryGetValue(name, out value);
            return value;
        }

        public DataNode SetProp(string name, dynamic value)
        {
            Props[name] = value;
            return this;
        }
        public void SetProps(IDictionary<string, dynamic> changes)
        {
            foreach (var change in changes)
            {
                Props[change.Key] = change.Value;
            }
        }

        public bool IsPrimitiveRoot()
        {
            return Type == NodeType.ArtboardPage
                || Type == NodeType.Artboard
                || Type == NodeType.StateBoard
                || Type == NodeType.Page
                || Type == NodeType.ArtboardTemplate;
        }

        public DataNode AddChild(string id, string type)
        {
            var node = new DataNode(id, type);
            EnsureChildren();
            Children.Add(node);
            return node;
        }

        public void InsertChild(DataNode node, int index)
        {
            EnsureChildren();
            if (index < 0)
            {
                index = 0;
            }
            else if (index > Children.Count)
            {
                index = Children.Count;
            }
            Children.Insert(index, node);
        }

        public void ReplaceChild(DataNode child)
        {
            if (Children == null)
            {
                return;
            }
            for (var i = 0; i < Children.Count; i++)
            {
                var node = Children[i];
                if (node.Id == child.Id)
                {
                    Children.RemoveAt(i);
                    Children.Insert(i, child);
                    break;
                }
            }
        }

        public void ChangeChildPosition(string childId, int newPosition)
        {
            var child = Children?.SingleOrDefault(x => x.Id == childId);
            if (child != null)
            {
                if (Children.Count == 1)
                {
                    return;
                }

                Children.Remove(child);
                newPosition = System.Math.Min(newPosition, Children.Count - 1);                
                Children.Insert(newPosition, child);
            }
        }

        public void RemoveChild(string id)
        {
            var child = Children?.SingleOrDefault(x => x.Id == id);
            if (child != null)
            {
                Children.Remove(child);
            }
        }

        public void PatchProps(PatchType patchType, string propName, dynamic item)
        {
            var array = GetProp(propName);
            var i = 0;
            switch (patchType)
            {
                case PatchType.Insert:
                    if (array == null)
                    {
                        array = new ArrayList();
                        SetProp(propName, array);
                    }
                    array.Add(item);
                    break;
                case PatchType.Remove:
                    foreach (var arrayItem in array)
                    {
                        if (arrayItem.id == item.id)
                        {
                            array.RemoveAt(i);
                            break;
                        }
                        ++i;
                    }
                    break;
                case PatchType.Change:
                    if (array == null)
                    {
                        array = new ArrayList();
                        SetProp(propName, array);
                        array.Add(item);
                    }
                    else
                    {
                        foreach (var arrayItem in array)
                        {
                            if (item.id == arrayItem.id)
                            {
                                array[i] = item;
                                break;
                            }
                            ++i;
                        }
                    }
                    break;
            }
        }

        private void EnsureChildren()
        {
            if (Children == null)
            {
                Children = new List<DataNode>();
            }
        }

        public static DataNode Create(string json)
        {
            var node = new DataNode();
            node.Read(json);
            return node;
        }

        public static DataNode Create(JsonReader reader)
        {
            var node = new DataNode();
            node.Read(reader);
            return node;
        }

        public static Dictionary<string, dynamic> ReadProps(JsonReader reader)
        {
            reader.Read();
            var result = Serializer.Deserialize<Dictionary<string, dynamic>>(reader);
            reader.Read();
            return result;
        }

        public static void WriteProps(JsonWriter writer, Dictionary<string, dynamic> props)
        {
            Serializer.Serialize(writer, props);
        }

        public void Visit(DataTreeVisitor visitor)
        {
            Visit(visitor, new DataNodePath(), 0);
        }

        protected bool Visit(DataTreeVisitor visitor, DataNodePath path, int level)
        {
            var isRoot = IsPrimitiveRoot();
            if (isRoot)
            {
                path[level] = Id;
            }
            if (!visitor.Visit(this, path))
            {
                return false;
            }
            if (isRoot)
            {
                path[level] = string.Empty;
            }

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    path[level] = child.Id;
                    var nextLevel = child.IsPrimitiveRoot() ? level + 1 : level;
                    if (!child.Visit(visitor, path, nextLevel))
                    {
                        return false;
                    }
                    path[level] = string.Empty;
                }
            }
            return true;
        }
    }
}