﻿using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpMap.Data.Providers.Db.Expressions;
using SharpMap.Expressions;
using SharpMap.Utilities;
using SharpMap.Utilities.SridUtility;

namespace SharpMap.Data.Providers.Db.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        static UnitTest1()
        {
            SridMap.DefaultInstance =
                new SridMap(new[] { new SridProj4Strategy(0, new GeometryServices().CoordinateSystemFactory) });
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        [TestMethod]
        public void TestSqlServer2008()
        {
            GeometryServices services = new GeometryServices();

            MsSqlServer2008Provider<long> search = new MsSqlServer2008Provider<long>(services.DefaultGeometryFactory,
                                                                                     ConfigurationManager.
                                                                                         ConnectionStrings["sql2008"].
                                                                                         ConnectionString,
                                                                                     "dbo",
                                                                                     "vw_iMARS_BRANCH", "ACSId", "Geom")
                                                       {
                                                           DefaultProviderProperties
                                                               = new ProviderPropertiesExpression(
                                                               new ProviderPropertyExpression[]
                                                                   {
                                                                       new WithNoLockExpression(true),
                                                                       new ForceIndexExpression(true)
                                                                   })
                                                       };

            CollectionBinaryExpression binaryExpression =
                new CollectionBinaryExpression(new PropertyNameExpression("PostCode"), CollectionOperator.In,
                                               new CollectionExpression(new[] { 3, 4, 5, 6 }));

            ProviderPropertiesExpression providerProps =
                new ProviderPropertiesExpression(
                    new ProviderPropertyExpression[]
                        {
                            new WithNoLockExpression(true),
                            //new OrderByCollectionExpression(new[] {"PostCode"}),
                            new ForceIndexExpression(true),
                            new IndexNamesExpression(new[] {"Index1", "Index2"})
                        });


            ProviderQueryExpression prov = new ProviderQueryExpression(providerProps, new AllAttributesExpression(),
                                                                       binaryExpression);

            object obj = search.ExecuteQuery(prov);

            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestSqlServer2008Distance()
        {
            GeometryServices services = new GeometryServices();

            MsSqlServer2008Provider<long> search = new MsSqlServer2008Provider<long>(services.DefaultGeometryFactory,
                                                                                     ConfigurationManager.
                                                                                         ConnectionStrings["sql2008"].
                                                                                         ConnectionString,
                                                                                     "dbo",
                                                                                     "GeoPlaces", "OId", "Geom")
                                                       {
                                                           DefaultProviderProperties
                                                               = new ProviderPropertiesExpression(
                                                               new ProviderPropertyExpression[]
                                                                   {
                                                                       new WithNoLockExpression(true),
                                                                       new ForceIndexExpression(true)
                                                                   })
                                                       };

            IGeometry point = services["EPSG:27700"].CreatePoint2D(500000, 180000);

            SpatialAnalysisExpression analysisExpresssion =
                new SpatialAnalysisDistanceExpression(new GeometryExpression(null),
                                                      new GeometryExpression(point));


            BinaryExpression expression = new BinaryExpression(analysisExpresssion, BinaryOperator.LessThanOrEqualTo,
                                                               new LiteralExpression<double>(2000.00));
            ProviderPropertiesExpression providerProps =
                new ProviderPropertiesExpression(
                    new ProviderPropertyExpression[]
                        {
                            new WithNoLockExpression(true),
                            new ForceIndexExpression(true)
                        });


            ProviderQueryExpression prov = new ProviderQueryExpression(providerProps, new AllAttributesExpression(),
                                                                       expression);

            object obj = search.ExecuteQuery(prov);

            Assert.IsNotNull(obj);
        }


        [TestMethod]
        public void TestSqlServer2008Paged()
        {
            GeometryServices services = new GeometryServices();

            MsSqlServer2008Provider<long> search = new MsSqlServer2008Provider<long>(services.DefaultGeometryFactory,
                                                                                     ConfigurationManager.
                                                                                         ConnectionStrings["sql2008"].
                                                                                         ConnectionString,
                                                                                     "dbo",
                                                                                     "vw_iMARS_BRANCH", "ACSId", "Geom")
                                                       {
                                                           DefaultProviderProperties
                                                               = new ProviderPropertiesExpression(
                                                               new ProviderPropertyExpression[]
                                                                   {
                                                                       new WithNoLockExpression(true),
                                                                       new ForceIndexExpression(true)
                                                                   })
                                                       };

            //var binaryExpression =
            //    new CollectionBinaryExpression(new PropertyNameExpression("PostCode"), CollectionOperator.In, new CollectionExpression(new[] { 3, 4, 5, 6 }));


            AttributeBinaryStringExpression binaryExpression = new AttributeBinaryStringExpression("PostCode",
                                                                                                   BinaryStringOperator.
                                                                                                       StartsWith, "W");


            ProviderPropertiesExpression providerProps =
                new ProviderPropertiesExpression(
                    new ProviderPropertyExpression[]
                        {
                            new WithNoLockExpression(true),
                            //new OrderByCollectionExpression(new[] {"PostCode"}),
                            new ForceIndexExpression(true),
                            new IndexNamesExpression(new[] {"Index1", "Index2"}),
                            new DataPageSizeExpression(10),
                            new DataPageNumberExpression(5)
                        });


            ProviderQueryExpression prov = new ProviderQueryExpression(providerProps, new AllAttributesExpression(),
                                                                       binaryExpression);

            object obj = search.ExecuteQuery(prov);

            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestMsSqlSpatialPaged()
        {
            GeometryServices services = new GeometryServices();

            MsSqlSpatialProvider search = new MsSqlSpatialProvider(services.DefaultGeometryFactory,
                                                                   ConfigurationManager.ConnectionStrings["mssqlspatial"
                                                                       ].
                                                                       ConnectionString,
                                                                   "ST",
                                                                   "dbo",
                                                                   "BRoads", "OID", "the_geom")
                                              {
                                                  DefaultProviderProperties
                                                      = new ProviderPropertiesExpression(
                                                      new ProviderPropertyExpression[]
                                                          {
                                                              new WithNoLockExpression(true),
                                                              new ForceIndexExpression(true)
                                                          })
                                              };

            //var binaryExpression =
            //    new CollectionBinaryExpression(new PropertyNameExpression("PostCode"), CollectionOperator.In, new CollectionExpression(new[] { 3, 4, 5, 6 }));


            AttributeBinaryStringExpression binaryExpression = new AttributeBinaryStringExpression("NAME",
                                                                                                   BinaryStringOperator.
                                                                                                       StartsWith, "W");


            ProviderPropertiesExpression providerProps =
                new ProviderPropertiesExpression(
                    new ProviderPropertyExpression[]
                        {
                            new WithNoLockExpression(true),
                            //new OrderByCollectionExpression(new[] {"NAME"}),
                            new ForceIndexExpression(true),
                            new IndexNamesExpression(new[] {"Index1", "Index2"}),
                            new DataPageSizeExpression(10),
                            new DataPageNumberExpression(5)
                        });


            ProviderQueryExpression prov = new ProviderQueryExpression(providerProps, new AllAttributesExpression(),
                                                                       binaryExpression);

            object obj = search.ExecuteQuery(prov);

            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestSqLite()
        {
            GeometryServices services = new GeometryServices();

            SpatiaLite2Provider search = new SpatiaLite2Provider(services.DefaultGeometryFactory,
                                                                 ConfigurationManager.ConnectionStrings["sqLite"].
                                                                     ConnectionString,
                                                                 "main",
                                                                 "regions", "OID", "XGeometryX");
            search.SpatiaLiteIndexType = SpatiaLite2IndexType.MBRCache;

            BinaryExpression binaryExpression =
                new BinaryExpression(new PropertyNameExpression("VHG5"),
                                     BinaryOperator.GreaterThan, new LiteralExpression<int>(6));

            ProviderPropertiesExpression providerProps =
                new ProviderPropertiesExpression(
                    new ProviderPropertyExpression[] { });


            ProviderQueryExpression prov = new ProviderQueryExpression(providerProps, new AllAttributesExpression(),
                                                                       binaryExpression);

            object obj = search.ExecuteQuery(prov);

            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestGetExtentsByOid()
        {
            GeometryServices services = new GeometryServices();

            MsSqlServer2008Provider<long> search = new MsSqlServer2008Provider<long>(services.DefaultGeometryFactory,
                                                                                     ConfigurationManager.
                                                                                         ConnectionStrings["sql2008"].
                                                                                         ConnectionString,
                                                                                     "dbo",
                                                                                     "vw_iMARS_BRANCH", "ACSId", "Geom")
                                                       {
                                                           DefaultProviderProperties
                                                               = new ProviderPropertiesExpression(
                                                               new ProviderPropertyExpression[]
                                                                   {
                                                                       new WithNoLockExpression(true),
                                                                       new ForceIndexExpression(true)
                                                                   })
                                                       };


            IExtents extents = search.GetExtentsByOid(1);
            Assert.IsNotNull(extents);
            Assert.IsFalse(extents.IsEmpty);
        }

        [TestMethod]
        public void TestGetFeatureByOid()
        {
            GeometryServices services = new GeometryServices();

            MsSqlServer2008Provider<long> search = new MsSqlServer2008Provider<long>(services.DefaultGeometryFactory,
                                                                                     ConfigurationManager.
                                                                                         ConnectionStrings["sql2008"].
                                                                                         ConnectionString,
                                                                                     "dbo",
                                                                                     "vw_iMARS_BRANCH", "ACSId", "Geom")
                                                       {
                                                           DefaultProviderProperties
                                                               = new ProviderPropertiesExpression(
                                                               new ProviderPropertyExpression[]
                                                                   {
                                                                       new WithNoLockExpression(true),
                                                                       new ForceIndexExpression(true)
                                                                   })
                                                       };


            IFeatureDataRecord record = search.GetFeatureByOid(1);
            Assert.IsNotNull(record);
        }


        [TestMethod]
        public void TestSridMap()
        {
            SridMap map = SridMap.DefaultInstance;

            ICoordinateSystem cs = map.Process(27700, default(ICoordinateSystem));

            Assert.IsNotNull(cs);


            int? srid = map.Process(cs, (int?)null);

            Assert.IsTrue(srid == 27700);


            ICoordinateSystem cs2 = map.Process("EPSG:27700", default(ICoordinateSystem));

            Assert.IsTrue(ReferenceEquals(cs, cs2));


            string code = map.Process(cs2, "");

            Debug.WriteLine(code);

            Assert.IsTrue(code == "EPSG:27700");
        }

        [TestMethod]
        public void TestWktToSridMap()
        {
            string wkt =
                "PROJCS[\"British_National_Grid\",GEOGCS[\"GCS_OSGB_1936\",DATUM[\"D_OSGB_1936\",SPHEROID[\"Airy_1830\",6377563.396,299.3249646]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",400000],PARAMETER[\"False_Northing\",-100000],PARAMETER[\"Central_Meridian\",-2],PARAMETER[\"Scale_Factor\",0.999601272],PARAMETER[\"Latitude_Of_Origin\",49],UNIT[\"Meter\",1]]";

            SridMap map = new SridMap(new[] { new SridProj4Strategy(0, new GeometryServices().CoordinateSystemFactory) });

            int? srid = map.Process(wkt, (int?)null);

            Assert.IsNotNull(srid);

            Assert.IsTrue(srid == 27700);
        }


        [TestMethod]
        public void TestEpsgToSridMap()
        {
            string wkt = "EPSG:27700";
            SridMap map = new SridMap(new[] { new SridProj4Strategy(0, new GeometryServices().CoordinateSystemFactory) });

            int? srid = map.Process(wkt, (int?)null);

            Assert.IsNotNull(srid);

            Assert.IsTrue(srid == 27700);
        }


        [TestMethod]
        public void TestGeometryProvider()
        {
            string wkt;
            GeometryServices g = new GeometryServices();
            GeometryProvider prov;

            IGeometry geom = g.DefaultGeometryFactory.WktReader.Read(File.ReadAllText("D:\\geometries.txt"));

            prov = new GeometryProvider(new[] { geom });


            using (IFeatureDataReader fdr =
                prov.ExecuteFeatureQuery(new FeatureQueryExpression(geom.Extents, SpatialOperation.Intersects)))
            {
                while (fdr.Read())
                {
                    Debug.WriteLine(fdr.Geometry.AsText());
                }
            }
        }

        [TestMethod]
        public void TestTOWGS84Conversion()
        {
            string fromWkt =
                "PROJCS[\"LGD2006 / UTM zone 32N\",GEOGCS[\"LGD2006\",DATUM[\"Libyan_Geodetic_Datum_2006\",SPHEROID[\"International 1924\",6378388,297,AUTHORITY[\"EPSG\",\"7022\"]],TOWGS84[-87,-98,-121,0,0,0,0],AUTHORITY[\"EPSG\",\"6754\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4754\"]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",9],PARAMETER[\"scale_factor\",0.9996],PARAMETER[\"false_easting\",500000],PARAMETER[\"false_northing\",-4000000],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],AUTHORITY[\"EPSG\",\"3199\"]]";
            string toWkt =
                "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]";

            GeometryServices gs = new GeometryServices();
            ICoordinateSystem srs = gs.CoordinateSystemFactory.CreateFromWkt(fromWkt);
            ICoordinateSystem dst = gs.CoordinateSystemFactory.CreateFromWkt(toWkt); //WGS84
            ICoordinateTransformation ics = gs.CoordinateTransformationFactory.CreateFromCoordinateSystems(srs, dst);
            Assert.IsNotNull(ics);
        }

        [TestMethod]
        public void TestMsSqlServer2008OrderBy()
        {
            GeometryServices services = new GeometryServices();

            MsSqlServer2008Provider<long> search = new MsSqlServer2008Provider<long>(services.DefaultGeometryFactory,
                                                                                     ConfigurationManager.
                                                                                         ConnectionStrings["sql2008"].
                                                                                         ConnectionString,
                                                                                     "dbo",
                                                                                     "vwGeoCustomers", "OID", "Geom")
            {
                DefaultProviderProperties
                    = new ProviderPropertiesExpression(
                    new ProviderPropertyExpression[]
                                                                   {
                                                                       new WithNoLockExpression(true),
                                                                       new ForceIndexExpression(true)
                                                                   })
            };

            //var binaryExpression =
            //    new CollectionBinaryExpression(new PropertyNameExpression("PostCode"), CollectionOperator.In, new CollectionExpression(new[] { 3, 4, 5, 6 }));


            AttributeBinaryStringExpression binaryExpression = new AttributeBinaryStringExpression("PostCode",
                                                                                                   BinaryStringOperator.
                                                                                                       StartsWith, "W");


            IGeometry testPoint = new GeometryServices()["EPSG:27700"].CreatePoint2D(500000, 180000);
            ProviderPropertiesExpression providerProps =
                new ProviderPropertiesExpression(
                    new ProviderPropertyExpression[]
                        {
                            new WithNoLockExpression(true),
                            //new OrderByCollectionExpression(
                            //    new []
                            //        {
                            //            new OrderByExpression(
                            //                new SpatialAnalysisDistanceExpression(new GeometryExpression(null),new GeometryExpression(testPoint) ),SortOrder.Ascending),
                            //        }),
                           
                            new DataPageSizeExpression(10),
                            new DataPageNumberExpression(0)
                        });


            ProviderQueryExpression prov = new ProviderQueryExpression(providerProps, new AllAttributesExpression(),
                                                                       binaryExpression,
                                                                       new SortExpressionCollectionExpression(
                                                                           new[]{new SortExpression( 
                                                                               new SpatialAnalysisDistanceExpression(
                                                                                   new GeometryExpression(null),
                                                                                   new GeometryExpression(testPoint)),SortOrder.Ascending)}));

            object obj = search.ExecuteQuery(prov);

            Assert.IsNotNull(obj);
        }
    }
}