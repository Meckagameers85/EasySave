using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySaveProject.Models
{
    /// <summary>
    /// États possibles d'une sauvegarde
    /// </summary>
    public enum BackupState
    {
        NotStarted,  // Pas encore démarrée
        Running,     // En cours d'exécution
        Paused,      // En pause
        Stopped,     // Arrêtée
        Completed,   // Terminée avec succès
        Error        // Erreur
    }

    /// <summary>
    /// Modèle pour suivre la progression d'une sauvegarde en temps réel
    /// Implémente INotifyPropertyChanged pour le binding WPF
    /// </summary>
    public class BackupProgress : INotifyPropertyChanged
    {
        private BackupState _state = BackupState.NotStarted;
        private double _progressPercentage = 0;
        private string _statusMessage = "Prêt";

        /// <summary>
        /// État actuel de la sauvegarde
        /// </summary>
        public BackupState State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanPlay));
                    OnPropertyChanged(nameof(CanPause));
                    OnPropertyChanged(nameof(CanStop));
                }
            }
        }

        /// <summary>
        /// Pourcentage de progression (0-100)
        /// </summary>
        public double ProgressPercentage
        {
            get => _progressPercentage;
            set
            {
                if (Math.Abs(_progressPercentage - value) > 0.01)
                {
                    _progressPercentage = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Message de statut affiché à l'utilisateur
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        // Propriétés calculées pour l'interface utilisateur
        public bool CanPlay => State == BackupState.NotStarted || State == BackupState.Paused || State == BackupState.Stopped;
        public bool CanPause => State == BackupState.Running;
        public bool CanStop => State == BackupState.Running || State == BackupState.Paused;

        /// <summary>
        /// Événement déclenché quand une propriété change (pour le binding WPF)
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}