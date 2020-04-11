using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shipbot.Controller.Core.Apps.Models;

namespace Shipbot.Controller.Core.Utilities
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }         
        
        public static void ForEach<T>(this IEnumerable<T> enumerable, Func<T, Task> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        } 
    }
    
    public static class LoggerExtensions
    {
        public static IDisposable BeginShipbotLogScope(this ILogger log, ApplicationEnvironmentKey applicationEnvironmentKey, [CallerMemberName] string? callerMemberName = null)
        {
            return InternalBeginShipbotLogScope(log, applicationEnvironmentKey.Application, applicationEnvironmentKey.Environment, callerMemberName ?? string.Empty);
        }
        
        public static IDisposable BeginShipbotLogScope(this ILogger log, string? application = null, string? environment = null, [CallerMemberName] string? callerMemberName = null)
        {
            return InternalBeginShipbotLogScope(log, application ?? string.Empty, environment ?? string.Empty, callerMemberName ?? string.Empty);
        }

        
        public static IDisposable BeginShipbotLogScope<T>(this ILogger<T> log, ApplicationEnvironmentKey applicationEnvironmentKey, [CallerMemberName] string? callerMemberName = null)
        {
            return InternalBeginShipbotLogScope(log, applicationEnvironmentKey.Application, applicationEnvironmentKey.Environment, callerMemberName ?? string.Empty);
        }
        
        public static IDisposable BeginShipbotLogScope<T>(this ILogger<T> log, string? application = null, string? environment = null, [CallerMemberName] string? callerMemberName = null)
        {
            return InternalBeginShipbotLogScope(log, application ?? string.Empty, environment ?? string.Empty, callerMemberName ?? string.Empty);
        }

        private static IDisposable InternalBeginShipbotLogScope(ILogger log, string application, string environment, string callerMemberName)
        {
            var fields = new Dictionary<string, object>();
            
            if (!string.IsNullOrWhiteSpace(application))
            {
                fields.Add("Application", application);
            };

            if (!string.IsNullOrWhiteSpace(environment))
            {
                fields.Add("Environment", environment);
            }
            
            return new ShipbotLoggerScope(log, fields, callerMemberName ?? string.Empty);
        }


        private class ShipbotLoggerScope : IDisposable
        {
            private readonly ILogger _log;
            private readonly string _callSite;
            private readonly IDisposable _scope;
            public ShipbotLoggerScope(ILogger log, Dictionary<string, object> fields, string callSite)
            {
                _log = log;
                _callSite = callSite;
                _scope = log.BeginScope(fields);

                if (callSite != string.Empty)
                {
                    _log.LogTrace($"{_callSite}() >>");
                }
            }
            
            public ShipbotLoggerScope(ILogger log, Dictionary<string, object> fields) : this (log, fields, string.Empty)
            {
            }

            public void Dispose()
            {
                if (_callSite != String.Empty)
                {
                    _log.LogTrace($"{_callSite}() <<");
                }
                
                _scope.Dispose();
            }
        }
        
    }
    
    public static class GuidExtensions
    {
        
        public static Guid CreateGuidFromString(this string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            // convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
            // ASSUME: UTF-8 encoding is always appropriate
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);

            // comput the hash of the name space ID concatenated with the name (step 4)
            byte[] hash;
            using (HashAlgorithm algorithm = SHA1.Create())
            {
                hash = algorithm.ComputeHash(nameBytes);
            }

            // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
            byte[] newGuid = new byte[16];
            Array.Copy(hash, 0, newGuid, 0, 16);

            // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
            newGuid[6] = (byte) ((newGuid[6] & 0x0F) | (5 << 4));

            // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
            newGuid[8] = (byte) ((newGuid[8] & 0x3F) | 0x80);

            // convert the resulting UUID to local byte order (step 13)
            SwapByteOrder(newGuid);
            return new Guid(newGuid);
        }

        // Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
        internal static void SwapByteOrder(byte[] guid)
        {
            SwapBytes(guid, 0, 3);
            SwapBytes(guid, 1, 2);
            SwapBytes(guid, 4, 5);
            SwapBytes(guid, 6, 7);
        }

        private static void SwapBytes(byte[] guid, int left, int right)
        {
            byte temp = guid[left];
            guid[left] = guid[right];
            guid[right] = temp;
        }

    }
}