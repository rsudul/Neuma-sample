using System;
using System.Collections.Generic;
using Godot;
using Neuma.Core.Relations;

namespace Neuma.Core.TerminalFeed
{
    public sealed class TerminalController : ITerminalController, IDisposable
    {
        private readonly LinkSelectionController _linkSelectionController;

        private readonly Dictionary<string, ITerminalApp> _apps = new();
        private ITerminalApp _currentApp;

        private bool _isDisposed;

        public ITerminalApp? CurrentApp => _currentApp;

        public event EventHandler OnAppChanged;
        public event EventHandler OnStateChanged;

        public TerminalController(IEnumerable<ITerminalApp> apps, LinkSelectionController linkSelectionController)
        {
            _linkSelectionController = linkSelectionController
                ?? throw new ArgumentNullException(nameof(linkSelectionController));

            foreach (var app in apps)
            {
                if (_apps.ContainsKey(app.AppId))
                {
                    continue;
                }
                _apps[app.AppId] = app;
            }
        }

        public void OpenApp(string appId)
        {
            if (_isDisposed)
            {
                return;
            }

            if (!_apps.TryGetValue(appId, out var app) || _currentApp == app)
            {
                return;
            }

            CloseCurrentApp();

            _currentApp = app;
            if (_currentApp is IAnchorSelectable selectableApp)
            {
                selectableApp.OnAnchorSelected += _linkSelectionController.HandleAnchorSelected;
            }
            _currentApp.OnStateChanged += OnAppStateChanged;
            _currentApp.OnOpen();

            OnAppChanged?.Invoke(this, EventArgs.Empty);
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void CloseCurrentApp()
        {
            if (_isDisposed || _currentApp == null)
            {
                return;
            }

            _currentApp.OnStateChanged -= OnAppStateChanged;
            if (_currentApp is IAnchorSelectable selectableApp)
            {
                selectableApp.OnAnchorSelected -= _linkSelectionController.HandleAnchorSelected;
            }
            _currentApp.OnClose();
            _currentApp = null;

            OnAppChanged?.Invoke(this, EventArgs.Empty);
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ProcessInput(TerminalCommand command)
        {
            if (_isDisposed)
            {
                return;
            }

            if (_currentApp != null)
            {
                _currentApp.HandleCommand(command);
            }
        }

        public void ProcessPointer(Vector2 normalizedPosition, bool isClick)
        {
            if (_isDisposed)
            {
                return;
            }

            if (_currentApp != null)
            {
                _currentApp.HandlePointer(normalizedPosition, isClick);
            }
        }

        private void OnAppStateChanged(object sender, EventArgs e)
        {
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                CloseCurrentApp();

                OnAppChanged = null;
                OnStateChanged = null;

                _apps.Clear();
            }

            _isDisposed = true;
        }
    }
}

