/* 
 * Argo
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: v2.4.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Workflows.Model
{
    /// <summary>
    /// ValueFrom describes a location in which to obtain the value to a parameter
    /// </summary>
    [DataContract]
    public partial class ArgoValueFrom :  IEquatable<ArgoValueFrom>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgoValueFrom" /> class.
        /// </summary>
        /// <param name="jqFilter">JQFilter expression against the resource object in resource templates.</param>
        /// <param name="jsonPath">JSONPath of a resource to retrieve an output parameter value from in resource templates.</param>
        /// <param name="_parameter">Parameter reference to a step or dag task in which to retrieve an output parameter value from (e.g. &#39;{{steps.mystep.outputs.myparam}}&#39;).</param>
        /// <param name="path">Path in the container to retrieve an output parameter value from in container templates.</param>
        public ArgoValueFrom(string jqFilter = default(string), string jsonPath = default(string), string _parameter = default(string), string path = default(string))
        {
            this.JqFilter = jqFilter;
            this.JsonPath = jsonPath;
            this.Parameter = _parameter;
            this.Path = path;
        }
        
        /// <summary>
        /// JQFilter expression against the resource object in resource templates
        /// </summary>
        /// <value>JQFilter expression against the resource object in resource templates</value>
        [DataMember(Name="jqFilter", EmitDefaultValue=false)]
        public string JqFilter { get; set; }

        /// <summary>
        /// JSONPath of a resource to retrieve an output parameter value from in resource templates
        /// </summary>
        /// <value>JSONPath of a resource to retrieve an output parameter value from in resource templates</value>
        [DataMember(Name="jsonPath", EmitDefaultValue=false)]
        public string JsonPath { get; set; }

        /// <summary>
        /// Parameter reference to a step or dag task in which to retrieve an output parameter value from (e.g. &#39;{{steps.mystep.outputs.myparam}}&#39;)
        /// </summary>
        /// <value>Parameter reference to a step or dag task in which to retrieve an output parameter value from (e.g. &#39;{{steps.mystep.outputs.myparam}}&#39;)</value>
        [DataMember(Name="parameter", EmitDefaultValue=false)]
        public string Parameter { get; set; }

        /// <summary>
        /// Path in the container to retrieve an output parameter value from in container templates
        /// </summary>
        /// <value>Path in the container to retrieve an output parameter value from in container templates</value>
        [DataMember(Name="path", EmitDefaultValue=false)]
        public string Path { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ArgoValueFrom {\n");
            sb.Append("  JqFilter: ").Append(JqFilter).Append("\n");
            sb.Append("  JsonPath: ").Append(JsonPath).Append("\n");
            sb.Append("  Parameter: ").Append(Parameter).Append("\n");
            sb.Append("  Path: ").Append(Path).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as ArgoValueFrom);
        }

        /// <summary>
        /// Returns true if ArgoValueFrom instances are equal
        /// </summary>
        /// <param name="input">Instance of ArgoValueFrom to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ArgoValueFrom input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.JqFilter == input.JqFilter ||
                    (this.JqFilter != null &&
                    this.JqFilter.Equals(input.JqFilter))
                ) && 
                (
                    this.JsonPath == input.JsonPath ||
                    (this.JsonPath != null &&
                    this.JsonPath.Equals(input.JsonPath))
                ) && 
                (
                    this.Parameter == input.Parameter ||
                    (this.Parameter != null &&
                    this.Parameter.Equals(input.Parameter))
                ) && 
                (
                    this.Path == input.Path ||
                    (this.Path != null &&
                    this.Path.Equals(input.Path))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.JqFilter != null)
                    hashCode = hashCode * 59 + this.JqFilter.GetHashCode();
                if (this.JsonPath != null)
                    hashCode = hashCode * 59 + this.JsonPath.GetHashCode();
                if (this.Parameter != null)
                    hashCode = hashCode * 59 + this.Parameter.GetHashCode();
                if (this.Path != null)
                    hashCode = hashCode * 59 + this.Path.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
