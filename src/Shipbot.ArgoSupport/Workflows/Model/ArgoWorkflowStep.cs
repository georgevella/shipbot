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
    /// WorkflowStep is a reference to a template to execute in a series of step
    /// </summary>
    [DataContract]
    public partial class ArgoWorkflowStep :  IEquatable<ArgoWorkflowStep>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgoWorkflowStep" /> class.
        /// </summary>
        /// <param name="arguments">Arguments hold arguments to the template.</param>
        /// <param name="continueOn">ContinueOn makes argo to proceed with the following step even if this step fails. Errors and Failed states can be specified.</param>
        /// <param name="name">Name of the step.</param>
        /// <param name="template">Template is the name of the template to execute as the step.</param>
        /// <param name="templateRef">TemplateRef is the reference to the template resource to execute as the step..</param>
        /// <param name="when">When is an expression in which the step should conditionally execute.</param>
        /// <param name="withItems">WithItems expands a step into multiple parallel steps from the items in the list.</param>
        /// <param name="withParam">WithParam expands a step into multiple parallel steps from the value in the parameter, which is expected to be a JSON list..</param>
        /// <param name="withSequence">WithSequence expands a step into a numeric sequence.</param>
        public ArgoWorkflowStep(ArgoArguments arguments = default(ArgoArguments), ArgoContinueOn continueOn = default(ArgoContinueOn), string name = default(string), string template = default(string), ArgoTemplateRef templateRef = default(ArgoTemplateRef), string when = default(string), List<ArgoItem> withItems = default(List<ArgoItem>), string withParam = default(string), ArgoSequence withSequence = default(ArgoSequence))
        {
            this.Arguments = arguments;
            this.ContinueOn = continueOn;
            this.Name = name;
            this.Template = template;
            this.TemplateRef = templateRef;
            this.When = when;
            this.WithItems = withItems;
            this.WithParam = withParam;
            this.WithSequence = withSequence;
        }
        
        /// <summary>
        /// Arguments hold arguments to the template
        /// </summary>
        /// <value>Arguments hold arguments to the template</value>
        [DataMember(Name="arguments", EmitDefaultValue=false)]
        public ArgoArguments Arguments { get; set; }

        /// <summary>
        /// ContinueOn makes argo to proceed with the following step even if this step fails. Errors and Failed states can be specified
        /// </summary>
        /// <value>ContinueOn makes argo to proceed with the following step even if this step fails. Errors and Failed states can be specified</value>
        [DataMember(Name="continueOn", EmitDefaultValue=false)]
        public ArgoContinueOn ContinueOn { get; set; }

        /// <summary>
        /// Name of the step
        /// </summary>
        /// <value>Name of the step</value>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// Template is the name of the template to execute as the step
        /// </summary>
        /// <value>Template is the name of the template to execute as the step</value>
        [DataMember(Name="template", EmitDefaultValue=false)]
        public string Template { get; set; }

        /// <summary>
        /// TemplateRef is the reference to the template resource to execute as the step.
        /// </summary>
        /// <value>TemplateRef is the reference to the template resource to execute as the step.</value>
        [DataMember(Name="templateRef", EmitDefaultValue=false)]
        public ArgoTemplateRef TemplateRef { get; set; }

        /// <summary>
        /// When is an expression in which the step should conditionally execute
        /// </summary>
        /// <value>When is an expression in which the step should conditionally execute</value>
        [DataMember(Name="when", EmitDefaultValue=false)]
        public string When { get; set; }

        /// <summary>
        /// WithItems expands a step into multiple parallel steps from the items in the list
        /// </summary>
        /// <value>WithItems expands a step into multiple parallel steps from the items in the list</value>
        [DataMember(Name="withItems", EmitDefaultValue=false)]
        public List<ArgoItem> WithItems { get; set; }

        /// <summary>
        /// WithParam expands a step into multiple parallel steps from the value in the parameter, which is expected to be a JSON list.
        /// </summary>
        /// <value>WithParam expands a step into multiple parallel steps from the value in the parameter, which is expected to be a JSON list.</value>
        [DataMember(Name="withParam", EmitDefaultValue=false)]
        public string WithParam { get; set; }

        /// <summary>
        /// WithSequence expands a step into a numeric sequence
        /// </summary>
        /// <value>WithSequence expands a step into a numeric sequence</value>
        [DataMember(Name="withSequence", EmitDefaultValue=false)]
        public ArgoSequence WithSequence { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ArgoWorkflowStep {\n");
            sb.Append("  Arguments: ").Append(Arguments).Append("\n");
            sb.Append("  ContinueOn: ").Append(ContinueOn).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Template: ").Append(Template).Append("\n");
            sb.Append("  TemplateRef: ").Append(TemplateRef).Append("\n");
            sb.Append("  When: ").Append(When).Append("\n");
            sb.Append("  WithItems: ").Append(WithItems).Append("\n");
            sb.Append("  WithParam: ").Append(WithParam).Append("\n");
            sb.Append("  WithSequence: ").Append(WithSequence).Append("\n");
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
            return this.Equals(input as ArgoWorkflowStep);
        }

        /// <summary>
        /// Returns true if ArgoWorkflowStep instances are equal
        /// </summary>
        /// <param name="input">Instance of ArgoWorkflowStep to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ArgoWorkflowStep input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Arguments == input.Arguments ||
                    (this.Arguments != null &&
                    this.Arguments.Equals(input.Arguments))
                ) && 
                (
                    this.ContinueOn == input.ContinueOn ||
                    (this.ContinueOn != null &&
                    this.ContinueOn.Equals(input.ContinueOn))
                ) && 
                (
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                ) && 
                (
                    this.Template == input.Template ||
                    (this.Template != null &&
                    this.Template.Equals(input.Template))
                ) && 
                (
                    this.TemplateRef == input.TemplateRef ||
                    (this.TemplateRef != null &&
                    this.TemplateRef.Equals(input.TemplateRef))
                ) && 
                (
                    this.When == input.When ||
                    (this.When != null &&
                    this.When.Equals(input.When))
                ) && 
                (
                    this.WithItems == input.WithItems ||
                    this.WithItems != null &&
                    this.WithItems.SequenceEqual(input.WithItems)
                ) && 
                (
                    this.WithParam == input.WithParam ||
                    (this.WithParam != null &&
                    this.WithParam.Equals(input.WithParam))
                ) && 
                (
                    this.WithSequence == input.WithSequence ||
                    (this.WithSequence != null &&
                    this.WithSequence.Equals(input.WithSequence))
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
                if (this.Arguments != null)
                    hashCode = hashCode * 59 + this.Arguments.GetHashCode();
                if (this.ContinueOn != null)
                    hashCode = hashCode * 59 + this.ContinueOn.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.Template != null)
                    hashCode = hashCode * 59 + this.Template.GetHashCode();
                if (this.TemplateRef != null)
                    hashCode = hashCode * 59 + this.TemplateRef.GetHashCode();
                if (this.When != null)
                    hashCode = hashCode * 59 + this.When.GetHashCode();
                if (this.WithItems != null)
                    hashCode = hashCode * 59 + this.WithItems.GetHashCode();
                if (this.WithParam != null)
                    hashCode = hashCode * 59 + this.WithParam.GetHashCode();
                if (this.WithSequence != null)
                    hashCode = hashCode * 59 + this.WithSequence.GetHashCode();
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