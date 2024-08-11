using HogWildSystem.BLL;
using HogWildSystem.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem
{
    public static class HogWildExtension
    {
        public static void AddBackendDependencies
            (this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<HogWildContext>(options);

            services.AddTransient<WorkingVersionsService>(ServiceProvider =>
            {
                var context = ServiceProvider.GetService<HogWildContext>();
                return new WorkingVersionsService(context);
            });

            services.AddTransient<CustomerService>(ServiceProvider =>
            {
                var context = ServiceProvider.GetService<HogWildContext>();
                return new CustomerService(context);
            });

            services.AddTransient<CategoryLookupService>(ServiceProvider =>
            {
                var context = ServiceProvider.GetService<HogWildContext>();
                return new CategoryLookupService(context);
            });

            services.AddTransient<InvoiceService>(ServiceProvider =>
            {
                var context = ServiceProvider.GetService<HogWildContext>();
                return new InvoiceService(context);
            });

            services.AddTransient<PartService>(ServiceProvider =>
            {
                var context = ServiceProvider.GetService<HogWildContext>();
                return new PartService(context);
            });
        }
    }
}
