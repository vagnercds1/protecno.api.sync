using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using protecno.api.sync.domain.models.generics;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;

namespace protecno.api.sync.domain.extensions
{
    public static class ObjectExtensions
    {
        public static IActionResult CreateRestResponse(this object _object, HttpStatusCode httpStatusCode = HttpStatusCode.OK, string[] messages = null, Pagination pagination = null)
        {
            models.ObjectResult objectRS;
            int statusCode = (Int32)httpStatusCode;
 
            objectRS = new models.ObjectResult()
            {
                Success = statusCode >= 100 && statusCode <= 299,
                Data = _object,
                Message = messages                
            };

            if (pagination != null)
            {
                objectRS.Pagination = new Pagination()
                {
                    Page = pagination.Page,
                    PageSize = pagination.PageSize,
                    Total = pagination.Total
                };
            }

            Microsoft.AspNetCore.Mvc.ObjectResult objectResult = new Microsoft.AspNetCore.Mvc.ObjectResult(_object);
            objectResult.StatusCode = statusCode;
            objectResult.Value = JsonConvert.SerializeObject(objectRS, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            return objectResult;
        }

        public static T To<T>(this object value, T defaultValue = default)
        {
            if (value == null)
                return defaultValue;

            try
            {
                if (typeof(T).IsEnum)
                {
                    if (value is char || value is string)
                    {
                        var vbyte = value.ToString();

                        if (value.ToString().Length == 1)
                            vbyte = ((sbyte)char.Parse(value.ToString())).ToString();

                        return (T)Enum.Parse(typeof(T), vbyte.ToString());
                    }

                    return (T)Enum.Parse(typeof(T), value.ToString());
                }
                try
                {
                    return (T)value;
                }
                catch
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T Clone<T>(this T obj)
        {
            string jsonObj = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(jsonObj);
        }

        public static string ToJsonString(this object obj)
        {
            if (obj == null)
                return string.Empty;

            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        }

        public static string Serialize<T>(this T instance)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, instance);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }
    }
}
