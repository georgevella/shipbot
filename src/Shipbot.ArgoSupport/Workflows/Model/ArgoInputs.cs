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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Workflows.Model
{
    /// <summary>
    /// Inputs are the mechanism for passing parameters, artifacts, volumes from one template to another
    /// </summary>
    [DataContract]
    public partial class ArgoInputs :  IEquatable<ArgoInputs>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgoInputs" /> class.
        /// </summary>
        /// <param name="artifacts">Artifact are a list of artifacts passed as inputs.</param>
        /// <param name="parameters">Parameters are a list of parameters passed as inputs.</param>
        public ArgoInputs(List<ArgoArtifact> artifacts = default(List<ArgoArtifact>), List<ArgoParameter> parameters = default(List<ArgoParameter>))
        {
            this.Artifacts = artifacts;
            this.Parameters = parameters;
        }
        
        /// <summary>
        /// Artifact are a list of artifacts passed as inputs
        /// </summary>
        /// <value>Artifact are a list of artifacts passed as inputs</value>
        [DataMember(Name="artifacts", EmitDefaultValue=false)]
        public List<ArgoArtifact> Artifacts { get; set; }

        /// <summary>
        /// Parameters are a list of parameters passed as inputs
        /// </summary>
        /// <value>Parameters are a list of parameters passed as inputs</value>
        [DataMember(Name="parameters", EmitDefaultValue=false)]
        public List<ArgoParameter> Parameters { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ArgoInputs {\n");
            sb.Append("  Artifacts: ").Append(Artifacts).Append("\n");
            sb.Append("  Parameters: ").Append(Parameters).Append("\n");
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
            return this.Equals(input as ArgoInputs);
        }

        /// <summary>
        /// Returns true if ArgoInputs instances are equal
        /// </summary>
        /// <param name="input">Instance of ArgoInputs to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ArgoInputs input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Artifacts == input.Artifacts ||
                    this.Artifacts != null &&
                    this.Artifacts.SequenceEqual(input.Artifacts)
                ) && 
                (
                    this.Parameters == input.Parameters ||
                    this.Parameters != null &&
                    this.Parameters.SequenceEqual(input.Parameters)
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
                if (this.Artifacts != null)
                    hashCode = hashCode * 59 + this.Artifacts.GetHashCode();
                if (this.Parameters != null)
                    hashCode = hashCode * 59 + this.Parameters.GetHashCode();
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
