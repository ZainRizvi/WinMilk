﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WinMilk.Gui
{
    public partial class PivotListPage : PhoneApplicationPage
    {
        public static readonly DependencyProperty ListsProperty =
               DependencyProperty.Register("Lists", typeof(ObservableCollection<RTM.TaskList>), typeof(PivotListPage),
                   new PropertyMetadata((ObservableCollection<RTM.TaskList>)null));

        public ObservableCollection<RTM.TaskList> Lists
        {
            get { return (ObservableCollection<RTM.TaskList>)GetValue(ListsProperty); }
            set { SetValue(ListsProperty, value); }
        }

        public static readonly DependencyProperty CurrentListProperty =
               DependencyProperty.Register("CurrentList", typeof(RTM.TaskList), typeof(PivotListPage),
                   new PropertyMetadata(new RTM.TaskList(), new PropertyChangedCallback(OnCurrentListChanged)));

        public RTM.TaskList CurrentList
        {
            get { return (RTM.TaskList)GetValue(CurrentListProperty); }
            set { SetValue(CurrentListProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(PivotListPage),
                new PropertyMetadata((bool)false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public PivotListPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllLists();

            CreateApplicationBar();
        }
        
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void LoadAllLists()
        {
            IsLoading = true;
            
            App.Rest.GetLists((lists) =>
            {
                IsLoading = false;

                Lists = new ObservableCollection<RTM.TaskList>();
                
                foreach (RTM.TaskList l in lists)
                {
                    Lists.Add(l);
                }

                string idStr;

                if (this.NavigationContext.QueryString.TryGetValue("id", out idStr))
                {
                    // set current list
                    int listId = int.Parse(idStr);

                    // find list by id, and select it
                    foreach (RTM.TaskList l in Lists)
                    {
                        if (l.Id == listId)
                        {
                            CurrentList = l;
                            break;
                        }
                    }
                }
            }, false);
        }

        private void LoadList(RTM.TaskList list)
        {
            IsLoading = true;

            App.Rest.GetTasksInList(list, (ObservableCollection<RTM.Task> tasks) =>
                {
                    list.Tasks = new ObservableCollection<RTM.Task>();

                    foreach (RTM.Task t in tasks)
                    {
                        list.Tasks.Add(t);
                    }

                    IsLoading = false;
                }, false);
        }

        private void CreateApplicationBar()
        {
            // Build ApplicationBar with localized strings
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton add = new ApplicationBarIconButton(new Uri("/icons/appbar.add.rest.png", UriKind.Relative));
            add.Text = AppResources.AddTaskAppbar;
            //add.Click += new EventHandler(add_Click);
            ApplicationBar.Buttons.Add(add);

            ApplicationBarIconButton sync = new ApplicationBarIconButton(new Uri("/icons/appbar.refresh.rest.png", UriKind.Relative));
            sync.Text = AppResources.SyncAppbar;
            //sync.Click += new EventHandler(sync_Click);
            ApplicationBar.Buttons.Add(sync);

            ApplicationBarMenuItem pin = new ApplicationBarMenuItem(AppResources.PinAppbar);
            //settings.Click += new EventHandler(settings_Click);
            ApplicationBar.MenuItems.Add(pin);
        }

        private void ListsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            if (e.AddedItems[0] is RTM.TaskList)
            {
                RTM.TaskList selectedList = e.AddedItems[0] as RTM.TaskList;

                if (selectedList.Tasks == null || selectedList.Tasks.Count == 0)
                {
                    LoadList(selectedList);
                }
            }
        }

        private static void OnCurrentListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PivotListPage target = d as PivotListPage;
            RTM.TaskList oldList = e.OldValue as RTM.TaskList;
            RTM.TaskList newList = target.CurrentList;
            target.OnCurrentListChanged(oldList, newList);
        }

        private void OnCurrentListChanged(RTM.TaskList oldList, RTM.TaskList newList)
        {
            if (newList != oldList)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    ListsPivot.SelectedItem = newList;
                }));
            }
        }
    }
}