using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Business.Domain;

namespace Carbon.Business.Services
{
    public interface ICommentsService
    {
        IQueryable<Comment> GetComments(User user, long projectId);
        List<object> GetCommentsList(User user, long projectId);
    }
}
