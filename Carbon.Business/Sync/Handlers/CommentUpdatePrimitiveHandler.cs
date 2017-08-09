using System;
using Carbon.Business.Domain;
using Newtonsoft.Json.Linq;
using Carbon.Framework.Util;

namespace Carbon.Business.Sync.Handlers
{
    //[PrimitiveHandler("comment_update")]
    class CommentUpdatePrimitiveHandler : CommentBasePrimitiveHandler
    {
        public override void Apply(Primitive primitive, ProjectModel projectModel, IDependencyContainer scope)
        {
            //var commentId = Guid.Parse(json["id"].Value<string>("uid"));

            //var data = json["data"];
            //var text = data.Value<string>("text");
            //var pageName = data.Value<string>("pageName");

            //var comment = context.UnitOfWork.Repository<Comment>().FindSingleBy(Comment.FindByProjectAndCommentIdSpec(projectModel.Project.Id, commentId));
            //if (comment != null)
            //{
            //    comment.Text = text;
            //    comment.DateTime = DateTime.Now.ToUniversalTime();
            //    context.UnitOfWork.Repository<Comment>().Update(comment);
            //    SendEmail("updated", comment, pageName, new Uri(data.Value<string>("host")), context);

            //    data["date"] = comment.DateTime;
            //}
        }
    }
}
