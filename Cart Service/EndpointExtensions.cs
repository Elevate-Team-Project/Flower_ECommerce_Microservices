using System.Reflection;

namespace Cart_Service
{
    public static class EndpointExtensions
    {
        public static void MapAllEndpoints(this WebApplication app)
        {
            var endpointTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass
                            && t.Namespace?.StartsWith("Catalog_Service.Features") == true
                            && t.Name.EndsWith("Endpoints")
);

            foreach (var type in endpointTypes)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name.StartsWith("Map")
                                && m.GetParameters().Length == 1
                                // FIX: Check for IEndpointRouteBuilder, not WebApplication
                                && m.GetParameters()[0].ParameterType == typeof(IEndpointRouteBuilder));

                foreach (var method in methods)
                {
                    // 'app' implements IEndpointRouteBuilder, so this works perfectly
                    method.Invoke(null, new object[] { app });
                }
            }
        }
    }
}