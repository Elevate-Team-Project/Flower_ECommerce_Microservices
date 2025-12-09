using System.Reflection;

namespace Catalog_Service
{
    public static class EndpointExtensions
    {
        public static void MapAllEndpoints(this WebApplication app)
        {
            var endpointTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && t.Namespace?.StartsWith("WorkoutService.Features") == true && t.Name == "Endpoints");

            foreach (var type in endpointTypes)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name.StartsWith("Map") && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(WebApplication));

                foreach (var method in methods)
                {
                    method.Invoke(null, new object[] { app });
                }
            }
        }
    }
}
