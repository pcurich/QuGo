﻿using QuGo.Core;
using QuGo.Core.Domain.Messages;
using QuGo.Services.Events;

namespace QuGo.Services.Messages
{
    public static class EventPublisherExtensions
    {
        /// <summary>
        /// Publishes the newsletter subscribe event.
        /// </summary>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="subscription">The newsletter subscription.</param>
        public static void PublishNewsletterSubscribe(this IEventPublisher eventPublisher, NewsLetterSubscription subscription)
        {
            eventPublisher.Publish(new EmailSubscribedEvent(subscription));
        }

        /// <summary>
        /// Publishes the newsletter unsubscribe event.
        /// </summary>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="subscription">The newsletter subscription.</param>
        public static void PublishNewsletterUnsubscribe(this IEventPublisher eventPublisher, NewsLetterSubscription subscription)
        {
            eventPublisher.Publish(new EmailUnsubscribedEvent(subscription));
        }

        public static void EntityTokensAdded<T, U>(this IEventPublisher eventPublisher, T entity, System.Collections.Generic.IList<U> tokens) where T : BaseEntity
        {
            eventPublisher.Publish(new EntityTokensAddedEvent<T, U>(entity, tokens));
        }

        public static void MessageTokensAdded<U>(this IEventPublisher eventPublisher, MessageTemplate message, System.Collections.Generic.IList<U> tokens)
        {
            eventPublisher.Publish(new MessageTokensAddedEvent<U>(message, tokens));
        }
    }
}