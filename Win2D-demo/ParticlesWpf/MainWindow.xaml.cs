using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ParticlesWpf
{
    public partial class MainWindow : Window
    {
        private TimeSpan _lastUpdateTime;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainPageOnLoaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += CompositionTargetOnRendering;

            _canvas._emiterPos = new Point(_canvas.ActualWidth / 2, _canvas.ActualHeight / 2);
        }

        private void CompositionTargetOnRendering(object sender, EventArgs o)
        {
            var e = (RenderingEventArgs) o;
            var dt = e.RenderingTime - _lastUpdateTime;
            _lastUpdateTime = e.RenderingTime;

            _canvas.Update(dt, (int) _targetCount.Value);
            _childCount.Text = $"#Particles: { _canvas._particles.Count}";
        }

        private void CaputreBoderOnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pt = e.GetPosition(_canvas);
                _canvas._emiterPos = pt;
            }
        }
    }

    public class ParticlesCanvas : FrameworkElement
    {
        Random _rand = new Random();
        public List<Particle> _particles = new List<Particle>();

        public Point _emiterPos;

        protected override void OnRender(DrawingContext dc)
        {
            foreach (var p in _particles)
                p.Render(dc);
        }

        public void Update(TimeSpan dt, int targetCountValue)
        {
            UpdateParticles(dt);

            if (_particles.Count < targetCountValue)
                EmitParticles(targetCountValue - _particles.Count);

            InvalidateVisual();
        }

        private void EmitParticles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var particle = new Particle(_emiterPos,
                    life: TimeSpan.FromSeconds(_rand.NextDouble() * 3),
                    angle: _rand.NextDouble() * 360,
                    speed: _rand.NextDouble() * 50 + 50,
                    size: _rand.NextDouble() * 10 + 10,
                    color: Colors.DarkBlue);

                _particles.Add(particle);
            }

        }

        private void UpdateParticles(TimeSpan dt)
        {
            for (int i = 0; i < _particles.Count;)
            {
                var p = _particles[i];
                p.Update(dt);
                if (!p.IsAlive)
                {
                    // remove by swapping with last to avoid array reallocation
                    _particles[i] = _particles[_particles.Count - 1];
                    _particles.RemoveAt(_particles.Count - 1);
                }
                else
                {
                    // move to next
                    i++;
                }
            }
        }
    }

    public class Particle
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

            var angleInRadians = angle * Math.PI / 180;
            _velocity = new Point(
            speed * Math.Cos(angleInRadians),
                -speed * Math.Sin(angleInRadians)
            );
        }

        public bool IsAlive
        {
            get { return _life > TimeSpan.Zero; }
        }

        public void Update(TimeSpan dt)
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

        internal void Render(DrawingContext dc)
        {
            var c = _color;
            c.A = (byte)(_alpha * 255.0);
            dc.DrawEllipse(new SolidColorBrush(c), null, _position, _size / 2, _size / 2);
        }
    }

}
