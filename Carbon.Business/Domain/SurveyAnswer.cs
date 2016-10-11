using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{
    public partial class SurveyAnswer : DomainObject
    {
        public virtual Guid SurveyId { get; set; }
        public virtual User User { get; set; }
        public virtual int QuestionId { get; set; }
        public virtual int? NumberValue { get; set; }
        public virtual string TextValue { get; set; }
    }   
}
