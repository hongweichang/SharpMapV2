﻿/*
 *	This file is part of SharpMap.Demo.FormatConverter
 *  SharpMap.Demo.FormatConverter is free software © 2008 Newgrove Consultants Limited, 
 *  http://www.newgrove.com; you can redistribute it and/or modify it under the terms 
 *  of the current GNU Lesser General Public License (LGPL) as published by and 
 *  available from the Free Software Foundation, Inc., 
 *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/    
 *  This program is distributed without any warranty; 
 *  without even the implied warranty of merchantability or fitness for purpose.  
 *  See the GNU Lesser General Public License for the full details. 
 *  
 *  Author: John Diss 2008
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Demo.FormatConverter.Common;
using SharpMap.Expressions;
using SharpMap.Utilities;

namespace SharpMap.Demo.FormatConverter
{
    public class Converter
    {
        public static readonly List<ProviderItem> _configureSourceProviders =
            new List<ProviderItem>();

        public static readonly List<ProviderItem> _configureTargetProviders =
            new List<ProviderItem>();

        public static readonly List<ProcessorItem> _featureDataRecordProcessors =
            new List<ProcessorItem>();

        private static readonly IGeometryServices _geometryServices;
        private bool firstRun = true;

        static Converter()
        {
            _geometryServices = new GeometryServices();

            ///ensure all the assemblies which may contain 'plugins' are loaded
            foreach (FileInfo f in new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.dll"))
            {
                AssemblyName name = AssemblyName.GetAssemblyName(f.Name);
                AppDomain.CurrentDomain.Load(name);
            }


            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                ///search for 'plugins' these are types decorated with certain attributes
                foreach (Type t in asm.GetTypes())
                {
                    object[] attrs = t.GetCustomAttributes(typeof(ConfigureProviderAttribute), true);
                    if (attrs.Length == 1)
                    {
                        var attr = (ConfigureProviderAttribute)attrs[0];
                        var pi = new ProviderItem { Builder = t, Name = attr.Name, ProviderType = attr.ProviderType };

                        if (typeof(IConfigureFeatureSource).IsAssignableFrom(t))
                            _configureSourceProviders.Add(pi);

                        if (typeof(IConfigureFeatureTarget).IsAssignableFrom(t))
                            _configureTargetProviders.Add(pi);
                    }

                    attrs = t.GetCustomAttributes(typeof(FeatureDataRecordProcessorAttribute), true);
                    if (attrs.Length > 0)
                    {
                        var attr = (FeatureDataRecordProcessorAttribute)attrs[0];
                        _featureDataRecordProcessors.Add(new ProcessorItem
                                                             {
                                                                 Description = attr.Description,
                                                                 Name = attr.Name,
                                                                 ProcessorType = t
                                                             });
                    }
                }
            }
        }


        public IList<ProviderItem> SourceProviders
        {
            get { return _configureSourceProviders; }
        }

        public IList<ProviderItem> TargetProviders
        {
            get { return _configureTargetProviders; }
        }

        public IList<ProcessorItem> FeatureProcessors
        {
            get { return _featureDataRecordProcessors; }
        }


        public bool Run()
        {
            if (firstRun)
            {
                ShowIntroduction();
                firstRun = false;
            }

            if (SourceProviders.Count == 0)
            {
                Console.WriteLine("\nNo source providers loaded");
                Console.ReadLine();
                return false;
            }

            if (TargetProviders.Count == 0)
            {
                Console.WriteLine("\nNo target providers loaded");
                Console.ReadLine();
                return false;
            }

            ProviderItem input = GetSourceProvider();

            IList<ProcessorItem> processors = new List<ProcessorItem>(GetProcessors());

            ProviderItem output = GetTargetProvider();

            var confirmBuilder = new StringBuilder();
            confirmBuilder.AppendLine("\nReady to process the following chain:\n");
            confirmBuilder.AppendLine("Read from:");
            confirmBuilder.AppendLine("\t" + input.Name);
            if (processors.Count > 0)
                confirmBuilder.AppendLine("Process with:");

            foreach (ProcessorItem pi in processors)
                confirmBuilder.AppendLine("\t" + pi.Name);

            confirmBuilder.AppendLine("Export to:");
            confirmBuilder.AppendLine("\t" + output.Name);


            Console.WriteLine(confirmBuilder.ToString());

            Console.WriteLine("\nType Y to continue or any other key to start over.");

            if (string.Compare(Console.ReadLine(), "Y", true) == 0)
                DoConversion(input, processors, output);


            Console.WriteLine("\nRun again? Y to run again any other key to quit");
            return string.Compare(Console.ReadLine(), "Y", true) == 0;
        }

        private void DoConversion(ProviderItem input, IEnumerable<ProcessorItem> processors, ProviderItem output)
        {
            using (IConfigureFeatureSource csource = (IConfigureFeatureSource)Activator.CreateInstance(input.Builder))
            {
                IFeatureProvider psource = csource.ConstructSourceProvider(_geometryServices);
                Type srcOidType = GetTypeParamsOfImplementedInterface(psource.GetType(), typeof(IFeatureProvider<>))[0];
                var realProcessors = new List<IProcessFeatureDataRecords>();

                foreach (ProcessorItem pi in processors)
                    realProcessors.Add((IProcessFeatureDataRecords)Activator.CreateInstance(pi.ProcessorType));

                Func<IEnumerable<IFeatureDataRecord>, IEnumerable<IFeatureDataRecord>> processChain = null;

                foreach (IProcessFeatureDataRecords processor in realProcessors)
                {
                    processChain = Equals(processChain, null)
                                       ? processor.Process
                                       : new Func<IEnumerable<IFeatureDataRecord>, IEnumerable<IFeatureDataRecord>>(
                                             o => processor.Process(processChain(o)));
                }

                processChain = processChain ??
                               new Func<IEnumerable<IFeatureDataRecord>, IEnumerable<IFeatureDataRecord>>(o => o);

                if (!psource.IsOpen)
                    psource.Open();

                FeatureQueryExpression exp = csource.ConstructSourceQueryExpression();
                IEnumerable<IFeatureDataRecord> sourceRecords = processChain(psource.ExecuteFeatureQuery(exp));

                //jd NOTE: for the time being we are assuming that the processors didn't change the shape of the original table

                FeatureDataTable fdt = psource.CreateNewTable();


                using (
                    IConfigureFeatureTarget ctarget =
                        (IConfigureFeatureTarget)Activator.CreateInstance(output.Builder))
                {
                    Type oidType =
                        GetTypeParamsOfBaseClass(fdt.GetType(), typeof(FeatureDataTable<>))[0];

                    using (IWritableFeatureProvider ptarget =
                         ctarget.ConstructTargetProvider(oidType, psource.GeometryFactory,
                                                                   _geometryServices.CoordinateSystemFactory,
                                                                   fdt))
                    {
                        if (!ptarget.IsOpen)
                            ptarget.Open();

                        //jd: TODO: sort out FeatureDataTable type clashes 
                        //e.g ShapeFileProvider (FeatureDataTable<UInt32>) and MsSqlServer2008Provider<Int32> (FeatureDataTable<Int32>)

                        Console.WriteLine("Beginning Import.");
                        int count = 0;
                        foreach (IFeatureDataRecord fdr in sourceRecords)
                        {
                            FeatureDataRow dfrs = fdt.NewRow();
                            dfrs.Geometry = fdr.Geometry;
                            object[] vals = new object[fdr.FieldCount];
                            fdr.GetValues(vals);
                            dfrs.ItemArray = vals;
                            ptarget.Insert(dfrs);
                            count++;
                        }

                        ptarget.Close();
                        Console.WriteLine(string.Format("{0} records processed", count));

                        ctarget.PostImport();
                    }
                }

            }

            Console.WriteLine("Finished");
        }

        private FeatureDataTable GetTypedFeatureDataTable(Type oidType, string oidColumnName, IGeometryFactory geometryFactory)
        {
            Type baseType = typeof(FeatureDataTable<>);
            Type realType = baseType.MakeGenericType(new[] { oidType });
            return (FeatureDataTable)Activator.CreateInstance(realType, oidColumnName, geometryFactory);
        }


        private Type[] GetTypeParamsOfImplementedInterface(Type implementor, Type interfaceToFind)
        {
            Type[] implementations = implementor.GetInterfaces();
            foreach (Type t in implementations)
            {
                if (!t.IsGenericType)
                    continue;

                if (t.GetGenericTypeDefinition() != interfaceToFind)
                    continue;

                return t.GetGenericArguments();
            }
            return null;
        }

        private Type[] GetTypeParamsOfBaseClass(Type implementor, Type baseClassToFind)
        {

            for (Type t = implementor; t != null; t = t.BaseType)
            {
                if (!t.IsGenericType)
                    continue;

                if (t.GetGenericTypeDefinition() != baseClassToFind)
                    continue;

                return t.GetGenericArguments();
            }
            return null;
        }

        private ProviderItem GetTargetProvider()
        {
            Console.WriteLine("The following target provider types are loaded:\n");
            for (int i = 0; i < TargetProviders.Count; i++)
            {
                ProviderItem pi = TargetProviders[i];
                Console.WriteLine(string.Format("\t{0} {1}", i, pi.Name));
            }

            while (true)
            {
                Console.WriteLine("\nPlease enter the number for the source provider you wish to use");
                int i;
                if (int.TryParse(Console.ReadLine(), out i))
                {
                    if (i > -1 && i < TargetProviders.Count)
                        return TargetProviders[i];
                }
                Console.WriteLine("Invalid provider");
            }
        }

        private IEnumerable<ProcessorItem> GetProcessors()
        {
            if (FeatureProcessors.Count == 0)
                yield break;

            Console.WriteLine("\nThe following processors are loaded:\n");
            for (int i = 0; i < FeatureProcessors.Count; i++)
            {
                ProcessorItem pi = FeatureProcessors[i];
                Console.WriteLine(string.Format("\t{0} {1} {2}", i, pi.Name, pi.Description));
            }

            Console.WriteLine(
                "\nPlease enter a comma seperated list of processor ids you wish to use,\nin the order you wish to use them.");

            string[] idstring = Console.ReadLine().Split(new[] { ',', ';', '|' });

            foreach (string s in idstring)
            {
                int i;
                if (int.TryParse(s, out i))
                {
                    if (i > -1 && i < FeatureProcessors.Count)
                        yield return FeatureProcessors[i];
                }
            }
        }

        private ProviderItem GetSourceProvider()
        {
            Console.WriteLine("The following source provider types are loaded:\n");
            for (int i = 0; i < SourceProviders.Count; i++)
            {
                ProviderItem pi = SourceProviders[i];
                Console.WriteLine(string.Format("\t{0} {1}", i, pi.Name));
            }

            while (true)
            {
                Console.WriteLine("\nPlease enter the number for the source provider you wish to use");
                int i;
                if (int.TryParse(Console.ReadLine(), out i))
                {
                    if (i > -1 && i < SourceProviders.Count)
                        return SourceProviders[i];
                }
                Console.WriteLine("Invalid provider");
            }
        }

        private void ShowIntroduction()
        {
            Console.WriteLine(
                "Application demonstrating the use of SharpMap v2 \nto convert and process between different data sources.");
            Console.WriteLine("© Newgrove Consultants Ltd / John Diss 2008 www.newgrove.com\n ");
        }

        #region Nested type: ProcessorItem

        public struct ProcessorItem
        {
            public Type ProcessorType { get; internal set; }
            public string Name { get; internal set; }
            public string Description { get; internal set; }
        }

        #endregion

        #region Nested type: ProviderItem

        public struct ProviderItem
        {
            public Type ProviderType { get; internal set; }
            public string Name { get; internal set; }
            public Type Builder { get; internal set; }
        }

        #endregion
    }
}