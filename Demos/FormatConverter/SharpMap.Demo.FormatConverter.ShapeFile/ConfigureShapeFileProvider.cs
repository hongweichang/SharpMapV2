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
using System.IO;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Data.Providers.ShapeFile;
using SharpMap.Demo.FormatConverter.Common;
using SharpMap.Expressions;
using SharpMap.Utilities;

namespace SharpMap.Demo.FormatConverter.ShapeFile
{
    [ConfigureProvider(typeof(ShapeFileProvider), "ShapeFile")]
    public class ConfigureShapeFileProvider : IConfigureFeatureSource, IConfigureFeatureTarget
    {
        private ShapeFileProvider _sourceProvider;
        private ShapeFileProvider _targetProvider;
        private bool disposed;

        #region IConfigureFeatureSource Members

        public IFeatureProvider ConstructSourceProvider(IGeometryServices geometryServices)
        {
            string path;
            while (true)
            {
                Console.WriteLine("Please enter the path to the input shapefile");
                path = Console.ReadLine();

                if (File.Exists(path))
                    break;
                Console.WriteLine("Invalid Path");
            }

            _sourceProvider = new ShapeFileProvider(path, geometryServices.DefaultGeometryFactory,
                                              geometryServices.CoordinateSystemFactory) { IsSpatiallyIndexed = false };
            _sourceProvider.Open();
            Console.WriteLine("\nINFO The shape type is: " + _sourceProvider.ShapeType + "\n");

            if (Array.IndexOf(new ShapeType[] { ShapeType.PointM, ShapeType.PointZ, ShapeType.MultiPointM, ShapeType.MultiPointZ, ShapeType.PolygonM, ShapeType.PolygonZ }, _sourceProvider.ShapeType) > -1)
                Console.WriteLine("Warning: The source shapefile contains Z or M values. This is work in progress and can cause issues.");

            ForceCoordinateOptions[] coordinateOptions = (ForceCoordinateOptions[])Enum.GetValues(typeof(ForceCoordinateOptions));
            Dictionary<int, ForceCoordinateOptions> fco = new Dictionary<int, ForceCoordinateOptions>();
            for (int i = 0; i < coordinateOptions.Length; i++)
                fco.Add(i, coordinateOptions[i]);

            ShapeFileReadStrictness[] strictnesses = (ShapeFileReadStrictness[])Enum.GetValues(typeof(ShapeFileReadStrictness));
            Dictionary<int, ShapeFileReadStrictness> strictness = new Dictionary<int, ShapeFileReadStrictness>();

            for (int i = 0; i < strictnesses.Length; i++)
                strictness.Add(i, strictnesses[i]);

            Console.WriteLine("Force Coordinate Options? Enter the id of one of the options below or leave blank.");
            foreach (KeyValuePair<int, ForceCoordinateOptions> option in fco)
            {
                Console.WriteLine(string.Format("{0} {1}", option.Key, option.Value));
            }
            int opt;
            if (int.TryParse(Console.ReadLine(), out opt))
            {
                if (fco.ContainsKey(opt))
                    _sourceProvider.ForceCoordinateOptions = fco[opt];

                Console.WriteLine(string.Format("The effective shape type is now {0}", _sourceProvider.EffectiveShapeType));
            }

            Console.WriteLine("Please select a Leniency option:");
            foreach (KeyValuePair<int, ShapeFileReadStrictness> pair in strictness)
            {
                Console.WriteLine(String.Format("{0} {1}", pair.Key, pair.Value));
            }

            if (int.TryParse(Console.ReadLine(), out opt))
            {
                if (strictness.ContainsKey(opt))
                    _sourceProvider.ReadStrictness = strictness[opt];
            }


            return _sourceProvider;
        }

        public virtual FeatureQueryExpression ConstructSourceQueryExpression()
        {
            CheckOpen();
            return new FeatureQueryExpression(
                new AllAttributesExpression(), null, null);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IConfigureFeatureTarget Members
        /// <remarks>
        /// Note <paramref name="oidType"/> Is ignored for shapefile provider.
        /// </remarks>
        public IWritableFeatureProvider ConstructTargetProvider(Type oidType, IGeometryFactory geometryFactory, ICoordinateSystemFactory csFactory, FeatureDataTable schemaTable)
        {
            EnsureColumnNamesValid(schemaTable);

            string directoryPath = GetDirectoryPath();

            CreateAction overwrite;
            string layerName = GetLayerName(directoryPath, out overwrite);

            ShapeType shapeType = GetShapeType();


            if (overwrite == CreateAction.Append)

                _targetProvider = new ShapeFileProvider(Path.Combine(directoryPath, layerName + ".shp"),
                                                        geometryFactory,
                                                        csFactory);
            else
                _targetProvider = ShapeFileProvider.Create(directoryPath, layerName, shapeType, schemaTable,
                                                           geometryFactory,
                                                           csFactory);

            _targetProvider.Open(WriteAccess.Exclusive);
            return _targetProvider;
        }

        private void EnsureColumnNamesValid(FeatureDataTable schemaTable)
        {
            foreach (DataColumn c in schemaTable.Columns)
                if (c.ColumnName.Length > MaxDbaseColumnNameLength)
                    throw new InvalidOperationException(string.Format("Column Name '{0}' is longer than the Dbase maximum length of {1}", c.ColumnName, MaxDbaseColumnNameLength));
        }

        private static ShapeType GetShapeType()
        {
            string[] names = Enum.GetNames(typeof(ShapeType));

            Console.WriteLine("Shapefiles allow only homogenous geometry types. The following shape types are available.\n");
            for (int i = 0; i < names.Length; i++)
            {
                Console.WriteLine(string.Format("\t{0} {1}", i, names[i]));
            }

            while (true)
            {
                Console.WriteLine("Please enter the id of the shape type to export.\nIt must match the geometry type in the input data set.");

                int i;

                if (int.TryParse(Console.ReadLine(), out i))
                {
                    if (i > -1 && i < names.Length)
                        return (ShapeType)Enum.Parse(typeof(ShapeType), names[i]);
                }

                Console.WriteLine("Invalid option.");
            }
        }

        private enum CreateAction
        {
            Append, CreateNew
        }

        private static readonly string[] mergeactions = new[] { "A", "a" };
        private const int MaxDbaseColumnNameLength = 11;

        private string GetLayerName(string directory, out CreateAction exaction)
        {
            while (true)
            {
                string layerName = null;

                Console.WriteLine("Please enter the name for the exported shapefile");
                layerName = Console.ReadLine();

                if (layerName != null && layerName.EndsWith(".shp", StringComparison.CurrentCultureIgnoreCase))
                    layerName = layerName.Substring(0,
                                                    layerName.IndexOf(".shp", StringComparison.CurrentCultureIgnoreCase));

                if (string.IsNullOrEmpty(layerName))
                {
                    Console.WriteLine("Invalid name");
                    continue;
                }

                if (File.Exists(string.Format("{0}\\{1}.shp", directory, layerName)))
                {
                    while (true)
                    {
                        Console.WriteLine("Shapefile already exists. Type A to append or any other key to choose another name.");

                        string action = Console.ReadLine();

                        if (Array.IndexOf(mergeactions, action) == -1)
                        {
                            Console.WriteLine("Invalid option.");
                            continue;
                        }

                        if (action == "A" || action == "a")
                            exaction = CreateAction.Append;
                        else
                            return GetLayerName(directory, out exaction);
                        return layerName;
                    }
                }
                exaction = CreateAction.CreateNew;
                return layerName;
            }
        }

        private string GetDirectoryPath()
        {
            while (true)
            {
                Console.WriteLine("Please enter the path to the output directory.");
                string directoryPath = Console.ReadLine();

                if (Directory.Exists(directoryPath))
                    return directoryPath;
            }
        }

        #endregion

        private void CheckOpen()
        {
            if (!_sourceProvider.IsOpen)
                _sourceProvider.Open();
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (_sourceProvider != null)
                    _sourceProvider.Close();

                if (_targetProvider != null)
                    _targetProvider.Close();

                disposed = true;
            }
        }

        ~ConfigureShapeFileProvider()
        {
            Dispose(false);
        }

        #region IConfigureFeatureSource Members


        public string OidColumnName
        {
            get { return "OID"; }
        }

        #endregion

        #region IConfigureFeatureTarget Members


        public virtual void PostImport()
        {

        }

        #endregion

        #region IConfigureFeatureSource Members


        public Type OidType
        {
            get { return typeof(UInt32); }
        }

        #endregion
    }
}