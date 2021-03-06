using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using SharpMap.Layers;
using SharpMap.Presentation;
using SharpMap.Presentation.Presenters;
using SharpMap.Presentation.Views;
using SharpMap.Rendering;
using SharpMap.Rendering.Rendering2D;
using SharpMap.Styles;
using SharpMap.Tools;
using Xunit;

#if BUFFERED
using NetTopologySuite.CoordinateSystems;
#else
using BufferedCoordinate = NetTopologySuite.Coordinates.Simple.Coordinate;
using BufferedCoordinateFactory = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using BufferedCoordinateSequence = NetTopologySuite.Coordinates.Simple.CoordinateSequence;
using BufferedCoordinateSequenceFactory = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;
#endif

namespace SharpMap.Tests.Presentation
{
    public class MapPresenter2DTests : IUseFixture<FixtureFactories>
    {
        private FixtureFactories _factories;

        public void SetFixture(FixtureFactories data)
        {
            _factories = data;
        }

        #region Manual fakes

        #region ViewEvents

        private class ViewEvents
        {
            public IMapView2D View;
            public IEventRaiser Hover;
            public IEventRaiser Begin;
            public IEventRaiser MoveTo;
            public IEventRaiser End;
        }

        #endregion

        #region TestTextRenderer2D
        private class TestTextRenderer2D : TextRenderer2D<Object>
        {
            public override IEnumerable<Object> RenderText(String text, StyleFont font, Rectangle2D layoutRectangle, Path2D flowPath, StyleBrush fontBrush, Matrix2D transform)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public override Size2D MeasureString(String text, StyleFont font)
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }
        #endregion

        #region TestVectorRenderer2D

        private class TestVectorRenderer2D : VectorRenderer2D<Object>
        {
            public override IEnumerable<Object> RenderPaths(IEnumerable<Path2D> paths, StylePen outline,
                                                            StylePen highlightOutline, StylePen selectOutline,
                                                            RenderState renderState)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<Object> RenderPaths(IEnumerable<Path2D> paths, StyleBrush fill,
                                                            StyleBrush highlightFill, StyleBrush selectFill,
                                                            StylePen outline,
                                                            StylePen highlightOutline, StylePen selectOutline,
                                                            RenderState renderState)
            {
                yield break;
            }

            public override IEnumerable<Object> RenderSymbols(IEnumerable<Point2D> locatiosn, Symbol2D symbolData,
                                                              RenderState renderState)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<Object> RenderSymbols(IEnumerable<Point2D> locations, Symbol2D symbolData,
                                                              ColorMatrix highlight, ColorMatrix select,
                                                              RenderState renderState)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<Object> RenderSymbols(IEnumerable<Point2D> locations, Symbol2D symbolData,
                                                              Symbol2D highlightSymbolData, Symbol2D selectSymbolData,
                                                              RenderState renderState)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<Object> RenderPaths(IEnumerable<Path2D> paths, StylePen line,
                                                            StylePen highlightLine, StylePen selectLine,
                                                            StylePen outline, StylePen highlightOutline,
                                                            StylePen selectOutline, RenderState renderState)
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion

        #region TestPresenter2D

        private class TestPresenter2D : MapPresenter2D
        {
            public TestPresenter2D(Map map, IMapView2D mapView)
                : base(map, mapView)
            {
            }

            internal TestView2D TestView
            {
                get { return View as TestView2D; }
            }

            protected override IVectorRenderer2D CreateVectorRenderer()
            {
                return new TestVectorRenderer2D();
            }

            protected override IRasterRenderer2D CreateRasterRenderer()
            {
                return null;
            }

            protected override ITextRenderer2D CreateTextRenderer()
            {
                return new TestTextRenderer2D();
            }

            protected override Type GetRenderObjectType()
            {
                return typeof(Object);
            }

            #region Test accessible members

            internal StyleColor BackgroundColor
            {
                get { return BackgroundColorInternal; }
            }

            internal ICoordinate GeoCenter
            {
                get { return GeoCenterInternal; }
                set { GeoCenterInternal = value; }
            }

            internal Double MaximumWorldWidth
            {
                get { return MaximumWorldWidthInternal; }
                set { MaximumWorldWidthInternal = value; }
            }

            internal Double MinimumWorldWidth
            {
                get { return MinimumWorldWidthInternal; }
                set { MinimumWorldWidthInternal = value; }
            }

            internal Double PixelWorldWidth
            {
                get { return PixelWorldWidthInternal; }
            }

            internal Double PixelWorldHeight
            {
                get { return PixelWorldHeightInternal; }
            }

            internal ViewSelection2D Selection
            {
                get { return SelectionInternal; }
            }

            internal Matrix2D ToViewTransform
            {
                get { return ToViewTransformInternal; }
            }

            internal Matrix2D ToWorldTransform
            {
                get { return ToWorldTransformInternal; }
            }

            internal IExtents2D ViewEnvelope
            {
                get { return ViewEnvelopeInternal; }
            }

            internal Double WorldAspectRatio
            {
                get { return WorldAspectRatioInternal; }
                set { WorldAspectRatioInternal = value; }
            }

            internal Double WorldHeight
            {
                get { return WorldHeightInternal; }
            }

            internal Double WorldWidth
            {
                get { return WorldWidthInternal; }
            }

            internal Double WorldUnitsPerPixel
            {
                get { return WorldUnitsPerPixelInternal; }
            }

            internal void ZoomToExtents()
            {
                ZoomToExtentsInternal();
            }

            internal void ZoomToViewBounds(Rectangle2D viewBounds)
            {
                ZoomToViewBoundsInternal(viewBounds);
            }

            internal void ZoomToWorldBounds(IExtents2D zoomBox)
            {
                ZoomToWorldBoundsInternal(zoomBox);
            }

            internal void ZoomToWorldWidth(Double newWorldWidth)
            {
                ZoomToWorldWidthInternal(newWorldWidth);
            }

            internal Point2D ToView(ICoordinate point)
            {
                return ToViewInternal(point);
            }

            internal Point2D ToView(Double x, Double y)
            {
                return ToViewInternal(x, y);
            }

            internal ICoordinate ToWorld(Point2D point)
            {
                return ToWorldInternal(point);
            }

            internal ICoordinate ToWorld(Double x, Double y)
            {
                return ToWorldInternal(x, y);
            }

            #endregion

            protected override void SetViewBackgroundColor(StyleColor fromColor, StyleColor toColor)
            {
            }

            protected override void SetViewGeoCenter(ICoordinate fromCoordinate, ICoordinate toCoordinate)
            {
            }

            protected override void SetViewMaximumWorldWidth(Double fromMaxWidth, Double toMaxWidth)
            {
            }

            protected override void SetViewMinimumWorldWidth(Double fromMinWidth, Double toMinWidth)
            {
            }

            protected override void SetViewEnvelope(IExtents2D fromEnvelope, IExtents2D toEnvelope)
            {
            }

            protected override void SetViewWorldAspectRatio(Double fromRatio, Double toRatio)
            {
            }
        }

        #endregion

        #region TestView2D

        private class TestView2D : IMapView2D
        {
            private readonly TestPresenter2D _presenter;
            internal Rectangle2D _bounds = new Rectangle2D(0, 0, 1000, 1000);

            public TestView2D(Map map)
            {
                _presenter = new TestPresenter2D(map, this);
            }

            public TestPresenter2D Presenter
            {
                get { return _presenter; }
            }

            public void RaiseBegin(Point2D point)
            {
                OnBeginAction(point);
            }

            public void RaiseEnd(Point2D point)
            {
                OnEndAction(point);
            }

            public void RaiseHover(Point2D point)
            {
                OnHover(point);
            }

            public void RaiseMoveTo(Point2D point)
            {
                OnMoveTo(point);
            }

            #region IMapView2D Members

            #region Events

            public event EventHandler<MapActionEventArgs<Point2D>> Hover;
            public event EventHandler<MapActionEventArgs<Point2D>> BeginAction;
            public event EventHandler<MapActionEventArgs<Point2D>> MoveTo;
            public event EventHandler<MapActionEventArgs<Point2D>> EndAction;
            public event EventHandler<MapViewPropertyChangeEventArgs<StyleColor>> BackgroundColorChangeRequested;
            public event EventHandler<MapViewPropertyChangeEventArgs<ICoordinate>> GeoCenterChangeRequested;
            public event EventHandler<LocationEventArgs> IdentifyLocationRequested;
            public event EventHandler<MapViewPropertyChangeEventArgs<Double>> MaximumWorldWidthChangeRequested;
            public event EventHandler<MapViewPropertyChangeEventArgs<Double>> MinimumWorldWidthChangeRequested;
            public event EventHandler<MapViewPropertyChangeEventArgs<Point2D>> OffsetChangeRequested;
            public event EventHandler SizeChanged;
            public event EventHandler<MapViewPropertyChangeEventArgs<IExtents2D>> ViewEnvelopeChangeRequested;
            public event EventHandler<MapViewPropertyChangeEventArgs<Double>> WorldAspectRatioChangeRequested;
            public event EventHandler ZoomToExtentsRequested;
            public event EventHandler<MapViewPropertyChangeEventArgs<Rectangle2D>> ZoomToViewBoundsRequested;
            public event EventHandler<MapViewPropertyChangeEventArgs<IExtents2D>> ZoomToWorldBoundsRequested;
            public event EventHandler<MapViewPropertyChangeEventArgs<Double>> ZoomToWorldWidthRequested;

            #endregion

            #region Properties

            public StyleColor BackgroundColor
            {
                get { return _presenter.BackgroundColor; }
                set { OnRequestBackgroundColorChange(BackgroundColor, value); }
            }

            public Double Dpi
            {
                get { return ScreenHelper.Dpi; }
            }

            public ICoordinate GeoCenter
            {
                get { return _presenter.GeoCenter; }
                set { OnRequestGeoCenterChange(GeoCenter, value); }
            }

            public Double MaximumWorldWidth
            {
                get { return _presenter.MaximumWorldWidth; }
                set { OnRequestMaximumWorldWidthChange(MaximumWorldWidth, value); }
            }

            public Double MinimumWorldWidth
            {
                get { return _presenter.MinimumWorldWidth; }
                set { OnRequestMinimumWorldWidthChange(MinimumWorldWidth, value); }
            }

            public Double PixelWorldWidth
            {
                get { return _presenter.PixelWorldWidth; }
            }

            public Double PixelWorldHeight
            {
                get { return _presenter.PixelWorldHeight; }
            }

            public ViewSelection2D Selection
            {
                get { return _presenter.Selection; }
            }

            public Matrix2D ToViewTransform
            {
                get { return _presenter.ToViewTransform; }
            }

            public Matrix2D ToWorldTransform
            {
                get { return _presenter.ToWorldTransform; }
            }

            public Point2D ToView(ICoordinate point)
            {
                return _presenter.ToView(point);
            }

            public Point2D ToView(Double x, Double y)
            {
                return _presenter.ToView(x, y);
            }

            public ICoordinate ToWorld(Point2D point)
            {
                return _presenter.ToWorld(point);
            }

            public ICoordinate ToWorld(Double x, Double y)
            {
                return _presenter.ToWorld(x, y);
            }

            public IExtents2D ViewEnvelope
            {
                get { return _presenter.ViewEnvelope; }
                set
                {
                    OnRequestViewEnvelopeChange(ViewEnvelope, value);
                }
            }

            public Size2D ViewSize
            {
                get { return _bounds.Size; }
                set
                {
                    _bounds = new Rectangle2D(_bounds.Location, value);
                    OnViewSizeChanged(value);
                }
            }

            public Double WorldAspectRatio
            {
                get { return _presenter.WorldAspectRatio; }
                set { OnRequestWorldAspectRatioChange(WorldAspectRatio, value); }
            }

            public Double WorldHeight
            {
                get { return _presenter.WorldHeight; }
            }

            public Double WorldWidth
            {
                get { return _presenter.WorldWidth; }
            }

            public Double WorldUnitsPerPixel
            {
                get { return _presenter.WorldUnitsPerPixel; }
            }

            #endregion

            #region Methods

            public void IdentifyLocation(ICoordinate worldPoint)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void Offset(Point2D offsetVector)
            {
                Point2D viewCenter = _bounds.Center;
                ICoordinate offsetGeoCenter = ToWorld(viewCenter + offsetVector);
                OnRequestGeoCenterChange(GeoCenter, offsetGeoCenter);
            }

            public void ShowRenderedObjects(IEnumerable renderedObjects)
            {
            }

            public void ZoomToExtents()
            {
                OnRequestZoomToExtents();
            }

            public void ZoomToViewBounds(Rectangle2D viewBounds)
            {
                OnRequestZoomToViewBounds(viewBounds);
            }

            public void ZoomToWorldBounds(IExtents2D zoomBox)
            {
                OnRequestZoomToWorldBounds(zoomBox);
            }

            public void ZoomToWorldWidth(Double newWorldWidth)
            {
                OnRequestZoomToWorldWidth(newWorldWidth);
            }

            #endregion

            #endregion

            #region IView Members

            public Boolean Visible
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public Boolean Enabled
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public void Hide()
            {
                throw new NotImplementedException();
            }

            public void Show()
            {
                throw new NotImplementedException();
            }

            public String Title
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            #endregion

            protected virtual void OnViewSizeChanged(Size2D sizeRequested)
            {
                EventHandler e = SizeChanged;

                if (e != null)
                {
                    e(this, EventArgs.Empty);
                }
            }

            protected virtual void OnHover(Point2D actionLocation)
            {
                EventHandler<MapActionEventArgs<Point2D>> @event = Hover;

                if (@event != null)
                {
                    MapActionEventArgs<Point2D> args = new MapActionEventArgs<Point2D>(actionLocation);
                    @event(this, args);
                }
            }

            protected virtual void OnBeginAction(Point2D actionLocation)
            {
                EventHandler<MapActionEventArgs<Point2D>> @event = BeginAction;

                if (@event != null)
                {
                    MapActionEventArgs<Point2D> args = new MapActionEventArgs<Point2D>(actionLocation);
                    @event(this, args);
                }
            }

            protected virtual void OnMoveTo(Point2D actionLocation)
            {
                EventHandler<MapActionEventArgs<Point2D>> @event = MoveTo;

                if (@event != null)
                {
                    MapActionEventArgs<Point2D> args = new MapActionEventArgs<Point2D>(actionLocation);
                    @event(this, args);
                }
            }

            protected virtual void OnEndAction(Point2D actionLocation)
            {
                EventHandler<MapActionEventArgs<Point2D>> @event = EndAction;

                if (@event != null)
                {
                    MapActionEventArgs<Point2D> args = new MapActionEventArgs<Point2D>(actionLocation);
                    @event(this, args);
                }
            }

            protected virtual void OnRequestBackgroundColorChange(StyleColor current, StyleColor requested)
            {
                EventHandler<MapViewPropertyChangeEventArgs<StyleColor>> e = BackgroundColorChangeRequested;

                if (e != null)
                {
                    e(this, new MapViewPropertyChangeEventArgs<StyleColor>(current, requested));
                }
            }

            protected virtual void OnRequestGeoCenterChange(ICoordinate current, ICoordinate requested)
            {
                EventHandler<MapViewPropertyChangeEventArgs<ICoordinate>> e = GeoCenterChangeRequested;

                if (e != null)
                {
                    MapViewPropertyChangeEventArgs<ICoordinate> args =
                        new MapViewPropertyChangeEventArgs<ICoordinate>(current, requested);

                    e(this, args);
                }
            }

            private void OnRequestMaximumWorldWidthChange(Double current, Double requested)
            {
                EventHandler<MapViewPropertyChangeEventArgs<Double>> e = MaximumWorldWidthChangeRequested;

                if (e != null)
                {
                    MapViewPropertyChangeEventArgs<Double> args =
                        new MapViewPropertyChangeEventArgs<Double>(current, requested);

                    e(this, args);
                }
            }

            private void OnRequestMinimumWorldWidthChange(Double current, Double requested)
            {
                EventHandler<MapViewPropertyChangeEventArgs<Double>> e = MinimumWorldWidthChangeRequested;

                if (e != null)
                {
                    MapViewPropertyChangeEventArgs<Double> args =
                        new MapViewPropertyChangeEventArgs<Double>(current, requested);

                    e(this, args);
                }
            }

            private void OnRequestViewEnvelopeChange(IExtents2D current, IExtents2D requested)
            {
                EventHandler<MapViewPropertyChangeEventArgs<IExtents2D>> e = ViewEnvelopeChangeRequested;

                if (e != null)
                {
                    MapViewPropertyChangeEventArgs<IExtents2D> args =
                        new MapViewPropertyChangeEventArgs<IExtents2D>(current, requested);

                    e(this, args);
                }
            }

            private void OnRequestWorldAspectRatioChange(Double current, Double requested)
            {
                EventHandler<MapViewPropertyChangeEventArgs<Double>> e = WorldAspectRatioChangeRequested;

                if (e != null)
                {
                    MapViewPropertyChangeEventArgs<Double> args =
                        new MapViewPropertyChangeEventArgs<Double>(current, requested);

                    e(this, args);
                }
            }

            private void OnRequestOffset(Point2D offset)
            {
                EventHandler<MapViewPropertyChangeEventArgs<Point2D>> e = OffsetChangeRequested;

                if (e != null)
                {
                    MapViewPropertyChangeEventArgs<Point2D> args =
                        new MapViewPropertyChangeEventArgs<Point2D>(Point2D.Zero, offset);

                    e(this, args);
                }
            }

            private void OnRequestZoomToExtents()
            {
                EventHandler e = ZoomToExtentsRequested;

                if (e != null)
                {
                    e(this, EventArgs.Empty);
                }
            }

            private void OnRequestZoomToViewBounds(Rectangle2D viewBounds)
            {
                EventHandler<MapViewPropertyChangeEventArgs<Rectangle2D>> e = ZoomToViewBoundsRequested;

                if (e != null)
                {
                    MapViewPropertyChangeEventArgs<Rectangle2D> args =
                        new MapViewPropertyChangeEventArgs<Rectangle2D>(_bounds, viewBounds);

                    e(this, args);
                }
            }

            private void OnRequestZoomToWorldBounds(IExtents2D zoomBox)
            {
                EventHandler<MapViewPropertyChangeEventArgs<IExtents2D>> e = ZoomToWorldBoundsRequested;

                if (e != null)
                {
                    MapViewPropertyChangeEventArgs<IExtents2D> args =
                        new MapViewPropertyChangeEventArgs<IExtents2D>(ViewEnvelope, zoomBox);

                    e(this, args);
                }
            }

            private void OnRequestZoomToWorldWidth(Double newWorldWidth)
            {
                EventHandler<MapViewPropertyChangeEventArgs<Double>> e = ZoomToWorldWidthRequested;

                if (e != null)
                {
                    MapViewPropertyChangeEventArgs<Double> args =
                        new MapViewPropertyChangeEventArgs<Double>(WorldWidth, newWorldWidth);

                    e(this, args);
                }
            }
        }

        #endregion

        #endregion

        [Fact]
        public void InitalizingMapPresenterWithEmptyMapHasUndefinedView()
        {
            MockRepository mocks = new MockRepository();

            Map map = new Map(_factories.GeoFactory);
            IMapView2D mapView = mocks.Stub<IMapView2D>();

            SetupResult.For(mapView.Dpi).Return(ScreenHelper.Dpi);
            mapView.ViewSize = new Size2D(200, 400);

            mocks.ReplayAll();

            TestPresenter2D mapPresenter = new TestPresenter2D(map, mapView);

            Assert.Equal(0, mapPresenter.WorldWidth);
            Assert.Equal(0, mapPresenter.WorldHeight);
            Assert.Equal(0, mapPresenter.WorldUnitsPerPixel);
            Assert.Equal(1, mapPresenter.WorldAspectRatio);
            Assert.Equal(0, mapPresenter.PixelWorldWidth);
            Assert.Equal(0, mapPresenter.PixelWorldHeight);
            Assert.Equal(null, mapPresenter.ToViewTransform);
            Assert.Equal(null, mapPresenter.ToWorldTransform);
        }

        [Fact]
        public void GettingGeoCenterOnUndefinedViewFails()
        {
            MockRepository mocks = new MockRepository();

            Map map = new Map(_factories.GeoFactory);
            IMapView2D mapView = mocks.Stub<IMapView2D>();

            SetupResult.For(mapView.Dpi).Return(ScreenHelper.Dpi);
            mapView.ViewSize = new Size2D(200, 400);

            mocks.ReplayAll();

            TestPresenter2D mapPresenter = new TestPresenter2D(map, mapView);

            // Changed to null from Point.Empty
            Assert.Throws<InvalidOperationException>(delegate { Assert.Equal(null, mapPresenter.GeoCenter); });
        }

        [Fact]
        public void TransformingPointFromViewToWorldOnUndefinedViewFails()
        {
            MockRepository mocks = new MockRepository();

            Map map = new Map(_factories.GeoFactory);
            IMapView2D mapView = mocks.Stub<IMapView2D>();

            SetupResult.For(mapView.Dpi).Return(ScreenHelper.Dpi);
            mapView.ViewSize = new Size2D(200, 400);

            mocks.ReplayAll();

            TestPresenter2D mapPresenter = new TestPresenter2D(map, mapView);
            // Changed to null from Point.Empty

            Assert.Throws<InvalidOperationException>(delegate
                                                         {
                                                             Assert.Equal(null,
                                                                          mapPresenter.ToWorld(new Point2D(100, 100)));
                                                         });
        }

        [Fact]
        public void TransformingPointFromWorldToViewOnUndefinedViewFails()
        {
            MockRepository mocks = new MockRepository();

            Map map = new Map(_factories.GeoFactory);
            IMapView2D mapView = mocks.Stub<IMapView2D>();

            SetupResult.For(mapView.Dpi).Return(ScreenHelper.Dpi);
            mapView.ViewSize = new Size2D(200, 400);

            mocks.ReplayAll();

            TestPresenter2D mapPresenter = new TestPresenter2D(map, mapView);

            Assert.Throws<InvalidOperationException>(delegate
                                                         {
                                                             Assert.Equal(Point2D.Empty,
                                                                          mapPresenter.ToView(
                                                                              _factories.GeoFactory.CoordinateFactory.
                                                                                  Create(50, 50)));
                                                         });
        }

        [Fact]
        public void InitalizingMapPresenterWithNonEmptyMapHasUndefinedView()
        {
            MockRepository mocks = new MockRepository();

            Map map = new Map(_factories.GeoFactory);
            map.AddLayer(DataSourceHelper.CreateFeatureFeatureLayer(_factories.GeoFactory));

            IMapView2D mapView = mocks.Stub<IMapView2D>();

            SetupResult.For(mapView.Dpi).Return(ScreenHelper.Dpi);
            mapView.ViewSize = new Size2D(1000, 1000);

            mocks.ReplayAll();

            TestPresenter2D mapPresenter = new TestPresenter2D(map, mapView);

            //Assert.Equal(map.Center[Ordinates.X], mapPresenter.GeoCenter[Ordinates.X]);
            //Assert.Equal(map.Center[Ordinates.Y], mapPresenter.GeoCenter[Ordinates.Y]);
            Assert.Equal(0, mapPresenter.WorldWidth);
            Assert.Equal(0, mapPresenter.WorldHeight);
            Assert.Equal(0, mapPresenter.WorldUnitsPerPixel);
            Assert.Equal(1, mapPresenter.WorldAspectRatio);
            Assert.Equal(0, mapPresenter.PixelWorldWidth);
            Assert.Equal(0, mapPresenter.PixelWorldHeight);
            Assert.Equal(null, mapPresenter.ToViewTransform);
            Assert.Equal(null, mapPresenter.ToWorldTransform);
        }

        [Fact]
        public void DisposingPresenterMakesItDisposed()
        {
            TestView2D view;
            TestPresenter2D mapPresenter = createPresenter(1000, 1000, out view);
            Assert.Equal(false, mapPresenter.IsDisposed);
            mapPresenter.Dispose();
            Assert.Equal(true, mapPresenter.IsDisposed);
        }

        [Fact]
        public void DisposingPresenterRaisesDisposedEvent()
        {
            TestView2D view;
            TestPresenter2D mapPresenter = createPresenter(1000, 1000, out view);
            Boolean disposedCalled = false;
            mapPresenter.Disposed += delegate { disposedCalled = true; };
            mapPresenter.Dispose();
            Assert.Equal(true, disposedCalled);
        }

        [Fact]
        public void DisposingPresenterUnwiresAllEvents()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 200);
            Map map = mapPresenter.Map;

            mapPresenter.Dispose();

            FieldInfo listChangedField = typeof(BindingList<ILayer>).GetField("onListChanged",
                                                                               BindingFlags.NonPublic |
                                                                               BindingFlags.Instance);
            Delegate listChanged = listChangedField.GetValue(map.Layers) as Delegate;
            Assert.NotNull(listChanged);

            Delegate[] listeners = listChanged.GetInvocationList();
            // the only listener should be the map
            Assert.Equal(1, listeners.Length);
            Assert.Same(map, listeners[0].Target);
        }

        [Fact]
        public void ViewEnvelopeMeasuresCorrectly()
        {
            TestView2D view;
            TestPresenter2D mapPresenter = createPresenter(120, 100, out view);

            mapPresenter.ZoomToExtents();

            IExtents expected = _factories.GeoFactory.CreateExtents2D(0, 0, 120, 100);
            Assert.Equal(_factories.GeoFactory.CoordinateFactory.Create(60, 50), mapPresenter.GeoCenter);
            Assert.Equal(1, mapPresenter.WorldUnitsPerPixel);
            Assert.Equal(120, mapPresenter.WorldWidth);
            Assert.Equal(100, mapPresenter.WorldHeight);
            Assert.Equal(expected, mapPresenter.ViewEnvelope);
        }

        [Fact]
        public void PanWhenNoWorldBoundsThrowsException()
        {
            Map map = new Map(_factories.GeoFactory);
            map.AddLayer(DataSourceHelper.CreateFeatureFeatureLayer(_factories.GeoFactory));
            //map.AddLayer(DataSourceHelper.CreateGeometryFeatureLayer());

            TestView2D view = new TestView2D(map);

            map.ActiveTool = StandardMapView2DMapTools.Pan;

            Assert.Throws<InvalidOperationException>(delegate
            {
                view.RaiseBegin(new Point2D(200, 250));
            });

            //view.RaiseMoveTo(new Point2D(250, 250));
            //view.RaiseEnd(new Point2D(250, 250));
        }

        [Fact]
        public void PanTest()
        {
            TestView2D view;
            //_factories.GeoFactory.CoordinateFactory.BitResolution = 24;
            TestPresenter2D mapPresenter = createPresenter(400, 800, out view);

            mapPresenter.ZoomToExtents();

            Assert.Equal(_factories.GeoFactory.CoordinateFactory.Create(60, 50), mapPresenter.GeoCenter);

            Map map = mapPresenter.Map;
            map.ActiveTool = StandardMapView2DMapTools.Pan;

            /*
             *  1
             * |*. 
             * |  `. 2
             * |    `*
             * |    
             * |____________
             * 
             */

            view.RaiseBegin(new Point2D(0, 0));
            view.RaiseMoveTo(new Point2D(200, 400));
            view.RaiseEnd(new Point2D(200, 400));

            ICoordinate expected = _factories.GeoFactory.CoordinateFactory.Create(0, 170);
            Assert.Equal<ICoordinate>(expected, mapPresenter.GeoCenter, EpsilonComparer.Default);
        }

        [Fact]
        public void ZoomWhenNoWorldBoundsThrowsException()
        {
            TestView2D view;
            TestPresenter2D mapPresenter = createPresenter(400, 500, out view);

            Map map = mapPresenter.Map;

            map.ActiveTool = StandardMapView2DMapTools.ZoomIn;

            Assert.Throws<InvalidOperationException>(delegate
            {
                view.RaiseBegin(new Point2D(100, 125));
            });

            //view.RaiseMoveTo(new Point2D(300, 375));
            //view.RaiseEnd(new Point2D(300, 375));
        }

        [Fact]
        public void ZoomingInFromViewCalculatesCorrectViewMetrics()
        {
            TestView2D view;
            TestPresenter2D mapPresenter = createPresenter(400, 500, out view);

            mapPresenter.ZoomToExtents();

            Assert.Equal(120, mapPresenter.WorldWidth);
            Assert.Equal(150, mapPresenter.WorldHeight);
            Assert.Equal(_factories.GeoFactory.CoordinateFactory.Create(60, 50), mapPresenter.GeoCenter);

            Map map = mapPresenter.Map;

            map.ActiveTool = new MapTool<IMapView2D, Point2D>("TestZoomIn", nullAction, testBeginZoomIn, testContinueZoomIn, testEndZoomIn);

            // Zoom using click
            view.RaiseBegin(new Point2D(200, 250));
            view.RaiseMoveTo(new Point2D(200, 250));
            view.RaiseEnd(new Point2D(200, 250));

            Assert.Equal(100, mapPresenter.WorldWidth);
            Assert.Equal(125, mapPresenter.WorldHeight);
            Assert.Equal(_factories.GeoFactory.CoordinateFactory.Create(60, 50), mapPresenter.GeoCenter);

            mapPresenter.ZoomToExtents();

            Assert.Equal(120, mapPresenter.WorldWidth);
            Assert.Equal(150, mapPresenter.WorldHeight);
            Assert.Equal(_factories.GeoFactory.CoordinateFactory.Create(60, 50), mapPresenter.GeoCenter);

            // Zoom using selection
            view.RaiseBegin(new Point2D(100, 125));
            view.RaiseMoveTo(new Point2D(300, 375));
            view.RaiseEnd(new Point2D(300, 375));

            Assert.Equal(60, mapPresenter.WorldWidth);
            Assert.Equal(75, mapPresenter.WorldHeight);
            Assert.Equal(_factories.GeoFactory.CoordinateFactory.Create(60, 50), mapPresenter.GeoCenter);
        }

        [Fact]
        public void QueryTest()
        {
        }

        [Fact]
        public void AddFeatureTest()
        {
        }

        [Fact]
        public void RemoveFeatureTest()
        {
        }

        [Fact]
        public void ZoomingToExtentsCentersMap()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 500);
            mapPresenter.ZoomToExtents();

            Map map = mapPresenter.Map;

            Assert.Equal(map.Center[Ordinates.X], mapPresenter.GeoCenter[Ordinates.X]);
            Assert.Equal(map.Center[Ordinates.Y], mapPresenter.GeoCenter[Ordinates.Y]);
        }

        [Fact]
        public void GetPixelSize_FixedZoom_Return8_75()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 500);

            mapPresenter.ZoomToWorldWidth(3500);
            Assert.Equal<Double>(8.75, mapPresenter.WorldUnitsPerPixel, EpsilonComparer.Default);
        }

        [Fact]
        public void GetWorldHeight_FixedZoom_Return1750()
        {
            MockRepository mocks = new MockRepository();

            // create a presenter with a view having Width = 400 and Height = 200
            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 200);

            mapPresenter.ZoomToWorldWidth(3500);
            Assert.Equal<Double>(1750, mapPresenter.WorldHeight, EpsilonComparer.Default);
        }

        [Fact]
        public void SetMinimumZoom_NegativeValue_ThrowException()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 200);

            Assert.Throws<ArgumentOutOfRangeException>(delegate { mapPresenter.MinimumWorldWidth = -1; });
        }

        [Fact]
        public void SetMaximumZoom_NegativeValue_ThrowException()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 200);

            Assert.Throws<ArgumentOutOfRangeException>(delegate { mapPresenter.MaximumWorldWidth = -1; });
        }

        [Fact]
        public void SetMaximumZoom_OKValue()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 200);

            mapPresenter.MaximumWorldWidth = 100.3;
            Assert.Equal(100.3, mapPresenter.MaximumWorldWidth);
        }

        [Fact]
        public void SetMinimumZoom_OKValue()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 200);

            mapPresenter.MinimumWorldWidth = 100.3;
            Assert.Equal(100.3, mapPresenter.MinimumWorldWidth);
        }

        [Fact]
        public void SetZoom_ValueOutsideMax()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 200);

            mapPresenter.MaximumWorldWidth = 100;
            mapPresenter.ZoomToWorldWidth(150);
            Assert.Equal(100, mapPresenter.WorldWidth);
        }

        [Fact]
        public void SetZoom_ValueBelowMin()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 200);

            mapPresenter.MinimumWorldWidth = 100;
            mapPresenter.ZoomToWorldWidth(50);
            Assert.Equal(100, mapPresenter.WorldWidth);
        }

        [Fact]
        public void ZoomToViewBoundsWhenNoWorldBoundsSetThrowsException()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 1000, 1000);
            Assert.Throws<InvalidOperationException>(delegate
                                                           {
                                                               mapPresenter.ZoomToViewBounds(new Rectangle2D(300,
                                                                                                             300,
                                                                                                             900,
                                                                                                             900));
                                                           });
        }

        [Fact]
        public void ZoomToViewBounds_NoAspectCorrection()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 1000, 1000);
            mapPresenter.ZoomToExtents();

            mapPresenter.ZoomToViewBounds(new Rectangle2D(300, 300, 900, 900));
            BufferedCoordinate expectedCoord = (BufferedCoordinate)_factories.GeoFactory.CoordinateFactory.Create(72, 38);
            Assert.Equal(expectedCoord, mapPresenter.GeoCenter);
            Assert.Equal(72, mapPresenter.WorldWidth);
            Assert.Equal(72, mapPresenter.WorldHeight);
        }

        [Fact]
        public void ZoomToViewBounds_WithAspectCorrection()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 200);
            // Set the aspect ratio, which is the number of height units per width unit
            // to 2 height units to 1 width unit
            mapPresenter.WorldAspectRatio = 2;
            // Zooming to extents should 
            mapPresenter.ZoomToExtents();
            Assert.Equal(120, mapPresenter.WorldWidth);
            Assert.Equal(120, mapPresenter.WorldHeight);
            // Zoom to a 200x100 rectangle in view coordinates
            mapPresenter.ZoomToViewBounds(new Rectangle2D(100, 50, 300, 150));
            Assert.Equal(_factories.GeoFactory.CoordinateFactory.Create(60, 50), mapPresenter.GeoCenter);
            Assert.Equal(60, mapPresenter.WorldWidth);
            Assert.Equal(60, mapPresenter.WorldHeight);
        }

        [Fact]
        public void ZoomToWorldBounds_NoAspectCorrection()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 1000, 1000);

            mapPresenter.ZoomToWorldBounds(_factories.GeoFactory.CreateExtents2D(20, 50, 100, 80));
            Assert.Equal(_factories.GeoFactory.CoordinateFactory.Create(60, 65), mapPresenter.GeoCenter);
            Assert.Equal(80, mapPresenter.WorldWidth);
        }

        [Fact]
        public void ZoomToWorldBounds_WithAspectCorrection()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 200);
            mapPresenter.WorldAspectRatio = 2;
            mapPresenter.ZoomToWorldBounds(_factories.GeoFactory.CreateExtents2D(20, 10, 100, 180));
            Assert.Equal(_factories.GeoFactory.CoordinateFactory.Create(60, 95), mapPresenter.GeoCenter);
            Assert.Equal(170, mapPresenter.WorldWidth);
            Assert.Equal(170, mapPresenter.WorldHeight);
        }

        [Fact]
        public void WorldToViewTests()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 1000, 1000);

            mapPresenter.GeoCenter = _factories.GeoFactory.CoordinateFactory.Create(23, 34);
            mapPresenter.ZoomToWorldWidth(2500);

            Point2D p1 = mapPresenter.ToView(8, 50);
            Point2D p2 = mapPresenter.ToView(_factories.GeoFactory.CoordinateFactory.Create(8, 50));
            Assert.Equal(new Point2D(494, 493.6), p1);
            Assert.Equal(p1, p2);
        }

        [Fact]
        public void ViewToWorldTests()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 500, 200);

            mapPresenter.GeoCenter = _factories.GeoFactory.CoordinateFactory.Create(23, 34);
            mapPresenter.ZoomToWorldWidth(1000);

            ICoordinate p1 = mapPresenter.ToWorld(242.5f, 92);
            ICoordinate p2 = mapPresenter.ToWorld(new Point2D(242.5f, 92));
            Assert.Equal(_factories.GeoFactory.CoordinateFactory.Create(8, 50), p1);
            Assert.Equal(p1, p2);
        }

        [Fact(Skip = "Incomplete")]
        public void GetMap_GeometryProvider_ReturnImage()
        {
            MockRepository mocks = new MockRepository();

            TestPresenter2D mapPresenter = createPresenter(mocks, 400, 200);

            Map map = mapPresenter.Map;

            GeometryLayer vLayer = new GeometryLayer("Geom layer", DataSourceHelper.CreateGeometryDatasource(_factories.GeoFactory));
            vLayer.Style.Outline = new StylePen(StyleColor.Red, 2f);
            vLayer.Style.EnableOutline = true;
            vLayer.Style.Line = new StylePen(StyleColor.Green, 2f);
            vLayer.Style.Fill = new SolidStyleBrush(StyleColor.Yellow);
            map.AddLayer(vLayer);

            GeometryLayer vLayer2 = new GeometryLayer("Geom layer 2", vLayer.DataSource);
            Stream data = Assembly.GetAssembly(typeof(Map))
                .GetManifestResourceStream("SharpMap.Styles.DefaultSymbol.png");
            vLayer2.Style.Symbol = new Symbol2D(data, new Size2D(16, 16));
            vLayer2.Style.Symbol.Offset = new Point2D(3, 4);
            vLayer2.Style.Symbol.Rotation = 45;
            vLayer2.Style.Symbol.Scale(0.4f);
            map.AddLayer(vLayer2);

            GeometryLayer vLayer3 = new GeometryLayer("Geom layer 3", vLayer.DataSource);
            vLayer3.Style.Symbol.Offset = new Point2D(3, 4);
            vLayer3.Style.Symbol.Rotation = 45;
            map.AddLayer(vLayer3);

            GeometryLayer vLayer4 = new GeometryLayer("Geom layer 4", vLayer.DataSource);
            vLayer4.Style.Symbol.Offset = new Point2D(3, 4);
            vLayer2.Style.Symbol.Scale(0.4f);
            map.AddLayer(vLayer4);

            mapPresenter.ZoomToExtents();

            map.Dispose();
        }

        private void nullAction(ActionContext<IMapView2D, Point2D> context) { }

        private void testBeginZoomIn(ActionContext<IMapView2D, Point2D> context)
        {
            IMapView2D view = context.MapView;
            view.Selection.Clear();
            view.Selection.AddPoint(context.CurrentPoint);
        }

        private void testContinueZoomIn(ActionContext<IMapView2D, Point2D> context)
        {
            context.MapView.Selection.AddPoint(context.CurrentPoint);
        }

        private void testEndZoomIn(ActionContext<IMapView2D, Point2D> context)
        {
            IMapView2D view = context.MapView;

            if (view.Selection.Path.Points.Count == 1)
            {
                zoomByFactor(view, context.CurrentPoint, 1.2);	// 0.83333333333333337
            }
            else
            {
                view.Selection.Close();
                context.MapView.ZoomToViewBounds(view.Selection.Path.Bounds);
            }
        }

        private static void zoomByFactor(IMapView2D view, Point2D zoomCenter, Double zoomFactor)
        {
            zoomFactor = 1 / zoomFactor;

            Size2D viewSize = view.ViewSize;
            Point2D viewCenter = new Point2D((viewSize.Width / 2), (viewSize.Height / 2));
            Point2D viewDifference = zoomCenter - viewCenter;

            Size2D zoomBoundsSize = new Size2D(viewSize.Width * zoomFactor, viewSize.Height * zoomFactor);
            Double widthDifference = zoomBoundsSize.Width - viewSize.Width;
            Double heightDifference = zoomBoundsSize.Height - viewSize.Height;
            Point2D zoomUpperLeft = new Point2D(viewDifference.X * zoomFactor - widthDifference / 2,
                viewDifference.Y * zoomFactor - heightDifference / 2);
            Rectangle2D zoomViewBounds = new Rectangle2D(zoomUpperLeft, zoomBoundsSize);

            view.ZoomToViewBounds(zoomViewBounds);
        }

        private TestPresenter2D createPresenter(MockRepository mocks, Double width, Double height)
        {
            Map map = new Map(_factories.GeoFactory);
            map.AddLayer(DataSourceHelper.CreateFeatureFeatureLayer(_factories.GeoFactory));
            //map.AddLayer(DataSourceHelper.CreateGeometryFeatureLayer());

            IMapView2D mapView = mocks.Stub<IMapView2D>();
            SetupResult.For(mapView.Dpi).Return(ScreenHelper.Dpi);
            mapView.ViewSize = new Size2D(width, height);

            TestPresenter2D mapPresenter = new TestPresenter2D(map, mapView);
            return mapPresenter;
        }

        private TestPresenter2D createPresenter(Double width, Double height, out TestView2D view)
        {
            Map map = new Map(_factories.GeoFactory);
            map.AddLayer(DataSourceHelper.CreateFeatureFeatureLayer(_factories.GeoFactory));
            //map.AddLayer(DataSourceHelper.CreateGeometryFeatureLayer());

            view = new TestView2D(map);

            view.ViewSize = new Size2D(width, height);

            TestPresenter2D mapPresenter = view.Presenter;
            return mapPresenter;
        }
    }
}
