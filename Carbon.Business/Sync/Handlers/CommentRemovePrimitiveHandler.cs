using System;
using Carbon.Business.Domain;
using Newtonsoft.Json.Linq;
using Carbon.Framework.Util;

namespace Carbon.Business.Sync.Handlers
{
    //[PrimitiveHandler("comment_remove")]
    public class CommentRemovePrimitiveHandler : CommentBasePrimitiveHandler
    {
        public override void Apply(Primitive primitive, ProjectModel projectModel, IDependencyContainer scope)
        {
            //var commentId = Guid.Parse(json["id"].Value<string>("uid"));

            //var data = json["data"];
            //var pageName = data.Value<string>("pageName");
            //var comment = context.UnitOfWork.Repository<Comment>().FindSingleBy(Comment.FindByProjectAndCommentIdSpec(projectModel.Project.Id, commentId));

            //if (comment != null)
            //{
            //    SendEmail("deleted", comment, pageName, new Uri(data.Value<string>("host")), context);
            //    var repository = context.UnitOfWork.Repository<Comment>();
            //    repository.Delete(comment);
            //    repository.DeleteAll(repository.FindAllBy(Comment.FindByParentIdSpec(comment.Uid)));
            //}

        }
    }
}
