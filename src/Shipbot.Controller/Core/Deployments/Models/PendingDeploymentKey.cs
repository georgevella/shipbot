// using System;
// using System.Collections.Generic;
// using Microsoft.AspNetCore.Mvc.Infrastructure;
// using Shipbot.Controller.Core.Apps.Models;
// using Shipbot.Controller.Core.Models;
//
// namespace Shipbot.Controller.Core.Deployments.Models
// {
//     internal class PendingDeploymentKey
//     {
//         public string Application { get; set;  }
//         
//         public string Environment { get; set; }
//
//         public PendingDeploymentKey(string application, string environment)
//         {
//             Application = application;
//             Environment = environment;
//         }
//
//         public PendingDeploymentKey(ApplicationEnvironmentKey applicationEnvironmentKey)
//             : this (applicationEnvironmentKey.Application, applicationEnvironmentKey.Environment)
//         {
//             
//         }
//
//         public PendingDeploymentKey()
//         {
//             
//         }
//     }
//     
//     internal class PendingDeploymentKeyEqualityComparer : IEqualityComparer<PendingDeploymentKey>
//     {
//         public bool Equals(PendingDeploymentKey x, PendingDeploymentKey y)
//         {
//             switch (x)
//             {
//                 case null when y == null:
//                     return true;
//                 case null:
//                     return false;
//             }
//
//             if (y == null) return false;
//             
//             if (ReferenceEquals(x, y)) return true;
//
//             return Equals(x.Application, y.Application) && Equals(x.Environment, y.Environment);
//         }
//
//         public int GetHashCode(PendingDeploymentKey obj)
//         {
//             unchecked
//             {
//                 return ((obj.Application != null ? obj.Application.GetHashCode() : 0) * 397) ^ (obj.Environment != null ? obj.Environment.GetHashCode() : 0);
//             }
//         }
//     }
// }