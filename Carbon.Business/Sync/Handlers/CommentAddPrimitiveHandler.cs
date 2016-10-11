using System;
using System.Linq;
using Carbon.Business.Domain;
using Newtonsoft.Json.Linq;

namespace Carbon.Business.Sync.Handlers
{
    /*
     * return {
            id: {
                command: 'comment_add',
                id:id
            },
            data:{
                text:text,
                parentId:parentId,
                pageId:pageId,
                time:time,
                x:x,
                y:y,
                number:number
            }
     */
    //[PrimitiveHandler("comment_add")]
    class CommentAddPrimitiveHandler : CommentBasePrimitiveHandler
    {
        public override void Apply(Primitive primitive, ProjectModel projectModel, PrimitiveContext context)
        {
            //var idString = json["id"].Value<string>("uid");
            //var data = json["data"];
            //var text = data.Value<string>("text");
            //Guid parentUid = Guid.Empty;
            //Guid.TryParse(data.Value<string>("parentUid"), out parentUid);
            //var pageId = data.Value<string>("pageId");
            //var time = data.Value<long>("time");
            //var noteX = data.Value<int>("x");
            //var noteY = data.Value<int>("y");
            //var number = data.Value<int>("number");
            //var pageName = data.Value<string>("pageName");
            //JObject[] res = null;
            //Guid id = string.IsNullOrEmpty(idString) ? Guid.NewGuid() : Guid.Parse(idString);

            //var repository = context.UnitOfWork.Repository<Comment>();

            //if (repository.FindSingleBy(Comment.FindByProjectAndCommentIdSpec(projectModel.Project.Id, id)) != null)
            //{
            //    return;
            //}

            //if (parentUid == Guid.Empty)
            //{
            //    number = repository.FindAllBy(Comment.FindByProjectSpec(projectModel.Project.Id)).Max(x => x.Number) + 1 ?? 1;
            //}

            //if (parentUid != Guid.Empty)
            //{
            //    var parentComment = repository.FindSingleBy(Comment.FindByProjectAndCommentIdSpec(projectModel.Project.Id, parentUid));
            //    if (parentComment == null)
            //    {
            //        return;
            //    }
            //    if (parentComment.Status == CommentStatus.Resolved)
            //    {
            //        var commentStatus = ChangeStatus(parentComment, CommentStatus.Opened, context);

            //        var commentAdd = new
            //        {
            //            id = new
            //            {
            //                command = "comment_add",
            //                uid = commentStatus.Id,
            //            },
            //            data = new
            //            {
            //                date = (commentStatus.DateTime),
            //                parentId = commentStatus.ParentUid,
            //                userName = commentStatus.User.FriendlyName,
            //                text = commentStatus.Text,
            //                status = CommentStatus.Opened
            //            }
            //        };
            //        var commentChange = new
            //        {
            //            id = new
            //            {
            //                command = "comment_change_status",
            //                uid = parentComment.Uid,
            //            },
            //            data = new
            //            {                            
            //                status = CommentStatus.Opened
            //            }
            //        };

            //        res = new[] { JObject.FromObject(commentAdd), JObject.FromObject(commentChange) };
            //    }
            //}

            //var comment = new Comment
            //{
            //    Project = projectModel.Project,
            //    Uid = id,
            //    Text = text,
            //    User = context.User,
            //    ParentUid = parentUid,
            //    DateTime = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(time),
            //    PageId = pageId,
            //    PageX = noteX,
            //    PageY = noteY,
            //    Number = number
            //};
                
            //repository.Insert(comment);
            
            //json["id"]["newId"] = true;
            //data["number"] = number;
            //data["date"] = comment.DateTime;
            //data["userName"] = comment.User.FriendlyName;

            //SendEmail("added", comment, pageName, new Uri(data.Value<string>("host")), context);

            //if (res != null)
            //{
            //    foreach (var primitive in res)
            //    {
            //        context.AddExtraPrimitive(primitive);
            //    }
            //}            
        }   
    }
}
