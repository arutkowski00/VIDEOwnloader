#region License

// VIDEOwnloader
// Copyright (C) 2016 Adam Rutkowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VIDEOwnloader.Common
{
    using ProgressItem = KeyValuePair<long, float>;

    public interface IEtaCalculator
    {
        /// <summary>
        ///     Calculates the Estimated Time of Arrival (Completion)
        /// </summary>
        DateTime Eta { get; }

        /// <summary>
        ///     Returns True when there is enough data to calculate the ETA.
        ///     Returns False if the ETA is still calculating.
        /// </summary>
        bool EtaIsAvailable { get; }

        /// <summary>
        ///     Calculates the Estimated Time Remaining.
        /// </summary>
        TimeSpan Etr { get; }

        /// <summary>
        ///     Clears all collected data.
        /// </summary>
        void Reset();

        /// <summary>
        ///     Updates the current progress.
        /// </summary>
        /// <param name="progress">
        ///     The current level of completion.
        ///     Must be between 0.0 and 1.0 (inclusively).
        /// </param>
        void Update(float progress);
    }

    /// <summary>
    ///     Calculates the "Estimated Time of Arrival"
    ///     (or more accurately, "Estimated Time of Completion"),
    ///     based on a "rolling average" of progress over time.
    /// </summary>
    public class EtaCalculator : IEtaCalculator
    {
        private const float FloatTolerance = 0.001f;
        private readonly long _maximumTicks;

        private readonly int _minimumData;
        private readonly Queue<ProgressItem> _queue;
        private readonly Stopwatch _timer;

        private ProgressItem _current;
        private ProgressItem _oldest;

        /// <summary>
        /// </summary>
        /// <param name="minimumData">
        ///     The minimum number of data points required before ETA can be calculated.
        /// </param>
        /// <param name="maximumDuration">
        ///     Determines how many seconds of data will be used to calculate the ETA.
        /// </param>
        public EtaCalculator(int minimumData, double maximumDuration)
        {
            _minimumData = minimumData;
            _maximumTicks = (long)(maximumDuration*Stopwatch.Frequency);
            _queue = new Queue<ProgressItem>(minimumData*2);
            _timer = Stopwatch.StartNew();
        }

        #region IEtaCalculator Members

        /// <summary>
        ///     Calculates the Estimated Time of Arrival (Completion)
        /// </summary>
        public DateTime Eta => DateTime.Now.Add(Etr);

        /// <summary>
        ///     Returns True when there is enough data to calculate the ETA.
        ///     Returns False if the ETA is still calculating.
        /// </summary>
        public bool EtaIsAvailable
            => (_queue.Count >= _minimumData) && (Math.Abs(_oldest.Value - _current.Value) > FloatTolerance);

        /// <summary>
        ///     Calculates the Estimated Time Remaining
        /// </summary>
        public TimeSpan Etr
        {
            get
            {
                // Create local copies of the oldest & current,
                // so that another thread can update them without locking:
                var oldest = _oldest;
                var current = _current;

                // Make sure we have enough items:
                if ((_queue.Count < _minimumData) || (Math.Abs(oldest.Value - current.Value) < FloatTolerance))
                    return TimeSpan.MaxValue;

                // Calculate the estimated finished time:
                var finishedInTicks = (1.0d - current.Value)*(current.Key - oldest.Key)/(current.Value - oldest.Value);

                return TimeSpan.FromSeconds(finishedInTicks/Stopwatch.Frequency);
            }
        }

        public void Reset()
        {
            _queue.Clear();

            _timer.Reset();
            _timer.Start();
        }

        /// <summary>
        ///     Adds the current progress to the calculation of ETA.
        /// </summary>
        /// <param name="progress">
        ///     The current level of completion.
        ///     Must be between 0.0 and 1.0 (inclusively).
        /// </param>
        public void Update(float progress)
        {
            // If progress hasn't changed, ignore:
            if (Math.Abs(_current.Value - progress) < FloatTolerance)
                return;

            // Clear space for this item:
            ClearExpired();

            // Queue this item:
            var currentTicks = _timer.ElapsedTicks;
            _current = new ProgressItem(currentTicks, progress);
            _queue.Enqueue(_current);

            // See if its the first item:
            if (_queue.Count == 1)
                _oldest = _current;
        }

        #endregion

        private void ClearExpired()
        {
            var expired = _timer.ElapsedTicks - _maximumTicks;
            while ((_queue.Count > _minimumData) && (_queue.Peek().Key < expired))
                _oldest = _queue.Dequeue();
        }
    }
}