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
        #region Constants
        /// <summary>
        /// The amound of ticks in a second
        /// </summary>
        protected const long ticksPerSecond = 10000000L;

        /// <summary>
        /// The amount of milliseconds in a second
        /// </summary>
        protected const long ticksPerMillisecond = 10000L;
        #endregion

        #region Fields
        /// <summary>
        /// UT Time of the last frame
        /// </summary>
        protected double lastFrame = 0d;

        /// <summary>
        /// Total elapsed time calculated by the watch in seconds
        /// </summary>
        protected long totalTicks = 0L;
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

        /// <summary>
        /// The current elapsed time of the watch
        /// </summary>
        public TimeSpan elapsed
        {
            get
            {
                if (this._isRunning) { UpdateWatch(); }
                return new TimeSpan(this.totalTicks);
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
                return this.totalTicks * ticksPerMillisecond;
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
                return this.totalTicks;
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
            this.lastFrame = Planetarium.GetUniversalTime();
            this._isRunning = true;
        }

        /// <summary>
        /// Stops the watch
        /// </summary>
        public void Stop()
        {
            UpdateWatch();
            this._isRunning = false;
        }

        /// <summary>
        /// Resets the watch to zero and starts it
        /// </summary>
        public void Restart()
        {
            this.totalTicks = 0L;
            this.lastFrame = 0d;
            this._isRunning = true;
        }

        /// <summary>
        /// Stops the watch and resets it to zero
        /// </summary>
        public void Reset()
        {
            this.totalTicks = 0L;
            this.lastFrame = 0d;
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
            this.totalTicks += (long)delta * ticksPerSecond;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Returns a string representation fo this instance
        /// </summary>
        public override string ToString()
        {
            return elapsed.ToString();
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