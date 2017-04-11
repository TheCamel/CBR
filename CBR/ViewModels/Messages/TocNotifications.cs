using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBR.ViewModels.Messages
{
    internal class TocNaviguateNotification : NotificationMessage<object>
    {
        public TocNaviguateNotification(object content) : base(content, ViewModelMessages.TocNaviguateTo)
        { }

        public TocNaviguateNotification(object sender, object content) : base(sender, content, ViewModelMessages.TocNaviguateTo)
        { }

        public TocNaviguateNotification(object sender, object target, object content)
            : base(sender, target, content, ViewModelMessages.TocNaviguateTo)
        { }
    }

    internal class TocChangedNotification : NotificationMessage<object>
    {
        public TocChangedNotification(object content) : base(content, ViewModelMessages.TocContentChanged)
        { }

        public TocChangedNotification(object sender, object content) : base(sender, content, ViewModelMessages.TocContentChanged)
        { }

        public TocChangedNotification(object sender, object target, object content)
            : base(sender, target, content, ViewModelMessages.TocContentChanged)
        { }
    }
}
