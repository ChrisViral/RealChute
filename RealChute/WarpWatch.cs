using System;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute
{
    /// <summary>
    /// A generic StopWatch clone which runs on KSP's internal clock
    /// </summary>
    public class WarpWatch
    {
        #region Fields
        protected double lastFrame = 0, total = 0;
        #endregion

        #region Propreties
        private bool _isRunning = false;
        /// <summary>
        /// If the watch is currently counting down time
        /// </summary>
        public bool isRunning
        {
            get { return this._isRunning; }
        }

        private TimeSpan _elapsed = TimeSpan.Zero;
        /// <summary>
        /// The current elapsed time of the watch
        /// </summary>
        public TimeSpan elapsed
        {
            get
            {
                if (this._isRunning) { UpdateWatch(); }
                return this._elapsed;
            }
        }

        /// <summary>
        /// The amount of milliseconds elapsed to the current watch
        /// </summary>
        public long elapsedMilliseconds
        {
            get
            {
                if (this._isRunning) { UpdateWatch(); }
                return (long)this.total * 1000;
            }
        }

        /// <summary>
        /// The amount of ticks elapsed to the current watch
        /// </summary>
        public long elapsedTicks
        {
            get
            {
                if (this._isRunning) { UpdateWatch(); }
                return (long)this.total * TimeSpan.TicksPerSecond;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new WarpWatch
        /// </summary>
        public WarpWatch() { }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the watch
        /// </summary>
        public void Start()
        {
            lastFrame = Planetarium.GetUniversalTime();
            this._isRunning = true;
        }

        /// <summary>
        /// Stops the watch
        /// </summary>
        public void Stop()
        {
            this._isRunning = false;
        }

        /// <summary>
        /// Stops the watch and resets it to zero
        /// </summary>
        public void Reset()
        {
            this.total = 0;
            this.lastFrame = 0;
            this._elapsed = TimeSpan.Zero;
            this._isRunning = false;
        }

        /// <summary>
        /// Updates the time on the watch
        /// </summary>
        protected virtual void UpdateWatch()
        {
            double current = Planetarium.GetUniversalTime();
            double delta = current - this.lastFrame;
            this.lastFrame = current;
            this.total += delta;
            this._elapsed.Add(TimeSpan.FromSeconds(delta));
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Returns a string representation fo this instance
        /// </summary>
        public override string ToString()
        {
            return _elapsed.ToString();
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Creates a new WarpWatch, starts it, and returns the current instance
        /// </summary>
        public static WarpWatch StartNew()
        {
            WarpWatch watch = new WarpWatch();
            watch.Start();
            return watch;
        }
        #endregion
    }
}