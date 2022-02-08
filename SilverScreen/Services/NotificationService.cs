﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SilverScreen.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SilverScreen.Services
{
    public class NotificationService
    {
        private IConfiguration Configuration;
        public NotificationService(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public Notification[] GetAllNotificationsForUser(int userId)
        {
            SilverScreenContext context = new SilverScreenContext(Configuration);
            var notificationsRaw = context.Notifications
                .Where(x => x.UserId == userId)
                .Include(x => x.User)
                .Include(x => x.Author)
                .Include(x => x.Movie)
                .ToArray();

            List<Notification> notifications = new List<Notification>();

            foreach(var notification in notificationsRaw)
            {
                if(notification.MovieId != null)
                {
                    notifications.Add(new Notification
                    {
                        Id = notification.Id,
                        Active = notification.Active,
                        Author = new User
                        {
                            Id = notification.Author.Id,
                            Avatar = notification.Author.Avatar,
                            Username = notification.Author.Username
                        },
                        Type = notification.Type,
                        Content = notification.Content,
                        Movie = new Movie
                        {
                            Id = notification.Movie.Id,
                            Title = notification.Movie.Title
                        },
                    });
                }
                else
                {
                    notifications.Add(new Notification
                    {
                        Id = notification.Id,
                        Active = notification.Active,
                        Author = new User
                        {
                            Id = notification.Author.Id,
                            Avatar = notification.Author.Avatar,
                            Username = notification.Author.Username
                        },
                        Type = notification.Type,
                        Content = notification.Content
                    });
                }
                
            }

            context.Dispose();
            return notifications.ToArray();
        }

        //Response codes: Duplicate/Error(-1), OK(0)
        public int SendFriendNotification(int userId, int friendId, string message)
        {
            SilverScreenContext context = new SilverScreenContext(Configuration);
            //Check if they are already friends, but again not my part ;)

            //Check if similar notification already exists. Refuse the request if something like this happens
            if((context.Notifications.Where(x => x.UserId == userId && 
                                                 x.AuthorId == friendId).Any())
                ||
                (context.Notifications.Where(x => x.UserId == friendId &&
                                                  x.AuthorId == userId).Any()))
            {
                context.Dispose();
                return -1;
            }
            else
            {
                Notification friendRequest = new Notification()
                {
                    Active = true,
                    AuthorId = userId,
                    UserId = friendId,
                    Content = message,
                    Type = "FriendRequest"
                };
                context.Add(friendRequest);
                context.SaveChanges();
                context.Dispose();
                return 0;
            }
        }

        public int SetFilmReleaseNotification(int userId, int movieID, bool status)
        {
            SilverScreenContext context = new SilverScreenContext(Configuration);
            if (status)
            {
                if (context.MovieNotifications.Where(x => x.UserId == userId && x.MovieId == movieID).Any())
                {
                    context.Remove(context.MovieNotifications.Where(x => x.UserId == userId && x.MovieId == movieID).FirstOrDefault());
                    context.SaveChanges();
                    context.Dispose();
                    return 0;
                }
                context.Dispose();
                return 404;
            }
            else
            {
                if (!context.MovieNotifications.Where(x => x.UserId == userId && x.MovieId == movieID).Any())
                {
                    MovieNotification movieNotification = new MovieNotification()
                    {
                        Date = DateTime.UtcNow.AddDays(10), //Replace with movie's release date
                        MovieId = movieID,
                        UserId = userId
                    };
                    context.Add(movieNotification);
                    context.SaveChanges();
                    context.Dispose();
                    return 0;
                }
                else
                {
                    context.Dispose();
                    return -1;
                }
            }
        }

        public int RespondToFriendRequest(int notificationId)
        {
            SilverScreenContext context = new SilverScreenContext(Configuration);
            var friendRequest = context.Notifications.Where(x => x.Id == notificationId).Include(x => x.User);
            if(friendRequest.Any())
            {
                //Connect to user service and add record with the two users (cant do that because its not my job)
                Notification newNotification = new Notification()
                {
                    Type = "TextOnly",
                    Content = friendRequest.FirstOrDefault().User.Username + " accepted your friend request.",
                    AuthorId = friendRequest.FirstOrDefault().UserId,
                    UserId = friendRequest.FirstOrDefault().AuthorId,
                    Active = true
                };
                context.Add(newNotification);                
                context.Remove(friendRequest.FirstOrDefault());
                context.SaveChanges();
                context.Dispose();
                return 0;
            }
            context.Dispose();
            return -1;
        }

        public int ToggleNotificationActivity(int notificationId)
        {
            SilverScreenContext context = new SilverScreenContext(Configuration);
            var notification = context.Notifications.Find(notificationId);
            if(notification != null)
            {
                if(notification.Active.Value)
                {
                    notification.Active = false;
                }
                else
                {
                    notification.Active = true;
                }
                context.SaveChanges();
                context.Dispose();
                return 0;
            }
            context.Dispose();
            return -1;
        }

        public int DeleteNotification(int notificationId)
        {
            SilverScreenContext context = new SilverScreenContext(Configuration);
            var notification = context.Notifications.Find(notificationId);
            if (notification != null)
            {
                context.Remove(notification);
                context.SaveChanges();
                context.Dispose();
                return 0;
            }
            context.Dispose();
            return -1;
        }
    }
}
