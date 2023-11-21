using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;


namespace Rhythm.Utilities
{

    /// <summary>
    /// 
    /// </summary>
    public class ReflectionUtils
    {
        private ReflectionUtils(){}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="revitThing"></param>
        /// <returns></returns>
        public static Dictionary<string,object> GetPropertiesAndValues(global::Revit.Elements.Element revitThing)
        {
            var thing = revitThing.InternalElement;
            var thingType = thing.GetType();

            
            IList<PropertyInfo> props = new List<PropertyInfo>(thingType.GetProperties());

            List<object> propertyValues = new List<object>();

            foreach (PropertyInfo prop in props)
            {
                object propValue = prop.GetValue(thing, null);

                propertyValues.Add(propValue);
            }

            //returns the outputs
            var outInfo = new Dictionary<string, object>
            {
                { "Property Name", props},
                { "Property Value", propertyValues}
            };
            return outInfo;
        }

    }
    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class MiscUtils
    {
        /// <summary>
        /// Get Null
        /// </summary>
        /// <returns></returns>
        public static object GetNull()
        {
            return null;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class CommandHelpers
    {
        /// <summary>
        /// This allows for runtime loading of commands specific to a Revit version. This is nice because we can have Revit 2021 DLLs and more.
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="commandName"></param>
        /// <param name="args"></param>
        /// <returns></returns>      
        public static object InvokeNode(string dllName, string commandName, object[] args)
        {
            var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace("bin", "extra"), dllName);
            byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);
            Assembly assembly = Assembly.Load(assemblyBytes);
            IEnumerable<Type> types = GetTypesSafely(assembly);

          
            foreach (Type objType in types)
            {
                if (objType.IsClass)
                {
                    if (objType.Name.Equals(commandName.Split('.')[0]))
                    {
                        object baseObject = Activator.CreateInstance(objType);

                        return objType.InvokeMember(commandName.Split('.')[1],
                            BindingFlags.Default | BindingFlags.InvokeMethod, null, baseObject, args);
                    }
                }
            }
            return null;
        }
       
        private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class ConvexHullUtilities
    {
        #region Misc
        const double Inch = 1.0 / 12.0;
        // ReSharper disable once ArrangeTypeMemberModifiers
        const double Sixteenth = Inch / 16.0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <typeparam name="tsource"></typeparam>
        /// <typeparam name="tkey"></typeparam>
        /// <returns></returns>
        public static tsource MinBy<tsource, tkey>(
            this IEnumerable<tsource> source,
            Func<tsource, tkey> selector)
        {
            return source.MinBy(selector, Comparer<tkey>.Default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="comparer"></param>
        /// <typeparam name="tsource"></typeparam>
        /// <typeparam name="tkey"></typeparam>
        /// <returns></returns>
        public static tsource MinBy<tsource, tkey>(
            this IEnumerable<tsource> source,
            Func<tsource, tkey> selector,
            IComparer<tkey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            using (IEnumerator<tsource> sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                    throw new InvalidOperationException("Sequence was empty");
                tsource min = sourceIterator.Current;
                tkey minKey = selector(min);
                while (sourceIterator.MoveNext())
                {
                    tsource candidate = sourceIterator.Current;
                    tkey candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }
                return min;
            }
        }
        #endregion
        #region Convex Hull
        /// <summary>
        /// Return the convex hull of a list of points 
        /// using the Jarvis march or Gift wrapping:
        /// https://en.wikipedia.org/wiki/Gift_wrapping_algorithm
        /// Written by Maxence.
        /// </summary>
        public static List<XYZ> ConvexHull(List<XYZ> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            XYZ startPoint = points.MinBy(p => p.X);
            var convexHullPoints = new List<XYZ>();
            XYZ walkingPoint = startPoint;
            XYZ refVector = XYZ.BasisY.Negate();
            do
            {
                convexHullPoints.Add(walkingPoint);
                XYZ wp = walkingPoint;
                XYZ rv = refVector;
                walkingPoint = points.MinBy(p =>
                {
                    double angle = (p - wp).AngleOnPlaneTo(rv, XYZ.BasisZ);
                    if (angle < 1e-10) angle = 2 * Math.PI;
                    return angle;
                });
                refVector = wp - walkingPoint;
            } while (walkingPoint != startPoint);
            convexHullPoints.Reverse();
            return convexHullPoints;
        }
        #endregion // Convex Hull
    }

    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class StringComparisonUtilities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            // Step 1
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }
            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }
            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }
            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

    }
    
}

