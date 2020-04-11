using System;

namespace Shipbot.Controller.Core.DeploymentSources.Models
{
    public class DeploymentSourceChange
    {
        public DeploymentSourceChange(DeploymentSourceChangeAction action, string valuePath, string currentValue, string newValue)
        {
            Action = action;
            ValuePath = valuePath;
            CurrentValue = currentValue;
            NewValue = newValue;
        }

        public string ValuePath { get; }
        
        public string CurrentValue { get; }
        
        public string NewValue { get; }
        
        public DeploymentSourceChangeAction Action { get; }
    }

    [Flags]
    public enum DeploymentSourceChangeAction
    {
        Update = 0,
        Replace = 0x1 << 1,
        
        Force = 0x1 << 8
        
    }

    public class DeploymentSourceChangeResult
    {
        public DeploymentSourceChangeResult(bool isSuccessful, DeploymentSourceChange change)
        {
            IsSuccessful = isSuccessful;
            Change = change;
        }

        public bool IsSuccessful { get; }
        
        public DeploymentSourceChange Change { get; }
    }
}