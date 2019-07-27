using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml;

namespace SecuroteckWebApplication.Models
{
    public class User
    {
        //Task2
        // TODO: Create a User Class for use with Entity Framework
        // Note that you can use the [key] attribute to set your ApiKey Guid as the primary key 
        [Key]
        public string ApiKey { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public virtual ICollection<Log> Logs { get; set; }
        public User() { }
    }

    #region Task13?
    public class Log
    {
        [Key]
        public string LogKey { get; set; }
        public string LogString { get; set; }
        public string LogDateTime { get; set; }
        public Log() { }
    }
    public class ArchivedLog
    {
        [Key]
        public string LogKey { get; set; }
        public string LogString { get; set; }
        public string UserKey { get; set; }
        public string DateTime { get; set; }
        public ArchivedLog() { }
    }
    #endregion

    public class UserDatabaseAccess
    {
        //Task3 
        /// <summary>
        /// Creates a new user in the database
        /// </summary>
        /// <param name="username"></param>
        /// <returns>The assigned ID to user</returns>
        public string CreateUser(string username)
        {
            Guid id = Guid.NewGuid();
            string role = "User";
            using (var ctx = new UserContext())
            {

                if (ctx.Users.FirstOrDefault() == null)
                {
                    role = "Admin";
                }
                User user = new User()
                {
                    ApiKey = id.ToString(),
                    UserName = username,
                    Role = role
                };
                ctx.Users.Add(user);
                ctx.SaveChanges();
                ctx.Dispose();
            }
            return id.ToString();
        }

        public void CreateLog(string APIKey, string action)
        {
            using(var ctx = new UserContext())
            {
                Guid id = Guid.NewGuid();
                Log log = new Log()
                {
                    LogKey = id.ToString(),
                    LogString = "User requested " + action,
                    LogDateTime = DateTime.Now.ToString()
                };
                User u = ctx.Users.FirstOrDefault(U => U.ApiKey == APIKey);
                u.Logs.Add(log);
                ctx.Logs.Add(log);
                ctx.SaveChanges();
            }
        }
        /// <summary>
        /// Finds a user based on their APIKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool FindKey(string key)
        {
            bool found = false;
            using (var ctx = new UserContext())
            {
                User user = ctx.Users.FirstOrDefault(U => U.ApiKey == key);
                if (user != null)
                {
                    found = true;
                }
                ctx.Dispose();
            }
            return found;
        }
        public bool FindUser(string username)
        {
            bool found = false;
            using (var ctx = new UserContext())
            {
                User user = ctx.Users.FirstOrDefault(U => U.UserName == username);
                if (user != null)
                {
                    found = true;
                }
                ctx.Dispose();
            }
            return found;
        }
        /// <summary>
        /// Finds a user based on APIKey and username
        /// </summary>
        /// <param name="key"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool FindUserandkey(string key, string username)
        {
            bool found = false;
            using (var ctx = new UserContext())
            {
                User user = ctx.Users.FirstOrDefault(U => U.ApiKey == key);
                if (user != null && user.UserName == username)
                {
                    found = true;
                }
                ctx.Dispose();
            }
            return found;
        }
        /// <summary>
        /// Returns the user instance related to the given APIKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public User ReturnUser(string key)
        {
            using (var ctx = new UserContext())
            {
                User user = ctx.Users.FirstOrDefault(U => U.ApiKey == key);
                if (user != null)
                {
                    ctx.Dispose();
                    return user;
                }
                else
                {
                    ctx.Dispose();
                    return null;
                }

            }
        }
        /// <summary>
        /// Changes the username of a user with the given APIKey
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public string ChangeUsername(string key, string newName)
        {
            string response = "User not found";
            using (var ctx = new UserContext())
            {
                User user = ctx.Users.FirstOrDefault(U => U.ApiKey == key);
                if (user != null)
                {
                    response = "user " + user.UserName + " changed to " + newName;
                    user.UserName = newName;
                    ctx.SaveChanges();
                }
                ctx.Dispose();
            }
            return response;
        }
        /// <summary>
        /// Deletes the user with a given APIKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string DeleteUser(string username)
        {
            string response = "User not found";
            using (var ctx = new UserContext())
            {
                User user = ctx.Users.FirstOrDefault(U => U.UserName == username);
                if (user != null)
                {
                    response = "User " + user.UserName + " has been deleted";
                    foreach(Log log in user.Logs)
                    {
                        ArchivedLog aLog = new ArchivedLog()
                        {
                            LogKey = log.LogKey,
                            LogString = log.LogString,
                            UserKey = user.ApiKey,
                            DateTime = log.LogDateTime
                        };
                        ctx.ArchivedLogs.Add(aLog);
                    }
                    ctx.Logs.RemoveRange(user.Logs);
                    ctx.Users.Remove(user);
                    ctx.SaveChanges();
                }
                ctx.Dispose();
            }
            return response;
        }

        public bool ChangeRole(string username, string role)
        {
            using (var ctx = new UserContext())
            {
                User user = ctx.Users.FirstOrDefault(User => User.UserName == username);
                if (user != null)
                {

                    user.Role = role;
                    ctx.SaveChanges();
                    return true;


                }
                else
                {
                    return false;
                }
            }
        }

    }


}