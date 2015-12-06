using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace ParticlesXaml
{
    public sealed partial class MainPage : Page
    {
        Random _rand = new Random();
        List<Particle>  _particles = new List<Particle>();
        private TimeSpan _lastUpdateTime;
        private Point _emiterPos;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += CompositionTargetOnRendering;

            _emiterPos = new Point(_canvas.ActualWidth / 2, _canvas.ActualHeight / 2);
        }

        private void CaputreBoderOnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(_canvas);
            if (pt.IsInContact)
                _emiterPos = pt.Position;
        }

        private void EmitParticles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var particle=  new Particle(_emiterPos, 
                    life: TimeSpan.FromSeconds(_rand.NextDouble()*3), 
                    angle: _rand.NextDouble() * 360, 
                    speed: _rand.NextDouble() * 50 + 50,
                    size: _rand.NextDouble() * 10 + 10,
                    color: Colors.DarkBlue);

                _canvas.Children.Add(particle.Shape);
                _particles.Add(particle);
            }
            
        }

        private void UpdateParticles(TimeSpan dt)
        {
            for (int i = 0; i < _particles.Count; )
            {
                var p = _particles[i];
                p.Update(dt);
                if (!p.IsAlive)
                {
                    // remove by swapping with last to avoid array reallocation
                    _particles[i] = _particles[_particles.Count - 1];
                    _particles.RemoveAt(_particles.Count-1);
                    _canvas.Children.Remove(p.Shape);
                }
                else
                {
                    p.Render();
                    i++;
                }
            }
        }

        private void CompositionTargetOnRendering(object sender, object o)
        {
            var e = (RenderingEventArgs) o;
            var dt = e.RenderingTime - _lastUpdateTime;
            _lastUpdateTime = e.RenderingTime;

            UpdateParticles(dt);

            if (_particles.Count < _targetCount.Value)
                EmitParticles((int)_targetCount.Value - _particles.Count);

            _childCount.Text = $"#Particles: {_particles.Count}";
        }
    }

    class Particle
    {
        private Point _position;
        private Point _velocity;
        private Color _color;
        private readonly TimeSpan _originalLife;
        private TimeSpan _life;
        private readonly double _originalSize;
        private double _size;
        private double _alpha;

        public Particle(Point position, TimeSpan life, double angle, double speed, double size, Color color)
        {
            _position = position;
            _originalLife = _life = life;
            _originalSize = _size = size;
            _color = color;
            _alpha = 1;

            var angleInRadians = angle*Math.PI/180;
            _velocity = new Point(
            speed*Math.Cos(angleInRadians),
                -speed*Math.Sin(angleInRadians)
            );

            Shape = CreateShape();
            Render();
        }

        public Shape Shape { get; }

        public bool IsAlive
        {
            get { return _life > TimeSpan.Zero; }
        }

        public void Update (TimeSpan dt)
        {
            _life -= dt;

            if (IsAlive)
            {
                var ageRatio = _life.TotalSeconds / _originalLife.TotalSeconds;
                _size = _originalSize * ageRatio;
                _alpha = ageRatio;

                _position.X += _velocity.X * dt.TotalSeconds;
                _position.Y += _velocity.Y * dt.TotalSeconds;
            }
        }

        public void Render()
        {
            var trans = (TranslateTransform)Shape.RenderTransform;
            trans.X = _position.X - Shape.Width / 2;
            trans.Y = _position.Y - Shape.Height /2;

            Shape.Opacity = _alpha;
         //   Shape.Width = _size;
         //   Shape.Height = _size;
        }

        private Shape CreateShape()
        {
            Ellipse shape = new Ellipse();
            shape.Width = _size;
            shape.Height = _size;
            shape.RenderTransformOrigin = new Point(0.5, 0.5);
            shape.RenderTransform = new TranslateTransform();
            shape.Fill = new SolidColorBrush(_color);
            return shape;
        }
    }
}
