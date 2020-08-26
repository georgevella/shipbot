using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shipbot.Data;
using Shipbot.SlackIntegration.Dao;

namespace Shipbot.Deployments.Dao
{
    /// <summary>
    ///     A DAO entity that describes the message sent in relation to a deployment.
    /// </summary>
    [Table("deploymentNotifications")]
    public class DeploymentNotification
    {
        private SlackMessage _slackMessage;
        private Deployment _deployment;
        private Action<object, string> LazyLoader { get; }

        [Key]
        public Guid Id { get; set; }

        public Guid SlackMessageId { get; set; }
        public SlackMessage SlackMessage
        {
            get => LazyLoader.Load( this, ref _slackMessage);
            set => _slackMessage = value;
        }

        public Guid DeploymentId { get; set; }

        public Deployment Deployment
        {
            get => LazyLoader.Load(this, ref _deployment);
            set => _deployment = value;
        }


        public DeploymentNotification(Action<object, string> lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

        public DeploymentNotification()
        {
            
        }
    }
}