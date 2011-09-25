using Ninject;
using Worki.Infrastructure.UnitOfWork;
using Ninject.Parameters;

namespace Worki.Infrastructure.Repository
{
	public static class ModelFactory
	{
		static IKernel _Kernel;

		public static void RegisterKernel(IKernel kernel)
		{
			_Kernel = kernel;
		}

		public static IUnitOfWork GetUnitOfWork()
		{
			return (IUnitOfWork)_Kernel.Get(typeof(IUnitOfWork));
		}

		public static T GetRepository<T>(IUnitOfWork unitOfWork)
		{
			return (T)_Kernel.Get(typeof(T), new ConstructorArgument("context", unitOfWork));
		}
	}
}