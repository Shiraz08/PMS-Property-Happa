﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Shared.Enum
{
    public static class SD
    {
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
        public static string AccessToken = "JWTToken";
        public static string RefreshToken = "RefreshToken";
        public static string CurrentAPIVersion = "v2";
        public const string Admin = "admin";
        public const string User = "user";
        public const string Customer = "customer";
        public enum ContentType
        {
            Json,
            MultipartFormData,
        }
        public enum SubscriptionTypes
        {
            Free
        }

        public static class TaskTypes
        {
            public const string Task = "Task";
            public const string TenantRequest = "TenantRequest";
            public const string OwnerRequest = "OwnerRequest";
            public const string WorkOrderRequest = "WorkOrderRequest";
        }
        
        public enum TaskStatusTypes
        {
          NotStarted,
          InProgress,
          Completed,
          OnHold,
        }


    }
}
