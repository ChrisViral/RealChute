using System;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute
{
    /// <summary>
    /// A generic Stopwatch clone which runs on KSP's internal clock
    /// </summary>
    public class PhysicsWatch
    {
        #region Constants
        /// <summary>
        /// The amount of ticks in a second
        /// </summary>
        protected const long TicksPerSecond = 10000000L;

        /// <summary>
        /// The amount of milliseconds in a second
        /// </summary>
        protected const long MillisecondPerSecond = 1000L;
        #endregion

        #region Fields
        /// <summary>
        /// UT of the last frame
        /// </summary>
        protected double lastCheck;

        /// <summary>
        /// Total elapsed time calculated by the watch in seconds
        /// </summary>
        protected double totalSeconds;
        #endregion

        #region Propreties
        /// <summary>
        /// If the watch is currently counting down time
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// The current elapsed time of the watch
        /// </summary>
        public TimeSpan Elapsed => new TimeSpan(this.ElapsedTicks);

        /// <summary>
        /// The amount of milliseconds elapsed to the current watch
        /// </summary>
        public long ElapsedMilliseconds
        {
            get
            {
                if (this.IsRunning) { UpdateWatch(); }
                return (long)Math.Round(this.totalSeconds * MillisecondPerSecond);
            }
        }

        /// <summary>
        /// The amount of ticks elapsed to the current watch
        /// </summary>
        public long ElapsedTicks
        {
            get
            {
                if (this.IsRunning) { UpdateWatch(); }
                return (long)(this.totalSeconds * TicksPerSecond);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new PhysicsWatch
        /// </summary>
        public PhysicsWatch() { }

        /// <summary>
        /// Creates a new PhysicsWatch starting at a certain amount of time
        /// </summary>
        /// <param name="seconds">Time to start at, in seconds</param>
        public PhysicsWatch(double seconds)
        {
            this.totalSeconds = seconds;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the watch
        /// </summary>
        public void Start()
        {
            if (!this.IsRunning)
            {
                this.lastCheck = Planetarium.GetUniversalTime();
                this.IsRunning = true;
            }
        }

        /// <summary>
        /// Stops the watch
        /// </summary>
        public void Stop()
        {
            if (this.IsRunning)
            {
                UpdateWatch();
                this.IsRunning = false;
            }
        }

        /// <summary>
        /// Resets the watch to zero and starts it
        /// </summary>
        public void Restart()
        {
            this.totalSeconds = 0d;
            this.lastCheck = Planetarium.GetUniversalTime();
            this.IsRunning = true;
        }

        /// <summary>
        /// Stops the watch and resets it to zero
        /// </summary>
        public void Reset()
        {
            this.totalSeconds = 0d;
            this.lastCheck = 0d;
            this.IsRunning = false;
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Updates the time on the watch
        /// </summary>
        protected virtual void UpdateWatch()
        {
            double current = Planetarium.GetUniversalTime();
            this.totalSeconds += current - this.lastCheck;
            this.lastCheck = current;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Returns a string representation fo this instance
        /// </summary>
        public override string ToString() => this.Elapsed.ToString();
        #endregion

        #region Static Methods
        /// <summary>
        /// Creates a new PhysicsWatch, starts it, and returns the current instance
        /// </summary>
        public static PhysicsWatch StartNew()
        {
            PhysicsWatch watch = new PhysicsWatch();
            watch.Start();
            return watch;
        }

        /// <summary>
        /// Creates a new PhysicsWatch from a certain amount of time, starts it, and returns the current instance
        /// </summary>
        /// <param name="seconds">Time to start the watch at, in seconds</param>
        public static PhysicsWatch StartNewFromTime(double seconds)
        {
            PhysicsWatch watch = new PhysicsWatch(seconds);
            watch.Start();
            return watch;
        }
        #endregion
    }
}