﻿using DynamicWin.Resources;
using DynamicWin.UI.Menu;
using DynamicWin.UI.Menu.Menus;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace DynamicWin.Main
{
    public partial class MainForm : Window
    {
        private static MainForm instance;
        public static MainForm Instance { get => instance; }

        public static Action<System.Windows.Input.MouseWheelEventArgs> onScrollEvent;


        private DateTime _lastRenderTime;
        private readonly TimeSpan _targetElapsedTime = TimeSpan.FromMilliseconds(16); // ~60 FPS

        public Action onMainFormRender;

        public MainForm()
        {
            InitializeComponent();

            CompositionTarget.Rendering += OnRendering;

            instance = this;

            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;
            this.AllowsTransparency = true;
            this.ShowInTaskbar = false;
            this.Title = "DynamicWin Overlay";

            SetMonitor(Settings.ScreenIndex);

            AddRenderer();

            Res.extensions.ForEach((x) => x.LoadExtension());
            MainForm.Instance.AllowDrop = true;
        }

        public void SetMonitor(int monitorIndex)
        {
            var screen = System.Windows.Forms.Screen.AllScreens[Math.Clamp(monitorIndex, 0, GetMonitorCount() - 1)];
            Settings.ScreenIndex = Math.Clamp(monitorIndex, 0, GetMonitorCount() - 1);

            if (screen != null)
            {
                if (!this.IsLoaded)
                    this.WindowStartupLocation = WindowStartupLocation.Manual;

                this.WindowState = WindowState.Normal;
                this.ResizeMode = ResizeMode.CanResize;

                var workingArea = screen.WorkingArea;

                this.Left = workingArea.Left;
                this.Top = workingArea.Top;
                this.Width = workingArea.Width;
                this.Height = workingArea.Height;

                this.ResizeMode = ResizeMode.NoResize;
            }
        }

        public static int GetMonitorCount()
        {
            return System.Windows.Forms.Screen.AllScreens.Length;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            var currentTime = DateTime.Now;
            if (currentTime - _lastRenderTime >= _targetElapsedTime)
            {
                _lastRenderTime = currentTime;

                onMainFormRender.Invoke();
            }
        }

        public bool isDragging = false;

        public void OnScroll(object? sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            onScrollEvent?.Invoke(e);
        }

        public void AddRenderer()
        {
            if (RendererMain.Instance != null) RendererMain.Instance.Destroy();

            var customControl = new RendererMain();
            
            var parent = new Grid();
            parent.Children.Add(customControl);

            this.Content = parent;
        }

        public void MainForm_DragEnter(object? sender, DragEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("DragEnter");

            isDragging = true;
            e.Effects = DragDropEffects.Copy;

            if (!(MenuManager.Instance.ActiveMenu is DropFileMenu)
                && !(MenuManager.Instance.ActiveMenu is ConfigureShortcutMenu))
            {
                MenuManager.OpenMenu(new DropFileMenu());
            }
        }

        public void MainForm_DragLeave(object? sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("DragLeave");

            isDragging = false;

            if (MenuManager.Instance.ActiveMenu is ConfigureShortcutMenu) return;
            MenuManager.OpenMenu(Res.HomeMenu);
        }

        bool isLocalDrag = false;

        internal void StartDrag(string[] files, Action callback)
        {
            if (isLocalDrag) return;

            Array.ForEach(files, file => { System.Diagnostics.Debug.WriteLine(file); });

            if (files == null) return;
            else if (files.Length <= 0) return;

            try
            {
                isLocalDrag = true;

                DataObject dataObject = new DataObject(DataFormats.FileDrop, files);
                var effects = DragDrop.DoDragDrop((DependencyObject)this, dataObject, DragDropEffects.Move | DragDropEffects.Copy);

                if (RendererMain.Instance != null) RendererMain.Instance.Destroy();
                this.Content = new Grid();
                AddRenderer();

                callback?.Invoke();

                isLocalDrag = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            if (e.Action == DragAction.Cancel)
            {
                isLocalDrag = false;
            }
            else if (e.Action == DragAction.Continue)
            {
                isLocalDrag = true;
            }
            else if (e.Action == DragAction.Drop)
            {
                isLocalDrag = false;
            }
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            base.OnDragOver(e);
        }

        public void OnDrop(object sender, System.Windows.DragEventArgs e)
        {
            isDragging = false;

            if(MenuManager.Instance.ActiveMenu is ConfigureShortcutMenu)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    ConfigureShortcutMenu.DropData(e);
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                DropFileMenu.Drop(e);
                MenuManager.Instance.QueueOpenMenu(Res.HomeMenu);
                Res.HomeMenu.isWidgetMode = false;
            }
        }
    }
}
