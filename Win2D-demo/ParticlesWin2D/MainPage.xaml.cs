using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;

namespace ParticlesWin2D
{
    public sealed partial class MainPage : Page
    {
        Random _rand = new Random();
        List<Particle> _particles = new List<Particle>();
        private Point _emiterPos;
        private int _targetCountValue = 100;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void MainPageOnLoaded(object sender, RoutedEventArgs e)
        {
  //          CompositionTarget.Rendering += CompositionTargetOnRendering;

            _canvas.TargetElapsedTime = TimeSpan.FromSeconds(1.0/60);
            _canvas.Draw += CanvasOnDraw;
            _canvas.Update += CanvasOnUpdate;

            _emiterPos = new Point(_canvas.ActualWidth / 2, _canvas.ActualHeight / 2);

            _targetCount.Value = _targetCountValue;
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

        private void CanvasOnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            var dt = args.Timing.ElapsedTime;

            UpdateParticles(dt);

            if (_particles.Count < _targetCountValue)
                EmitParticles(_targetCountValue - _particles.Count);
        }

        private void CanvasOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            foreach (var p in _particles)
                p.Render(args.DrawingSession);

            /* args.DrawingSession.DrawText($"#Particles: {_particles.Count}",
                new Vector2(0, (float) (sender.Size.Height - 30)), Colors.Black);*/
        }

        private void CompositionTargetOnRendering(object sender, object e)
        {
            _childCount.Text = $"#Particles: {_particles.Count}";
        }

        private void TargetCountOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // CanvasControl's Update and Draw events are called on worker thread so can't access UI elements directly. Copy value here. 
            _targetCountValue = (int) _targetCount.Value;
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

        internal void Render(CanvasDrawingSession ds)
        {
            var c = _color;
            c.A = (byte)(_alpha * 255.0);
            ds.FillCircle((float) _position.X, (float) _position.Y, (float)_size/2, c);
        }
    }
}
