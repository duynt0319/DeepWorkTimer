using System;

namespace DeepWorkTimer.Models
{
    /// <summary>
    /// Enum defining different types of phases
    /// </summary>
    public enum PhaseType
    {
        Focus,      // Focused work time
        ShortBreak, // Short break between focus sessions
        LongBreak,  // Long break between blocks
        Lunch,      // Lunch break
        LightTask   // Light work, review
    }

    /// <summary>
    /// Model representing a phase in the work day
    /// </summary>
    public class PhaseModel
    {
        /// <summary>
        /// Phase start time
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Phase end time
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Display name of the phase
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Phase type
        /// </summary>
        public PhaseType Type { get; set; }

        /// <summary>
        /// Calculate phase duration
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;

        /// <summary>
        /// Constructor
        /// </summary>
        public PhaseModel(TimeSpan startTime, TimeSpan endTime, string name, PhaseType type)
        {
            StartTime = startTime;
            EndTime = endTime;
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Check if current time falls within this phase
        /// </summary>
        public bool IsCurrentPhase(TimeSpan currentTime)
        {
            return currentTime >= StartTime && currentTime < EndTime;
        }

        /// <summary>
        /// Calculate remaining time in phase
        /// </summary>
        public TimeSpan GetRemainingTime(TimeSpan currentTime)
        {
            if (currentTime >= EndTime)
                return TimeSpan.Zero;
            
            if (currentTime < StartTime)
                return EndTime - StartTime;
            
            return EndTime - currentTime;
        }
    }
}