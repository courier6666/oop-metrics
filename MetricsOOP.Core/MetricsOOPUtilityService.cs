using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MetricsOOP.Core
{
    public class MetricsOOPUtilityService
    {
        private readonly Assembly _assembly;
        private readonly List<Type> _classes;
        private readonly Dictionary<Type, HashSet<Type>> _classInheritanceHierarchyGraph;

        private MetricsOOPUtilityService(Assembly assembly, List<Type> classes, Dictionary<Type, HashSet<Type>> classHierarchy)
        {
            _assembly = assembly;
            _classes = classes;
            _classInheritanceHierarchyGraph = classHierarchy;
        }

        public static MetricsOOPUtilityService CreateWithContext(string dllName)
        {
            var assembly = Assembly.Load(dllName);
            var classes = LoadClasses(assembly);
            var classHierarchy = BuildHierarchyGraph(classes);
            return new MetricsOOPUtilityService(assembly, classes, classHierarchy);
        }

        public (int MaxDepth, List<Type> DepthChain) CalculateMaxDepthOfInheritance()
        {
            var leaves = _classInheritanceHierarchyGraph.
                Where(kv => kv.Value.Count == 0).
                Select(kv => kv.Key);

            var maxChain = new List<Type>();

            foreach (var leaf in leaves)
            {
                var chain = GetDepth(leaf);
                if (chain.Count > maxChain.Count)
                {
                    maxChain = chain;
                }
            }

            return (maxChain.Count - 1, maxChain);
        }

        /// <returns>Returns a dicitonary where the key is a number of children and the value is a number of classes that have such number of chidren .</returns>
        public Dictionary<int, List<Type>> CalculateNumberOfChildren()
        {
            var result = new Dictionary<int, List<Type>>();

            foreach (var @class in _classes)
            {
                var noc = _classInheritanceHierarchyGraph[@class].Count;
                if (result.ContainsKey(noc))
                {
                    result[noc].Add(@class);
                }
                else
                {
                    result[noc] = [@class];
                }
            }

            return result;
        }

        public double CalculateMethodHidingFactor()
        {
            int totalSumOfOpenMethodsForClasses = _classes.Sum(c => GetOpenMethodsForClass(c));
            int totalSumOfHiddenMethodsForClasses = _classes.Sum(c => GetHiddenMethodsForClass(c));

            if (totalSumOfOpenMethodsForClasses == 0)
            {
                return 1.0;
            }

            return (double)totalSumOfHiddenMethodsForClasses / (totalSumOfOpenMethodsForClasses + totalSumOfHiddenMethodsForClasses);
        }


        public double CalculateAttributeHidingFactor()
        {
            int totalSumOfOpenAttributesForClasses = _classes.Sum(c => GetOpenAttributesForClass(c));
            int totalSumOfHiddenAttributesForClasses = _classes.Sum(c => GetHiddenAttributesForClass(c));

            if (totalSumOfOpenAttributesForClasses == 0)
            {
                return 1.0;
            }

            return (double)totalSumOfHiddenAttributesForClasses / (totalSumOfOpenAttributesForClasses + totalSumOfHiddenAttributesForClasses);
        }

        public double CalculateMethodInheritanceFactor()
        {
            var totalSumOfInheritedMethodsForClasses = _classes.Sum(c => GetNumberOfInheritedMethodsForClass(c));

            if (totalSumOfInheritedMethodsForClasses == 0)
            {
                return 0;
            }

            var allMethods = _classes.Sum(c => GetAllAvailableMethods(c));

            if (allMethods == 0)
            {
                return 0;
            }

            return (double)totalSumOfInheritedMethodsForClasses / allMethods;
        }

        public double CalculateAttributeInheritanceFactor()
        {
            var totalSumOfInheritedAndOverriddenFieldsForClasses = _classes.Sum(c => GetNumberOfInheritedFieldsForClass(c));

            if (totalSumOfInheritedAndOverriddenFieldsForClasses == 0)
            {
                return 0;
            }

            var allFields = _classes.Sum(c => GetAllAvailableFields(c));

            if (allFields == 0)
            {
                return 0;
            }

            return (double)totalSumOfInheritedAndOverriddenFieldsForClasses / allFields;
        }

        public double CalculatePolymorphismObjectFactor()
        {
            var numerator = _classes.Sum(c => GetNumberOfOverriddenMethodsForClass(c));
            var denominator = _classes.Sum(c => GetNumberOfNewMethodsForClass(c) * CalculateDescendantsCount(c));

            return (double)numerator / denominator;
        }

        private static List<Type> LoadClasses(Assembly assembly)
        {
            bool IsStaticClass(Type type)
            {
                return type.IsClass &&
                       type.IsAbstract &&
                       type.IsSealed;
            }

            return assembly.GetTypes().Where(t => t.IsClass && !IsStaticClass(t)).ToList();
        }

        private static Dictionary<Type, HashSet<Type>> BuildHierarchyGraph(List<Type> classes)
        {
            var hierarchyGraph = new Dictionary<Type, HashSet<Type>>();

            foreach (var cls in classes)
            {
                var baseType = cls.BaseType;
                if (baseType != null && baseType != typeof(object))
                {
                    if (!hierarchyGraph.ContainsKey(baseType))
                    {
                        hierarchyGraph[baseType] = new HashSet<Type>();
                    }
                    hierarchyGraph[baseType].Add(cls);
                }
            }

            foreach (var cls in classes)
            {
                if (!hierarchyGraph.ContainsKey(cls))
                {
                    hierarchyGraph[cls] = new HashSet<Type>();
                }
            }

            return hierarchyGraph;
        }

        private List<Type> GetDepth(Type? type)
        {
            if (type == null || type == typeof(object))
            {
                return [];
            }

            var nextDepth = GetDepth(type.BaseType);
            nextDepth.Add(type);
            return nextDepth;
        }

        private static bool IsCountableMethod(MethodInfo m) =>
    !m.IsSpecialName &&
    !m.Name.Contains('<') &&
    m.DeclaringType != typeof(object);

        private static int GetOpenMethodsForClass(Type type)
        {
            var openMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsVirtual || (m.IsVirtual && m.GetBaseDefinition().DeclaringType == type))
                .ToList();

            return openMethods.Count();
        }

        private static int GetHiddenMethodsForClass(Type type)
        {
            var hiddenMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsVirtual || (m.IsVirtual && m.GetBaseDefinition().DeclaringType == type));

            return hiddenMethods.Count();
        }

        private static int GetOpenAttributesForClass(Type type)
        {
            var openAttributes = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var openProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            return openAttributes.Length + openProperties.Length;
        }

        private static int GetHiddenAttributesForClass(Type type)
        {
            var hiddenAttributes = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var hiddenProperties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            return hiddenAttributes.Length + hiddenProperties.Length;
        }

        private static int GetNumberOfInheritedFieldsForClass(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.DeclaringType != type
                && m.DeclaringType != typeof(object))
                .ToList();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.DeclaringType != type
                && m.DeclaringType != typeof(object))
                .ToList();

            return fields.Count + properties.Count;
        }

        private static int GetAllAvailableMethods(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => IsCountableMethod(m) && ((m.DeclaringType != type && !m.IsPrivate) || m.DeclaringType == type));

            return methods.Count();
        }

        private static int GetAllAvailableFields(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.DeclaringType != typeof(object) && ((m.DeclaringType != type && !m.IsPrivate) || m.DeclaringType == type));

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.DeclaringType != typeof(object) && ((m.DeclaringType != type && (!m.GetGetMethod()?.IsPrivate ?? false)) || m.DeclaringType == type));

            return fields.Count() + properties.Count();
        }

        private static int GetNumberOfNewMethodsForClass(Type type)
        {
            return type.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m =>
                    !m.IsPrivate &&
                    IsCountableMethod(m) &&
                    m.GetBaseDefinition().DeclaringType == type)
                .Count();
        }

        private int GetNumberOfOverriddenMethodsForClass(Type type)
        {
            var methodsOverridden = type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m =>
                    !m.IsPrivate &&
                    IsCountableMethod(m) &&
                    m.GetBaseDefinition().DeclaringType != type &&
                    (m.GetBaseDefinition().IsVirtual || m.GetBaseDefinition().IsAbstract) &&
                    _classes.Contains(m.GetBaseDefinition().DeclaringType))
                .ToList();

            return methodsOverridden.Count;
        }

        private int GetNumberOfInheritedMethodsForClass(Type type)
        {
            var methodsInherited = type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => IsCountableMethod(m) &&
                    !m.IsPrivate &&
                    m.DeclaringType != type)
                .ToList();

            return methodsInherited.Count;
        }

        private int CalculateDescendantsCount(Type type)
        {
            if (!_classInheritanceHierarchyGraph.ContainsKey(type) || _classInheritanceHierarchyGraph[type].Count == 0)
            {
                return 0;
            }

            int count = _classInheritanceHierarchyGraph[type].Count;
            foreach (var child in _classInheritanceHierarchyGraph[type])
            {
                count += CalculateDescendantsCount(child);
            }

            return count;
        }
    }
}
