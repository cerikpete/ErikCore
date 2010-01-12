using System;
using System.Web;
using System.Web.Mvc;

namespace IoC.Windsor
{
    /// <summary>
    /// A <see cref="IControllerFactory" /> implementation that uses Castle Windsor to instantiate controllers.
    /// </summary>
    public class WindsorControllerFactory : DefaultControllerFactory
    {
        /// <summary>
        /// Gets a controller instance of the specified type.
        /// </summary>
        /// <param name="controllerType">The type of the controller.</param>
        /// <returns>An instance of the <paramref name="controllerType"/>.</returns>
        protected override IController GetControllerInstance(Type controllerType)
        {
            // If the controllerType argument is null, the DefaultControllerFactory
            // could not find a controller based on the configured routes.  If that's the
            // case generate an HTTP 404 response code.
            if (controllerType == null)
            {
                throw new HttpException(404, "The page cannot be found");
            }
            return (IController)IoC.Resolve(controllerType);
        }

        /// <summary>
        /// Releases the controller.
        /// </summary>
        /// <param name="controller">The controller to be released.</param>
        /// <remarks>This method disposes the controller if it implements <see cref="IDisposable"/> and
        /// releases it from the container.</remarks>
        public override void ReleaseController(IController controller)
        {
            IDisposable disposable = controller as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
            IoC.Release(controller);
        }
    }
}