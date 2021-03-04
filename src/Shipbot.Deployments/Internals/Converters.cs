using System.Linq;

namespace Shipbot.Deployments.Internals
{
    internal static class Converters
    {
        public static Models.Deployment ConvertToDeploymentModel(this Dao.Deployment deploymentDao) =>
            new Models.Deployment(
                deploymentDao.Id,
                deploymentDao.ApplicationId,
                deploymentDao.ImageRepository,
                deploymentDao.UpdatePath,
                deploymentDao.CurrentImageTag,
                deploymentDao.TargetImageTag,
                (Models.DeploymentStatus) deploymentDao.Status,
                (Models.DeploymentType) deploymentDao.Type,
                deploymentDao.NameSuffix,
                deploymentDao.CreationDateTime,
                deploymentDao.DeploymentDateTime,
                deploymentDao.InstanceId,
                deploymentDao.Parameters);
    }
}