using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carbon.Business.Domain.DataTree;
using Newtonsoft.Json.Linq;

namespace Carbon.Business.Sync
{
    public class PrimitiveHelper
    {
        private readonly DataNode _page;
        public PrimitiveHelper(DataNode page)
        {
            this._page = page;
        }

        public void Visit(Func<DataNode, DataNode, bool> visitorAction)
        {
            var elements = new Queue<DataNode>();
            elements.Enqueue(this._page);

            while (elements.Any())
            {
                var current = elements.Dequeue();               
                var children = current.Children;
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        if (!visitorAction(child, current))
                        {
                            return;
                        }
                        if (child.Children != null)
                        {
                            elements.Enqueue(child);
                        }
                    }
                }
            }
        }


        public static DataNode ElementById(DataNode page, string id)
        {
            DataNode res = null;

            new PrimitiveHelper(page).Visit((e, p) =>
                                            {
                                                if(e.Id == id)
                                                {
                                                    res = e;
                                                    return false;
                                                }

                                                return true;
                                            });            

            return res;
        }

        public static void InsertElement(JObject container, JToken element, int position)
        {
            var children = container.Value<JArray>("children") ?? new JArray();
            if(position < 0)
            {
                position = 0;
            }

            if (position < children.Count)
            {
                children[position].AddBeforeSelf(element);
            }
            else
            {
                children.Add(element);
                container["children"] = children;
            }
        }

        public static void MoveElement(DataNode element, DataNode newElement, JObject page,  string parentId, string pageId, int order)
        {
            //var currentParent = element.Parent.Parent.Parent;
            //if (currentParent is JArray)
            //{
            //    return;
            //}
            //if (parentId == pageId.ToString())
            //{
            //    element.Remove();
            //    InsertElement((JObject) page, newElement, order);
            //}
            //else if (currentParent.Value<string>("id") != parentId)
            //{
            //    element.Remove();
            //    var newParent = ElementById(page, parentId);
            //    if (newParent != null)
            //    {
            //        InsertElement((JObject)newParent, newElement, order);
            //    }
            //}
            //else
            //{
            //    var children = currentParent["children"].Value<JArray>();
            //    var currentPosition = children.IndexOf(element);
            //    element.Remove();
            //    InsertElement((JObject)currentParent, newElement, order);
            //}
        }
    }
}
