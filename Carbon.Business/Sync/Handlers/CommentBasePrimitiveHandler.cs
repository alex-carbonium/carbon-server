using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Business.Domain;
using Carbon.Business.Jobs;
using Carbon.Business.Services;
using Carbon.Framework.JobScheduling;
using Carbon.Framework.Extensions;
using Carbon.Framework.UnitOfWork;

namespace Carbon.Business.Sync.Handlers
{
    public abstract class CommentBasePrimitiveHandler : PrimitiveHandler
    {
        public CommentBasePrimitiveHandler()
        {                        
        }

        protected Comment ChangeStatus(Comment comment, CommentStatus newStatus, PrimitiveContext context)
        {
            var project = comment.Project;

            comment.Status = newStatus;
            var commentStatus = new Comment
            {
                CommentType = CommentType.Status,
                ParentUid = comment.Uid,
                DateTime = DateTime.Now.ToUniversalTime(),
                Project = project,
                //User = context.User,
                Uid = Guid.NewGuid(),
                Text = newStatus == CommentStatus.Resolved ? "Marked as resolved" : "Re-opened", //TODO: add translation
                PageId = comment.PageId
            };
            context.UnitOfWork.Repository<Comment>().Update(comment);
            context.UnitOfWork.Repository<Comment>().Insert(commentStatus);

            return commentStatus;
        }

        protected IEnumerable<User> FindEmailRecipients(Project project, Comment comment, List<Comment> thread, PrimitiveContext context)
        {
            var users = new List<Tuple<User, Permission>>();
            //var userService = context.Scope.Resolve<IUserService>();
            //foreach (var acl in project.Acls)
            //{                
            //    var user = userService.FindUserBySID(acl.SID);
            //    if (!user.IsGuest && !string.IsNullOrEmpty(user.Email) && !user.Email.Contains("@carbonium.io"))
            //    {
            //        users.Add(new Tuple<User, Permission>(user, acl.Permission));
            //    }
            //}

            //var usersToRemove = new List<User>();
            //usersToRemove.Add(comment.User);
            //            var reviewers = from u in users
            //                where (u.Item2 == PredefinedRoles.GuestRole || u.Item2 == PredefinedRoles.ReviewerRole)
            //                    && !thread.Any(t => t.User == u.Item1)
            //                select u.Item1;
            //usersToRemove.AddRange(reviewers);

            //foreach (var user in usersToRemove)
            //{
            //    var u = users.SingleOrDefault(x => x.Item1 == user);
            //    if (u != null)
            //    {
            //        users.Remove(u);
            //    }
            //}

            return users.Select(x => x.Item1);
        }     

        protected void SendEmail(string action, Comment comment, string pageName, Uri host, PrimitiveContext context)
        {
            Comment parentComment;
            if (comment.ParentUid == Guid.Empty)
            {
                parentComment = comment;
            }
            else
            {
                parentComment = context.UnitOfWork.FindAll<Comment>().Single(x => x.Uid == comment.ParentUid);
            }

            var childComments = context.UnitOfWork.FindAll<Comment>()
                .Where(x => x.Project == parentComment.Project && x.ParentUid == parentComment.Uid)
                .OrderBy(x => x.DateTime)
                .ToList();

            var thread = new List<Comment>();
            thread.Add(parentComment);
            thread.AddRange(childComments);

            var recipients = FindEmailRecipients(comment.Project, comment, thread, context);
            if (!recipients.Any())
            {
                return;
            }

            var email = new Email();
            email.To = string.Join(",", recipients.Select(x => x.Email));

            var isDeleting = action == "deleted";
            var isDeletingAll = isDeleting && comment == parentComment;            

            email.TemplateName = "ProjectComment";
            
            email.Model = new
            {
                CommentAction = action,
                CommentAuthor = comment.User.FriendlyName,
                ProjectName = comment.Project.Name,

                ProjectLink = host.AddPath("/Designer/Workplace/").AddPath(comment.Project.Id.ToString()),
                PageLink = host.AddPath("/Designer/Workplace/").AddPath(comment.Project.Id.ToString()).AddPath(comment.PageId),
                PageName = pageName,
                TitleCommentText = parentComment.Text,
                Comments = thread.Select(x => new
                {
                    Author = x.User.FriendlyName,
                    DateTime = x.DateTime.ToLocalTime().ToString("HH:mm ddd, MMM dd"),
                    Text = x.Text,
                    IsCurrent = !isDeleting && x == comment,
                    IsDeleted = isDeleting && (x == comment || isDeletingAll)
                })
            };

            context.Scope.Resolve<IJobScheduler>().ScheduleImmediately<EmailJob>(email.ToString());
        }
    }
}
