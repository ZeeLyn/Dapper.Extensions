using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Autofac;
using Autofac.Features.AttributeFilters;

namespace Dapper.Extensions
{
    [SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage", Justification = "Allowing the inherited AttributeUsageAttribute to be used avoids accidental override or conflict at this level.")]
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class DependencyDapperAttribute : ParameterFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyFilterAttribute"/> class.
        /// </summary>
        /// <param name="serviceKey">The key that the dependency should have in order to satisfy the parameter.</param>
        /// <param name="readOnly"></param>
        public DependencyDapperAttribute(string serviceKey, bool readOnly = false)
        {
            ServiceKey = serviceKey;
            ReadOnly = readOnly;
        }

        public DependencyDapperAttribute(bool readOnly = false)
        {
            ReadOnly = readOnly;
        }

        public bool ReadOnly { get; }

        /// <summary>
        /// Gets the key the dependency is expected to have to satisfy the parameter.
        /// </summary>
        /// <value>
        /// The <see cref="object"/> corresponding to a registered service key on a component.
        /// Resolved components must be keyed with this value to satisfy the filter.
        /// </value>
        public string ServiceKey { get; }

        /// <summary>
        /// Resolves a constructor parameter based on keyed service requirements.
        /// </summary>
        /// <param name="parameter">The specific parameter being resolved that is marked with this attribute.</param>
        /// <param name="context">The component context under which the parameter is being resolved.</param>
        /// <returns>
        /// The instance of the object that should be used for the parameter value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="parameter" /> or <paramref name="context" /> is <see langword="null" />.
        /// </exception>
        public override object ResolveParameter(ParameterInfo parameter, IComponentContext context)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrWhiteSpace(ServiceKey))
                return ReadOnly ? context.ResolveKeyed("_slave", parameter.ParameterType, new NamedParameter("readOnly", true)) : context.Resolve(parameter.ParameterType);
            return ReadOnly ? context.ResolveKeyed($"{ServiceKey}_slave", parameter.ParameterType, new NamedParameter("readOnly", true)) : context.ResolveKeyed(ServiceKey, parameter.ParameterType);
        }
    }
}
