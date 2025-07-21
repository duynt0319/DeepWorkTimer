using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using DeepWorkTimer.Models;

namespace DeepWorkTimer.ViewModels
{
    /// <summary>
    /// Main ViewModel for the countdown timer application
    /// </summary>
    public class CountdownViewModel : ViewModelBase
    {
        private readonly DispatcherTimer _timer;
        private PhaseModel? _currentPhase;
        private PhaseModel? _nextPhase;
        private TimeSpan _remainingTime;
        private string _currentTimeDisplay = "";
        private SolidColorBrush _phaseColor = System.Windows.Media.Brushes.Gray;

        /// <summary>
        /// List of all phases in the day
        /// </summary>
        public ObservableCollection<PhaseModel> Phases { get; }

        /// <summary>
        /// Current phase
        /// </summary>
        public PhaseModel? CurrentPhase
        {
            get => _currentPhase;
            private set => SetProperty(ref _currentPhase, value);
        }

        /// <summary>
        /// Next phase
        /// </summary>
        public PhaseModel? NextPhase
        {
            get => _nextPhase;
            private set => SetProperty(ref _nextPhase, value);
        }

        /// <summary>
        /// Remaining time of the current phase
        /// </summary>
        public TimeSpan RemainingTime
        {
            get => _remainingTime;
            private set => SetProperty(ref _remainingTime, value);
        }

        /// <summary>
        /// Current time display
        /// </summary>
        public string CurrentTimeDisplay
        {
            get => _currentTimeDisplay;
            private set => SetProperty(ref _currentTimeDisplay, value);
        }

        /// <summary>
        /// Color corresponding to the phase type
        /// </summary>
        public SolidColorBrush PhaseColor
        {
            get => _phaseColor;
            private set => SetProperty(ref _phaseColor, value);
        }

        /// <summary>
        /// Current phase name for display
        /// </summary>
        public string CurrentPhaseName => CurrentPhase?.Name ?? "End of day";

        /// <summary>
        /// Next phase name
        /// </summary>
        public string NextPhaseName => NextPhase?.Name ?? "End";

        /// <summary>
        /// Start time of the next phase
        /// </summary>
        public string NextPhaseStartTime => NextPhase?.StartTime.ToString(@"hh\:mm") ?? "--:--";

        /// <summary>
        /// End time of the current phase
        /// </summary>
        public string CurrentPhaseEndTime => CurrentPhase?.EndTime.ToString(@"hh\:mm") ?? "--:--";

        /// <summary>
        /// Remaining time display as string
        /// </summary>
        public string RemainingTimeDisplay => RemainingTime.ToString(@"mm\:ss");

        /// <summary>
        /// Compact display information
        /// </summary>
        public string PhaseInfo
        {
            get
            {
                if (CurrentPhase == null) return "End of work day";

                var current = CurrentPhase.Name;
                var endTime = CurrentPhase.EndTime.ToString(@"hh\:mm");

                if (NextPhase != null)
                {
                    var nextTime = NextPhase.StartTime.ToString(@"hh\:mm");
                    return $"{current} until {endTime} | Next: {NextPhase.Name} at {nextTime}";
                }

                return $"{current} until {endTime}";
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CountdownViewModel()
        {
            Phases = new ObservableCollection<PhaseModel>();
            InitializePhases();

            // Initialize timer with 1 second interval
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Update immediately for the first time
            UpdateCurrentPhase();
        }

        /// <summary>
        /// Initialize all phases in the day according to requirements
        /// </summary>
        private void InitializePhases()
        {
            Phases.Clear();

            // Block 1: 8:00 – 9:30
            Phases.Add(new PhaseModel(new TimeSpan(8, 0, 0), new TimeSpan(8, 25, 0), "Focus #1", PhaseType.Focus));
            Phases.Add(new PhaseModel(new TimeSpan(8, 25, 0), new TimeSpan(8, 30, 0), "Short Break", PhaseType.ShortBreak));
            Phases.Add(new PhaseModel(new TimeSpan(8, 30, 0), new TimeSpan(8, 55, 0), "Focus #2", PhaseType.Focus));
            Phases.Add(new PhaseModel(new TimeSpan(8, 55, 0), new TimeSpan(9, 0, 0), "Short Break", PhaseType.ShortBreak));
            Phases.Add(new PhaseModel(new TimeSpan(9, 0, 0), new TimeSpan(9, 30, 0), "Focus #3", PhaseType.Focus));

            // Rest: 9:30 – 9:45
            Phases.Add(new PhaseModel(new TimeSpan(9, 30, 0), new TimeSpan(9, 45, 0), "Long Break", PhaseType.LongBreak));

            // Block 2: 9:45 – 11:15
            Phases.Add(new PhaseModel(new TimeSpan(9, 45, 0), new TimeSpan(10, 10, 0), "Focus #4", PhaseType.Focus));
            Phases.Add(new PhaseModel(new TimeSpan(10, 10, 0), new TimeSpan(10, 15, 0), "Short Break", PhaseType.ShortBreak));
            Phases.Add(new PhaseModel(new TimeSpan(10, 15, 0), new TimeSpan(10, 40, 0), "Focus #5", PhaseType.Focus));
            Phases.Add(new PhaseModel(new TimeSpan(10, 40, 0), new TimeSpan(10, 45, 0), "Short Break", PhaseType.ShortBreak));
            Phases.Add(new PhaseModel(new TimeSpan(10, 45, 0), new TimeSpan(11, 15, 0), "Focus #6", PhaseType.Focus));

            // Light Task: 11:15 – 12:00
            Phases.Add(new PhaseModel(new TimeSpan(11, 15, 0), new TimeSpan(12, 0, 0), "Light Task", PhaseType.LightTask));

            // Lunch: 12:00 – 12:50
            Phases.Add(new PhaseModel(new TimeSpan(12, 0, 0), new TimeSpan(12, 50, 0), "Lunch Break", PhaseType.Lunch));

            // Block 3: 12:50 – 14:20
            Phases.Add(new PhaseModel(new TimeSpan(12, 50, 0), new TimeSpan(13, 15, 0), "Focus #7", PhaseType.Focus));
            Phases.Add(new PhaseModel(new TimeSpan(13, 15, 0), new TimeSpan(13, 20, 0), "Short Break", PhaseType.ShortBreak));
            Phases.Add(new PhaseModel(new TimeSpan(13, 20, 0), new TimeSpan(13, 45, 0), "Focus #8", PhaseType.Focus));
            Phases.Add(new PhaseModel(new TimeSpan(13, 45, 0), new TimeSpan(13, 50, 0), "Short Break", PhaseType.ShortBreak));
            Phases.Add(new PhaseModel(new TimeSpan(13, 50, 0), new TimeSpan(14, 20, 0), "Focus #9", PhaseType.Focus));

            // Rest: 14:20 – 14:35
            Phases.Add(new PhaseModel(new TimeSpan(14, 20, 0), new TimeSpan(14, 35, 0), "Long Break", PhaseType.LongBreak));

            // Block 4: 14:35 – 16:05
            Phases.Add(new PhaseModel(new TimeSpan(14, 35, 0), new TimeSpan(15, 0, 0), "Focus #10", PhaseType.Focus));
            Phases.Add(new PhaseModel(new TimeSpan(15, 0, 0), new TimeSpan(15, 5, 0), "Short Break", PhaseType.ShortBreak));
            Phases.Add(new PhaseModel(new TimeSpan(15, 5, 0), new TimeSpan(15, 30, 0), "Focus #11", PhaseType.Focus));
            Phases.Add(new PhaseModel(new TimeSpan(15, 30, 0), new TimeSpan(15, 35, 0), "Short Break", PhaseType.ShortBreak));
            Phases.Add(new PhaseModel(new TimeSpan(15, 35, 0), new TimeSpan(16, 5, 0), "Focus #12", PhaseType.Focus));

            // Review / Light Task: 16:05 – 17:30
            Phases.Add(new PhaseModel(new TimeSpan(16, 5, 0), new TimeSpan(17, 30, 0), "Review & Light Task", PhaseType.LightTask));
        }

        /// <summary>
        /// Handle timer tick event
        /// </summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateCurrentPhase();
        }

        /// <summary>
        /// Update current phase and related information
        /// </summary>
        private void UpdateCurrentPhase()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            CurrentTimeDisplay = DateTime.Now.ToString("HH:mm:ss");

            // Find current phase
            var newCurrentPhase = Phases.FirstOrDefault(p => p.IsCurrentPhase(currentTime));

            // Find next phase
            PhaseModel? newNextPhase = null;
            if (newCurrentPhase != null)
            {
                var currentIndex = Phases.IndexOf(newCurrentPhase);
                if (currentIndex >= 0 && currentIndex < Phases.Count - 1)
                {
                    newNextPhase = Phases[currentIndex + 1];
                }
            }
            else
            {
                // If no current phase, find the next closest phase
                newNextPhase = Phases.FirstOrDefault(p => p.StartTime > currentTime);
            }

            // Update if there are changes
            if (newCurrentPhase != CurrentPhase)
            {
                CurrentPhase = newCurrentPhase;
                UpdatePhaseColor();
                OnPropertyChanged(nameof(CurrentPhaseName));
                OnPropertyChanged(nameof(CurrentPhaseEndTime));
                OnPropertyChanged(nameof(PhaseInfo));
            }

            if (newNextPhase != NextPhase)
            {
                NextPhase = newNextPhase;
                OnPropertyChanged(nameof(NextPhaseName));
                OnPropertyChanged(nameof(NextPhaseStartTime));
                OnPropertyChanged(nameof(PhaseInfo));
            }

            // Update remaining time
            if (CurrentPhase != null)
            {
                RemainingTime = CurrentPhase.GetRemainingTime(currentTime);
            }
            else
            {
                RemainingTime = TimeSpan.Zero;
            }

            OnPropertyChanged(nameof(RemainingTimeDisplay));
        }

        /// <summary>
        /// Update color according to phase type
        /// </summary>
        private void UpdatePhaseColor()
        {
            if (CurrentPhase == null)
            {
                PhaseColor = System.Windows.Media.Brushes.Gray;
                return;
            }

            PhaseColor = CurrentPhase.Type switch
            {
                PhaseType.Focus => System.Windows.Media.Brushes.DodgerBlue,      // Blue for Focus
                PhaseType.ShortBreak => System.Windows.Media.Brushes.Gold,       // Gold for Short Break
                PhaseType.LongBreak => System.Windows.Media.Brushes.Orange,      // Orange for Long Break
                PhaseType.Lunch => System.Windows.Media.Brushes.OrangeRed,       // Orange-Red for Lunch
                PhaseType.LightTask => System.Windows.Media.Brushes.LightGray,   // Light Gray for Light Task
                _ => System.Windows.Media.Brushes.Gray
            };
        }

        /// <summary>
        /// Cleanup when disposing
        /// </summary>
        public void Cleanup()
        {
            _timer?.Stop();
        }
    }
}