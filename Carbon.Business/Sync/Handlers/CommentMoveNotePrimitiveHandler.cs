using System;
using Carbon.Business.Domain;
using Newtonsoft.Json.Linq;

namespace Carbon.Business.Sync.Handlers
{
    //[PrimitiveHandler("comment_move_note")]
    class CommentMoveNotePrimitiveHandler : CommentBasePrimitiveHandler
    {
        public override void Apply(Primitive primitive, ProjectModel projectModel, PrimitiveContext context)
        {
            //var commentId = Guid.Parse(json["id"].Value<string>("uid"));
           
            //var data = json["data"];
            //var x = data.Value<int>("pageX");
            //var y = data.Value<int>("pageY");
            //var repository = context.UnitOfWork.Repository<Comment>();
            //var comment = repository.FindSingleBy(Comment.FindByProjectAndCommentIdSpec(projectModel.Project.Id, commentId));
            //if (comment == null)
            //{
            //    return;
            //}

            //comment.PageX = x;
            //comment.PageY = y;

            //repository.Update(comment); 
        }
    }
}
