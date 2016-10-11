using System;
using Carbon.Business.Domain;
using Newtonsoft.Json.Linq;

namespace Carbon.Business.Sync.Handlers
{
    //[PrimitiveHandler("comment_change_status")]
    public class CommentChangeStatusPrimitiveHandler : CommentBasePrimitiveHandler
    {
        public override void Apply(Primitive primitive, ProjectModel projectModel, PrimitiveContext context)
        {
            //var commentId = Guid.Parse(json["id"].Value<string>("uid"));
            
            //var data = json["data"];            
            //var newStatus = (CommentStatus)data.Value<int>("status");
            //var pageName = data.Value<string>("pageName");

            //var comment =
            //    context.UnitOfWork.Repository<Comment>()
            //        .FindSingleBy(Comment.FindByProjectAndCommentIdSpec(projectModel.Project.Id, commentId));

            //if (comment == null || comment.Status == newStatus)
            //{
            //    return;
            //}

            //Comment commentStatus = ChangeStatus(comment, newStatus, context);

            //var action = comment.Status == CommentStatus.Resolved ? "resolved" : "re-opened";
            //SendEmail(action, comment, pageName, new Uri(data.Value<string>("host")), context);

            //data["date"] = commentStatus.DateTime;

            //var newPrimitive = new
            //{
            //    id = new
            //    {
            //        command = "comment_add",
            //        uid = commentStatus.Uid,
            //    },
            //    data = new
            //    {
            //        date = (commentStatus.DateTime),
            //        parentId = commentStatus.ParentUid,
            //        userName = commentStatus.User.FriendlyName,
            //        text = commentStatus.Text,
            //        status = comment.Status,
            //        type = 1 //0 - normal, 1 - cannot edit
            //    }
            //};

            //context.AddExtraPrimitive(JObject.FromObject(newPrimitive));
        }
    }
}
