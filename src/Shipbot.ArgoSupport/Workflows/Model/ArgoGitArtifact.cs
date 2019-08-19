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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace AutoDeploy.ArgoSupport.Workflows.Model
{
    /// <summary>
    /// GitArtifact is the location of an git artifact
    /// </summary>
    [DataContract]
    public partial class ArgoGitArtifact :  IEquatable<ArgoGitArtifact>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgoGitArtifact" /> class.
        /// </summary>
        [JsonConstructor]
        protected ArgoGitArtifact() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgoGitArtifact" /> class.
        /// </summary>
        /// <param name="depth">Depth specifies clones/fetches should be shallow and include the given number of commits from the branch tip.</param>
        /// <param name="fetch">Fetch specifies a number of refs that should be fetched before checkout.</param>
        /// <param name="insecureIgnoreHostKey">InsecureIgnoreHostKey disables SSH strict host key checking during git clone.</param>
        /// <param name="passwordSecret">PasswordSecret is the secret selector to the repository password.</param>
        /// <param name="repo">Repo is the git repository (required).</param>
        /// <param name="revision">Revision is the git commit, tag, branch to checkout.</param>
        /// <param name="sshPrivateKeySecret">SSHPrivateKeySecret is the secret selector to the repository ssh private key.</param>
        /// <param name="usernameSecret">UsernameSecret is the secret selector to the repository username.</param>
        public ArgoGitArtifact(int? depth = default(int?), List<string> fetch = default(List<string>), bool? insecureIgnoreHostKey = default(bool?), k8s.Models.V1SecretKeySelector passwordSecret = default(k8s.Models.V1SecretKeySelector), string repo = default(string), string revision = default(string), k8s.Models.V1SecretKeySelector sshPrivateKeySecret = default(k8s.Models.V1SecretKeySelector), k8s.Models.V1SecretKeySelector usernameSecret = default(k8s.Models.V1SecretKeySelector))
        {
            // to ensure "repo" is required (not null)
            if (repo == null)
            {
                throw new InvalidDataException("repo is a required property for ArgoGitArtifact and cannot be null");
            }
            else
            {
                this.Repo = repo;
            }
            this.Depth = depth;
            this.Fetch = fetch;
            this.InsecureIgnoreHostKey = insecureIgnoreHostKey;
            this.PasswordSecret = passwordSecret;
            this.Revision = revision;
            this.SshPrivateKeySecret = sshPrivateKeySecret;
            this.UsernameSecret = usernameSecret;
        }
        
        /// <summary>
        /// Depth specifies clones/fetches should be shallow and include the given number of commits from the branch tip
        /// </summary>
        /// <value>Depth specifies clones/fetches should be shallow and include the given number of commits from the branch tip</value>
        [DataMember(Name="depth", EmitDefaultValue=false)]
        public int? Depth { get; set; }

        /// <summary>
        /// Fetch specifies a number of refs that should be fetched before checkout
        /// </summary>
        /// <value>Fetch specifies a number of refs that should be fetched before checkout</value>
        [DataMember(Name="fetch", EmitDefaultValue=false)]
        public List<string> Fetch { get; set; }

        /// <summary>
        /// InsecureIgnoreHostKey disables SSH strict host key checking during git clone
        /// </summary>
        /// <value>InsecureIgnoreHostKey disables SSH strict host key checking during git clone</value>
        [DataMember(Name="insecureIgnoreHostKey", EmitDefaultValue=false)]
        public bool? InsecureIgnoreHostKey { get; set; }

        /// <summary>
        /// PasswordSecret is the secret selector to the repository password
        /// </summary>
        /// <value>PasswordSecret is the secret selector to the repository password</value>
        [DataMember(Name="passwordSecret", EmitDefaultValue=false)]
        public k8s.Models.V1SecretKeySelector PasswordSecret { get; set; }

        /// <summary>
        /// Repo is the git repository
        /// </summary>
        /// <value>Repo is the git repository</value>
        [DataMember(Name="repo", EmitDefaultValue=false)]
        public string Repo { get; set; }

        /// <summary>
        /// Revision is the git commit, tag, branch to checkout
        /// </summary>
        /// <value>Revision is the git commit, tag, branch to checkout</value>
        [DataMember(Name="revision", EmitDefaultValue=false)]
        public string Revision { get; set; }

        /// <summary>
        /// SSHPrivateKeySecret is the secret selector to the repository ssh private key
        /// </summary>
        /// <value>SSHPrivateKeySecret is the secret selector to the repository ssh private key</value>
        [DataMember(Name="sshPrivateKeySecret", EmitDefaultValue=false)]
        public k8s.Models.V1SecretKeySelector SshPrivateKeySecret { get; set; }

        /// <summary>
        /// UsernameSecret is the secret selector to the repository username
        /// </summary>
        /// <value>UsernameSecret is the secret selector to the repository username</value>
        [DataMember(Name="usernameSecret", EmitDefaultValue=false)]
        public k8s.Models.V1SecretKeySelector UsernameSecret { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ArgoGitArtifact {\n");
            sb.Append("  Depth: ").Append(Depth).Append("\n");
            sb.Append("  Fetch: ").Append(Fetch).Append("\n");
            sb.Append("  InsecureIgnoreHostKey: ").Append(InsecureIgnoreHostKey).Append("\n");
            sb.Append("  PasswordSecret: ").Append(PasswordSecret).Append("\n");
            sb.Append("  Repo: ").Append(Repo).Append("\n");
            sb.Append("  Revision: ").Append(Revision).Append("\n");
            sb.Append("  SshPrivateKeySecret: ").Append(SshPrivateKeySecret).Append("\n");
            sb.Append("  UsernameSecret: ").Append(UsernameSecret).Append("\n");
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
            return this.Equals(input as ArgoGitArtifact);
        }

        /// <summary>
        /// Returns true if ArgoGitArtifact instances are equal
        /// </summary>
        /// <param name="input">Instance of ArgoGitArtifact to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ArgoGitArtifact input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Depth == input.Depth ||
                    (this.Depth != null &&
                    this.Depth.Equals(input.Depth))
                ) && 
                (
                    this.Fetch == input.Fetch ||
                    this.Fetch != null &&
                    this.Fetch.SequenceEqual(input.Fetch)
                ) && 
                (
                    this.InsecureIgnoreHostKey == input.InsecureIgnoreHostKey ||
                    (this.InsecureIgnoreHostKey != null &&
                    this.InsecureIgnoreHostKey.Equals(input.InsecureIgnoreHostKey))
                ) && 
                (
                    this.PasswordSecret == input.PasswordSecret ||
                    (this.PasswordSecret != null &&
                    this.PasswordSecret.Equals(input.PasswordSecret))
                ) && 
                (
                    this.Repo == input.Repo ||
                    (this.Repo != null &&
                    this.Repo.Equals(input.Repo))
                ) && 
                (
                    this.Revision == input.Revision ||
                    (this.Revision != null &&
                    this.Revision.Equals(input.Revision))
                ) && 
                (
                    this.SshPrivateKeySecret == input.SshPrivateKeySecret ||
                    (this.SshPrivateKeySecret != null &&
                    this.SshPrivateKeySecret.Equals(input.SshPrivateKeySecret))
                ) && 
                (
                    this.UsernameSecret == input.UsernameSecret ||
                    (this.UsernameSecret != null &&
                    this.UsernameSecret.Equals(input.UsernameSecret))
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
                if (this.Depth != null)
                    hashCode = hashCode * 59 + this.Depth.GetHashCode();
                if (this.Fetch != null)
                    hashCode = hashCode * 59 + this.Fetch.GetHashCode();
                if (this.InsecureIgnoreHostKey != null)
                    hashCode = hashCode * 59 + this.InsecureIgnoreHostKey.GetHashCode();
                if (this.PasswordSecret != null)
                    hashCode = hashCode * 59 + this.PasswordSecret.GetHashCode();
                if (this.Repo != null)
                    hashCode = hashCode * 59 + this.Repo.GetHashCode();
                if (this.Revision != null)
                    hashCode = hashCode * 59 + this.Revision.GetHashCode();
                if (this.SshPrivateKeySecret != null)
                    hashCode = hashCode * 59 + this.SshPrivateKeySecret.GetHashCode();
                if (this.UsernameSecret != null)
                    hashCode = hashCode * 59 + this.UsernameSecret.GetHashCode();
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
