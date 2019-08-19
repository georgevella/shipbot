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
    /// ArtifactLocation describes a location for a single or multiple artifacts. It is used as single artifact in the context of inputs/outputs (e.g. outputs.artifacts.artname). It is also used to describe the location of multiple artifacts such as the archive location of a single workflow step, which the executor will use as a default location to store its files.
    /// </summary>
    [DataContract]
    public partial class ArgoArtifactLocation :  IEquatable<ArgoArtifactLocation>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgoArtifactLocation" /> class.
        /// </summary>
        /// <param name="archiveLogs">ArchiveLogs indicates if the container logs should be archived.</param>
        /// <param name="artifactory">Artifactory contains artifactory artifact location details.</param>
        /// <param name="git">Git contains git artifact location details.</param>
        /// <param name="hdfs">HDFS contains HDFS artifact location details.</param>
        /// <param name="http">HTTP contains HTTP artifact location details.</param>
        /// <param name="raw">Raw contains raw artifact location details.</param>
        /// <param name="s3">S3 contains S3 artifact location details.</param>
        public ArgoArtifactLocation(bool? archiveLogs = default(bool?), ArgoArtifactoryArtifact artifactory = default(ArgoArtifactoryArtifact), ArgoGitArtifact git = default(ArgoGitArtifact), ArgoHDFSArtifact hdfs = default(ArgoHDFSArtifact), ArgoHTTPArtifact http = default(ArgoHTTPArtifact), ArgoRawArtifact raw = default(ArgoRawArtifact), ArgoS3Artifact s3 = default(ArgoS3Artifact))
        {
            this.ArchiveLogs = archiveLogs;
            this.Artifactory = artifactory;
            this.Git = git;
            this.Hdfs = hdfs;
            this.Http = http;
            this.Raw = raw;
            this.S3 = s3;
        }
        
        /// <summary>
        /// ArchiveLogs indicates if the container logs should be archived
        /// </summary>
        /// <value>ArchiveLogs indicates if the container logs should be archived</value>
        [DataMember(Name="archiveLogs", EmitDefaultValue=false)]
        public bool? ArchiveLogs { get; set; }

        /// <summary>
        /// Artifactory contains artifactory artifact location details
        /// </summary>
        /// <value>Artifactory contains artifactory artifact location details</value>
        [DataMember(Name="artifactory", EmitDefaultValue=false)]
        public ArgoArtifactoryArtifact Artifactory { get; set; }

        /// <summary>
        /// Git contains git artifact location details
        /// </summary>
        /// <value>Git contains git artifact location details</value>
        [DataMember(Name="git", EmitDefaultValue=false)]
        public ArgoGitArtifact Git { get; set; }

        /// <summary>
        /// HDFS contains HDFS artifact location details
        /// </summary>
        /// <value>HDFS contains HDFS artifact location details</value>
        [DataMember(Name="hdfs", EmitDefaultValue=false)]
        public ArgoHDFSArtifact Hdfs { get; set; }

        /// <summary>
        /// HTTP contains HTTP artifact location details
        /// </summary>
        /// <value>HTTP contains HTTP artifact location details</value>
        [DataMember(Name="http", EmitDefaultValue=false)]
        public ArgoHTTPArtifact Http { get; set; }

        /// <summary>
        /// Raw contains raw artifact location details
        /// </summary>
        /// <value>Raw contains raw artifact location details</value>
        [DataMember(Name="raw", EmitDefaultValue=false)]
        public ArgoRawArtifact Raw { get; set; }

        /// <summary>
        /// S3 contains S3 artifact location details
        /// </summary>
        /// <value>S3 contains S3 artifact location details</value>
        [DataMember(Name="s3", EmitDefaultValue=false)]
        public ArgoS3Artifact S3 { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ArgoArtifactLocation {\n");
            sb.Append("  ArchiveLogs: ").Append(ArchiveLogs).Append("\n");
            sb.Append("  Artifactory: ").Append(Artifactory).Append("\n");
            sb.Append("  Git: ").Append(Git).Append("\n");
            sb.Append("  Hdfs: ").Append(Hdfs).Append("\n");
            sb.Append("  Http: ").Append(Http).Append("\n");
            sb.Append("  Raw: ").Append(Raw).Append("\n");
            sb.Append("  S3: ").Append(S3).Append("\n");
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
            return this.Equals(input as ArgoArtifactLocation);
        }

        /// <summary>
        /// Returns true if ArgoArtifactLocation instances are equal
        /// </summary>
        /// <param name="input">Instance of ArgoArtifactLocation to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ArgoArtifactLocation input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.ArchiveLogs == input.ArchiveLogs ||
                    (this.ArchiveLogs != null &&
                    this.ArchiveLogs.Equals(input.ArchiveLogs))
                ) && 
                (
                    this.Artifactory == input.Artifactory ||
                    (this.Artifactory != null &&
                    this.Artifactory.Equals(input.Artifactory))
                ) && 
                (
                    this.Git == input.Git ||
                    (this.Git != null &&
                    this.Git.Equals(input.Git))
                ) && 
                (
                    this.Hdfs == input.Hdfs ||
                    (this.Hdfs != null &&
                    this.Hdfs.Equals(input.Hdfs))
                ) && 
                (
                    this.Http == input.Http ||
                    (this.Http != null &&
                    this.Http.Equals(input.Http))
                ) && 
                (
                    this.Raw == input.Raw ||
                    (this.Raw != null &&
                    this.Raw.Equals(input.Raw))
                ) && 
                (
                    this.S3 == input.S3 ||
                    (this.S3 != null &&
                    this.S3.Equals(input.S3))
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
                if (this.ArchiveLogs != null)
                    hashCode = hashCode * 59 + this.ArchiveLogs.GetHashCode();
                if (this.Artifactory != null)
                    hashCode = hashCode * 59 + this.Artifactory.GetHashCode();
                if (this.Git != null)
                    hashCode = hashCode * 59 + this.Git.GetHashCode();
                if (this.Hdfs != null)
                    hashCode = hashCode * 59 + this.Hdfs.GetHashCode();
                if (this.Http != null)
                    hashCode = hashCode * 59 + this.Http.GetHashCode();
                if (this.Raw != null)
                    hashCode = hashCode * 59 + this.Raw.GetHashCode();
                if (this.S3 != null)
                    hashCode = hashCode * 59 + this.S3.GetHashCode();
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
