using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Windows;

namespace projektlabor.noah.planmeldung.utils
{
    public static class LibraryExtensions
    {
        /// <summary>
        /// Method from: https://stackoverflow.com/a/12618521
        /// Removes all event handlers subscribed to the specified routed event from the specified element.
        /// </summary>
        /// <param name="element">The UI element on which the routed event is defined.</param>
        /// <param name="routedEvent">The routed event for which to remove the event handlers.</param>
        public static void RemoveRoutedEventHandlers(this UIElement element, RoutedEvent routedEvent)
        {
            // Get the EventHandlersStore instance which holds event handlers for the specified element.
            // The EventHandlersStore class is declared as internal.
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
                "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

            // If no event handlers are subscribed, eventHandlersStore will be null.
            if (eventHandlersStore == null)
                return;

            // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
            // for getting an array of the subscribed event handlers.
            var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
                "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
                eventHandlersStore, new object[] { routedEvent });

            // Iteratively remove all routed event handlers from the element.
            foreach (var routedEventHandler in routedEventHandlers)
                element.RemoveHandler(routedEvent, routedEventHandler.Handler);
        }

        /// <summary>
        /// Converts the string to a stream
        /// </summary>
        /// <param name="text">The text to convert</param>
        public static Stream ToStream(this string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Searches for a specific attribute of a field on a class
        /// </summary>
        /// <param name="value">The enum that shall be searched</param>
        /// <param name="verifier">A verifier function that can be set optionally. The first attribute that matches will be choosen.</param>
        /// <returns>Null if no corresponding attribute got found, otherwise the first matching attribute</returns>
        public static ATTR GetAttribute<ATTR>(this Enum value, Func<ATTR, bool> verifier = null) where ATTR : Attribute
        {
            // Ensures that the verifier is set
            if (verifier == null)
                verifier = _ => true;

            // Gets the type of the enum
            Type valueType = value.GetType();
            Type typeATTR = typeof(ATTR);

            // Gets the member
            MemberInfo member = valueType.GetMember(value.ToString()).FirstOrDefault(m => m.DeclaringType == valueType);

            // Gets all attributes from the field
            return member.GetCustomAttributes(typeATTR, false)
                // Casts the values
                .Select(x=>(ATTR)x)
                // Filters the attribute that matches
                .FirstOrDefault(x => verifier((ATTR)x));
        }
    }
}
