using System;

namespace CBR.Core.Helpers
{
    /// <summary>
    /// This attribute allows a method to be targeted as a recipient for a message.
    /// It requires that the Type is registered with the MessageMediator through the
    /// <seealso cref="MessageMediator.Register"/> method
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// [MediatorMessageSinkAttribute("DoBackgroundCheck")]
    /// void OnBackgroundCheck(object parameter) { ... }
    /// 
    /// mediator.NotifyColleagues("DoBackgroundCheck", new CheckParameters());
    /// ...
    /// mediator.NotifyColleagues(new SomeDataClass(...));
    /// 
    /// ]]>
    /// </example>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MediatorMessageSinkAttribute : Attribute
    {
        /// <summary>
        /// Message key
        /// </summary>
        public object MessageKey { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MediatorMessageSinkAttribute()
        {
            MessageKey = null;
        }

        /// <summary>
        /// Constructor that takes a message key
        /// </summary>
        /// <param name="messageKey">Message Key</param>
        public MediatorMessageSinkAttribute(string messageKey)
        {
            MessageKey = messageKey;
        }
    }
}